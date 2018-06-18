using System;
using UnityEngine;

namespace Utils.Tweens
{
  public class TweenTransitions
  {
    public static float Linear(float ratio)
    {
      return ratio;
    }

    public static float ExpoOut(float ratio)
    {
      return 1f - Mathf.Pow(2f, -10f * ratio);
    }

    public static float EaseOutBack(float ratio)
    {
      var invfloat = ratio - 1.0;
      const double overshootOrAmplitude = 1.70158;
      return (float)(Math.Pow(invfloat, 2.0) * ((overshootOrAmplitude + 1.0) * invfloat + overshootOrAmplitude) + 1.0);
    }

    public static float BounceOut(float ratio)
    {
      if (ratio < 1f / 2.75f)
      {
        return 7.5625f * ratio * ratio;
      }
      if (ratio < 2f / 2.75f)
      {
        return 7.5625f * (ratio -= 1.5f / 2.75f) * ratio + .75f;
      }
      if (ratio < 2.5f / 2.75f)
      {
        return 7.5625f * (ratio -= 2.25f / 2.75f) * ratio + .9375f;
      }
      return 7.5625f * (ratio -= 2.625f / 2.75f) * ratio + .984375f;
    }
  }
}
