using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Utils.Threading
{
  public class UnityDispatcher : MonoBehaviour, IDispatcher, IDispatcherWait
  {
    private static readonly List<Action> _actions = new List<Action>();
    private static readonly object _lockObj = new object();
    private static readonly List<EventWaitHandle> _waitHandles = new List<EventWaitHandle>();

    private static int _unityThreadId;

    /// <summary>
    ///   Dispatches the action asynchronously on the Unity main thread.
    ///   The action will execute in the next frame update phase.
    /// </summary>
    /// <param name="action">The action to execute on the dispatch thread.</param>
    public void Dispatch(Action action)
    {
      if (Thread.CurrentThread.ManagedThreadId == _unityThreadId)
      {
        action();
        return;
      }

      lock (_lockObj)
      {
        _actions.Add(action);
      }
    }

    /// <summary>
    ///   Dispatches the action asynchronously on the Unity main thread but blocks
    ///   the current thread until the action is executed.
    ///   The action will execute in the next frame update phase.
    /// </summary>
    /// <param name="action">The action to execute on the dispatch thread.</param>
    public void DispatchWait(Action action)
    {
      if (Thread.CurrentThread.ManagedThreadId == _unityThreadId)
      {
        // Make sure you don't block the main thread if calling from the main thread causing a deadlock
        action();
      }
      else
      {
        EventWaitHandle handle;
        lock (_lockObj)
        {
          _actions.Add(action);
          handle = new EventWaitHandle(false, EventResetMode.ManualReset);
          _waitHandles.Add(handle);
        }
        handle.WaitOne();
      }
    }

    /// <summary>
    ///   This method should be executed on the main dispatch thread to
    ///   execute the queued dispatch actions.
    /// </summary>
    public void ExecuteQueue()
    {
      List<Action> actionsCopy;
      List<EventWaitHandle> waitHandlesCopy;
      lock (_lockObj)
      {
        if (_actions.Count == 0) return;

        actionsCopy = new List<Action>(_actions);
        waitHandlesCopy = new List<EventWaitHandle>(_waitHandles);
        _actions.Clear();
        _waitHandles.Clear();
      }

      foreach (var action in actionsCopy)
      {
        try
        {
          action();
        }
        catch (Exception e)
        {
          Debug.LogException(e);
        }
      }

      foreach (var waitHandle in waitHandlesCopy)
      {
        try
        {
          waitHandle.Set();
        }
        catch (Exception e)
        {
          Debug.LogException(e);
        }
      }
    }

    private void Awake()
    {
      _unityThreadId = Thread.CurrentThread.ManagedThreadId;
      DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
      ExecuteQueue();
    }
  }
}