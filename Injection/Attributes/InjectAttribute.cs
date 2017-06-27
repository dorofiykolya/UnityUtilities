using System;

namespace Injection
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
    public class InjectAttribute : Attribute
    {

    }
}
