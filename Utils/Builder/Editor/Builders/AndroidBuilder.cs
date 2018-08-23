using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Utils.BuildPipeline.Builders
{
  public class AndroidBuilder : IBuilder
  {
    private const string KeystorePath = BuilderArguments.Android.KeystoreName;
    private const string KeystorePass = BuilderArguments.Android.KeystorePass;
    private const string KeyaliasName = BuilderArguments.Android.KeyaliasName;
    private const string KeyaliasPass = BuilderArguments.Android.KeyaliasPass;

    public BuildReport Build(BuildConfiguration config, ILogger logger)
    {
      var args = config.Arguments;

      BuilderUtils.AssertRequiredArguments<BuilderArguments.Android>(args, true);

      args.AssertKeys(KeystorePath, KeystorePass, KeyaliasName, KeyaliasPass);

      PlayerSettings.Android.keystoreName = args[KeystorePath];
      PlayerSettings.Android.keystorePass = args[KeystorePass];

      PlayerSettings.Android.keyaliasName = args[KeyaliasName];
      PlayerSettings.Android.keyaliasPass = args[KeyaliasPass];

      if (args.Contains(BuilderArguments.Android.PreferredInstallLocation))
      {
        PlayerSettings.Android.preferredInstallLocation = args.GetValueByEnum<AndroidPreferredInstallLocation>(BuilderArguments.Android.PreferredInstallLocation);
      }

      PlayerSettings.Android.targetArchitectures = args.Contains(BuilderArguments.Android.TargetArchitectures)
        ? args.GetValueByEnum<AndroidArchitecture>(BuilderArguments.Android.TargetArchitectures)
        : AndroidArchitecture.ARMv7;

      PlayerSettings.Android.buildApkPerCpuArchitecture =
        args.Contains(BuilderArguments.Android.BuildApkPerCpuArchitecture);

      PlayerSettings.Android.disableDepthAndStencilBuffers = args.Contains(BuilderArguments.Android.DisableDepthAndStencilBuffers);
      PlayerSettings.Android.forceSDCardPermission = args.Contains(BuilderArguments.Android.ForceSDCardPermission);
      PlayerSettings.Android.forceInternetPermission = args.Contains(BuilderArguments.Android.ForceInternetPermission);
      PlayerSettings.Android.androidIsGame = args.Contains(BuilderArguments.Android.AndroidIsGame);
      PlayerSettings.Android.useAPKExpansionFiles = args.Contains(BuilderArguments.Android.UseAPKExpansionFiles);
      PlayerSettings.Android.bundleVersionCode = config.BuildNumber;

      EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Internal;

      var result = UnityEditor.BuildPipeline.BuildPlayer(config.BuildPlayerOptions);

      return result;
    }
  }
}
