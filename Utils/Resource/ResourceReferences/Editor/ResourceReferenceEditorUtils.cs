using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace References.Editor
{
  public class ResourceReferenceEditorUtils
  {
    public static string GetResourcePath(Object value)
    {
      var path = AssetDatabase.GetAssetPath(value);
      if (path != null)
      {
        var index = path.LastIndexOf("Resources");
        if (index != -1)
        {
          var resourcePath = path.Remove(0, index + "Resources".Length);
          resourcePath = Path.ChangeExtension(resourcePath, "").Replace('\\', '/').Trim('.', '/', '\\');
          return resourcePath;
        }
      }
      return null;
    }
  }

  [CustomPropertyDrawer(typeof(ResourceReference), true)]
  public class ResourceReferenceDrawer : PropertyDrawer
  {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      var serializedProperty = property.FindPropertyRelative("_path");
      var path = serializedProperty.stringValue;
      var serializedType = property.FindPropertyRelative("_serializedType");
      var type = GetResourceType(serializedType);
      if (type == null) type = System.Type.GetType(serializedType.stringValue.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)[0].Trim());
      var lastValue = Resources.Load(path);
      label.text = label.text + string.Format(" [{0}]", string.IsNullOrEmpty(path) ? "null" : path);
      label.tooltip = label.text;
      var newValue = EditorGUI.ObjectField(position, label, lastValue, type, false);

      if ((lastValue != newValue && ResourceReferenceEditorUtils.GetResourcePath(newValue) != null) || newValue == null)
      {
        serializedProperty.stringValue = ResourceReferenceEditorUtils.GetResourcePath(newValue);
      }
    }

    private Type GetResourceType(SerializedProperty property)
    {
      var attributes = fieldInfo.GetCustomAttributes(typeof(ResourceReferenceTypeAttribute), true);
      var referenceTypeAttribute = (ResourceReferenceTypeAttribute)(attribute ?? (attributes.Length > 0 ? attributes[0] : null));
      if (referenceTypeAttribute != null && referenceTypeAttribute.Type != null)
      {
        return referenceTypeAttribute.Type;
      }

      try
      {
        var target = property.serializedObject.targetObject.GetType().GetField(property.name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.GetField | BindingFlags.SetField);
        if (target != null)
        {
          var obj = target.GetValue(property.serializedObject.targetObject);
          var typeField = typeof(ResourceReference).GetField("_type", BindingFlags.NonPublic | BindingFlags.Instance);
          if (typeField != null)
          {
            var type = typeField.GetValue(obj) as Type;
            if (type != null)
            {
              return type;
            }
          }
        }
      }
      catch (Exception e)
      {

      }

      return typeof(Object);
    }
  }
}
