using UnityEngine;

namespace Utils
{
  public class RectTransformBehaviour : TransformBehaviour
  {
    private RectTransform _rectTransform;

    public RectTransform rectTransform
    {
      get
      {
        return _rectTransform ?? (_rectTransform = GetComponent<RectTransform>());
      }
    }
  }
}
