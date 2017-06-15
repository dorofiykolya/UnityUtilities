using System.Collections;
using UnityEngine;

namespace Utils
{
  public interface ICoroutineProvider
  {
    Coroutine StartCoroutine(IEnumerator enumerator);
    void StopCoroutine(Coroutine coroutine);
  }
}
