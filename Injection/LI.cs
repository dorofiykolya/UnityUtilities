using System;

namespace Injection
{
  /// <summary>
  /// Lazy Inject
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public sealed class LI<T>
  {
    public static implicit operator T(LI<T> lazy)
    {
      return lazy.Value;
    }

    private readonly Func<object> _factory;
    private T _value;
    private bool _initialized;

    public LI(Func<object> factory)
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