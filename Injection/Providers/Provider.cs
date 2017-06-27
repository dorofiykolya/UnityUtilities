using System;

namespace Injection
{
  public abstract class Provider : IProvider
  {
    protected Type Type;
    protected Object Value;
    protected Boolean Disposed;

    protected Provider(Type type, Object value)
    {
      this.Type = type;
      this.Value = value;
    }
    public virtual void Dispose()
    {
      if (!Disposed)
      {
        this.Type = null;
        this.Value = null;
        Disposed = true;
      }
    }

    public abstract object Apply(IInjector injector, Type type);
  }
}
