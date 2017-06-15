namespace UnityEngine
{
  public interface IAsyncImagePreloader
  {
    void Play(bool startFrame = false);
    void Stop();
    void SetData(AsyncPreloaderData data);
    void SetTarget(Component graphic);
  }
}
