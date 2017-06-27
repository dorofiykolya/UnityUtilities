using System;
using System.Reflection;

namespace Injection
{
  public abstract class MemberDescription
  {
    protected MemberDescription(MemberInfo memberInfo, Attribute attribute)
    {
      MemberInfo = memberInfo;
      Name = memberInfo.Name;
      Attribute = attribute;
    }

    public virtual void Apply(object target, Type targetType, IInjector injector)
    {

    }

    public string Name { get; protected set; }

    public Attribute Attribute { get; protected set; }

    public MemberInfo MemberInfo { get; protected set; }

    public abstract MemberKind Kind { get; }

    public abstract Type Type { get; }

    public abstract void SetValue(object target, object value);

    public abstract object GetValue(object target);
  }
}
