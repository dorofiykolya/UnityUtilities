using System.Linq;
using Utils.Editor;

namespace UnityEditor
{
  public class Editor<T> : Editor where T : UnityEngine.Object
  {
    public T Target
    {
      get { return (T)target; }
    }

    public T[] Targets
    {
      get { return targets.Cast<T>().ToArray(); }
    }

    protected bool BeginFade(bool opened, string header)
    {
      var result = EditorUtils.FoldoutHeader(header, opened, 0f);
      EditorUtils.BeginFade(this, result);
      return result;
    }

    protected void EndFade()
    {
      EditorUtils.EndFade(this);
    }

    private void OnDestroy()
    {
      EditorUtils.DisposeFade(this);
    }
  }
}
