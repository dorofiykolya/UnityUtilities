using System;
using System.Collections;
using System.Collections.Generic;
using AsyncBundles;
using UnityEngine;
using UnityEngine.Assertions;

namespace Utils.AsyncImage
{
  public class AsyncTextureAtlas : IAsyncTextureAtlas
  {
    private static int _instances;

    private readonly Lifetime _lifetime;
    private readonly ITexturePath _path;
    private readonly AsyncBundleManager _bundleManager;
    private readonly ICoroutineProvider _coroutineProvider;
    private readonly int _instanceId;
    private readonly HashSet<AsyncSprite> _refs;
    private readonly Dictionary<string, Sprite> _sprites;
    private readonly Signal<IAsyncTextureAtlas> _onLoaded;
    private readonly Signal<IAsyncTextureAtlas> _onUnloaded;
    private readonly Signal<IAsyncTextureAtlas> _onRelease;
    private readonly Signal<IAsyncTextureAtlas> _onRetain;

    private bool _loaded;
    private Lifetime.Definition _loadDefinition;
    private AsyncTextureAtlasSettings _settings;

    public AsyncTextureAtlas(Lifetime lifetime, ITexturePath path, AsyncBundleManager bundleManager, ICoroutineProvider coroutineProvider)
    {
      _lifetime = lifetime;
      _path = path;
      _bundleManager = bundleManager;
      _coroutineProvider = coroutineProvider;
      _instanceId = ++_instances;
      _refs = new HashSet<AsyncSprite>();
      _sprites = new Dictionary<string, Sprite>();
      _onLoaded = new Signal<IAsyncTextureAtlas>(lifetime);
      _onUnloaded = new Signal<IAsyncTextureAtlas>(lifetime);
      _onRelease = new Signal<IAsyncTextureAtlas>(lifetime);
      _onRetain = new Signal<IAsyncTextureAtlas>(lifetime);
    }

    public int InstanceId { get { return _instanceId; } }
    public ITexturePath TexturePath { get { return _path; } }
    public int RefsCount { get { return _refs.Count; } }
    public IEnumerable<AsyncSprite> Refs { get { return _refs; } }

    public Sprite Get(string spriteName)
    {
      Sprite result;
      _sprites.TryGetValue(spriteName, out result);
      return result;
    }

    public void Release(AsyncSprite asyncSprite)
    {
      Assert.IsNotNull(asyncSprite);
      if (_refs.Remove(asyncSprite))
      {
        _onRelease.Fire(this);
        if (_refs.Count == 0)
        {
          StopLoad();
        }
      }
    }

    public void Retain(AsyncSprite asyncSprite)
    {
      Assert.IsNotNull(asyncSprite);
      if (_refs.Add(asyncSprite))
      {
        _onRetain.Fire(this);
        StartLoad();
      }
    }

    public void Unload()
    {
      StopLoad();
    }

    public void SubscribeOnLoaded(Lifetime lifetime, Action<IAsyncTextureAtlas> listener)
    {
      _onLoaded.Subscribe(lifetime, listener);
    }

    public void SubscribeOnUnLoaded(Lifetime lifetime, Action<IAsyncTextureAtlas> listener)
    {
      _onUnloaded.Subscribe(lifetime, listener);
    }

    public void SubscribeOnRelease(Lifetime lifetime, Action<IAsyncTextureAtlas> listener)
    {
      _onRelease.Subscribe(lifetime, listener);
    }

    public void SubscribeOnRetain(Lifetime lifetime, Action<IAsyncTextureAtlas> listener)
    {
      _onRetain.Subscribe(lifetime, listener);
    }

    private void StartLoad()
    {
      if (_path.IsReady)
      {
        LoadBundle();
      }
      else
      {
        _path.SubscribeOnReady(_lifetime, path =>
        {
          if (_refs.Count != 0)
          {
            StartLoad();
          }
          else
          {
            StopLoad();
          }
        });
      }
    }

    private void StopLoad()
    {
      if (_loadDefinition != null)
      {
        var definition = _loadDefinition;
        _loadDefinition = null;

        if (_loaded)
        {
          _onUnloaded.Fire(this);
        }
        definition.Terminate();
      }
    }

    private void LoadBundle()
    {
      if (_loadDefinition == null)
      {
        var definition = Lifetime.Define(_lifetime);
        _loadDefinition = definition;

        var asyncBundle = _bundleManager.Get(_path.Uri, _path.Version);
        definition.Lifetime.AddBracket(() =>
        {
          var startLoad = false;
          Action<AsyncBundle> onLoaded = bundle =>
          {
            if (startLoad) return;
            startLoad = true;
            if (!asyncBundle.Loader.IsError && asyncBundle.Bundle != null)
            {
              var type = AsyncResourceType.GetType(_path.ResourceType);
              var asyncLoad = asyncBundle.Bundle.LoadAssetAsync(_path.FileName, type);
              var coroutine = _coroutineProvider.StartCoroutine(LoadAsync(asyncLoad));

              var asyncLifetime = Lifetime.Define(definition.Lifetime);
              asyncLifetime.Lifetime.AddAction(() =>
              {
                _coroutineProvider.StopCoroutine(coroutine);
              });
            }
          };

          asyncBundle.SubscribeOnLoaded(definition.Lifetime, onLoaded);
          asyncBundle.SubscribeOnUnloaded(definition.Lifetime, bundle =>
          {
            StopLoad();
          });
          asyncBundle.Retain(this);
          if (asyncBundle.Loader.IsLoaded)
          {
            onLoaded(asyncBundle);
          }
        }, () =>
        {
          _sprites.Clear();
          if (_settings != null)
          {
            if (_settings.Texture2D != null)
            {
              Resources.UnloadAsset(_settings.Texture2D);
            }
            _settings = null;
          }
          asyncBundle.Release(this);
        });
      }
    }

    private IEnumerator LoadAsync(AssetBundleRequest asyncRequest)
    {
      yield return asyncRequest;

      var spriteAsset = asyncRequest.asset as Sprite;
      if (spriteAsset != null)
      {
        _sprites[spriteAsset.name] = spriteAsset;
        _settings = null;
        _loaded = true;
        _onLoaded.Fire(this);
        yield break;
      }

      var settings = asyncRequest.asset as AsyncTextureAtlasSettings;
      if (settings != null)
      {
        foreach (var sprite in settings.Sprites)
        {
          if (sprite != null)
          {
            _sprites[sprite.name] = sprite;
          }
        }
        _settings = settings;
        _loaded = true;
        _onLoaded.Fire(this);
      }
    }
  }
}