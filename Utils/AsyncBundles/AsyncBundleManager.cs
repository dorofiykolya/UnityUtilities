using System.Collections.Generic;
using Utils;

namespace AsyncBundles
{
  public class AsyncBundleManager
  {
    private readonly Lifetime _lifetime;
    private readonly ICoroutineProvider _coroutineProvider;
    private readonly Dictionary<string, AsyncBundle> _map;

    public AsyncBundleManager(Lifetime lifetime, ICoroutineProvider coroutineProvider)
    {
      _lifetime = lifetime;
      _coroutineProvider = coroutineProvider;
      _map = new Dictionary<string, AsyncBundle>();
      _lifetime.AddAction(() =>
      {
        _map.Clear();
      });
    }

    public AsyncBundle Get(string path, int version)
    {
      AsyncBundle result;
      if (!_map.TryGetValue(path, out result))
      {
        result = new AsyncBundle(_lifetime, path, version, _coroutineProvider);
        _map[path] = result;
      }
      return result;
    }
  }
}
