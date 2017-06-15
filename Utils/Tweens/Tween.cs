using System;
using UnityEngine;

namespace Utils.Tweens
{
  public delegate float TweenTransition(float ratio);

  public class Tween
  {
    private TweenTransition _transition;
    private float _from;
    private float _to;
    private float _duration;
    private float _passedTime;

    public Tween(float from, float to, float duration, TweenTransition transition)
    {
      Reset(from, to, duration, transition);
    }

    public Tween Reset(float from, float to, float duration, TweenTransition transition)
    {
      if (_duration < 0f)
      {
        throw new ArgumentException("Duration can not be less than zero");
      }

      _from = from;
      _to = to;
      _duration = duration;
      _transition = transition;
      _passedTime = 0f;

      return this;
    }

    public float Value
    {
      get { return _from + (_to - _from) * ValueRatio; }
    }

    public float Ratio
    {
      get
      {
        if (Math.Abs(_passedTime - _duration) <= float.Epsilon)
        {
          return 1f;
        }
        return _passedTime / _duration;
      }
    }

    public bool IsComplete
    {
      get { return Math.Abs(Ratio - 1f) <= float.Epsilon; }
    }

    public float From
    {
      get { return _from; }
    }

    public float To
    {
      get { return _to; }
    }

    public float Duration
    {
      get { return _duration; }
    }

    public float PassedTime
    {
      get { return _passedTime; }
      set
      {
        _passedTime = Mathf.Clamp(value, 0, _duration);
      }
    }

    public void AdvanceTime(float deltaTime)
    {
      _passedTime += deltaTime;
      _passedTime = Math.Min(_passedTime, _duration);
    }

    private float ValueRatio
    {
      get
      {
        if (Math.Abs(_passedTime - _duration) <= float.Epsilon)
        {
          return Calculate(1f);
        }
        return Calculate(_passedTime / _duration);
      }
    }

    private float Calculate(float ratio)
    {
      return _transition(ratio);
    }
  }
}
