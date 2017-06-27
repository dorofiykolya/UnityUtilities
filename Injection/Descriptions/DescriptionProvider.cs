using System;
using System.Collections.Generic;
using System.Linq;

namespace Injection
{
  public class DescriptionProvider
  {
    private static readonly Type AttributeType = typeof(Attribute);

    private readonly HashSet<Type> _mappedAttributes = new HashSet<Type>();
    private readonly Dictionary<Type, TypeProvider> _byType = new Dictionary<Type, TypeProvider>();
    private readonly Dictionary<Type, HashSet<TypeProvider>> _byAttribute = new Dictionary<Type, HashSet<TypeProvider>>();
    private readonly DescriptionProvider _parent;

    public DescriptionProvider(DescriptionProvider parent = null)
    {
      var target = parent;
      while (target != null)
      {
        if (target == this)
        {
          throw new ArgumentException(GetType() + ": An object cannot be added as a parent to itself or one of its panres");
        }
        target = target._parent;
      }
      _parent = parent;
    }

    public virtual DescriptionProvider Parent { get { return _parent; } }

    public virtual void MapAttribute<T>() where T : Attribute
    {
      _mappedAttributes.Add(typeof(T));
    }

    public virtual void MapAttribute(Type type)
    {
      if (!type.IsSubclassOf(AttributeType)) throw new ArgumentException();
      _mappedAttributes.Add(type);
    }

    public virtual void UnMapAttribute<T>() where T : Attribute
    {
      _mappedAttributes.Remove(typeof(T));
    }

    public virtual void UnMapAttribute(Type type)
    {
      if (!type.IsSubclassOf(AttributeType)) throw new ArgumentException();
      _mappedAttributes.Remove(type);
    }

    public virtual bool IsMappedAttribute(Type type, bool inherited = true)
    {
      if (type == null) return false;
      return _mappedAttributes.Contains(type) || (inherited && _parent != null && _parent.IsMappedAttribute(type));
    }

    public virtual IEnumerable<Type> MappedAttributes(bool inherited = true)
    {
      if (inherited && _parent != null)
      {
        return _mappedAttributes.Concat(_parent.MappedAttributes());
      }
      return _mappedAttributes;
    }

    public virtual TypeProvider AddProvider(TypeProvider provider)
    {
      AddTypeProvider(provider);
      if (provider.Parsed)
      {
        AddToAttribute(provider);
      }
      return provider;
    }

    public virtual void ParseProvider(TypeProvider provider, MemberKind kind = MemberKind.All)
    {
      AddTypeProvider(provider);
      provider.Parse(kind);
      AddToAttribute(provider);
    }

    public virtual void AddProvider<T>(MemberKind kind = MemberKind.All)
    {
      AddProvider(typeof(T), kind);
    }

    public virtual void AddProvider(Type type, MemberKind kind = MemberKind.All)
    {
      if (GetTypeProvider(type, kind) == null)
      {
        CreateTypeProvider(type, kind);
      }
    }

    public virtual TypeProvider GetProvider<T>(MemberKind kind = MemberKind.All) where T : class
    {
      return GetProvider(typeof(T), kind);
    }

    public virtual TypeProvider GetProvider(Type type, MemberKind kind = MemberKind.All)
    {
      var result = GetTypeProvider(type, kind) ?? CreateTypeProvider(type, kind);
      return result;
    }

    public virtual IEnumerable<TypeProvider> GetProvidersByAttribute<T>() where T : Attribute
    {
      HashSet<TypeProvider> result;
      _byAttribute.TryGetValue(typeof(T), out result);
      return result;
    }

    protected virtual void AddTypeProvider(TypeProvider provider)
    {
      _byType.Add(provider.Type, provider);
    }

    protected virtual TypeProvider GetTypeProvider(Type type, MemberKind kind = MemberKind.All)
    {
      TypeProvider result;
      if (!_byType.TryGetValue(type, out result) && _parent != null)
      {
        return _parent.GetTypeProvider(type, kind);
      }
      if (result != null && !result.Parsed)
      {
        result.Parse(kind);
        AddToAttribute(result);
      }
      return result;
    }

    protected virtual TypeProvider CreateTypeProvider(Type type, MemberKind kind = MemberKind.All)
    {
      var provider = new TypeProvider(this, type);
      AddTypeProvider(provider);
      provider.Parse(kind);
      AddToAttribute(provider);
      return provider;
    }

    private void AddToAttribute(TypeProvider provider)
    {
      foreach (var attribute in provider.MembersAttributes)
      {
        HashSet<TypeProvider> hashSet;
        if (!_byAttribute.TryGetValue(attribute, out hashSet))
        {
          hashSet = new HashSet<TypeProvider>();
          _byAttribute.Add(attribute, hashSet);
        }
        hashSet.Add(provider);
      }
      foreach (var attribute in provider.TypeAttributes)
      {
        Type attributeType = attribute.GetType();
        HashSet<TypeProvider> hashSet;
        if (!_byAttribute.TryGetValue(attributeType, out hashSet))
        {
          hashSet = new HashSet<TypeProvider>();
          _byAttribute.Add(attributeType, hashSet);
        }
        hashSet.Add(provider);
      }
    }
  }
}
