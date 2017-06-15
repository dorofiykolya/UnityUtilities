using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Utils.AsyncBundles.Loaders
{
  public class AsyncWWWBundleLoader : IAsyncBundleLoader
  {
    private readonly Lifetime _lifetime;
    private readonly ICoroutineProvider _coroutineProvider;
    private readonly Func<bool> _destroyInstances;

    private readonly Signal<IAsyncBundleLoader> _onComplete;
    private UnityWebRequest _bundleRequest;
    private bool _bundleRequestDone;
    private AssetBundleRequest _request;

    public AsyncWWWBundleLoader(Lifetime lifetime, ICoroutineProvider coroutineProvider, Func<bool> destroyInstances)
    {
      _lifetime = lifetime;
      _coroutineProvider = coroutineProvider;
      _destroyInstances = destroyInstances;
      _onComplete = new Signal<IAsyncBundleLoader>(lifetime);
    }

    public bool IsDone { get; private set; }
    public float Progress { get; private set; }
    public bool IsLoading { get; private set; }
    public bool IsLoaded { get; private set; }
    public AssetBundle Bundle { get; private set; }
    public Func<IAsyncBundleLoader> Next { get; set; }
    public bool Unloaded { get; private set; }

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

    public void Unload(bool unloadAllLoadedObjects)
    {

      if (_bundleRequestDone)
      {
        UnloadBundle(unloadAllLoadedObjects);
      }
      IsLoading = false;
      Bundle = null;
      IsDone = false;
      IsLoaded = false;
      Unloaded = true;
    }

    private IEnumerator LoadAsync(string url, int version)
    {
      version = 1;
      if (Bundle == null && _bundleRequest == null)
      {
        Unloaded = false;
        IsLoading = true;

        UnityWebRequest bundleRequest = UnityWebRequest.GetAssetBundle(url, Convert.ToUInt32(version), 0);
        var sendRequest = bundleRequest.Send();
        while (!sendRequest.isDone)
        {
          yield return null;
        }

        _bundleRequest = bundleRequest;
        IsDone = false;
        IsLoaded = false;
        Bundle = null;
        _bundleRequestDone = false;
        while (!bundleRequest.isDone)
        {
          if (!IsLoading)
          {
            UnloadBundle(true);
            yield break;
          }

          Progress = bundleRequest.downloadProgress / 2f;
          yield return null;
        }

        _bundleRequestDone = true;

        DownloadHandlerAssetBundle dh = (DownloadHandlerAssetBundle)bundleRequest.downloadHandler;
        Bundle = dh.assetBundle;
        if (Bundle == null)
        {
          Debug.LogError("bundle not load from: " + url);
        }

        if (!IsLoading)
        {
          UnloadBundle(true);
          yield break;
        }

        IsDone = true;
        IsLoaded = true;
        DisposeRequest();

        yield return null;
        _onComplete.Fire(this);
      }
    }

    private void DisposeRequest()
    {
      if (_bundleRequest != null)
      {
        _bundleRequest.Dispose();
        _bundleRequest = null;
      }
    }

    private void UnloadBundle(bool unloadAllLoadedObjects)
    {
      if (Bundle != null)
      {
        Bundle.Unload(unloadAllLoadedObjects);
        Bundle = null;
      }

      DisposeRequest();

      _request = null;
      _bundleRequestDone = false;
    }
  }
}
