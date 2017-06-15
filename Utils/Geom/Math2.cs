using System;

namespace Utils
{
  public class Math2
  {
    public const float PIf = (float)Math.PI;

    public static double DegToRad(double angle)
    {
      return Math.PI * angle / 180.0;
    }

    public static float DegToRad(float angle)
    {
      return PIf * angle / 180.0f;
    }

    public static double RadToDeg(double angle)
    {
      return angle * (180.0 / Math.PI);
    }

    public static float RadToDeg(float angle)
    {
      return angle * (180.0f / PIf);
    }

    public static int CeilToInt(double value)
    {
      return (int)Math.Ceiling(value);
    }

    public static bool InRange(int value, int min, int max)
    {
      return value >= min && value <= max;
    }

    public static int Sqr(int value)
    {
      return value * value;
    }

    public static double Sqr(double value)
    {
      return value * value;
    }

    public static int EnsureRange(int value, int min, int max)
    {
      if (min > max) throw new ArgumentException();

      if (value < min)
        value = min;
      if (value > max)
        value = max;
      return value;
    }

    public static double Clamp(double value)
    {
      if (value < 0.0) return 0.0;
      if (value > 1.0) return 1.0;
      return value;
    }

    public static int Clamp(int value, int min, int max)
    {
      if (min > max) throw new ArgumentException();

      if (value < min)
        value = min;
      if (value > max)
        value = max;
      return value;
    }

    public static double Clamp01(double value)
    {
      if (value < 0.0) return 0.0;
      if (value > 1.0) return 1.0;
      return value;
    }
  }
}
