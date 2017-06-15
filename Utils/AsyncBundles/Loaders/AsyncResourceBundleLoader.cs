using System;
using System.Collections;
using UnityEngine;

namespace Utils.AsyncBundles.Loaders
{
  public class AsyncResourceBundleLoader : IAsyncBundleLoader
  {
    private readonly Lifetime _lifetime;
    private readonly ICoroutineProvider _coroutineProvider;
    private readonly Func<bool> _destroyInstances;
    private readonly Signal<IAsyncBundleLoader> _onComplete;
    private ResourceRequest _request;
    private AssetBundleCreateRequest _bundleRequest;

    public AsyncResourceBundleLoader(Lifetime lifetime, ICoroutineProvider coroutineProvider, Func<bool> destroyInstances)
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

    private IEnumerator LoadAsync(string url, int version)
    {
      Unloaded = false;
      var request = Resources.LoadAsync<TextAsset>(url);
      _request = request;
      IsLoading = true;
      IsDone = false;

      while (!request.isDone)
      {
        Progress = request.progress / 2f;
        if (!IsLoading)
        {
          yield break;
        }
        yield return null;
      }

      var textAsset = request.asset as TextAsset;
      if (textAsset != null)
      {
        _bundleRequest = AssetBundle.LoadFromMemoryAsync(textAsset.bytes);
        while (!_bundleRequest.isDone)
        {
          Progress = 0.5f + _bundleRequest.progress / 2f;
          yield return null;
        }
        if (_bundleRequest.assetBundle == null)
        {
          //Debug.LogError("bundle not load: " + url);
        }
        else
        {
          Bundle = _bundleRequest.assetBundle;
        }
      }
      else
      {
        //Debug.LogError("bundle not load: " + url);
      }
      IsDone = true;
      IsLoading = false;
      IsLoaded = true;

      yield return null;

      _onComplete.Fire(this);
    }

    private void Unload(bool unloadAllLoadedObjects)
    {
      if (_bundleRequest != null && Bundle == null && !_bundleRequest.isDone)
      {
        var go = new GameObject("WaitAndDispose");
        GameObject.DontDestroyOnLoad(go);
        go.AddComponent<EmptyMonoBehaviour>().StartCoroutine(WaitAndDispose(_bundleRequest, go));
      }
      if (Bundle != null)
      {
        Bundle.Unload(unloadAllLoadedObjects);
        Bundle = null;
      }

      _bundleRequest = null;
      _request = null;
      IsLoading = false;
      IsLoaded = false;
      IsDone = false;
      Unloaded = true;
    }

    private IEnumerator WaitAndDispose(AssetBundleCreateRequest bundleRequest, GameObject go)
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
