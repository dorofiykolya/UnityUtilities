using UnityEngine;

namespace Utils
{
  public static class GameObjectExtension
  {
    public static void AddChild(this GameObject parent, GameObject child)
    {
      child.transform.SetParent(parent.transform, false);
    }

    public static void AddChild(this GameObject parent, Transform child)
    {
      child.SetParent(parent.transform, false);
    }

    public static void AddChild(this Transform parent, Transform child)
    {
      child.SetParent(parent, false);
    }
  }
}
