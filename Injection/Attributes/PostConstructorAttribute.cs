using System;

namespace Injection
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class PostConstructorAttribute : Attribute
    {
    }
}
