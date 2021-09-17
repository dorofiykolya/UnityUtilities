using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils
{
  public interface ISignalSubsribe
  {
    void Subscribe(Lifetime lifetime, Action handler);
  }

  public class Signal : ISignalSubsribe
  {
    private readonly Lifetime _lifetime;
    private readonly object _lock = new object();
    private readonly List<Action> _sinks = new List<Action>(1);

    public Signal(Lifetime lifetime)
    {
      _lifetime = lifetime;
      if (lifetime == null)
        throw new ArgumentNullException("lifetime");

      lifetime.AddAction(() =>
      {
        lock (_sinks)
        {
          _sinks.Clear();
        }
      });
    }

    public void Subscribe(Lifetime lifetime, Action handler)
    {
      if (_lifetime.IsTerminated)
      {
        throw new InvalidOperationException("Already terminated");
      }

      Action opening = () =>
      {
        lock (_lock)
        {
          if (_sinks.Any(action => ReferenceEquals(action, handler)))
          {
            throw new InvalidOperationException(string.Format("The handler “{0}” is already sinking the signal.",
              handler));
          }

          _sinks.Add(handler);
        }
      };

      Action closing = () =>
      {
        lock (_lock)
        {
          for (var index = 0; index < _sinks.Count; index++)
          {
            var action = _sinks[index];
            if (!ReferenceEquals(action, handler)) continue;
            _sinks.RemoveAt(index);
            return;
          }
        }
      };

      lifetime.AddBracket(opening, closing);
    }

    public void Fire()
    {
      List<Action> copy;
      lock (_lock)
      {
        copy = ListPool<Action>.Pop(_sinks);
      }

      foreach (var action in copy)
      {
        try
        {
          action();
        }
#if !UNITY_EDITOR
        catch (Exception ex)
        {
          Debug.LogError("Error firing, exception: " + ex);
        }
#endif
        finally
        {

        }
      }

      ListPool.Push(copy);
    }
  }

  public interface ISignalSubsribe<T1>
  {
    void Subscribe(Lifetime lifetime, Action<T1> handler);
  }

  public class Signal<T1> : ISignalSubsribe<T1>
  {
    private readonly Lifetime _lifetime;
    private readonly object _lock = new object();
    private readonly List<Action<T1>> _sinks = new List<Action<T1>>(1);

    public Signal(Lifetime lifetime)
    {
      _lifetime = lifetime;
      if (lifetime == null)
        throw new ArgumentNullException("lifetime");

      lifetime.AddAction(() =>
      {
        lock (_sinks)
        {
          _sinks.Clear();
        }
      });
    }

    public void Subscribe(Lifetime lifetime, Action<T1> handler)
    {
      if (_lifetime.IsTerminated)
      {
        throw new InvalidOperationException("Already terminated");
      }

      Action opening = () =>
      {
        lock (_lock)
        {
          if (_sinks.Any(action => ReferenceEquals(action, handler)))
          {
            throw new InvalidOperationException(string.Format("The handler “{0}” is already sinking the signal.",
              handler));
          }

          _sinks.Add(handler);
        }
      };

      Action closing = () =>
      {
        lock (_lock)
        {
          for (var index = 0; index < _sinks.Count; index++)
          {
            var action = _sinks[index];
            if (!ReferenceEquals(action, handler)) continue;
            _sinks.RemoveAt(index);
            return;
          }
        }
      };

      lifetime.AddBracket(opening, closing);
    }

    public void Fire(T1 value)
    {
      List<Action<T1>> copy;
      lock (_lock)
      {
        copy = ListPool<Action<T1>>.Pop(_sinks);
      }

      foreach (var action in copy)
      {
        try
        {
          action(value);
        }
        catch (Exception ex)
        {
#if UNITY_EDITOR
          Debug.LogError(ex.StackTrace);
          throw ex;
#else
          Debug.LogError("Error firing, exception: " + ex);
#endif
        }
      }
      ListPool.Push(copy);
    }
  }

  public interface ISignalSubsribe<T1, T2>
  {
    void Subscribe(Lifetime lifetime, Action<T1, T2> handler);
  }

  public class Signal<T1, T2> : ISignalSubsribe<T1, T2>
  {
    private readonly Lifetime _lifetime;
    private readonly object _lock = new object();
    private readonly List<Action<T1, T2>> _sinks = new List<Action<T1, T2>>(1);

    public Signal(Lifetime lifetime)
    {
      _lifetime = lifetime;
      if (lifetime == null)
        throw new ArgumentNullException("lifetime");

      lifetime.AddAction(() =>
      {
        lock (_sinks)
        {
          _sinks.Clear();
        }
      });
    }

    public void Subscribe(Lifetime lifetime, Action<T1, T2> handler)
    {
      if (_lifetime.IsTerminated)
      {
        throw new InvalidOperationException("Already terminated");
      }

      Action opening = () =>
      {
        lock (_lock)
        {
          if (_sinks.Any(action => ReferenceEquals(action, handler)))
          {
            throw new InvalidOperationException(string.Format("The handler “{0}” is already sinking the signal.",
              handler));
          }

          _sinks.Add(handler);
        }
      };

      Action closing = () =>
      {
        lock (_lock)
        {
          for (var index = 0; index < _sinks.Count; index++)
          {
            var action = _sinks[index];
            if (!ReferenceEquals(action, handler)) continue;
            _sinks.RemoveAt(index);
            return;
          }
        }
      };

      lifetime.AddBracket(opening, closing);
    }

    public void Fire(T1 value, T2 value2)
    {
      List<Action<T1, T2>> copy;
      lock (_lock)
      {
        copy = ListPool<Action<T1, T2>>.Pop(_sinks);
      }

      foreach (var action in copy)
      {
        try
        {
          action(value, value2);
        }
        catch (Exception ex)
        {
#if UNITY_EDITOR
          throw ex;
#else
          Debug.LogError("Error firing, exception: " + ex);
#endif
        }
      }
      ListPool.Push(copy);
    }
  }
}

