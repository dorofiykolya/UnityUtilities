using System;
using System.Reflection;

namespace Injection
{
  public class FieldDescription : MemberDescription
  {
    private readonly FieldInfo _fieldInfo;

    public FieldDescription(FieldInfo fieldInfo, Attribute attribute) : base(fieldInfo, attribute)
    {
      _fieldInfo = fieldInfo;
    }

    public override MemberKind Kind { get { return MemberKind.Field; } }

    public override Type Type
    {
      get
      {
        if (_fieldInfo != null)
        {
          return _fieldInfo.FieldType;
        }
        return null;
      }
    }
    public override void SetValue(Object target, Object value)
    {
      if (_fieldInfo != null)
      {
        _fieldInfo.SetValue(target, value);
      }
    }

    public override object GetValue(object target)
    {
      if (_fieldInfo != null)
      {
        _fieldInfo.GetValue(target);
      }
      return null;
    }

    public override void Apply(object target, Type targetType, IInjector injector)
    {
      var provider = injector.GetProvider(_fieldInfo.FieldType);
      SetValue(target, provider.Apply(injector, targetType));
    }
  }
}
