using System;
using System.Collections.Generic;
using Utils;
using Utils.AsyncImage;

namespace UnityEngine
{
  public interface IAsyncTextureAtlas
  {
    void SubscribeOnLoaded(Lifetime lifetime, Action<IAsyncTextureAtlas> listener);
    void SubscribeOnUnLoaded(Lifetime lifetime, Action<IAsyncTextureAtlas> listener);
    void SubscribeOnRelease(Lifetime lifetime, Action<IAsyncTextureAtlas> listener);
    void SubscribeOnRetain(Lifetime lifetime, Action<IAsyncTextureAtlas> listener);
    ITexturePath TexturePath { get; }
    int RefsCount { get; }
    IEnumerable<AsyncSprite> Refs { get; }
    Sprite Get(string spriteName);
    void Release(AsyncSprite asyncSprite);
    void Retain(AsyncSprite asyncSprite);
    void Unload();
  }
}