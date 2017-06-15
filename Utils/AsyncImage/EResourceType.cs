using System;
using UnityEngine;

namespace Utils.AsyncImage
{
  public enum BundleResourceType
  {
    Sprite = 0,
    AtlasSettings = 1
  }

  public static class AsyncResourceType
  {
    public static Type GetType(BundleResourceType resourceType)
    {
      Type resultType;
      switch (resourceType)
      {
        case BundleResourceType.Sprite:
          resultType = typeof(Sprite);
          break;
        case BundleResourceType.AtlasSettings:
          resultType = typeof(AsyncTextureAtlasSettings);
          break;
        default:
          resultType = typeof(UnityEngine.Object);
          break;
      }
      return resultType;
    }
  }
}
