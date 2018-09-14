using System;
using System.Collections.Generic;

namespace Utils.Persistences
{
  public abstract class Persistence<T> : Persistence where T : Persistence, new()
  {
    protected Persistence()
    {

    }

    public new T this[string key]
    {
      get
      {
        T result = (T)GetPersistance(key);
        if (result == null)
        {
          result = new T();
          Initialize(result, key, Provider, this);
          SetPersistance(key, result);
        }

        return result;
      }
    }
  }

  public class Persistence
  {
    public static readonly Type[] AvailableTypes = { typeof(int), typeof(string), typeof(float), typeof(long), typeof(bool) };
    public static string ConcatPath(string fullPath, string key)
    {
      return fullPath + '/' + key;
    }

    private readonly Dictionary<string, Persistence> _paths = new Dictionary<string, Persistence>();
    private bool _initialized;
    private string _key;
    private IPersistanceProvider _persistanceProvider;
    private Persistence _parent;
    private string _fullPath;
    private int? _intCache;
    private long? _longCache;
    private float? _floatCache;
    private string _stringCache;
    private bool _stringHasCache;
    private object _defaultValue;

    public Persistence()
    {

    }

    public Persistence(IPersistanceProvider persistanceProvider)
    {
      Initialize(this, null, persistanceProvider, null);
    }

    protected static Persistence Initialize(Persistence persistence, string key, IPersistanceProvider persistanceProvider, Persistence parent)
    {
      if (persistence._initialized) throw new InvalidOperationException("Persistence have initialized");
      persistence._key = key;
      persistence._persistanceProvider = persistanceProvider;
      persistence._parent = parent;
      if (parent != null)
      {
        persistence._fullPath = ConcatPath(parent._fullPath, key);
      }
      else
      {
        persistence._fullPath = key;
      }

      persistence._initialized = true;
      return persistence;
    }

    protected Persistence GetPersistance(string key)
    {
      Persistence result;
      _paths.TryGetValue(key, out result);
      return result;
    }

    protected void SetPersistance(string key, Persistence value)
    {
      _paths[key] = value;
    }

    public IPersistanceProvider Provider
    {
      get { return _persistanceProvider; }
    }

    public Persistence this[string key]
    {
      get
      {
        Persistence result;
        if (!_paths.TryGetValue(key, out result))
        {
          _paths[key] = result = Initialize(new Persistence(), key, _persistanceProvider, this);
        }
        return result;
      }
    }

    public string FullPath
    {
      get { return _fullPath; }
    }

    public Persistence Parent
    {
      get { return _parent; }
    }

    public string Key
    {
      get { return _key; }
    }

    public object DefaultValue
    {
      get { return _defaultValue; }
      set
      {
        if (value is int)
        {
          if (!_persistanceProvider.HasKey(_fullPath))
          {
            _intCache = (int)value;
          }
          else
          {
            _intCache = _persistanceProvider.GetInt(_fullPath);
          }
        }
        else if (value is bool)
        {
          if (!_persistanceProvider.HasKey(_fullPath))
          {
            _intCache = ((bool)value ? 1 : 0);
          }
          else
          {
            _intCache = _persistanceProvider.GetInt(_fullPath);
          }
        }
        else if (value is float)
        {
          if (!_persistanceProvider.HasKey(_fullPath))
          {
            _floatCache = (float)value;
          }
          else
          {
            _floatCache = _persistanceProvider.GetFloat(_fullPath);
          }
        }
        else if (value is string)
        {
          if (!_persistanceProvider.HasKey(_fullPath))
          {
            _stringCache = (string)value;
            _stringHasCache = true;
          }
          else
          {
            _stringHasCache = true;
            _stringCache = _persistanceProvider.GetString(_fullPath);
          }
        }
        else if (value is long)
        {
          if (!_persistanceProvider.HasKey(GetRightLongPath(_fullPath)))
          {
            _longCache = (long)value;
          }
          else
          {
            int right = _persistanceProvider.GetInt(GetRightLongPath(_fullPath));
            int left = _persistanceProvider.GetInt(GetLeftLongPath(_fullPath));
            _longCache = CombineToLong(left, right);
          }
        }
        else
        {
          throw new ArgumentException("type not supported: " + (value != null ? value.GetType().FullName : "null"));
        }
      }
    }

