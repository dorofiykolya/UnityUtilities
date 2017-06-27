using System;

namespace Utils
{
  public interface ILazy { }

  public interface ILazy<out T> : ILazy
  {
    T Value { get; }
  }

  public class Lazy<T> : ILazy<T>
  {
    public static implicit operator T(Lazy<T> lazy)
    {
      return lazy.Value;
    }

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