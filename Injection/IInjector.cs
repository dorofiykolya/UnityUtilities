using System;

namespace Injection
{
  public interface IInjector : IInject
  {
    IMapping Map<T>() where T : class;
    IMapping Map(Type type);
    void Unmap<T>() where T : class;
    void Unmap(Type type);
    IInjector Parent { get; }
    T Get<T>() where T : class;
    Object Get(Type type);
    IProvider GetProvider(Type type);
    void MapProvider<T>(IProvider provider) where T : class;
    void MapProvider(Type type, IProvider provider);
    void UnmapProvider<T>() where T : class;
    void UnmapProvider(Type type);
    DescriptionProvider DescriptionProvider { get; }
  }
}
