using System;
using System.Collections.Generic;

namespace Utils.Containers
{
  public interface IContainer<T> : IDisposable, IEnumerable<T>
  {
    void Map(T value);
    T2 Get<T2>() where T2 : T;
  }
}
