using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace References
{
  [Serializable]
  public class ResourceReference : ISerializationCallbackReceiver
  {
    public static string SerializeType(Type type)
    {
      return type.FullName + ", " + type.Assembly.GetName().Name;
    }

    [SerializeField]
    private string _serializedType;
    [SerializeField]
    private string _path;
    [SerializeField]
    private Type _type;

#if UNITY_EDITOR

    [SerializeField]
    private string _guid;

#endif

    public ResourceReference()
    {
      _type = typeof(Object);
      _serializedType = SerializeType(_type);
    }

    public ResourceReference(Type type)
    {
      _type = type;
      _serializedType = SerializeType(_type);
    }

    public string Path
    {
      get { return _path; }
    }

    public Type Type
    {
      get { return _type; }
    }

    public T GetAsset<T>() where T : Object
    {
      return Resources.Load<T>(_path);
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
      _type = Type.GetType(_serializedType);

      if (_type == null)
      {
        _type = typeof(Object);
      }
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
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
