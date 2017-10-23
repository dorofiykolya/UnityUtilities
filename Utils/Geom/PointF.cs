using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
  [Serializable]
  public class PointF : IEquatable<PointF>, IComparable<PointF>, ICloneable
  {
    private static readonly Stack<PointF> Pool = new Stack<PointF>(128);

    public static PointF Pop()
    {
      if (Pool.Count == 0) return new PointF();
      return Pool.Pop();
    }

    public static PointF Pop(float x, float y)
    {
      if (Pool.Count == 0) return new PointF(x, y);
      return Pool.Pop().SetTo(x, y);
    }

    public static PointF Pop(Point point)
    {
      if (Pool.Count == 0) return new PointF(point.x, point.y);
      return Pool.Pop().SetTo(point.x, point.y);
    }

    public static void Push(PointF point)
    {
      if (!Pool.Contains(point))
      {
        Pool.Push(point);
      }
    }


    public static readonly PointF Zero = new PointF(0.0f, 0.0f);

    public float x;
    public float y;

    public PointF(float x = 0, float y = 0)
    {
      this.x = x;
      this.y = y;
    }

    public PointF(Point point)
    {
      x = point.x;
      y = point.y;
    }

    public PointF(PointF point)
    {
      x = point.x;
      y = point.y;
    }

    public static implicit operator PointF(Point point)
    {
      return new PointF(point.x, point.y);
    }

    public static implicit operator Point(PointF pointF)
    {
      if (pointF == null) return new Point();
      return new Point((int)pointF.x, (int)pointF.y);
    }

    public static implicit operator Vector2(PointF point)
    {
      if (point == null) return new Vector2();
      return new Vector2(point.x, point.y);
    }

    public static implicit operator PointF(Vector2 pointF)
    {
      return new PointF((int)pointF.x, (int)pointF.y);
    }

    public static float Distance(PointF point1, PointF point2)
    {
      var x = point1.x - point2.x;
      var y = point1.y - point2.y;
      if (x < 0f) x = -x;
      if (y < 0f) y = -y;
      return Mathf.Sqrt(x * x + y * y);
    }

    public static float Distance(PointF point1, Point point2)
    {
      var x = point1.x - point2.x;
      var y = point1.y - point2.y;
      if (x < 0f) x = -x;
      if (y < 0f) y = -y;
      return Mathf.Sqrt(x * x + y * y);
    }

    public static float Distance(Point point1, PointF point2)
    {
      var x = point1.x - point2.x;
      var y = point1.y - point2.y;
      if (x < 0f) x = -x;
      if (y < 0f) y = -y;
      return Mathf.Sqrt(x * x + y * y);
    }

    public static float Distance(Point point1, Point point2)
    {
      var x = point1.x - point2.x;
      var y = point1.y - point2.y;
      if (x < 0f) x = -x;
      if (y < 0f) y = -y;
      return Mathf.Sqrt(x * x + y * y);
    }

    public static float Distance(ref PointF point1, ref PointF point2)
    {
      var x = point1.x - point2.x;
      var y = point1.y - point2.y;
      if (x < 0f) x = -x;
      if (y < 0f) y = -y;
      return Mathf.Sqrt(x * x + y * y);
    }

    public static PointF operator +(PointF s1, PointF s2)
    {
      return new PointF(s1.x + s2.x, s1.y + s2.y);
    }

    public static PointF operator -(PointF s1, PointF s2)
    {
      return new PointF(s1.x - s2.x, s1.y - s2.y);
    }

    public static PointF operator *(PointF s1, PointF s2)
    {
      return new PointF(s1.x * s2.x, s1.y * s2.y);
    }

    public static PointF operator *(PointF s1, float s2)
    {
      return new PointF(s1.x * s2, s1.y * s2);
    }

    public static PointF operator *(PointF s1, int s2)
    {
      return new PointF(s1.x * s2, s1.y * s2);
    }

    public static PointF operator /(PointF s1, PointF s2)
    {
      return new PointF(s1.x / s2.x, s1.y / s2.y);
    }

    public static PointF operator /(PointF s1, float s2)
    {
      return new PointF(s1.x / s2, s1.y / s2);
    }

    public static PointF operator /(PointF s1, int s2)
    {
      return new PointF(s1.x / s2, s1.y / s2);
    }

    public static bool operator ==(PointF s1, PointF s2)
    {
      if (ReferenceEquals(s1, s2)) return true;
      if (ReferenceEquals(s1, null)) return false;
      if (ReferenceEquals(null, s2)) return false;
      return s1.Equals(ref s2);
    }

    public static bool operator !=(PointF s1, PointF s2)
    {
      if (ReferenceEquals(s1, s2)) return false;
      if (ReferenceEquals(s1, null)) return true;
      if (ReferenceEquals(null, s2)) return true;
      return !s1.Equals(ref s2);
    }

    public PointF SetTo(float x = 0.0f, float y = 0.0f)
    {
      this.x = x;
      this.y = y;
      return this;
    }

    public PointF SetTo(PointF point)
    {
      x = point.x;
      y = point.y;
      return this;
    }

    public PointF CopyFrom(PointF point)
    {
      x = point.x;
      y = point.y;
      return this;
    }

    public PointF CopyFrom(Point point)
    {
      x = point.x;
      y = point.y;
      return this;
    }

    public void Add(PointF point)
    {
      x += point.x;
      y += point.y;
    }

    public void Remove(PointF point)
    {
      x -= point.x;
      y -= point.y;
    }

    public bool IsNaN
    {
      get { return float.IsNaN(x) || float.IsNaN(y); }
    }

    public float Length
    {
      get { return (float)Math.Sqrt(x * x + y * y); }
    }

    public bool Equals(ref PointF other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return x.Equals(other.x) && y.Equals(other.y);
    }

    public bool Equals(PointF other)
    {
      return other.x == x && other.y == y;
    }

    public override bool Equals(object other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      if (other.GetType() != this.GetType()) return false;
      return Equals((PointF)other);
    }

    public bool Equals(PointF other, float threshold)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      if (other.GetType() != this.GetType()) return false;
      if (threshold < 0) threshold = -threshold;
      return Mathf.Abs(x - other.x) <= threshold && Mathf.Abs(y - other.y) <= threshold;
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (x.GetHashCode() * 397) ^ y.GetHashCode();
      }
    }

    public int CompareTo(PointF other)
    {
      if (Length > other.Length) return 1;
      return -1;
    }

    public override string ToString()
    {
      return String.Format("[PointF(x:{0}, y:{1})]", x, y);
    }

    public PointF Copy()
    {
      return new PointF(x, y);
    }

    public object Clone()
    {
      return new PointF(x, y);
    }
  }
}