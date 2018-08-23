using UnityEditor;

namespace Utils.BuildPipeline
{
  public class BuildConfiguration
  {
    public BuildConfiguration(Arguments arguments, ScriptingDefines defines, int buildNumber, string buildVersion,
      BuildPlayerOptions buildPlayerOptions)
    {
      BuildPlayerOptions = buildPlayerOptions;
      Arguments = arguments;
      Defines = defines;
      BuildNumber = buildNumber;
      BuildVersion = buildVersion;
    }

    public BuildPlayerOptions BuildPlayerOptions { get; private set; }

    public Arguments Arguments { get; private set; }

    public ScriptingDefines Defines { get; private set; }

    public int BuildNumber { get; private set; }

    public string BuildVersion { get; private set; }

    /// <summary>
    ///   <para>The scenes to be included in the build. If empty, the currently open scene will be built. Paths are relative to the project folder (AssetsMyLevelsMyScene.unity).</para>
    /// </summary>
    public string[] Scenes
    {
      get { return BuildPlayerOptions.scenes; }
      set
      {
        var buildPlayerOptions = BuildPlayerOptions;
        buildPlayerOptions.scenes = value;
        BuildPlayerOptions = buildPlayerOptions;
      }
    }

    /// <summary>
    ///   <para>The path where the application will be built.</para>
    /// </summary>
    public string LocationPathName
    {
      get { return BuildPlayerOptions.locationPathName; }
      set
      {
        var buildPlayerOptions = BuildPlayerOptions;
        buildPlayerOptions.locationPathName = value;
        BuildPlayerOptions = buildPlayerOptions;
      }
    }

    /// <summary>
    ///   <para>The path to an manifest file describing all of the asset bundles used in the build (optional).</para>
    /// </summary>
    public string AssetBundleManifestPath
    {
      get { return BuildPlayerOptions.assetBundleManifestPath; }
      set
      {
        var buildPlayerOptions = BuildPlayerOptions;
        buildPlayerOptions.assetBundleManifestPath = value;
        BuildPlayerOptions = buildPlayerOptions;
      }
    }

    /// <summary>
    ///   <para>The BuildTargetGroup to build.</para>
    /// </summary>
    public BuildTargetGroup TargetGroup
    {
      get { return BuildPlayerOptions.targetGroup;}
      set
      {
        var buildPlayerOptions = BuildPlayerOptions;
        buildPlayerOptions.targetGroup = value;
        BuildPlayerOptions = buildPlayerOptions;
      }
    }

    /// <summary>
    ///   <para>The BuildTarget to build.</para>
    /// </summary>
    public BuildTarget Target
    {
      get { return BuildPlayerOptions.target; }
      set
      {
        var buildPlayerOptions = BuildPlayerOptions;
        buildPlayerOptions.target = value;
        BuildPlayerOptions = buildPlayerOptions;
      }
    }

    /// <summary>
    ///   <para>Additional BuildOptions, like whether to run the built player.</para>
    /// </summary>
    public BuildOptions Options
    {
      get { return BuildPlayerOptions.options; }
      set
      {
        var buildPlayerOptions = BuildPlayerOptions;
        buildPlayerOptions.options = value;
        BuildPlayerOptions = buildPlayerOptions;
      }
    }
  }
}
