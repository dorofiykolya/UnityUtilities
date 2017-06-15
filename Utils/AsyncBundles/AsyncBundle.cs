using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Utils;
using Utils.AsyncBundles.Loaders;

namespace AsyncBundles
{
  public class AsyncBundle
  {
    private enum State : byte
    {
      Retain,
      Release
    }

    private readonly HashSet<object> _refs;
    private readonly Signal<AsyncBundle> _onLoaded;
    private readonly Signal<AsyncBundle> _onUnloaded;
    private readonly Lifetime _lifetime;
    private readonly string _path;
    private readonly int _version;
    private readonly ICoroutineProvider _coroutineProvider;
    private readonly LoaderInfo _loaderInfo;
    private AssetBundle _bundle;
    private State _state = State.Release;

    public AsyncBundle(Lifetime lifetime, string path, int version, ICoroutineProvider coroutineProvider)
    {
      _lifetime = lifetime;
      _path = path;
      _version = version;
      _coroutineProvider = coroutineProvider;
      UnloadInstances = true;
      _refs = new HashSet<object>();
      _onLoaded = new Signal<AsyncBundle>(lifetime);
      _onUnloaded = new Signal<AsyncBundle>(lifetime);
      _loaderInfo = new LoaderInfo(this);
      lifetime.AddAction(() =>
      {
        UnloadInstances = true;
      });
    }

    public object[] Refs { get { return _refs.ToArray(); } }
    public ILoaderInfo Loader { get { return _loaderInfo; } }
    public string Path { get { return _path; } }
    public AssetBundle Bundle { get { return _bundle; } }
    public bool UnloadInstances { get; set; }

    public void SubscribeOnLoaded(Lifetime lifetime, Action<AsyncBundle> listener)
    {
      _onLoaded.Subscribe(lifetime, listener);
    }

    public void SubscribeOnUnloaded(Lifetime lifetime, Action<AsyncBundle> listener)
    {
      _onUnloaded.Subscribe(lifetime, listener);
    }

    public void Release(object context)
    {
      Assert.IsNotNull(context);
      _refs.Remove(context);
      if (_refs.Count == 0)
      {
        UnloadInternal();
      }
    }

    public void Retain(object context)
    {
      Assert.IsNotNull(context);
      _refs.Add(context);
      LoadInternal();
    }

    public void Unload()
    {
      UnloadInternal();
    }

    private void LoadInternal()
    {
      if (_state == State.Release && !_loaderInfo.IsLoaded && !_loaderInfo.IsLoading && !_loaderInfo.IsError)
      {
        _state = State.Retain;
        _loaderInfo.Load(_path, _version);
      }
    }

    private void UnloadInternal()
    {
      if (_state == State.Retain)
      {
        _state = State.Release;
        if (_loaderInfo.IsLoaded)
        {
          _onUnloaded.Fire(this);
        }
        _loaderInfo.Unload();
      }
    }

    public interface ILoaderInfo
    {
      float Progress { get; }
      bool IsLoading { get; }
      bool IsLoaded { get; }
      bool IsError { get; }
    }

    private class LoaderInfo : ILoaderInfo
    {
      private readonly AsyncBundle _bundle;
      private IAsyncBundleLoader _loader;
      private Lifetime.Definition _loaderDefinition;
      private Lifetime.Definition _completeDefinition;

      public LoaderInfo(AsyncBundle bundle)
      {
        _bundle = bundle;
      }

      public float Progress { get { return _loader != null ? _loader.Progress : 0f; } }
      public bool IsLoading { get; private set; }
      public bool IsLoaded { get; private set; }
      public bool IsError { get; private set; }

      public void Load(string path, int version)
      {
        if (_loaderDefinition == null)
        {
          _loaderDefinition = Lifetime.Define(_bundle._lifetime);
          _loaderDefinition.Lifetime.AddAction(() =>
          {
            _bundle._onUnloaded.Fire(_bundle);
            _loaderDefinition = null;
            IsLoaded = false;
            IsLoading = false;
            IsError = false;
          });
          IsLoading = true;

          if (IsWWW(path))
          {
            _loader = new AsyncWWWBundleLoader(_loaderDefinition.Lifetime, _bundle._coroutineProvider, () => _bundle.UnloadInstances);
          }
          else if (IsStreamingAssets(path))
          {
            _loader = new AsyncStreamingAssetBundleLoader(_loaderDefinition.Lifetime, _bundle._coroutineProvider, () => _bundle.UnloadInstances);
          }
          else
          {
            _loader = new AsyncResourceBundleLoader(_loaderDefinition.Lifetime, _bundle._coroutineProvider, () => _bundle.UnloadInstances)
            {
              Next = () => new AsyncStreamingAssetBundleLoader(_loaderDefinition.Lifetime, _bundle._coroutineProvider, () => _bundle.UnloadInstances)
            };
          }

          _completeDefinition = Lifetime.Define(_loaderDefinition.Lifetime);

          _loader.SubscribeOnComplete(_completeDefinition.Lifetime, loader => OnComplete(_loaderDefinition.Lifetime, loader, path, version));
          _loader.Load(path, version);
        }
      }

      private void OnComplete(Lifetime lifetime, IAsyncBundleLoader loader, string path, int version)
      {
        IsLoading = false;
        if (loader.Bundle == null)
        {
          if (loader.Next != null)
          {
            _completeDefinition.Terminate();
            _completeDefinition = Lifetime.Define(lifetime);
            _loader = loader.Next();
            _loader.SubscribeOnComplete(_completeDefinition.Lifetime, (resultLoader) => OnComplete(lifetime, resultLoader, path, version));
            _loader.Load(path, version);
          }
          else
          {
            IsError = true;
            IsLoaded = true;
            InvokeComplete(loader, path, version);
          }
        }
        else
        {
          IsLoaded = true;
          InvokeComplete(loader, path, version);
        }
      }

      public void Unload()
      {
        if (_loaderDefinition != null)
        {
          _loaderDefinition.Terminate();
        }
      }

      private void InvokeComplete(IAsyncBundleLoader loader, string path, int version)
      {
        if (IsError)
        {
          Debug.LogError("bundle not load:" + path + ", version:" + version);
        }
        else
        {
          _bundle._bundle = loader.Bundle;
          _bundle._onLoaded.Fire(_bundle);
        }
      }

      private bool IsWWW(string value)
      {
        return value.StartsWith("http://") || value.StartsWith("https://");
      }

      private bool IsStreamingAssets(string value)
      {
        return value.Contains("://");
      }
    }
  }
}
