using System;
using System.Linq;

namespace Injection
{
  public class FactoryProvider : Provider
  {
    private readonly bool _postConstructor;

    public FactoryProvider(Type type, object value, bool postConstructor = true) : base(type, value)
    {
      _postConstructor = postConstructor;
    }

    public override object Apply(IInjector injector, Type type)
    {
      var targetType = Value as Type;
      var result = CreateInstance(injector, targetType);
      if (result != null)
      {
        injector.Inject(result);
      }
      if (_postConstructor)
      {
        PostContructor(injector, result, targetType);
      }
      return result;
    }

    public object Create(IInjector injector, Type type)
    {
      var targetType = Value as Type;
      var result = CreateInstance(injector, targetType);
      return result;
    }

    private void PostContructor(IInjector injector, object result, Type type)
    {
      var methods = injector.DescriptionProvider.GetProvider(type).GetByAttribute<PostConstructorAttribute>(MemberKind.Method);
      if (methods != null)
      {
        foreach (var method in methods)
        {
          method.Apply(result, type, injector);
        }
      }
    }

    private object CreateInstance(IInjector injector, Type type)
    {
      var provider = injector.DescriptionProvider.GetProvider(type);
      var constructors = provider.GetByAttribute<InjectAttribute>();
      ConstructorDescription constructor = null;
      if (constructors != null)
      {
        constructor = constructors.FirstOrDefault(m => m.Kind == MemberKind.Constructor) as ConstructorDescription;
      }
      if (constructor == null)
      {
        constructor = provider.DefaultConstructor;
      }
      if (constructor != null)
      {
        return constructor.CreateInstance(type, injector);
      }

      var result = Activator.CreateInstance(type);
      return result;
    }
  }
}
