using System;
using UnityEditor;
using UnityEngine;

namespace UnityEditor
{
  [CustomPropertyDrawer(typeof(EnumFlagAttribute))]
  public class EnumFlagDrawer : PropertyDrawer
  {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      EnumFlagAttribute flagSettings = (EnumFlagAttribute)attribute;
      Enum targetEnum = (Enum)Enum.Parse(fieldInfo.FieldType, property.intValue.ToString());

      string propName = flagSettings.name;
      if (string.IsNullOrEmpty(propName))
        propName = ObjectNames.NicifyVariableName(property.name);

      EditorGUI.BeginProperty(position, label, property);
      Enum enumNew = EditorGUI.EnumFlagsField(position, propName, targetEnum);
      property.intValue = (int)Convert.ChangeType(enumNew, targetEnum.GetType());
      EditorGUI.EndProperty();
    }
  }
}