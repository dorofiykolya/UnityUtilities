using UnityEngine.Assertions;
using UnityEngine.UI;

namespace UnityEngine
{
  public static class AsyncImageUtils
  {
    public static void Set(this Image image, AsyncSprite sprite, bool @override = false)
    {
      SetSprite(image, sprite, @override);
    }

    //public static void Set(this Graphic image, AsyncSprite sprite, bool @override = false)
    //{
    //  SetSprite(image, sprite);
    //}

    public static void SetSprite(Image image, AsyncSprite sprite, bool @override = false)
    {
      if (image == null && !Application.isEditor) return;

      var asyncImage = image.GetComponent<AsyncImage>();
      if (asyncImage == null)
      {
        asyncImage = image.gameObject.AddComponent<AsyncImage>();
      }
      asyncImage.Sprite = sprite;
      asyncImage.spriteOverride = @override;
    }

    public static void SetSprite(Graphic graphic, AsyncSprite sprite, bool @override = false)
    {
      if (graphic == null && !Application.isEditor) return;

      var asyncImage = graphic.GetComponent<AsyncImage>();
      if (asyncImage == null)
      {
        asyncImage = graphic.gameObject.AddComponent<AsyncImage>();
      }
      asyncImage.Sprite = sprite;
      asyncImage.spriteOverride = @override;
    }

    public static void SetSprite(Component graphic, Sprite sprite, bool @override = false, bool nullIsTransparent = false)
    {
      if (graphic == null && !Application.isEditor) return;

      var image = graphic as Image;
      if (image != null)
      {
        if (@override)
        {
          image.overrideSprite = sprite;
        }
        else
        {
          image.sprite = sprite;
        }
        if (nullIsTransparent)
        {
          var c = image.color;
          c.a = sprite == null ? 0 : 1f;
          image.color = c;
        }
      }
      else
      {
        var spriteRenderer = graphic as SpriteRenderer;
        if (spriteRenderer != null)
        {
          spriteRenderer.sprite = sprite;
        }
      }
    }

    public static void SetSprite(Graphic graphic, Sprite sprite, bool @override = false)
    {
      if (graphic == null && !Application.isEditor) return;

      var image = graphic as Image;
      if (image != null)
      {
        if (@override)
        {
          image.overrideSprite = sprite;
        }
        else
        {
          image.sprite = sprite;
        }
      }
    }

    public static void SetRef(ref Image image, Graphic graphic)
    {
      var imageResult = graphic as Image;
      if (imageResult != null)
      {
        image = imageResult;
      }
    }

    public static Component GetGraphic(GameObject gameObject)
    {
      Component result = gameObject.GetComponent<Graphic>();
      if (result == null)
      {
        result = gameObject.GetComponent<SpriteRenderer>();
      }
      return result;
    }

    public static void GetPreloader(ref AsyncPreloaderBase preloader, GameObject gameObject, bool attachIfEmpty, System.Type preloaderType)
    {
      Assert.IsTrue(typeof(AsyncPreloaderBase).IsAssignableFrom(preloaderType));
      if (preloader == null)
      {
        preloader = gameObject.GetComponent<AsyncPreloaderBase>();
      }
      if (preloader == null && attachIfEmpty)
      {
        preloader = gameObject.AddComponent(preloaderType) as AsyncPreloaderBase;
      }
    }

    public static AsyncSprite GetSprite(Graphic graphic)
    {
      var asyncImage = graphic.GetComponent<AsyncImage>();
      if (asyncImage != null)
      {
        return asyncImage.Sprite;
      }
      return null;
    }

    public static AsyncSprite GetSprite(Image image)
    {
      var asyncImage = image.GetComponent<AsyncImage>();
      if (asyncImage != null)
      {
        return asyncImage.Sprite;
      }
      return null;
    }
  }
}
