using UnityEngine.UI;
using Utils;

namespace UnityEngine
{
  [ExecuteInEditMode]
  //[RequireComponent(typeof(Graphic))]
  public class AsyncImage : MonoBehaviour
  {
    public bool spriteOverride;
    [SerializeField]
    private Component _graphic;
    [SerializeField]
    private AsyncPreloaderBase _preloader;
    [SerializeField]
    private bool _disablePreloader;
    [SerializeField]
    private bool _setNativeSize = false;
    [SerializeField]
    private bool _nullIsTransparent = false;

    private AsyncSprite _sprite;
    private bool _enabled;
    private bool _preloaderEnabled;
    private bool _isApplicationQuited;
    private Lifetime.Definition _loadDefinition;
    private Lifetime.Definition _unloadDefinition;
    private GameObject _preloaderObject;


    public AsyncSprite Sprite
    {
      get { return _sprite; }
      set { SetSprite(value); }
    }

    public bool Preloader
    {
      get
      {
        return _preloaderEnabled;
      }
      set
      {
        if (_preloaderEnabled != value)
        {
          _preloaderEnabled = value;
          UpdatePreloader();
        }
      }
    }

    public Component Graphic
    {
      get { return _graphic; }
      set { _graphic = value; }
    }

    private void Awake()
    {
      _isApplicationQuited = false;

      if (_graphic == null)
      {
        _graphic = GetComponent<Graphic>();
      }
      if (_graphic == null)
      {
        _graphic = GetComponent<SpriteRenderer>();
      }
      SetGraphicEnable(_sprite != null && _sprite.Sprite != null);
    }

    private void OnEnable()
    {
      _enabled = true;
      Retain();
    }

    private void OnDisable()
    {
      _enabled = false;
      Release();
    }

    private void OnDestroy()
    {
      if (!_isApplicationQuited)
      {
        SetSprite((AsyncSprite)null);
      }
    }

    private void SetSprite(AsyncSprite sprite)
    {
      SetGraphicEnable(sprite != null && sprite.Sprite != null);
      if (!ReferenceEquals(_sprite, sprite))
      {
        if (_sprite != null)
        {
          TerminateLoadDefinition();
          TerminateUnloadDefinition();
          Release();
          _sprite = null;
        }
        if (sprite != null)
        {
          _preloaderEnabled = sprite.PreloaderData != null && sprite.PreloaderData.HasData;
          _sprite = sprite;

          _sprite.SubscribeOnUnloaded(DefineUnloadDefinition(), SpriteOnUnloaded);

          Retain();
        }
        else
        {
          SetImage(null);
        }
      }
    }

    private void SetGraphicEnable(bool enabled)
    {
      if (_graphic is Image) (_graphic as Image).enabled = enabled;
    }

    private void Release()
    {
      if (_sprite != null)
      {
        //SetImage(null);
        _sprite.Release(this);
      }
    }

    private void Retain()
    {
      if (_enabled)
      {
        if (_sprite != null)
        {
          _sprite.Retain(this);
          if (_sprite.IsReady)
          {
            SetImage(_sprite);
          }
          else
          {
            TerminateLoadDefinition();
            _sprite.SubscribeOnLoaded(DefineLoadDefiniton(), SpriteOnLoaded);
          }
        }
        SetGraphicEnable(_sprite != null && _sprite.Sprite != null);
        UpdatePreloader();
      }
    }

    private void SetImage(AsyncSprite sprite)
    {
      if (sprite != null)
      {
        SetSprite(sprite.Sprite);
      }
      else
      {
        SetSprite((Sprite)null);
      }
    }

    private void SetSprite(Sprite sprite)
    {
      SetGraphicEnable(sprite != null);
      AsyncImageUtils.SetSprite(_graphic, sprite, spriteOverride, _nullIsTransparent);
      if (_setNativeSize && sprite != null)
      {
        var graphic = _graphic as Graphic;
        if (graphic != null)
        {
          graphic.SetNativeSize();
        }
      }
    }

    private void SpriteOnUnloaded(AsyncSprite asyncSprite)
    {
      //SetImage(null);
    }

    private void SpriteOnLoaded(AsyncSprite sprite)
    {
      if (_enabled)
      {
        SetImage(sprite);
      }
      UpdatePreloader();
    }

    private void UpdatePreloader()
    {
      if (!_disablePreloader)
      {
        if (_preloaderEnabled && _sprite != null && !_sprite.IsReady)
        {
          AsyncImageUtils.GetPreloader(ref _preloader, gameObject, true, _sprite.PreloaderData != null ? _sprite.PreloaderData.GetPreloaderType(_sprite) : typeof(AsyncImagePreloader));
          _preloader.SetData(_sprite.PreloaderData);
          _preloader.SetTarget(_graphic);
          _preloader.Play(true);
          SetGraphicEnable(true);
        }
        else if (!_preloaderEnabled || (_sprite != null && _sprite.IsReady))
        {
          AsyncImageUtils.GetPreloader(ref _preloader, gameObject, false, _sprite != null && _sprite.PreloaderData != null ? _sprite.PreloaderData.GetPreloaderType(_sprite) : typeof(AsyncImagePreloader));
          if (_preloader != null)
          {
            _preloader.Stop();
          }
        }
      }
    }

    private void OnApplicationQuit()
    {
      _isApplicationQuited = true;
    }

    private Lifetime DefineLoadDefiniton()
    {
      return _loadDefinition != null ? _loadDefinition.Lifetime : (_loadDefinition = Lifetime.Define(Lifetime.Eternal)).Lifetime;
    }

    private void TerminateLoadDefinition()
    {
      if (_loadDefinition != null)
      {
        var def = _loadDefinition;
        _loadDefinition = null;
        def.Terminate();
      }
    }

    private Lifetime DefineUnloadDefinition()
    {
      return _unloadDefinition != null ? _unloadDefinition.Lifetime : (_unloadDefinition = Lifetime.Define(Lifetime.Eternal)).Lifetime;
    }

    private void TerminateUnloadDefinition()
    {
      if (_unloadDefinition != null)
      {
        var def = _unloadDefinition;
        _unloadDefinition = null;
        def.Terminate();
      }
    }
  }
}
