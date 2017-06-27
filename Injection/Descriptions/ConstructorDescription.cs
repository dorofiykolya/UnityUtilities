using System;
using System.Reflection;

namespace Injection
{
  public class ConstructorDescription : MethodBaseDescription
  {
    private readonly ConstructorInfo _constructorInfo;

    public ConstructorDescription(ConstructorInfo constructorInfo, Attribute attribute) : base(constructorInfo, attribute, false)
    {
      _constructorInfo = constructorInfo;
    }

    public object CreateInstance(Type type, IInjector injector)
    {
      var parameters = GetParameterValues(type, injector);
      try
      {
        return _constructorInfo.Invoke(parameters);
      }
      catch (Exception exception)
      {
        throw exception;
      }
    }

    public override MemberKind Kind { get { return MemberKind.Constructor; } }
    public override Type Type { get { return null; } }
  }
}
