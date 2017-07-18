using System;
using System.Collections.Generic;
using System.Reflection;

namespace Injection
{
  public class MethodProvider : Provider
  {
    private readonly MethodInfo _methodInfo;
    private readonly bool _optional;

    public MethodProvider(MethodInfo methodInfo, Type type, object value, bool optional = false) : base(type, value)
    {
      _methodInfo = methodInfo;
      _optional = optional;
    }

    public override object Apply(IInjector injector, Type type)
    {
      var parameters = GatherParameterValues(type, injector);
      return _methodInfo.Invoke(Value, parameters);
    }

    protected virtual object[] GatherParameterValues(Type targetType, IInjector injector)
    {
      if (_methodInfo == null)
        return new object[0];
      List<object> parameters = new List<object>();
      ParameterInfo[] parameterInfos = _methodInfo.GetParameters();
      int length = parameterInfos.Length;
      for (int i = 0; i < length; i++)
      {
        Type parameterType = parameterInfos[i].ParameterType;
        var isLazy = parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(LI<>);
        var provider = injector.GetProvider(isLazy ? parameterType.GetGenericArguments()[0] : parameterType);
        if (provider == null)
        {
          if (parameterInfos[i].IsOptional)
          {
            parameters.Add(parameterInfos[i].DefaultValue);
            continue;
          }
          if (_optional)
          {
            parameters.Add(null);
            continue; //TODO: Check optional parameters are in order (last) for this break to work, else use continue
          }
          throw new InvalidOperationException(
              "Injector is missing a mapping to handle constructor injection into target type '"
              + targetType.FullName + "'. \nTarget dependency: " + parameterType.FullName +
              ", method: " + _methodInfo.Name + ", parameter: " + (i + 1)
          );
        }
        parameters.Add(provider.Apply(injector, targetType));
      }
      return parameters.ToArray();
    }

  }
}
