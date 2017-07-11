using System;
using System.Reflection;

namespace Injection
{
  public class MethodDescription : MethodBaseDescription
  {
    private readonly MethodInfo _methodInfo;

    public MethodDescription(MethodInfo methodInfo, Attribute attribute) : base(methodInfo, attribute)
    {
      _methodInfo = methodInfo;
    }

    public MethodInfo Info { get { return _methodInfo; } }

    public virtual Type ReturnType { get { return _methodInfo.ReturnType; } }

    public override MemberKind Kind { get { return MemberKind.Method; } }

    public override Type Type { get { return null; } }

    public override Type ProviderType { get { return null; } }

    public override void SetValue(object target, object value)
    {
      throw new NotImplementedException();
    }

    public override object GetValue(object target)
    {
      throw new NotImplementedException();
    }

    public override void Apply(object target, Type targetType, IInjector injector)
    {
      var parameters = GetParameterValues(targetType, injector);
      _methodInfo.Invoke(target, parameters);
    }
  }
}
