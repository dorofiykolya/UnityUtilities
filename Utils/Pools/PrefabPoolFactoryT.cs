using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utils
{
  public class PrefabPoolFactory<T> : IDisposable where T : Component
  {
    private readonly List<GameObject> _instances;
    private readonly Stack<GameObject> _pool;
    private T _prefab;
    private readonly Action<GameObject> _onInstantiate;
    private readonly Action<GameObject> _onRelease;

    public PrefabPoolFactory(T prefab, Action<GameObject> onInstantiate = null, Action<GameObject> onRelease = null)
    {
      _prefab = prefab;
      _onInstantiate = onInstantiate;
      _onRelease = onRelease;
      _pool = new Stack<GameObject>();
      _instances = new List<GameObject>();
    }

    public T Prefab { get { return _prefab; } }

    public int Instances { get { return _instances.Count; } }

    public T Instantiate()
    {
      T instance;
      if (_pool.Count != 0)
      {
        instance = _pool.Pop().GetComponent<T>();
      }
      else
      {
        var prefab = GetPrefab();
        instance = Object.Instantiate<T>(prefab);
      }
      _instances.Add(instance.gameObject);

      if (_onInstantiate != null) _onInstantiate(instance.gameObject);

      return instance;
    }

    public T Instantiate(Transform transform)
    {
      T instance;
      if (_pool.Count != 0)
      {
        instance = _pool.Pop().GetComponent<T>();
        instance.transform.SetParent(transform, false);
      }
      else
      {
        var prefab = GetPrefab();
        instance = Object.Instantiate<T>(prefab, transform, false);
      }
      _instances.Add(instance.gameObject);

      if (_onInstantiate != null) _onInstantiate(instance.gameObject);

      return instance;
    }

    public void Collect()
    {
      foreach (var gameObject in _pool)
      {
        if (gameObject != null)
        {
          Object.Destroy(gameObject);
        }
      }
      _pool.Clear();
    }

    public bool IsInstantiated(GameObject value)
    {
      return _instances.Contains(value);
    }

    public bool IsInstantiated(T value)
    {
      return _instances.Contains(value.gameObject);
    }

    public bool IsReleased(GameObject gameObject)
    {
      return _pool.Contains(gameObject);
    }

    public bool IsReleased(T value)
    {
      return _pool.Contains(value.gameObject);
    }

    public void Release(GameObject value)
    {
      if (value == null)
      {
        throw new NullReferenceException("value can't bee null:" + _prefab);
      }
      if (_pool.Contains(value))
      {
        throw new ArgumentException("pool contains this gameObject: " + _prefab);
      }
      if (!_instances.Contains(value))
      {
        throw new ArgumentException("gameObject not in instance list: " + _prefab + ", value in pool: " + (_pool.Contains(value)));
      }
      _instances.Remove(value);

      _pool.Push(value);

      if (_onRelease != null) _onRelease(value);
    }

    public void Release(T value)
    {
      Release(value.gameObject);
    }

    public void Dispose()
    {
      if (_instances.Count != 0)
      {
        foreach (var gameObject in _instances)
        {
          Object.Destroy(gameObject);
        }
        _instances.Clear();
      }
      if (_pool.Count != 0)
      {
        foreach (var gameObject in _pool)
        {
          Object.Destroy(gameObject);
        }
        _pool.Clear();
      }
      _prefab = null;
    }

    private T GetPrefab()
    {
      return _prefab;
    }
  }
}
