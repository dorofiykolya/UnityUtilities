using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Utils.AsyncBundles.Loaders
{
  public class AsyncStreamingAssetBundleLoader : IAsyncBundleLoader
  {
    private Lifetime _lifetime;
    private ICoroutineProvider _coroutineProvider;
    private readonly Func<bool> _destroyInstances;
    private object _bundleRequest;
    private bool _bundleRequestDone;
    private AssetBundleRequest _request;
    private Signal<IAsyncBundleLoader> _onComplete;

    public AsyncStreamingAssetBundleLoader(Lifetime lifetime, ICoroutineProvider coroutineProvider, Func<bool> destroyInstances)
    {
      _lifetime = lifetime;
      _coroutineProvider = coroutineProvider;
      _destroyInstances = destroyInstances;
      _onComplete = new Signal<IAsyncBundleLoader>(lifetime);
    }

    public void SubscribeOnComplete(Lifetime lifetime, Action<IAsyncBundleLoader> listener)
    {
      _onComplete.Subscribe(lifetime, listener);
    }

    public void Load(string url, int version)
    {
      var coroutine = _coroutineProvider.StartCoroutine(LoadAsync(url, version));
      _lifetime.AddAction(() =>
      {
        if (coroutine != null)
        {
          _coroutineProvider.StopCoroutine(coroutine);
        }
        Unload(_destroyInstances());
      });
    }

    public bool IsDone { get; private set; }
    public float Progress { get; private set; }
    public bool IsLoading { get; private set; }
    public bool IsLoaded { get; private set; }
    public AssetBundle Bundle { get; private set; }
    public Func<IAsyncBundleLoader> Next { get; set; }
    public bool Unloaded { get; private set; }

    public IEnumerator LoadAsync(string url, int version)
    {
      if (Bundle == null && _bundleRequest == null)
      {
        Unloaded = false;
        var path = url.Contains(":/") ? url : Path.Combine(Application.streamingAssetsPath, url);

        IsLoading = true;
        IsDone = false;
        IsLoaded = false;
        Bundle = null;
        _bundleRequestDone = false;

        if (path.Contains("://"))
        {
          var wwwBundleRequest = new WWW(path);
          _bundleRequest = wwwBundleRequest;

          while (!wwwBundleRequest.isDone)
          {
            Progress = wwwBundleRequest.progress / 2f;
            yield return null;
          }

          _bundleRequestDone = true;
          Bundle = wwwBundleRequest.assetBundle;
        }
        else
        {
          if (File.Exists(path) == false)
          {
            IsDone = true;
            IsLoaded = true;
            IsLoading = false;
            Bundle = null;
            _bundleRequestDone = true;
            _onComplete.Fire(this);
            yield break;
          }

          var bundleRequest = AssetBundle.LoadFromFileAsync(path);
          _bundleRequest = bundleRequest;

          while (!bundleRequest.isDone)
          {
            Progress = bundleRequest.progress / 2f;
            yield return null;
          }

          _bundleRequestDone = true;
          Bundle = bundleRequest.assetBundle;
        }

        if (!IsLoading)
        {
          UnloadBundle(true);
          yield break;
        }

        IsDone = true;
        IsLoaded = true;

        yield return null;

        _onComplete.Fire(this);
      }
    }

    private void UnloadBundle(bool unloadAllLoadedObjects)
    {
      if (Bundle != null)
      {
        Bundle.Unload(unloadAllLoadedObjects);
        Bundle = null;
      }
      _bundleRequest = null;
      _request = null;
      _bundleRequestDone = false;
    }

    public void Unload(bool unloadAllLoadedObjects)
    {
      IsLoading = false;
      IsDone = false;
      var www = _bundleRequest as WWW;
      if (www != null)
      {
        www.Dispose();
      }
      if (_bundleRequestDone)
      {
        UnloadBundle(unloadAllLoadedObjects);
      }
      else
      {
        if (_bundleRequest is AssetBundleCreateRequest)
        {
          var go = new GameObject("WaitAndDispose");
          GameObject.DontDestroyOnLoad(go);
          go.AddComponent<EmptyMonoBehaviour>().StartCoroutine(WaitAndDispose(go, (AssetBundleCreateRequest)_bundleRequest));
          _bundleRequest = null;
        }
      }
      Bundle = null;
      Unloaded = true;
    }

    private IEnumerator WaitAndDispose(GameObject go, AssetBundleCreateRequest bundleRequest)
    {
      while (!bundleRequest.isDone)
      {
        yield return null;
      }
      var bundle = bundleRequest.assetBundle;
      if (bundle != null)
      {
        bundle.Unload(true);
      }
      go.GetComponent<EmptyMonoBehaviour>().StopAllCoroutines();
      GameObject.Destroy(go);
    }
  }
}
