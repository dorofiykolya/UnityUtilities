using System;
using UnityEngine.Assertions;

namespace Utils.BuildPipeline
{
  public class KeyAttribute : Attribute
  {
    public string Description { get; set; }
    public bool Required { get; set; }
    public bool RequiredValue { get; set; }
    public string[] AvailableValues { get; set; }

    public KeyAttribute(bool required)
    {
      Required = required;
    }

    public KeyAttribute(bool required, params string[] availableValues)
    {
      Required = required;
      AvailableValues = availableValues;
    }

    public KeyAttribute(bool required, Type enumType)
    {
      Assert.IsTrue(enumType.IsEnum, "type must be Enum");
      Required = required;
      RequiredValue = true;
      AvailableValues = Enum.GetNames(enumType);
    }
  }
}
