using System;
using System.Collections.Generic;

namespace Injection
{
  public class Injector : IInjector, IDisposable
  {
    private readonly Dictionary<Type, IMapping> _map;
    private readonly Dictionary<Type, IProvider> _provider;
    private IInjector _parent;

    public Injector() : this(null, null) { }

    public Injector(IInjector parent) : this(parent, null) { }

    public Injector(DescriptionProvider provider) : this(null, provider) { }

    public Injector(IInjector parent, DescriptionProvider provider)
    {
      _map = new Dictionary<Type, IMapping>(128);
      _provider = new Dictionary<Type, IProvider>(128);

      Parent = parent;
      if (parent != null && provider == null)
      {
        DescriptionProvider = parent.DescriptionProvider;
      }
      else
      {
        DescriptionProvider = provider ?? new DescriptionProvider();
      }
      DescriptionProvider.MapAttribute<InjectAttribute>();
      DescriptionProvider.MapAttribute<PostConstructorAttribute>();
      DescriptionProvider.MapAttribute<PostInjectAttribute>();
      if (GetProvider(typeof(DescriptionProvider), true) == null)
      {
        Map<DescriptionProvider>().ToValue(DescriptionProvider);
      }
      Map<IInjector>().ToValue(this);
      Map<Injector>().ToValue(this);
      Map<IInject>().ToValue(this);
    }

    public bool PostInject { get; set; }

    public void Dispose()
    {
      Clear();
    }

    public void Clear()
    {
      _map.Clear();
      _provider.Clear();
    }

    public virtual void Inject(object value)
    {
      ApplyInject(value);
    }

    public IMapping Map<T>() where T : class
    {
      return Map(typeof(T));
    }

    public IMapping Map(Type type)
    {
      IMapping result;
      if (!_map.TryGetValue(type, out result))
      {
        result = new Mapping(this, type);
        _map.Add(type, result);
      }
      return result;
    }

    public void Unmap<T>() where T : class
    {
      Unmap(typeof(T));
    }

    public void Unmap(Type type)
    {
      IMapping result;
      if (_map.TryGetValue(type, out result))
      {
        UnmapProvider(result.Type);
        result.Dispose();
        _map.Remove(type);
      }
    }

    public IInjector Parent
    {
      get { return _parent; }
      set
      {
        if (value != _parent)
        {
          var current = value;
          while (current != null)
          {
            if (current == this) throw new ArgumentException();
            current = current.Parent;
          }
          _parent = value;
        }
      }
    }

    public T Get<T>() where T : class
    {
      return (T)Get(typeof(T));
    }

    public object Get(Type type)
    {
      var provider = GetProvider(type, true);
      if (provider != null)
      {
        return provider.Apply(this, type);
      }
      return null;
    }

    public IProvider GetProvider(Type type, bool includeParents)
    {
      IProvider result;
      if (!_provider.TryGetValue(type, out result) && includeParents && Parent != null)
      {
        result = Parent.GetProvider(type, true);
      }
      return result;
    }

    public void MapProvider<T>(IProvider provider) where T : class
    {
      MapProvider(typeof(T), provider);
    }

    public void MapProvider(Type type, IProvider provider)
    {
      UnmapProvider(type);
      _provider[type] = provider;
    }

    public void UnmapProvider<T>() where T : class
    {
      UnmapProvider(typeof(T));
    }

    public DescriptionProvider DescriptionProvider { get; private set; }

    public void UnmapProvider(Type type)
    {
      IProvider result;
      if (_provider.TryGetValue(type, out result))
      {
        result.Dispose();
        _provider.Remove(type);
      }
    }

    protected virtual void ApplyProvider(object value, TypeProvider typeProvider)
    {
      if (typeProvider.Parent != null)
      {
        ApplyProvider(value, typeProvider.Parent);
      }

      var members = typeProvider.GetByAttribute<InjectAttribute>(true);
      if (members != null)
      {
        foreach (var member in members)
        {
          var kind = member.Kind;
          if ((kind & MemberKind.Field) == MemberKind.Field ||
              (kind & MemberKind.Property) == MemberKind.Property)
          {
            var provider = GetProvider(member.ProviderType, true);
            if (provider != null)
            {
              if (member.ProviderType != member.Type)
              {
                member.SetValue(value, CreateLazy(provider, member.Type, member.ProviderType));
              }
              else
              {
                member.SetValue(value, provider.Apply(this, member.Type));
              }
            }
          }
        }
      }
    }

    private object CreateLazy(IProvider provider, Type type, Type providerType)
    {
      Func<object> factory = () => provider.Apply(this, type);
      return Activator.CreateInstance(typeof(LI<>).MakeGenericType(providerType), factory);
    }

    private void ApplyInject(object value)
    {
      var typeProvider = DescriptionProvider.GetProvider(value.GetType());
      ApplyProvider(value, typeProvider);
      if (PostInject)
      {
        ApplyPostInject(value, typeProvider);
      }
    }

    private void ApplyPostInject(object target, TypeProvider provider)
    {
      var methods = provider.GetByAttribute<PostInjectAttribute>(MemberKind.Method, true);
      if (methods != null)
      {
        foreach (var method in methods)
        {
          method.Apply(target, target.GetType(), this);
        }
      }
    }
  }
}
