using UnityEngine;

namespace Utils
{
  public static class Vector3Extension
  {
    public static DVector3 XFrom(this DVector3 target, DVector3 from)
    {
      target.x = from.x;
      return target;
    }

    public static DVector3 SetX(this DVector3 target, float x)
    {
      target.x = x;
      return target;
    }

    public static DVector3 YFrom(this DVector3 target, DVector3 from)
    {
      target.y = from.y;
      return target;
    }

    public static DVector3 SetY(this DVector3 target, float y)
    {
      target.y = y;
      return target;
    }

    public static DVector3 ZFrom(this DVector3 target, DVector3 from)
    {
      target.z = from.z;
      return target;
    }

    public static DVector3 SetZ(this DVector3 target, float z)
    {
      target.z = z;
      return target;
    }

    public static Vector2 ToVector2FromXZ(this DVector3 target)
    {
      return new Vector2((float)target.x, (float)target.z);
    }

    public static Vector3 XFrom(this Vector3 target, Vector3 from)
    {
      target.x = from.x;
      return target;
    }

    public static Vector3 SetX(this Vector3 target, float x)
    {
      target.x = x;
      return target;
    }

    public static Vector3 YFrom(this Vector3 target, Vector3 from)
    {
      target.y = from.y;
      return target;
    }

    public static Vector3 SetY(this Vector3 target, float y)
    {
      target.y = y;
      return target;
    }

    public static Vector3 ZFrom(this Vector3 target, Vector3 from)
    {
      target.z = from.z;
      return target;
    }

    public static Vector3 SetZ(this Vector3 target, float z)
    {
      target.z = z;
      return target;
    }

    public static Vector2 ToVector2FromXZ(this Vector3 target)
    {
      return new Vector2(target.x, target.z);
    }
  }
}
