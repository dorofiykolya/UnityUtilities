using UnityEditor;
using UnityEngine;
using Utils.Editor;

namespace Utils.AsyncImage.Editor
{
  [CustomEditor(typeof(UnityEngine.AsyncImage), true)]
  public class AsyncImageEditor : Editor<UnityEngine.AsyncImage>
  {
    private static bool _spriteAreaOpen;

    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();

      _spriteAreaOpen = BeginFade(_spriteAreaOpen, "Sprite");
      EditorUtils.BeginVertical();
      EditorGUILayout.LabelField("Name:", Target.Sprite != null ? Target.Sprite.Name : "null");
      EditorGUILayout.LabelField("IsReady:", (Target.Sprite != null && Target.Sprite.IsReady).ToString());
      EditorGUILayout.ObjectField("Sprite:", Target.Sprite != null ? Target.Sprite.Sprite : null, typeof(Sprite), true);
      EditorUtils.EndVertical();
      EndFade();
    }
  }
}
