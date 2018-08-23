using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Utils.BuildPipeline.Builders
{
  public class DefaultBuilder : IBuilder
  {
    public virtual BuildReport Build(BuildConfiguration config, ILogger logger)
    {
      return UnityEditor.BuildPipeline.BuildPlayer(config.BuildPlayerOptions);
    }
  }
}
