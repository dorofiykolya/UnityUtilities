using System;
using System.Collections.Generic;

namespace Utils
{
  public class KeepReference
  {
    private readonly Lifetime _lifetime;
    private readonly List<Action> _listeners = new List<Action>();
    private int _count;

    public KeepReference(Lifetime lifetime)
    {
      _lifetime = lifetime;
    }

    public void Keep(Lifetime lifetime)
    {
      ++_count;
      var lt = Lifetime.Intersection(_lifetime, lifetime);
      lt.Lifetime.AddAction(() =>
      {
        --_count;
        Fire();
      });
    }

    public Lifetime.Definition Keep()
    {
      var def = Lifetime.Define(Lifetime.Eternal);
      Keep(def.Lifetime);
      return def;
    }

    public void AddAction(Action listener)
    {
      if (_listeners.Contains(listener)) throw new ArgumentException();
      _listeners.Add(listener);
    }

    private void Fire()
    {
      if (_count == 0)
      {
        foreach (var listener in _listeners)
        {
          listener();
        }
        _listeners.Clear();
      }
    }
  }
}
