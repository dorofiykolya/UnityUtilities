using System.Collections;
using UnityEngine;

namespace Utils
{
  public class CoroutineProvider : ICoroutineProvider
  {
    private readonly MonoBehaviour _monoBehaviour;

    public CoroutineProvider(MonoBehaviour monoBehaviour)
    {
      _monoBehaviour = monoBehaviour;
    }

    public Coroutine StartCoroutine(IEnumerator enumerator)
    {
      var result = _monoBehaviour.StartCoroutine(enumerator);
      return result;
    }

    public void StopCoroutine(Coroutine coroutine)
    {
      _monoBehaviour.StopCoroutine(coroutine);
    }
  }
}
