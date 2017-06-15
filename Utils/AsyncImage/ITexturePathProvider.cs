using System;

namespace Utils.AsyncImage
{
  public interface ITexturePathProvider
  {
    ITexturePath Get(string spriteName);
    bool HasPath(string spriteName);
  }

  public interface ITexturePath
  {
    string Name { get; }
    string FileName { get; }
    string Uri { get; }
    bool IsReady { get; }
    int Version { get; }
    BundleResourceType ResourceType { get; }
    bool Exists { get; }
    void SubscribeOnReady(Lifetime lifetime, Action<ITexturePath> listener);
  }
}
