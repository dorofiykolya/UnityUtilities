using UnityEngine;

namespace Utils
{
  public class TransformBehaviour : MonoBehaviour
  {
    private Transform _transform;

    new public Transform transform { get { return _transform ?? (_transform = base.transform); } }
  }
}