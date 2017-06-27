using System;

namespace Injection
{
  public class ValueProvider : Provider
  {
    public ValueProvider(Type type, object value) : base(type, value)
    {

    }

    public override object Apply(IInjector injector, Type type)
    {
      return this.Value;
    }
  }
}
