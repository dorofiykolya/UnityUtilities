using System;

namespace Utils
{
  public interface IDispatcher
  {
    void Dispatch(Action action);
  }

  public interface IDispatcherWait
  {
    void DispatchWait(Action action);
  }
}