using System;
using System.Reflection;

namespace Injection
{
  public class PropertyDescription : MemberDescription
  {
    private readonly PropertyInfo _propertyInfo;

    public PropertyDescription(PropertyInfo propertyInfo, Attribute attribute) : base(propertyInfo, attribute)
    {
      _propertyInfo = propertyInfo;
    }

    public override MemberKind Kind { get { return MemberKind.Property; } }

    public override Type Type
    {
      get
      {
        if (_propertyInfo != null)
        {
          return _propertyInfo.PropertyType;
        }
        return null;
      }
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
      var provider = injector.GetProvider(_propertyInfo.PropertyType);
      SetValue(target, provider.Apply(injector, targetType));
    }
  }
}
