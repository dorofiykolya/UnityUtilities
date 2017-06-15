using System;
using UnityEngine;

[Serializable]
public struct Point : IEquatable<Point>, IComparable<Point>, ICloneable
{
  public static readonly Point Zero = new Point(0, 0);
  public static readonly Point One = new Point(1, 1);

  public int x;
  public int y;

  public Point(int x = 0, int y = 0)
  {
    this.x = x;
    this.y = y;
  }

  public Point(Point point)
  {
    x = point.x;
    y = point.y;
  }

  public static implicit operator Vector2(Point point)
  {
    return new Vector2(point.x, point.y);
  }

  public static implicit operator Point(Vector2 pointF)
  {
    return new Point((int)pointF.x, (int)pointF.y);
  }

  public static float Distance(Point point1, Point point2)
  {
    var x = point1.x - point2.x;
    var y = point1.y - point2.y;
    if (x < 0) x = -x;
    if (y < 0) y = -y;
    return Mathf.Sqrt(x * x + y * y);
  }

  public static float Distance(ref Point point1, ref Point point2)
  {
    var x = point1.x - point2.x;
    var y = point1.y - point2.y;
    if (x < 0) x = -x;
    if (y < 0) y = -y;
    return Mathf.Sqrt(x * x + y * y);
  }

  public static Point operator -(Point s1, Point s2)
  {
    return new Point(s1.x - s2.x, s1.y - s2.y);
  }

  public static Point operator *(Point s1, Point s2)
  {
    return new Point(s1.x * s2.x, s1.y * s2.y);
  }

  public static Point operator *(Point s1, float s2)
  {
    return new Point((int)(s1.x * s2), (int)(s1.y * s2));
  }

  public static Point operator *(Point s1, int s2)
  {
    return new Point(s1.x * s2, s1.y * s2);
  }

  public static Point operator /(Point s1, Point s2)
  {
    return new Point(s1.x / s2.x, s1.y / s2.y);
  }

  public static Point operator /(Point s1, float s2)
  {
    return new Point((int)(s1.x / s2), (int)(s1.y / s2));
  }

  public static Point operator /(Point s1, int s2)
  {
    return new Point(s1.x / s2, s1.y / s2);
  }

  public static bool operator ==(Point s1, Point s2)
  {
    return s1.Equals(ref s2);
  }

  public static bool operator !=(Point s1, Point s2)
  {
    return !s1.Equals(ref s2);
  }

  public static Point operator +(Point s1, Point s2)
  {
    return new Point(s1.x + s2.x, s1.y + s2.y);
  }

  public void SetTo(int x, int y)
  {
    this.x = x;
    this.y = y;
  }
  public float Length
  {
    get { return Mathf.Sqrt(x * x + y * y); }
  }

  public int SqrLength
  {
    get { return x * x + y * y; }
  }

  public bool Equals(ref Point other)
  {
    return x == other.x && y == other.y;
  }

  public bool Equals(Point other)
  {
    return other.x == x && other.y == y;
  }

  public bool Equals(int x, int y)
  {
    return this.x == x && this.y == y;
  }

  public override bool Equals(object other)
  {
    if (ReferenceEquals(null, other)) return false;
    return other is Point && Equals((Point)other);
  }

  public override int GetHashCode()
  {
    unchecked
    {
      return (x * 397) ^ y;
    }
  }

  public int CompareTo(Point other)
  {
    if (Length > other.Length) return 1;
    return -1;
  }

  public override string ToString()
  {
    return string.Format("[Point(x:{0}, y:{1})]", x, y);
  }

  /// <summary>
  /// format
  /// </summary>
  /// <param name="format">"xCoord:{x}, yCoord:{y}"</param>
  /// <returns></returns>
  public string ToString(string format)
  {
    return format.Replace("{x}", x.ToString()).Replace("{y}", y.ToString());
  }

  public object Clone()
  {
    return new Point(x, y);
  }

  public Point Copy()
  {
    return new Point(x, y);
  }

  public void CopyFrom(PointF point)
  {
    x = (int)point.x;
    y = (int)point.y;
  }

  public void CopyFrom(Point point)
  {
    x = point.x;
    y = point.y;
  }

  public void CopyFrom(ref Point point)
  {
    x = point.x;
    y = point.y;
  }

  public int Hash { get { return x + y; } }

  public static float Angle(Point from, Point to)
  {
    return Mathf.Atan2(to.y - from.y, to.x - from.x);
  }

  public static float Angle(Vector2 from, Vector2 to)
  {
    return Mathf.Atan2(to.y - from.y, to.x - from.x);
  }

  public static float Angle(Point from, Vector2 to)
  {
    return Mathf.Atan2(to.y - from.y, to.x - from.x);
  }

  public static float Angle(Vector2 from, Point to)
  {
    return Mathf.Atan2(to.y - from.y, to.x - from.x);
  }

  public Point Offset(int xOffset, int yOffset)
  {
    x += xOffset;
    y += yOffset;
    return this;
  }
}

