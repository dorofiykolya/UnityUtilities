using UnityEngine.UI;

namespace UnityEngine
{
  [ExecuteInEditMode]
  //[RequireComponent(typeof(Graphic))]
  public class AsyncImagePreloader : AsyncPreloaderBase
  {
    [SerializeField]
    private Component _graphic;

    public Sprite[] Sprites;
    public float Fps = 12;
    public bool IsPlay;

    [SerializeField]
    private int _currentFrame;

    [SerializeField]
    private bool _releaseOnDisable = true;

    private Sprite _currentSprite;
    private float _passedTime;

    public override void Play(bool startFrame = false)
    {
      IsPlay = true;
      if (startFrame)
      {
        _currentFrame = 0;
        if (_currentFrame >= 0 && _currentFrame < Sprites.Length)
        {
          SetTexture(Sprites[_currentFrame], true);
        }
      }
    }

    public override void Stop()
    {
      IsPlay = false;
    }

    public override void SetData(AsyncPreloaderData data)
    {
      if (Sprites == null || Sprites.Length == 0)
      {
        Sprites = data.Sprites;
        Fps = data.Fps;
      }
    }

    public override void SetTarget(Component graphic)
    {
      _graphic = graphic;
    }

    private void SetTexture(Sprite sprite, bool force = false)
    {
      if (sprite != null && (_currentSprite != sprite || force))
      {
        _currentSprite = sprite;
        AsyncImageUtils.SetSprite(_graphic, sprite, false);
      }
    }

    private void Awake()
    {
      _graphic = AsyncImageUtils.GetGraphic(gameObject);
    }

    private void Update()
    {
      if (IsPlay)
      {
        _passedTime += Time.deltaTime * Fps;
        int count = (int)_passedTime;
        if (count > 0)
        {
          _currentFrame++;
          _passedTime -= count;
        }
        if (Sprites != null)
        {
          if (_currentFrame < 0) _currentFrame = 0;
          if (_currentFrame >= Sprites.Length)
          {
            _currentFrame = 0;
          }
          if (_currentFrame >= 0 && _currentFrame < Sprites.Length)
          {
            SetTexture(Sprites[_currentFrame]);
          }
          if (_currentFrame >= Sprites.Length)
          {
            _currentFrame = 0;
          }
        }
      }
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {
      if (_releaseOnDisable)
      {
        AsyncImageUtils.SetSprite(_graphic, (Sprite)null, false);
      }
    }

    private void OnDestroy()
    {

    }
  }
}
