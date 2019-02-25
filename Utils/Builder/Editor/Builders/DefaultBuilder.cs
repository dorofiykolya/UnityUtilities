using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Utils.BuildPipeline.Builders
{
  public class DefaultBuilder : IBuilder
  {
    public virtual BuildReport Build(BuildConfiguration config, ILogger logger)
    {
      PreBuild(config, logger);
      var result = UnityEditor.BuildPipeline.BuildPlayer(config.BuildPlayerOptions);
      PostBuild(result, logger);
      return result;
    }

    public virtual string Help
    {
      get { return ""; }
    }

    protected virtual void PreBuild(BuildConfiguration config, ILogger logger)
    {

    }

    protected virtual void PostBuild(BuildReport result, ILogger logger)
    {

    }
  }
}
