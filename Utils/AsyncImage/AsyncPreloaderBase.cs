using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine
{
  public abstract class AsyncPreloaderBase : MonoBehaviour, IAsyncImagePreloader
  {
    public virtual void Play(bool startFrame = false)
    {
      
    }

    public virtual void SetData(AsyncPreloaderData data)
    {
      
    }

    public virtual void SetTarget(Component graphic)
    {
      
    }

    public virtual void Stop()
    {
      
    }
  }
}
