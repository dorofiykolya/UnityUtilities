using System;
using System.Collections;
using System.Collections.Generic;

namespace Utils.Containers
{
  public class Container<T> : IContainer<T>
  {
    private readonly Dictionary<Type, T> _map = new Dictionary<Type, T>();

    public void Map(T value)
    {
      _map[value.GetType()] = value;
    }

    public T2 Get<T2>() where T2 : T
    {
      T result;
      if (_map.TryGetValue(typeof(T2), out result))
      {
        return (T2)result;
      }
      return default(T2);
    }

    public void Dispose()
    {
      _map.Clear();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator()
    {
      return _map.Values.GetEnumerator();
    }
  }
}
