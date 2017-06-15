using UnityEngine;

public struct DVector3
{
  public static DVector3 operator +(DVector3 left, DVector3 right)
  {
    return new DVector3(left.x + right.x, left.y + right.y, left.z + right.z);
  }

  public static DVector3 operator -(DVector3 left, DVector3 right)
  {
    return new DVector3(left.x - right.x, left.y - right.y, left.z - right.z);
  }

  public static DVector3 operator *(DVector3 left, double right)
  {
    return new DVector3(left.x * right, left.y * right, left.z * right);
  }

  public static DVector3 operator /(DVector3 left, double right)
  {
    return new DVector3(left.x / right, left.y / right, left.z / right);
  }

  public static implicit operator Vector3(DVector3 vector)
  {
    return new Vector3((float)vector.x, (float)vector.y, (float)vector.z);
  }

  public static implicit operator DVector3(Vector3 vector)
  {
    return new DVector3(vector.x, vector.y, vector.z);
  }

  public double x;
  public double y;
  public double z;

  public DVector3(double x, double y, double z)
  {
    this.x = x;
    this.y = y;
    this.z = z;
  }

  public DVector3 AsInteger()
  {
    return new DVector3((int)x, (int)y, (int)z);
  }

  public DVector3 AsDecimal()
  {
    int ix = (int)x;
    int iy = (int)y;
    int iz = (int)z;
    return new DVector3(x - ix, y - iy, z - iz);
  }

  public override string ToString()
  {
    return string.Format("{0}, {1}, {2}", x, y, z);
  }

  public static DVector3 Lerp(DVector3 a, DVector3 b, double t)
  {
    t = Clamp01(t);
    return new DVector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
  }

  public static double Clamp01(double value)
  {
    if (value < 0.0) return 0.0;
    if (value > 1.0) return 1.0;
    return value;
  }

  public Vector3 ToVector3()
  {
    return (Vector3)this;
  }
}
