using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Injection
{
  public class TypeProvider : IEquatable<TypeProvider>
  {
    protected static readonly Type ObjectType = typeof(object);
    private static readonly object PoolLock = new object();
    private static readonly Stack<Enumerable> Pool = new Stack<Enumerable>();
    private static int _instanceCount;

    private readonly int _index;
    private readonly Type _type;
    private readonly Type _baseType;
    private readonly int _memberCapacity;
    private readonly DescriptionProvider _provider;
    private HashSet<Type> _membersAttributes;
    private List<Attribute> _typeAttributes;
    private Dictionary<Type, List<MemberDescription>> _members;
    private TypeProvider _parent;
    private bool _parsed;

    public TypeProvider(DescriptionProvider provider, Type type, Type baseType = null, int memberCapacity = 4)
    {
      _index = Interlocked.Increment(ref _instanceCount);
      _provider = provider;
      _type = type;
      _memberCapacity = memberCapacity;
      _baseType = baseType;
    }

    public static Enumerable Pop()
    {
      lock (PoolLock)
      {
        if (Pool.Count != 0)
        {
          var result = Pool.Pop();
          result.InPool = false;
          return result;
        }
      }
      return new Enumerable();
    }

    public static void Push(Enumerable enumerator)
    {
      lock (PoolLock)
      {
        if (!enumerator.InPool)
        {
          enumerator.InPool = true;
          enumerator.GetEnumerator().Dispose();
          Pool.Push(enumerator);
        }
      }
    }

    public virtual bool Parsed { get { return _parsed; } }

    public virtual IEnumerable<MemberDescription> GetByAttribute<T>(bool autoRelease = false) where T : Attribute
    {
      return GetByAttribute(typeof(T), autoRelease);
    }

    public virtual IEnumerable<MemberDescription> GetByAttribute<T>(MemberKind kind, bool autoRelease = false) where T : Attribute
    {
      return GetByAttribute(typeof(T), kind, autoRelease);
    }

    public virtual IEnumerable<MemberDescription> GetByAttribute(Type type, bool autoRelease = false)
    {
      List<MemberDescription> result;
      if ((_members ?? (_members = new Dictionary<Type, List<MemberDescription>>())).TryGetValue(type, out result))
      {
        if (autoRelease)
        {
          return TypeProvider.Pop().Set(result, autoRelease);
        }
      }
      return result;
    }

    public virtual IEnumerable<MemberDescription> GetByAttribute(Type type, MemberKind kind, bool autoRelease = false)
    {
      List<MemberDescription> result;
      if ((_members ?? (_members = new Dictionary<Type, List<MemberDescription>>())).TryGetValue(type, out result))
      {
        if (autoRelease)
        {
          return TypeProvider.Pop().Set(result, autoRelease, kind);
        }
        return result.Where(k => k.Kind == kind);
      }
      return null;
    }

    public virtual IEnumerable<MemberDescription> Members
    {
      get
      {
        foreach (var value in (_members ?? (_members = new Dictionary<Type, List<MemberDescription>>())).Values)
        {
          foreach (var memberDescription in value)
          {
            yield return memberDescription;
          }
        }
      }
    }

    public virtual TypeProvider Parent { get { return _parent; } }

    public virtual DescriptionProvider DescriptionProvider { get { return _provider; } }

    public virtual Type Type { get { return _type; } }

    public virtual Type BaseType { get { return _baseType; } }

    public virtual ICollection<Type> MembersAttributes { get { return (_membersAttributes ?? (_membersAttributes = new HashSet<Type>())); } }

    public virtual ICollection<Attribute> TypeAttributes { get { return (_typeAttributes ?? (_typeAttributes = new List<Attribute>())); } }

    public virtual IEnumerable<ConstructorDescription> Constructors
    {
      get { return Members.Where(m => m.Kind == MemberKind.Constructor).Cast<ConstructorDescription>(); }
    }

    public virtual IEnumerable<MethodDescription> Methods
    {
      get { return Members.Where(m => m.Kind == MemberKind.Method).Cast<MethodDescription>(); }
    }

    public virtual IEnumerable<FieldDescription> Fields
    {
      get { return Members.Where(m => m.Kind == MemberKind.Field).Cast<FieldDescription>(); }
    }

    public virtual IEnumerable<PropertyDescription> Properties
    {
      get { return Members.Where(m => m.Kind == MemberKind.Property).Cast<PropertyDescription>(); }
    }

    public virtual IEnumerable<MemberDescription> GetMembers(MemberKind kind)
    {
      return Members.Where(m => m.Kind == kind);
    }

    public virtual ConstructorDescription DefaultConstructor { get; protected set; }

    public override int GetHashCode()
    {
      return _index;
    }

    public override bool Equals(object other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      if (other.GetType() != GetType()) return false;
      return Equals((TypeProvider)other);
    }

    public virtual bool Equals(TypeProvider other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return _index == other._index;
    }

    public virtual void Parse(MemberKind kind = MemberKind.All)
    {
      if (!_parsed)
      {
        _parsed = true;
        var provider = DescriptionProvider;
        ParseParentProvider(_type, kind, provider);
        ParseTypeAttributes(_type, kind, provider);
        ParseFields(_type, kind, provider);
        ParseProperties(_type, kind, provider);
        ParseMethods(_type, kind, provider);
        ParseConstructors(_type, kind, provider);
      }
    }

    protected virtual void ParseParentProvider(Type type, MemberKind kind, DescriptionProvider provider)
    {
      var baseType = type.BaseType;
      if (baseType != null && baseType != ObjectType && (BaseType == null || BaseType != baseType))
      {
        _parent = provider.GetProvider(baseType, kind);
      }
    }

    protected virtual void ParseTypeAttributes(Type type, MemberKind kind, DescriptionProvider provider)
    {
      foreach (var customAttribute in type.GetCustomAttributes(true))
      {
        if (provider.IsMappedAttribute(customAttribute.GetType()))
        {
          (_typeAttributes ?? (_typeAttributes = new List<Attribute>())).Add((Attribute)customAttribute);
        }
      }
    }

    protected virtual void ParseFields(Type type, MemberKind kind, DescriptionProvider provider)
    {
      if ((kind & MemberKind.Field) == MemberKind.Field)
      {
        var fields =
        type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                            BindingFlags.GetField | BindingFlags.SetField | BindingFlags.DeclaredOnly);
        var length = fields.Length;
        for (int i = 0; i < length; i++)
        {
          var field = fields[i];
          var attributes = field.GetCustomAttributes(true);
          var attrLen = attributes.Length;
          for (int j = 0; j < attrLen; j++)
          {
            var attribute = attributes[j] as Attribute;
            Type attributeType;
            if (attribute != null && provider.IsMappedAttribute(attributeType = attribute.GetType()))
            {
              AddDescription(attributeType, new FieldDescription(field, attribute));
            }
          }
        }
      }
    }

    protected virtual void ParseProperties(Type type, MemberKind kind, DescriptionProvider provider)
    {
      if ((kind & MemberKind.Property) == MemberKind.Property)
      {
        var properties =
            type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                                BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.DeclaredOnly);

        var length = properties.Length;
        for (int i = 0; i < length; i++)
        {
          var property = properties[i];
          var attributes = property.GetCustomAttributes(true);
          var attrLen = attributes.Length;
          for (int j = 0; j < attrLen; j++)
          {
            var attribute = attributes[j] as Attribute;
            Type attributeType;
            if (attribute != null && provider.IsMappedAttribute(attributeType = attribute.GetType()))
            {
              AddDescription(attributeType, new PropertyDescription(property, attribute));
            }
          }
        }
      }
    }

    protected virtual void ParseMethods(Type type, MemberKind kind, DescriptionProvider provider)
    {
      if ((kind & MemberKind.Method) == MemberKind.Method)
      {
        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
        var length = methods.Length;
        for (int i = 0; i < length; i++)
        {
          var method = methods[i];
          var attributes = method.GetCustomAttributes(true);
          var attrLen = attributes.Length;
          for (int j = 0; j < attrLen; j++)
          {
            var attribute = attributes[j] as Attribute;
            Type attributeType;
            if (attribute != null && provider.IsMappedAttribute(attributeType = attribute.GetType()))
            {
              AddDescription(attributeType, new MethodDescription(method, attribute));
            }
          }
        }
      }
    }

    protected virtual void ParseConstructors(Type type, MemberKind kind, DescriptionProvider provider)
    {
      if ((kind & MemberKind.Constructor) == MemberKind.Constructor)
      {
        ConstructorInfo[] constructors = type.GetConstructors();
        ConstructorInfo constructorToInject = null;
        int maxParameters = -1;
        foreach (ConstructorInfo constructor in constructors)
        {
          object[] attributes = constructor.GetCustomAttributes(true);
          var attrLen = attributes.Length;
          for (int i = 0; i < attrLen; i++)
          {
            var attribute = attributes[i] as Attribute;
            Type attributeType;
            if (attribute != null && provider.IsMappedAttribute(attributeType = attribute.GetType()))
            {
              AddDescription(attributeType, new ConstructorDescription(constructor, attribute));
            }
          }
          if (constructor.GetParameters().Length > maxParameters)
          {
            constructorToInject = constructor;
            maxParameters = constructor.GetParameters().Length;
          }
        }
        if (constructorToInject != null)
          DefaultConstructor = new ConstructorDescription(constructorToInject, null);
      }
    }

    protected void AddDescription(Attribute attribute, MemberDescription description)
    {
      AddDescription(attribute.GetType(), description);
    }

    protected void AddDescription(Type attributeType, MemberDescription description)
    {
      Type type = attributeType;
      List<MemberDescription> result;
      if (!(_members ?? (_members = new Dictionary<Type, List<MemberDescription>>())).TryGetValue(type, out result))
      {
        result = new List<MemberDescription>(_memberCapacity);
        (_members ?? (_members = new Dictionary<Type, List<MemberDescription>>())).Add(type, result);
      }
      result.Add(description);
      MapMemberAttribute(type);
    }

    protected virtual void MapMemberAttribute(Type attributeType)
    {
      (_membersAttributes ?? (_membersAttributes = new HashSet<Type>())).Add(attributeType);
    }

    public class Enumerable : IEnumerable<MemberDescription>
    {
      public bool InPool;
      public bool AutoRelease;
      private Enumarator _enumerator;

      public Enumerable Set(List<MemberDescription> members, bool autoRelease, MemberKind kind = MemberKind.All)
      {
        AutoRelease = autoRelease;
        if (_enumerator == null)
        {
          _enumerator = new Enumarator(this);
        }
        _enumerator.Set(members, kind);
        _enumerator.Reset();
        return this;
      }

      public IEnumerator<MemberDescription> GetEnumerator()
      {
        return _enumerator;
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return _enumerator;
      }
    }

    public class Enumarator : IEnumerator<MemberDescription>
    {
      private readonly Enumerable _enumerable;
      private List<MemberDescription> _members;
      private int _index;
      private MemberDescription _current;
      private MemberKind _kind;
      private bool _isAll;

      public Enumarator(Enumerable enumerable)
      {
        _enumerable = enumerable;
      }

      public Enumarator Set(List<MemberDescription> members, MemberKind kind = MemberKind.All)
      {
        _kind = kind;
        _isAll = _kind == MemberKind.All;
        _members = members;
        _current = null;
        return this;
      }

      public MemberDescription Current
      {
        get
        {
          return _current;
        }
      }

      object IEnumerator.Current
      {
        get
        {
          return _current;
        }
      }

      public void Dispose()
      {
        _members = null;
        _current = null;
      }

      public bool MoveNext()
      {
        bool result = false;
        if (_members != null)
        {
          if (!_isAll)
          {
            while (_index >= 0 && _index < _members.Count && (_members[_index].Kind & _kind) != _members[_index].Kind)
            {
              _index++;
            }
          }
          if (_index >= 0 && _index < _members.Count)
          {
            _current = _members[_index];
            _index++;
            result = true;
          }
          else
          {
            _current = null;
          }
        }
        if (!result)
        {
          if (_enumerable.AutoRelease)
          {
            TypeProvider.Push(_enumerable);
          }
        }
        return result;
      }

      public void Reset()
      {
        _index = 0;
        _current = null;
      }
    }
  }
}
