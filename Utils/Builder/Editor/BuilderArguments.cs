using UnityEditor;

namespace Utils.BuildPipeline
{
  public class BuilderArguments
  {
    [Key(true, typeof(BuildTarget), RequiredValue = true, Description = "build target")]
    public const string BuildTarget = "-buildTarget";

    [Key(true, RequiredValue = true, Description = "output path (file or folder)")]
    public const string Out = "-out";

    [Key(true, RequiredValue = true, Description = "build version (0.0.buildNumber)")]
    public const string BuildVersion = "-buildVersion";

    [Key(true, RequiredValue = true, Description = "build number (int)")]
    public const string BuildNumber = "-buildNumber";

    [Key(false, Description = "define DEBUG else define RELEASE")]
    public const string Debug = "-debug";

    [Key(false, Description = "verbose logs")]
    public const string Verbose = "-verbose";

    [Key(false, Description = "Remove unused Engine code from your build (IL2CPP-only).")]
    public const string StripEngineCode = "-stripEngineCode";

    [Key(false, typeof(ScriptingImplementation), Description = "Sets the scripting framework for a BuildTargetPlatformGroup.")]
    public const string ScriptingBackend = "-scriptingBackend";

    public class Android
    {
      [Key(true, RequiredValue = true, Description = "Android keystore name. (Name or path to keystore file)")]
      public const string KeystoreName = "-keystoreName";

      [Key(true, RequiredValue = true, Description = "Android keystore password")]
      public const string KeystorePass = "-keystorePass";

      [Key(true, RequiredValue = true, Description = "Android key alias name.")]
      public const string KeyaliasName = "-keyaliasName";

      [Key(true, RequiredValue = true, Description = "Android key alias password.")]
      public const string KeyaliasPass = "-keyaliasPass";

      [Key(false, Description = "Use APK Expansion Files.")]
      public const string UseAPKExpansionFiles = "-useAPKExpansionFiles";

      [Key(false, typeof(AndroidPreferredInstallLocation), Description = "Preferred application install location.")]
      public const string PreferredInstallLocation = "-preferredInstallLocation";

      [Key(false, Description = "Publish the build as a game rather than a regular application. This option affects devices running Android 5.0 Lollipop and later")]
      public const string AndroidIsGame = "-androidIsGame";

      [Key(false, Description = "Force SD card permission.")]
      public const string ForceSDCardPermission = "-forceSDCardPermission";

      [Key(false, Description = "Force internet permission flag.")]
      public const string ForceInternetPermission = "-forceInternetPermission";

      [Key(false, Description = "Disable Depth and Stencil Buffers.")]
      public const string DisableDepthAndStencilBuffers = "-disableDepthAndStencilBuffers";

      [Key(false, typeof(AndroidArchitecture), Description = "A set of CPU architectures for the Android build target. (enum flag, example use: \"-targetArchitectures ARMv7,ARM64\")")]
      public const string TargetArchitectures = "-targetArchitectures";

      [Key(false, Description = "Create a separate APK for each CPU architecture.")]
      public const string BuildApkPerCpuArchitecture = "-buildApkPerCpuArchitecture";

      [Key(false, typeof(MobileTextureSubtarget), Description = "Android platform options.")]
      public const string AndroidBuildSubtarget = "-androidBuildSubtarget";
    }

    public class IOS
    {

    }

    public class WebGL
    {

    }

    public class Standalone
    {

    }
  }
}