    public T GetValue<T>()
    {
      var type = typeof(T);
      if (type == typeof(int))
      {
        return (T)((object)IntValue);
      }
      if (type == typeof(bool))
      {
        return (T)((object)BoolValue);
      }
      if (type == typeof(float))
      {
        return (T)((object)FloatValue);
      }
      if (type == typeof(string))
      {
        return (T)((object)StringValue);
      }
      if (type == typeof(long))
      {
        return (T)((object)LongValue);
      }
      throw new ArgumentException("type not supported: " + typeof(T).FullName);
    }

    public void SetValue<T>(T value)
    {
      var type = typeof(T);
      if (type == typeof(int))
      {
        IntValue = (int)((object)value);
      }
      else if (type == typeof(bool))
      {
        BoolValue = (bool)((object)value);
      }
      else if (type == typeof(float))
      {
        FloatValue = (float)((object)value);
      }
      else if (type == typeof(string))
      {
        StringValue = (string)((object)value);
      }
      else if (type == typeof(long))
      {
        LongValue = (long)(object)value;
      }
      else
      {
        throw new ArgumentException("type not supported: " + typeof(T).FullName);
      }
    }

    public long LongValue
    {
      get
      {
        if (_longCache.HasValue) return _longCache.Value;
        int right = _persistanceProvider.GetInt(GetRightLongPath(_fullPath));
        int left = _persistanceProvider.GetInt(GetLeftLongPath(_fullPath));
        return CombineToLong(left, right);
      }
      set
      {
        _longCache = value;
        var left = GetLeft(value);
        var right = GetRight(value);
        _persistanceProvider.SetInt(GetRightLongPath(_fullPath), right);
        _persistanceProvider.SetInt(GetLeftLongPath(_fullPath), left);
        _persistanceProvider.Save();
      }
    }

    public bool BoolValue
    {
      get
      {
        if (_intCache.HasValue) return _intCache.Value != 0;
        return _persistanceProvider.GetInt(_fullPath) != 0;
      }
      set
      {
        _intCache = value ? 1 : 0;
        _persistanceProvider.SetInt(_fullPath, value ? 1 : 0);
        _persistanceProvider.Save();
      }
    }

    public int IntValue
    {
      get
      {
        if (_intCache.HasValue) return _intCache.Value;
        return _persistanceProvider.GetInt(_fullPath);
      }
      set
      {
        _intCache = value;
        _persistanceProvider.SetInt(_fullPath, value);
        _persistanceProvider.Save();
      }
    }

    public float FloatValue
    {
      get
      {
        if (_floatCache.HasValue) return _floatCache.Value;
        return _persistanceProvider.GetFloat(_fullPath);
      }
      set
      {
        _floatCache = value;
        _persistanceProvider.SetFloat(_fullPath, value);
        _persistanceProvider.Save();
      }
    }

    public string StringValue
    {
      get
      {
        if (_stringHasCache) return _stringCache;
        return _persistanceProvider.GetString(_fullPath);
      }
      set
      {
        _stringCache = value;
        _stringHasCache = true;
        _persistanceProvider.SetString(_fullPath, value);
        _persistanceProvider.Save();
      }
    }

    private static string GetLeftLongPath(string path)
    {
      return path + "@32";
    }

    private static string GetRightLongPath(string path)
    {
      return path + "@0";
    }

    private static int GetLeft(long value)
    {
      return (int)(0xFFFFFFFF & (value >> 32));
    }

    private static int GetRight(long value)
    {
      return (int)(0xFFFFFFFF & value);
    }

    private static long CombineToLong(int left, int right)
    {
      return ((long)left << 32) | (long)right;
    }
  }
}
