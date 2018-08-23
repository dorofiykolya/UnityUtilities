using System;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Assertions;

namespace Utils.BuildPipeline
{
  public class Builder
  {
    private readonly IBuildersProvider _provider;
    private readonly IBuilderProcessor[] _processors;
    private readonly ILogger _logger;
    private readonly string[] _scenes;

    public Builder(IBuildersProvider provider, string[] scenes = null, ILogger logger = null, IBuilderProcessor[] processors = null)
    {
      _provider = provider;
      _processors = processors;
      _scenes = scenes ?? ScenePaths;
      _logger = logger ?? Debug.unityLogger;
    }

    public void Build()
    {
      var args = BuilderUtils.GetCommandLineArguments();
      Build(args);
    }

    public void Build(string[] args)
    {
      Build(BuilderUtils.GetCommandLineArguments(args));
    }

    public void Build(Arguments args)
    {
      BuilderUtils.AssertRequiredArguments<BuilderArguments>(args, true);

      args.AssertKeys(BuilderArguments.BuildTarget, BuilderArguments.BuildVersion, BuilderArguments.Out, BuilderArguments.BuildNumber);

      if (args.IsVerbose)
      {
        _logger.Log(Help);

        _logger.Log("command line arguments: " + Environment.NewLine + args.ToString("\t{0}={1};" + Environment.NewLine));
      }

      BuildTarget buildTarget;
      Assert.IsTrue(BuilderUtils.TryParse(args[BuilderArguments.BuildTarget], out buildTarget), BuilderUtils.GetMessageNotValidArgs(BuilderArguments.BuildTarget, args));

      Version version;
      Assert.IsTrue(BuilderUtils.TryParse(args[BuilderArguments.BuildVersion], out version), BuilderUtils.GetMessageNotValidArgs(BuilderArguments.BuildVersion, args));

      int buildNumber;
      Assert.IsTrue(int.TryParse(args[BuilderArguments.BuildNumber], NumberStyles.Any, NumberFormatInfo.InvariantInfo, out buildNumber), BuilderUtils.GetMessageNotValidArgs(BuilderArguments.BuildNumber, args));

      version = new Version(version.Major, version.Minor, buildNumber);

      if (args.IsVerbose)
      {
        _logger.Log("Version: " + version);
      }

      PlayerSettings.bundleVersion = version.ToString();
      BuildOptions buildOptions = BuildOptions.None;
      BuildTargetGroup targetGroup = UnityEditor.BuildPipeline.GetBuildTargetGroup(buildTarget);
      ScriptingDefines defines = new ScriptingDefines(targetGroup, args.IsVerbose);

      ScriptingImplementation scriptingImplementation;
      if (BuilderUtils.TryGetScriptingBackend(args, targetGroup, out scriptingImplementation))
      {
        PlayerSettings.SetScriptingBackend(targetGroup, scriptingImplementation);
      }

      if (args.Contains(BuilderArguments.Debug))
      {
        buildOptions |= BuildOptions.Development;
        defines.Add(Defines.DEBUG);
      }
      else
      {
        defines.Remove(Defines.DEBUG);
        defines.Add(Defines.RELEASE);
      }

      var options = new BuildPlayerOptions
      {
        options = buildOptions,
        target = buildTarget,
        targetGroup = targetGroup,
        scenes = _scenes,
        locationPathName = args[BuilderArguments.Out]
      };

      var config = new BuildConfiguration(args, defines, buildNumber, version.ToString(), options);

      var report = Build(config);

      if (args.IsVerbose)
      {
        _logger.Log("command line build result: " + report.summary.result);
      }
    }

    public BuildReport Build(BuildConfiguration configuration)
    {
      OnPreBuild(configuration);
      var target = _provider.Get(configuration.Target);
      var report = target.Build(configuration, _logger);
      OnPostBuild(report);
      return report;
    }

    public string Help
    {
      get
      {
        var builder = new StringBuilder();
        builder.Append("command line args:");
        builder.Append(Environment.NewLine);
        builder.Append(BuilderUtils.GetArgumentsDescription(typeof(BuilderArguments), 4));
        builder.Append(Environment.NewLine);

        builder.Append("    Android" + Environment.NewLine + BuilderUtils.GetArgumentsDescription(typeof(BuilderArguments.Android), 8));
        builder.Append(Environment.NewLine);

        builder.Append("    IOS" + Environment.NewLine + BuilderUtils.GetArgumentsDescription(typeof(BuilderArguments.IOS), 8));
        builder.Append(Environment.NewLine);

        builder.Append("    WebGL" + Environment.NewLine + BuilderUtils.GetArgumentsDescription(typeof(BuilderArguments.WebGL), 8));
        builder.Append(Environment.NewLine);

        builder.Append("    Standalone" + Environment.NewLine + BuilderUtils.GetArgumentsDescription(typeof(BuilderArguments.Standalone), 8));
        builder.Append(Environment.NewLine);

        return builder.ToString();
      }
    }

    /// <summary>
    /// default editor scenes paths (Unity -> Build Settings -> Scenes In Build (only enabled))
    /// </summary>
    public static string[] ScenePaths
    {
      get { return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(); }
    }

    private void OnPreBuild(BuildConfiguration configuration)
    {
      if (_processors != null)
      {
        foreach (var processor in _processors)
        {
          processor.OnPreBuild(configuration);
        }
      }
    }

    private void OnPostBuild(BuildReport report)
    {
      if (_processors != null)
      {
        foreach (var processor in _processors)
        {
          processor.OnPostBuild(report);
        }
      }
    }
  }
}
