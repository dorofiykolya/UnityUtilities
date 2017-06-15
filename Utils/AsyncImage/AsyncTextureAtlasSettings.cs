using System;

namespace UnityEngine
{
  public class AsyncTextureAtlasSettings : ScriptableObject
  {
    public Texture2D Texture2D;
    public Sprite[] Sprites;
    public bool DoNotUnload;

    [ContextMenu("Sort Sprites")]
    private void SortSprites()
    {
      Array.Sort(Sprites, (item1, item2) => item1.name.CompareTo(item2.name));
    }
  }
}
