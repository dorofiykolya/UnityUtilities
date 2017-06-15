using System;
using System.Collections;

namespace UnityEngine
{
  public class AsyncPreloaderData
  {
    private readonly Hashtable _map = new Hashtable();
    private Type _preloaderType = typeof(AsyncImagePreloader);

    public Sprite[] Sprites;
    public float Fps;

    public object this[string key]
    {
      get { return _map[key]; }
      set { _map[key] = value; }
    }

    public T Get<T>(string key) where T : class
    {
      return _map[key] as T;
    }

    public virtual bool HasData
    {
      get { return Fps > 0 && Sprites != null && Sprites.Length != 0; }
    }

    public virtual void SetPreloaderType<T>() where T : class, IAsyncImagePreloader
    {
      _preloaderType = typeof(T);
    }

    public virtual Type GetPreloaderType(AsyncSprite sprite = null)
    {
      return _preloaderType;
    }
  }
}
