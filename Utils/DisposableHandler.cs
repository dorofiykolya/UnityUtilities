using System;

namespace Utils
{
  public class DisposableHandler : IDisposable
  {
    private Action _onDispose;

    public DisposableHandler(Action onDispose)
    {
      _onDispose = onDispose;
    }

    public void Dispose()
    {
      if (_onDispose != null)
      {
        var dispose = _onDispose;
        _onDispose = null;
        dispose();
      }
    }
  }
}
