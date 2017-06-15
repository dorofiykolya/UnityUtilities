using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using Utils;

namespace UnityEngine
{
  public class AsyncSprite
  {
    private readonly HashSet<Object> _refObjects;
    private readonly ICoroutineProvider _coroutineProvider;
    private readonly IAsyncTextureAtlas _textureAtlas;
    private readonly string _spriteName;
    private readonly AsyncPreloaderData _asyncPreloaderData;
    private readonly Signal<AsyncSprite> _onLoaded;
    private readonly Signal<AsyncSprite> _onUnloaded;
    private Coroutine _loadCoroutine;
    private ResourceRequest _request;

    public AsyncSprite(Lifetime lifetime, IAsyncTextureAtlas textureAtlas, string spriteName, AsyncPreloaderData preloaderData = null)
    {
      _refObjects = new HashSet<Object>();
      _onLoaded = new Signal<AsyncSprite>(lifetime);
      _onUnloaded = new Signal<AsyncSprite>(lifetime);
      _textureAtlas = textureAtlas;
      _spriteName = spriteName;
      _asyncPreloaderData = preloaderData;
      if (_textureAtlas != null)
      {
        _textureAtlas.SubscribeOnLoaded(lifetime, atlas => _onLoaded.Fire(this));
        _textureAtlas.SubscribeOnUnLoaded(lifetime, atlas => _onUnloaded.Fire(this));
      }
    }

    public bool IsReady { get { return Sprite != null; } }
    public string Name { get { return _spriteName; } }
    public Sprite Sprite { get { return _textureAtlas != null ? _textureAtlas.Get(_spriteName) : null; } }
    public int RefsCount { get { return _refObjects.Count; } }
    public IEnumerable<Object> Refs { get { return _refObjects; } }
    public AsyncPreloaderData PreloaderData { get { return _asyncPreloaderData; } }
    
    public void SubscribeOnLoaded(Lifetime lifetime, Action<AsyncSprite> listener)
    {
      _onLoaded.Subscribe(lifetime, listener);
    }

    public void SubscribeOnUnloaded(Lifetime lifetime, Action<AsyncSprite> listener)
    {
      _onUnloaded.Subscribe(lifetime, listener);
    }

    public void Release(Object image)
    {
      Assert.IsNotNull(image);
      _refObjects.Remove(image);
      if (_refObjects.Count == 0)
      {
        if (_textureAtlas != null)
        {
          _textureAtlas.Release(this);
        }
      }
    }

    public void Retain(Object image)
    {
      Assert.IsNotNull(image);
      _refObjects.Add(image);

      if (_textureAtlas != null)
      {
        _textureAtlas.Retain(this);
      }
    }

    public void Unload()
    {
      if (_textureAtlas != null)
      {
        _textureAtlas.Unload();
      }
    }
  }
}
