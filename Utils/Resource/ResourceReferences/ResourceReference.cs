using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace References
{
  [Serializable]
  public class ResourceReference
  {
    [SerializeField]
    private string _path;
    [SerializeField]
    private Type _type;

    public ResourceReference()
    {
      _type = typeof(Object);
    }

    public ResourceReference(Type type)
    {
      _type = type;
    }

    public string Path
    {
      get { return _path; }
    }

    public T GetAsset<T>() where T : Object
    {
      return Resources.Load<T>(_path);
    }
  }

  [Serializable]
  public class ResourceReference<T> : ResourceReference where T : Object
  {
    public static implicit operator T(ResourceReference<T> value)
    {
      return value.Asset;
    }

    public ResourceReference() : base(typeof(T))
    {

    }

    public T Asset
    {
      get { return base.GetAsset<T>(); }
    }
  }
}
