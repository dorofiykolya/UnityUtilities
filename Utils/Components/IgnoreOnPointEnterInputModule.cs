using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utils.Components
{
  public class IgnoreOnPointEnterInputModule : StandaloneInputModule
  {
    public override bool IsPointerOverGameObject(int pointerId)
    {
      PointerEventData pointerEventData = this.GetLastPointerEventData(pointerId);
      if (pointerEventData != null)
        if ((UnityEngine.Object)pointerEventData.pointerEnter != (UnityEngine.Object)null)
        {
          var ignore = pointerEventData.pointerEnter.GetComponent<IgnoreOnPointerEnter>();
          if (ignore)
          {
            return !ignore.ignore;
          }

          return true;
        }
      return false;
    }
  }
}
