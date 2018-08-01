using System;
#if !UNITY_WEBGL
using System.Threading;
#endif

namespace Utils
{
  public class ThreadPoolExecutor
  {
    private readonly IDispatcher _dispatcher;

    public ThreadPoolExecutor(IDispatcher dispatcher)
    {
      _dispatcher = dispatcher;
    }

    public IThreadPoolResult QueueUserWorkItem(Action action)
    {
      var result = new ThreadPoolResult();
#if !UNITY_WEBGL
      ThreadPool.QueueUserWorkItem(sender =>
#endif
      {
        try
        {
          action();
          _dispatcher.Dispatch(result.Resolve);
        }
        catch (Exception exception)
        {
          _dispatcher.Dispatch(() => result.Resolve(exception));
        }
      }
#if !UNITY_WEBGL
      );
#endif

      return result;
    }

    public IThreadPoolResult<T> QueueUserWorkItem<T>(Func<T> action)
    {
      var result = new ThreadPoolResult<T>();
#if !UNITY_WEBGL
      ThreadPool.QueueUserWorkItem(sender =>
#endif
      {
        try
        {
          var data = action();
          _dispatcher.Dispatch(() => result.Resolve(data));
        }
        catch (Exception exception)
        {
          _dispatcher.Dispatch(() => result.Resolve(exception));
        }
      }
#if !UNITY_WEBGL
      );
#endif

      return result;
    }
  }

  public interface IThreadPoolResult<T> : IThreadPoolResult
  {
    T Result { get; }
    void Then(Action<T> onSuccess = null, Action<Exception> onFail = null);
  }

  public interface IThreadPoolResult
  {
    bool IsCompleted { get; }
    Exception Error { get; }
    void Then(Action onSuccess = null, Action<Exception> onFail = null);
  }

  public class ThreadPoolResult<T> : ThreadPoolResult, IThreadPoolResult<T>
  {
    private T _data;
    private Action<T> _onSuccessT;

    public T Result
    {
      get
      {
        if (Error != null)
        {
          throw Error;
        }
        return _data;
      }
      set
      {
        _data = value;
      }
    }

    internal void Resolve(T data)
    {
      if (!_isCompleted)
      {
        Result = data;
        Resolve();
        if (_onSuccessT != null)
        {
          _onSuccessT(_data);
        }
      }
    }

    public void Then(Action<T> onSuccess = null, Action<Exception> onFail = null)
    {
      if (!_isCompleted)
      {
        if (onSuccess != null)
        {
          _onSuccessT = onSuccess;
        }
        Then((Action)null, onFail);
      }
    }
  }

  public class ThreadPoolResult : IThreadPoolResult
  {
    protected bool _isCompleted;

    public bool IsCompleted { get { return _isCompleted; } }
    public Exception Error { get; set; }

    private Action _onSuccess;
    private Action<Exception> _onFail;

    internal void Resolve(Exception exception)
    {
      Error = exception;
      Resolve();
    }

    internal void Resolve()
    {
      if (!_isCompleted)
      {
        _isCompleted = true;
        if (Error != null)
        {
          if (_onFail != null)
          {
            _onFail(Error);
          }
        }
        else
        {
          if (_onSuccess != null)
          {
            _onSuccess();
          }
        }
        _onFail = null;
        _onSuccess = null;
      }
    }

    public void Then(Action onSuccess = null, Action<Exception> onFail = null)
    {
      if (!_isCompleted)
      {
        if (onSuccess != null)
        {
          _onSuccess = onSuccess;
        }
        if (onFail != null)
        {
          _onFail = onFail;
        }
      }
    }
  }
}
