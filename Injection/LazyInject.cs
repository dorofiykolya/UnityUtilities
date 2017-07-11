using System;

namespace Injection
{
  public sealed class LazyInject<T>
  {
    public static implicit operator T(LazyInject<T> lazy)
    {
      return lazy.Value;
    }

    private readonly Func<object> _factory;
    private T _value;
    private bool _initialized;

    public LazyInject(Func<object> factory)
    {
      _factory = factory;
    }

    public T Value
    {
      get
      {
        lock (_factory)
        {
          if (!_initialized)
          {
            _value = (T)_factory();
            _initialized = true;
          }
        }

        return _value;
      }
    }
  }
}