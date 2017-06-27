using System;

namespace Injection
{
  public class Mapping : IMapping
  {
    private IInjector _injector;
    private Type _type;

    public Mapping(IInjector injector, Type type)
    {
      _injector = injector;
      _type = type;
    }

    public Type Type { get { return _type; } }

    public void Dispose()
    {
      _injector = null;
      _type = null;
    }

    public IProvider Provider
    {
      get { return _injector.GetProvider(_type); }
    }

    public void ToFactory<TF>() where TF : class
    {
      ToFactory(typeof(TF));
    }

    public void ToFactory(Type type)
    {
      if (type.IsInterface || type.IsAbstract)
      {
        throw new ArgumentException();
      }
      ToProvider(new FactoryProvider(_type, type));
    }

    public void AsFactory()
    {
      if (_type.IsInterface || _type.IsAbstract)
      {
        throw new ArgumentException();
      }
      ToFactory(_type);
    }

    public void ToValue(object value)
    {
      ToProvider(new ValueProvider(_type, value));
    }

    public void ToSingleton<TS>(bool oneInstance = true) where TS : class
    {
      ToSingleton(typeof(TS), oneInstance);
    }

    public void ToSingleton(Type type, bool oneInstance = true)
    {
      if (type.IsInterface || type.IsAbstract)
      {
        throw new ArgumentException();
      }
      ToProvider(new SingletonProvider(_type, type, oneInstance));
    }

    public void ToProvider(IProvider provider)
    {
      _injector.MapProvider(_type, provider);
    }

    public void AsSingleton()
    {
      if (_type.IsInterface || _type.IsAbstract)
      {
        throw new ArgumentException();
      }
      ToSingleton(_type);
    }
  }
}
