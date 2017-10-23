using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace References
{
  [Serializable]
  public class ResourceReference : ISerializationCallbackReceiver
  {
    [SerializeField]
    private string _path;
    [SerializeField]
    private Type _type;
    [SerializeField]
    private string _serializedType;

    public ResourceReference()
    {
      _type = typeof(Object);
      _serializedType = _type.FullName + ", " + _type.Assembly.GetName().Name;
    }

    public ResourceReference(Type type)
    {
      _type = type;
      _serializedType = _type.FullName + ", " + _type.Assembly.GetName().Name;
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
      _type = System.Type.GetType(_serializedType);

      if (_type == null)
      {
        throw new InvalidOperationException(string.Format("'{0}' type was not found", _serializedType));
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
