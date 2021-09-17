using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils.Threading
{
  public class WebGLDispatcher : MonoBehaviour, IDispatcher
  {
    private Queue<Action> _queue = new Queue<Action>();

    public void Dispatch(Action action)
    {
      _queue.Enqueue(action);
    }

    private void Update()
    {
      while (_queue.Count != 0)
      {
        _queue.Dequeue()();
      }
    }
  }
}
