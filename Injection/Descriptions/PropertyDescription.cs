using System;
using System.Reflection;

namespace Injection
{
  public class PropertyDescription : MemberDescription
  {
    private readonly PropertyInfo _propertyInfo;
    private readonly Type _type;

    public PropertyDescription(PropertyInfo propertyInfo, Attribute attribute) : base(propertyInfo, attribute)
    {
      _propertyInfo = propertyInfo;
      if (_propertyInfo != null)
      {
        _type = _propertyInfo.PropertyType;
        if (_type.IsGenericType && _type.GetGenericTypeDefinition() == typeof(LazyInject<>))
        {
          _type = _type.GetGenericArguments()[0];
        }
      }
    }

    public override MemberKind Kind
    {
      get { return MemberKind.Property; }
    }

    public override Type Type
    {
      get { return _propertyInfo != null ? _propertyInfo.PropertyType : null; }
    }

    public override Type ProviderType
    {
      get { return _type; }
    }

    public override void SetValue(object target, object value)
    {
      if (_propertyInfo != null)
      {
        _propertyInfo.SetValue(target, value, null);
      }
    }

    public override object GetValue(object target)
    {
      if (_propertyInfo != null)
      {
        _propertyInfo.GetValue(target, null);
      }
      return null;
    }

    public override void Apply(object target, Type targetType, IInjector injector)
    {
      var provider = injector.GetProvider(ProviderType);
      SetValue(target, provider.Apply(injector, targetType));
    }
  }
}
