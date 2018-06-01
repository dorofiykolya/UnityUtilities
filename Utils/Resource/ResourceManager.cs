using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
  public class ResourceManager : IDisposable
  {
    private readonly ICoroutineProvider _coroutineProvider;
    private readonly Dictionary<string, ResourceResult> _map;

    public ResourceManager(ICoroutineProvider coroutineProvider)
    {
      _coroutineProvider = coroutineProvider;
      _map = new Dictionary<string, ResourceResult>();
    }

    public ResourceResult<T> Get<T>(string prefab) where T : UnityEngine.Object
    {
      ResourceResult result;
      if (!_map.TryGetValue(prefab, out result))
      {
        result = new ResourceResult<T>(_coroutineProvider, prefab);
        _map[prefab] = result;
      }

      return (ResourceResult<T>)result;
    }

    public void Dispose()
    {
      foreach (var value in _map.Values)
      {
        value.Dispose();
      }
    }

    public struct ResourceLoadProgress
    {
      public string Path;
      public float Progress;
    }

    public class ResourceResult : IDisposable
    {
      public virtual void Dispose()
      {

      }
    }

    public class ResourceResult<T> : ResourceResult where T : UnityEngine.Object
    {
      private readonly ICoroutineProvider _coroutineProvider;
      private readonly string _path;
      private readonly Signal<ResourceResult<T>> _onResult;
      private T _result;
      private Coroutine _coroutine;
      private ResourceRequest _loadAsync;
      private bool _unload;
      private float _progress;

      public ResourceResult(ICoroutineProvider coroutineProvider, string path)
      {
        _coroutineProvider = coroutineProvider;
        _path = path;
        _onResult = new Signal<ResourceResult<T>>(Lifetime.Eternal);
      }

      public float Progress { get { return _progress; } }
      public bool IsCompleted { get; private set; }
      public bool IsError { get; private set; }
      public T Result { get { return _result; } }

      public void Collect()
      {
        if (_result != null)
        {
          Resources.UnloadAsset(_result);
          _result = null;
        }

        IsCompleted = false;
        IsError = false;
      }

      public override void Dispose()
      {
        _unload = true;
        Collect();
      }

      public ResourceResult<T> LoadAsync(Lifetime lifetime, Action<ResourceResult<T>> onResult)
      {
        if (!lifetime.IsTerminated)
        {
          _onResult.Subscribe(lifetime, onResult);
        }
        if (IsCompleted)
        {
          FireOnResult();
        }
        else
        {
          if (_loadAsync == null)
          {
            IsCompleted = false;
            IsError = false;
            _unload = false;
            _loadAsync = Resources.LoadAsync<T>(_path);
            StopCoroutine();
            _coroutine = _coroutineProvider.StartCoroutine(LoadAsyncProcess());
          }
        }

        return this;
      }

      private IEnumerator LoadAsyncProcess()
      {
        while (!_loadAsync.isDone)
        {
          if (_unload)
          {
            _loadAsync = null;
            _result = null;
            IsCompleted = false;
            yield break;
          }
          _progress = _loadAsync.progress;
          yield return null;
        }

        IsCompleted = true;
        _result = _loadAsync.asset as T;
        _loadAsync = null;
        if (_result == null)
        {
          IsError = true;
        }
        FireOnResult();
      }

      private void StopCoroutine()
      {
        if (_coroutine != null)
        {
          _coroutineProvider.StopCoroutine(_coroutine);
        }
      }

      private void FireOnResult()
      {
        _onResult.Fire(this);
      }
    }
  }
}
