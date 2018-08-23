using UnityEditor.Build.Reporting;

namespace Utils.BuildPipeline
{
  public interface IBuilderProcessor
  {
    void OnPreBuild(BuildConfiguration configuration);
    void OnPostBuild(BuildReport report);
  }
}
