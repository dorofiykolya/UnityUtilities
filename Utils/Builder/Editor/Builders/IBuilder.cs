using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Utils.BuildPipeline.Builders
{
  public interface IBuilder
  {
    BuildReport Build(BuildConfiguration config, ILogger logger);
  }
}
