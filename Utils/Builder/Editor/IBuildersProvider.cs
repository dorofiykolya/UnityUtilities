﻿using System.Collections.Generic;
using UnityEditor;
using Utils.BuildPipeline.Builders;

namespace Utils.BuildPipeline
{
  public interface IBuildersProvider
  {
    IBuilder Get(BuildTarget target);
  }

  public class DefaultBuildersProvider : IBuildersProvider
  {
    private static readonly Dictionary<BuildTarget, IBuilder> Builders = new Dictionary<BuildTarget, IBuilder>
    {
      { BuildTarget.StandaloneWindows, new WindowsBuilder(BuildTarget.StandaloneWindows) },
      { BuildTarget.StandaloneWindows64, new WindowsBuilder(BuildTarget.StandaloneWindows64) },
      { BuildTarget.Android, new AndroidBuilder() },
      { BuildTarget.WebGL, new WebGLBuilder() },
      { BuildTarget.iOS, new IOSBuilder() },
      { BuildTarget.WSAPlayer, new DefaultBuilder() },
    };

    public IBuilder Get(BuildTarget target)
    {
      IBuilder builder;
      if (Builders.TryGetValue(target, out builder))
      {
        return builder;
      }
      return new DefaultBuilder();
    }
  }
}
