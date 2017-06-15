using System;
using System.Collections.Generic;
using AsyncBundles;
using Utils;
using Utils.AsyncImage;

namespace UnityEngine
{
  public class AsyncSpriteManager
  {
    private readonly Lifetime _lifetime;
    private readonly ITexturePathProvider _provider;
    private readonly AsyncBundleManager _bundleManager;
    private readonly ICoroutineProvider _coroutineProvider;
    private readonly AsyncPreloaderData _preloaderData;
    private readonly Dictionary<string, AsyncSprite> _mapSprites;
    private readonly Dictionary<ITexturePath, IAsyncTextureAtlas> _mapAtlases;
    private readonly Signal<IAsyncTextureAtlas> _onAtlasCreated;

    public AsyncSpriteManager(Lifetime lifetime, ITexturePathProvider provider, AsyncBundleManager bundleManager, ICoroutineProvider coroutineProvider, AsyncPreloaderData preloaderData = null)
    {
      _lifetime = lifetime;
      _provider = provider;
      _bundleManager = bundleManager;
      _coroutineProvider = coroutineProvider;
      _preloaderData = preloaderData;
      _mapSprites = new Dictionary<string, AsyncSprite>();
      _mapAtlases = new Dictionary<ITexturePath, IAsyncTextureAtlas>();
      _onAtlasCreated = new Signal<IAsyncTextureAtlas>(lifetime);

      lifetime.AddAction(() =>
      {
        _mapAtlases.Clear();
        _mapSprites.Clear();
      });
    }

    public void SubscribeOnAtlasCreated(Lifetime lifetime, Action<IAsyncTextureAtlas> listener)
    {
      _onAtlasCreated.Subscribe(lifetime, listener);
    }

    public AsyncSprite Get(string spriteName)
    {
      AsyncSprite result;
      if (!_mapSprites.TryGetValue(spriteName, out result))
      {
        var atlas = GetTextureAtlas(spriteName);
        result = new AsyncSprite(_lifetime, atlas, spriteName, _preloaderData);
        _mapSprites[spriteName] = result;
      }
      return result;
    }

    public bool HasSprite(string name)
    {
      return _provider.HasPath(name);
    }

    private IAsyncTextureAtlas GetTextureAtlas(string spriteName)
    {
      var path = _provider.Get(spriteName);
      IAsyncTextureAtlas result;
      if (!_mapAtlases.TryGetValue(path, out result))
      {
        result = new AsyncTextureAtlas(_lifetime, path, _bundleManager, _coroutineProvider);
        _mapAtlases[path] = result;
        _onAtlasCreated.Fire(result);
      }
      return result;
    }

    public IDisposable LockUnload()
    {
      return new LockUnloadCache(_lifetime, _mapAtlases, this);
    }

    private class LockUnloadCache : IDisposable
    {
      private readonly Lifetime.Definition _lifetime;

      public LockUnloadCache(Lifetime lifetime, Dictionary<ITexturePath, IAsyncTextureAtlas> atlases, AsyncSpriteManager spriteManager)
      {
        _lifetime = Lifetime.Define(lifetime);
        var sprite = new AsyncSprite(_lifetime.Lifetime, null, null);
        spriteManager.SubscribeOnAtlasCreated(_lifetime.Lifetime, atlas =>
        {
          atlas.SubscribeOnRetain(_lifetime.Lifetime, textureAtlas =>
          {
            textureAtlas.Retain(sprite);
          });
        });

        foreach (var atlas in atlases.Values)
        {
          if (atlas.RefsCount != 0)
          {
            atlas.Retain(sprite);
          }
          else
          {
            atlas.SubscribeOnRetain(_lifetime.Lifetime, textureAtlas =>
            {
              textureAtlas.Retain(sprite);
            });
          }
        }

        _lifetime.Lifetime.AddAction(() =>
        {
          foreach (var atlas in atlases.Values)
          {
            atlas.Release(sprite);
          }
        });
      }

      public void Dispose()
      {
        _lifetime.Terminate();
      }
    }

  }
}
