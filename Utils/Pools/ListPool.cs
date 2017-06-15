using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class ListPool
{
  public static List<T> Pop<T>()
  {
    return ListPool<T>.Pop();
  }

  public static List<T> Pop<T>(int capacity)
  {
    return ListPool<T>.Pop(capacity);
  }

  public static List<T> Pop<T>(IEnumerable<T> value)
  {
    return ListPool<T>.Pop(value);
  }

  public static void Push<T>(List<T> list)
  {
    ListPool<T>.Push(list);
  }

  public static IEnumerable<T> Enumerable<T>(List<T> list)
  {
    return EnumerableListPool<T>.Pop(list);
  }
}

public class ListPool<T>
{
  private static readonly Stack<List<T>> Stack = new Stack<List<T>>(8);

  public static List<T> Pop()
  {
    return PopList();
  }

  public static List<T> Pop(int capacity)
  {
    return PopList(capacity);
  }

  public static List<T> Pop(IEnumerable<T> value)
  {
    var result = PopList();
    result.AddRange(value);
    return result;
  }

  public static void Push(List<T> list)
  {
    if (list != null)
    {
      PushList(list);
    }
  }

  private static List<T> PopList()
  {
    if (Stack.Count == 0) return new List<T>(8);
    return Stack.Pop();
  }

  private static List<T> PopList(int capacity)
  {
    if (Stack.Count == 0) return new List<T>(capacity);
    var result = Stack.Pop();
    if (result.Capacity < capacity) result.Capacity = capacity;
    return result;
  }

  private static void PushList(List<T> list)
  {
    list.Clear();
    if (Stack.Contains(list))
    {
      throw new ArgumentException();
    }
    Stack.Push(list);
  }
}

public class EnumerablePool
{
  public static IEnumerable<T> Enumerator<T>(T[] list)
  {
    return EnumerableArrayPool<T>.Pop(list);
  }

  public static IEnumerable<T> Enumerator<T>(List<T> list)
  {
    return EnumerableListPool<T>.Pop(list);
  }

  public static IEnumerable<T> Enumerator<T>(ReadOnlyCollection<T> list)
  {
    return EnumerableReadOnlyCollectionPool<T>.Pop(list);
  }
}

public class EnumerableArrayPool<T>
{
  private static readonly Stack<Enumerable> Stack = new Stack<Enumerable>();

  public static IEnumerable<T> Pop(T[] list)
  {
    if (Stack.Count != 0)
    {
      var enumerable = Stack.Pop();
      enumerable.SetTarget(list);
      return enumerable;
    }
    return new Enumerable().SetTarget(list);
  }

  private static void Push(Enumerable enumerable)
  {
    Stack.Push(enumerable);
  }

  public class Enumerable : IEnumerable<T>
  {
    private readonly Enumerator _enumerator;

    public Enumerable()
    {
      _enumerator = new Enumerator(this);
    }

    public Enumerable SetTarget(T[] list)
    {
      _enumerator.Reset();
      _enumerator.SetTarget(list);
      return this;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _enumerator;
    }

    public IEnumerator<T> GetEnumerator()
    {
      return _enumerator;
    }
  }

  public class Enumerator : IEnumerator<T>
  {
    private readonly Enumerable _enumerable;
    private int _index;
    private T[] _list;

    public Enumerator(Enumerable enumerable)
    {
      _enumerable = enumerable;
    }

    public bool MoveNext()
    {
      if (_index >= 0 && _index < _list.Length)
      {
        _index++;
        return true;
      }
      Push(_enumerable);
      Dispose();
      return false;
    }

    public void Reset()
    {
      _index = 0;
    }

    public T Current
    {
      get { return _list[_index - 1]; }
    }

    object IEnumerator.Current
    {
      get { return _list[_index - 1]; }
    }

    public void Dispose()
    {
      _list = null;
      _index = 0;
    }

    public void SetTarget(T[] list)
    {
      _list = list;
    }
  }
}

public class EnumerableListPool<T>
{
  private static readonly Stack<Enumerable> Stack = new Stack<Enumerable>();

  public static IEnumerable<T> Pop(List<T> list)
  {
    if (Stack.Count != 0)
    {
      var enumerable = Stack.Pop();
      enumerable.SetTarget(list);
      return enumerable;
    }
    return new Enumerable().SetTarget(list);
  }

  private static void Push(Enumerable enumerable)
  {
    Stack.Push(enumerable);
  }

  public class Enumerable : IEnumerable<T>
  {
    private readonly Enumerator _enumerator;

    public Enumerable()
    {
      _enumerator = new Enumerator(this);
    }

    public Enumerable SetTarget(List<T> list)
    {
      _enumerator.Reset();
      _enumerator.SetTarget(list);
      return this;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _enumerator;
    }

    public IEnumerator<T> GetEnumerator()
    {
      return _enumerator;
    }
  }

  public class Enumerator : IEnumerator<T>
  {
    private readonly Enumerable _enumerable;
    private int _index;
    private List<T> _list;

    public Enumerator(Enumerable enumerable)
    {
      _enumerable = enumerable;
    }

    public bool MoveNext()
    {
      if (_index >= 0 && _index < _list.Count)
      {
        _index++;
        return true;
      }
      Push(_enumerable);
      Dispose();
      return false;
    }

    public void Reset()
    {
      _index = 0;
    }

    public T Current
    {
      get { return _list[_index - 1]; }
    }

    object IEnumerator.Current
    {
      get { return _list[_index - 1]; }
    }

    public void Dispose()
    {
      _list = null;
      _index = 0;
    }

    public void SetTarget(List<T> list)
    {
      _list = list;
    }
  }
}

public class EnumerableReadOnlyCollectionPool<T>
{
  private static readonly Stack<Enumerable> Stack = new Stack<Enumerable>();

  public static IEnumerable<T> Pop(ReadOnlyCollection<T> list)
  {
    if (Stack.Count != 0)
    {
      var enumerable = Stack.Pop();
      enumerable.SetTarget(list);
      return enumerable;
    }
    return new Enumerable().SetTarget(list);
  }

  private static void Push(Enumerable enumerable)
  {
    Stack.Push(enumerable);
  }

  public class Enumerable : IEnumerable<T>
  {
    private readonly Enumerator _enumerator;

    public Enumerable()
    {
      _enumerator = new Enumerator(this);
    }

    public Enumerable SetTarget(ReadOnlyCollection<T> list)
    {
      _enumerator.Reset();
      _enumerator.SetTarget(list);
      return this;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _enumerator;
    }

    public IEnumerator<T> GetEnumerator()
    {
      return _enumerator;
    }
  }

  public class Enumerator : IEnumerator<T>
  {
    private readonly Enumerable _enumerable;
    private int _index;
    private ReadOnlyCollection<T> _list;

    public Enumerator(Enumerable enumerable)
    {
      _enumerable = enumerable;
    }

    public bool MoveNext()
    {
      if (_index >= 0 && _index < _list.Count)
      {
        _index++;
        return true;
      }
      Push(_enumerable);
      Dispose();
      return false;
    }

    public void Reset()
    {
      _index = 0;
    }

    public T Current
    {
      get { return _list[_index - 1]; }
    }

    object IEnumerator.Current
    {
      get { return _list[_index - 1]; }
    }

    public void Dispose()
    {
      _list = null;
      _index = 0;
    }

    public void SetTarget(ReadOnlyCollection<T> list)
    {
      _list = list;
    }
  }
}
