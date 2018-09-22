using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utils.Collections
{
  public class LinkedQueue<T> : IEnumerable<T>
  {
    private readonly LinkedList<T> _items = new LinkedList<T>();

    public void Clear()
    {
      if (_items != null && _items.Count > 0) _items.Clear();
    }

    public int Count
    {
      get { return _items.Count; }
    }

    public void Enqueue(T item)
    {
      _items.AddLast(item);
    }

    public T Dequeue()
    {
      if (_items.First == null)
        throw new InvalidOperationException("queue is empty");

      var item = _items.First.Value;
      _items.RemoveFirst();

      return item;
    }

    public T Peek()
    {
      if (_items.First == null)
      {
        throw new InvalidOperationException("queue is empty");
      }

      var item = _items.First.Value;

      return item;
    }

    public T Last
    {
      get
      {
        if (_items.Last == null)
        {
          throw new InvalidOperationException("queue is empty");
        }
        return _items.Last.Value;
      }
    }

    public bool Remove(T item)
    {
      return _items.Remove(item);
    }

    public bool RemoveAt(int index)
    {
      return Remove(_items.Skip(index).First());
    }

    public bool Contains(T item)
    {
      return _items.Contains(item);
    }

    public IEnumerator<T> GetEnumerator()
    {
      return ((IEnumerable<T>)_items).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}
