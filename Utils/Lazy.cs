using System;

namespace Utils
{
  public class Lazy<T>
  {
    private readonly Func<T> _factory;
    private T _value;
    private bool _initialized;

    public Lazy(Func<T> factory)
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
            _value = _factory();
            _initialized = true;
          }
        }

        return _value;
      }
    }
  }
}