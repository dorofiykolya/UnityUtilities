using System;

namespace Injection
{
  public interface IProvider : IDisposable
  {
    object Apply(IInjector injector, Type type);
  }
}
