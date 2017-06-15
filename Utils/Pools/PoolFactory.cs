using System;
using System.Collections.Generic;

namespace Utils
{
  public class PoolFactory<T>
  {
    private readonly Func<T> _factory;
    private readonly Stack<T> _stack;

    public PoolFactory(Func<T> factory)
    {
      _factory = factory;
      _stack = new Stack<T>();
    }

    public T Pop()
    {
      if (_stack.Count != 0)
      {
        return _stack.Pop();
      }
      return _factory();
    }

    public void Push(T value)
    {
      if (_stack.Contains(value))
      {
        throw new ArgumentException();
      }
      _stack.Push(value);
    }
  }
}
