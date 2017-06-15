using System;
using UnityEngine;

namespace Utils.AsyncBundles.Loaders
{
  public interface IAsyncBundleLoader
  {
    void SubscribeOnComplete(Lifetime lifetime, Action<IAsyncBundleLoader> listener);
    void Load(string url, int version);
    bool IsDone { get; }
    float Progress { get; }
    bool IsLoading { get; }
    bool IsLoaded { get; }
    AssetBundle Bundle { get; }
    Func<IAsyncBundleLoader> Next { get; set; }
  }
}
