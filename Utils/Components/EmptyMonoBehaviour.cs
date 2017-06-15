using UnityEngine;

namespace Utils
{
  public class EmptyMonoBehaviour : MonoBehaviour
  {

    private void OnDestroy()
    {
      if (isActiveAndEnabled)
      {
        StopAllCoroutines();
      }
    }
  }
}
