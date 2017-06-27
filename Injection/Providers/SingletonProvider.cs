using System;

namespace Injection
{
  public class SingletonProvider : Provider
  {
    private readonly bool _oneInstance;
    private object _instance;
    public SingletonProvider(Type type, object value, bool oneInstance) : base(type, value)
    {
      _oneInstance = oneInstance;
    }

    public override void Dispose()
    {
      base.Dispose();
      _instance = null;
    }

    public override object Apply(IInjector injector, Type type)
    {
      if (_instance == null)
      {
        var clazz = Value as Type;
        if (_oneInstance && type != clazz)
        {
          _instance = injector.Get(clazz);
        }
        if (_instance == null)
        {
          var factory = new FactoryProvider(clazz, clazz);
          _instance = factory.Create(injector, clazz);
          factory.Dispose();
          injector.Inject(_instance);
        }
      }
      return _instance;
    }
  }
}
