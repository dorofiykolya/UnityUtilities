using System;
using UnityEngine;

namespace Utils
{
  public class AnimatorSpeedBehaviour : MonoBehaviour
  {
    private float _lastSpeed;

    public Animator[] Animators;
    public float Speed = 1f;

    private void Update()
    {
      if (Animators != null)
      {
        if (Math.Abs(_lastSpeed - Speed) > float.Epsilon)
        {
          _lastSpeed = Speed;
          foreach (var animator in Animators)
          {
            if (animator != null)
            {
              animator.speed = Speed;
            }
          }
        }
      }
    }
  }
}
