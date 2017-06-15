using System;

namespace Utils.Tweens
{
  public class TweenTransitions
  {
    public static float Linear(float ratio)
    {
      return ratio;
    }

    public static float EaseOutBack(float ratio)
    {
      var invfloat = ratio - 1.0;
      const double overshootOrAmplitude = 1.70158;
      return (float)(Math.Pow(invfloat, 2.0) * ((overshootOrAmplitude + 1.0) * invfloat + overshootOrAmplitude) + 1.0);
    }
  }
}
