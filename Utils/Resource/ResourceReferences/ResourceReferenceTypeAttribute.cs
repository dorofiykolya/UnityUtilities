using System;

namespace References
{
  public class ResourceReferenceTypeAttribute : Attribute
  {
    public Type Type;

    public ResourceReferenceTypeAttribute(Type type)
    {
      Type = type;
    }
  }
}
