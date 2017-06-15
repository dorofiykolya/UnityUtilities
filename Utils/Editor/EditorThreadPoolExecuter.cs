using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Utils.Editor
{
  public static class EditorThreadPoolExecuter
  {
    private static readonly Queue<Action> _queue = new Queue<Action>();
    private static readonly object _queueSync = new object();

    static EditorThreadPoolExecuter()
    {
      EditorApplication.update += Update;
    }

    public static IExecuteResult<T> ExecuteInBackground<T>(Func<T> action)
    {
      var result = new ExecuteThreadResult<T>();

      ThreadPool.QueueUserWorkItem(state =>
      {
        var actionResult = action();
        lock (_queueSync)
        {
          _queue.Enqueue(() =>
          {
            result.Resolve(actionResult);
          });
        }
      });

      return result;
    }

    public static IExecuteResult ExecuteInBackground(Action action)
    {
      var result = new ExecuteThreadResult();

      ThreadPool.QueueUserWorkItem(state =>
      {
        action();
        lock (_queueSync)
        {
          _queue.Enqueue(result.Resolve);
        }
      });

      return result;
    }

    private static void Update()
    {
      Action[] actions = null;
      lock (_queueSync)
      {
        if (_queue.Count != 0)
        {
          actions = _queue.ToArray();
          _queue.Clear();
        }
      }
      if (actions != null && actions.Length != 0)
      {
        foreach (var action in actions)
        {
          try
          {
            action();
          }
          catch (Exception e)
          {
            Debug.LogError(e);
          }
        }
      }
    }

    public class ExecuteThreadResult : IExecuteResult
    {
      private List<Action> _result;
      protected bool _completed;

      public bool IsComplete { get { return _completed; } }

      public IExecuteResult ThenInUnityThread(Action action)
      {
        if (_completed)
        {
          action();
        }
        else
        {
          if (_result == null)
          {
            _result = new List<Action>();
          }
          _result.Add(action);
        }
        return this;
      }

      public void Resolve()
      {
        _completed = true;
        if (_result != null)
        {
          foreach (var action in _result)
          {
            try
            {
              action();
            }
            catch (Exception e)
            {
              Debug.LogError(e);
            }
          }
        }
      }
    }
  }

  public class ExecuteThreadResult<T> : EditorThreadPoolExecuter.ExecuteThreadResult, IExecuteResult<T>
  {
    private List<Action<T>> _result;

    public IExecuteResult<T> ThenInUnityThread(Action<T> action)
    {
      if (_completed)
      {
        action(Result);
      }
      else
      {
        if (_result == null)
        {
          _result = new List<Action<T>>();
        }
        _result.Add(action);
      }
      return this;
    }

    public void Resolve(T value)
    {
      Result = value;
      base.Resolve();
      if (_result != null)
      {
        foreach (var action in _result)
        {
          try
          {
            action(Result);
          }
          catch (Exception e)
          {
            Debug.LogError(e);
          }
        }
      }
    }

    public T Result { get; private set; }
  }


  public interface IExecuteResult<T> : IExecuteResult
  {
    T Result { get; }
    IExecuteResult<T> ThenInUnityThread(Action<T> action);
  }

  public interface IExecuteResult
  {
    IExecuteResult ThenInUnityThread(Action action);
  }
}
