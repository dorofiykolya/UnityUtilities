﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utils
{
  public class PrefabPoolFactory : IDisposable
  {
    private readonly List<GameObject> _instances;
    private readonly Stack<GameObject> _pool;
    private GameObject _prefab;

    public PrefabPoolFactory(GameObject prefab)
    {
      _prefab = prefab;
      _instances = new List<GameObject>();
      _pool = new Stack<GameObject>();
    }

    public int Instances { get { return _instances.Count; } }

    public T Instantiate<T>() where T : Component
    {
      T instance;
      if (_pool.Count != 0)
      {
        instance = _pool.Pop().GetComponent<T>();
      }
      else
      {
        var prefab = GetPrefab();
        instance = Object.Instantiate(prefab).GetComponent<T>();
      }
      _instances.Add(instance.gameObject);
      return instance;
    }

    public T Instantiate<T>(Transform transform) where T : Component
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
        instance = ((GameObject)Object.Instantiate(prefab, transform, false)).GetComponent<T>();
      }
      _instances.Add(instance.gameObject);
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

    public bool IsReleased(GameObject gameObject)
    {
      return _pool.Contains(gameObject);
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
    }

    public void Release(Component value)
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

    private GameObject GetPrefab()
    {
      return _prefab;
    }
  }
}
