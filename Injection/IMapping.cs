using System;

namespace Injection
{
  public interface IMapping : IDisposable
  {
    Type Type { get; }
    IProvider Provider { get; }
    void ToFactory<T>() where T : class;
    void ToFactory(Type type);
    void ToValue(Object value);
    void ToSingleton<T>(bool oneInstance = true) where T : class;
    void ToSingleton(Type type, bool oneInstance = true);
    void ToProvider(IProvider provider);
    void AsSingleton();
    void AsFactory();
  }
}
