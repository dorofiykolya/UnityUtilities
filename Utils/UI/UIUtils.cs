using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Utils
{
  public class UIUtils
  {    
    public static void AddClickListener(Button button, UnityAction listener)
    {
      if (button != null)
      {
        button.onClick.AddListener(listener);
      }
    }

    public static void RemoveClickListener(Button button, UnityAction listener)
    {
      if (button != null)
      {
        button.onClick.RemoveListener(listener);
      }
    }

    public static void AddValueChangeListener(Slider slider, UnityAction<float> listener)
    {
      if (slider)
      {
        slider.onValueChanged.AddListener(listener);
      }
    }

    public static void RemoveValueChangeListener(Slider slider, UnityAction<float> listener)
    {
      if (slider)
      {
        slider.onValueChanged.RemoveListener(listener);
      }
    }

    public static void AddToggleListener(Toggle toggle, UnityAction<bool> listener)
    {
      if (toggle)
      {
        toggle.onValueChanged.AddListener(listener);
      }
    }

    public static void RemoveValueChangeListener(Toggle toggle, UnityAction<bool> listener)
    {
      if (toggle)
      {
        toggle.onValueChanged.RemoveListener(listener);
      }
    }

    public static void FindAndSet<T>(ref T component, GameObject gameObject) where T : Component
    {
      if (component == null)
      {
        component = gameObject.GetComponent<T>();
        if (component == null)
        {
          component = gameObject.GetComponentInChildren<T>();
        }
      }
    }

    public static void SetText(Text textField, string text)
    {
      if (textField != null) textField.text = text;
    }

    public static string GetText(Text textField, string defaultValue = null)
    {
      if (textField != null) return textField.text;
      return defaultValue;
    }

    public static void SetActive(GameObject gameObject, bool active)
    {
      if (gameObject != null) gameObject.SetActive(active);
    }

    public static Sprite GetSprite(Graphic graphic)
    {
      var image = graphic as Image;
      if (image != null)
      {
        return image.sprite;
      }
      return null;
    }

    public static void SetSprite(Graphic graphic, Sprite sprite)
    {
      var image = graphic as Image;
      if (image != null)
      {
        image.sprite = sprite;
      }
    }
  }
}
