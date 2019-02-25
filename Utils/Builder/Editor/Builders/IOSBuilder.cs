using System;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.iOS;

namespace Utils.BuildPipeline.Builders
{
  public class IOSBuilder : DefaultBuilder
  {
    public override BuildReport Build(BuildConfiguration config, ILogger logger)
    {
      var args = config.Arguments;

      BuilderUtils.AssertRequiredArguments<BuilderArguments.IOS>(args, true);

      PlayerSettings.iOS.buildNumber = config.BuildNumber.ToString();
      
      var ios = args.Fill<iOSArgs>();

      args.SetStaticPropertiesFromFiels<PlayerSettings.iOS>(ios, true);
      args.OnExist(BuilderArguments.IOS.IPadLaunchScreenType, (key, value) => PlayerSettings.iOS.SetiPadLaunchScreenType(args.GetValueByEnum<iOSLaunchScreenType>(BuilderArguments.IOS.IPadLaunchScreenType)));
      args.OnExist(BuilderArguments.IOS.IPhoneLaunchScreenType, (key, value) => PlayerSettings.iOS.SetiPadLaunchScreenType(args.GetValueByEnum<iOSLaunchScreenType>(BuilderArguments.IOS.IPhoneLaunchScreenType)));
      
      PreBuild(config, logger);
      var result = UnityEditor.BuildPipeline.BuildPlayer(config.BuildPlayerOptions);
      PostBuild(result, logger);

      return result;
    }

    public override string Help
    {
      get
      {
        return "iOS" + Environment.NewLine + BuilderUtils.GetArgumentsDescription(typeof(BuilderArguments.IOS), 2);
      }
    }

    public class iOSArgs
    {
      public bool hideHomeButton;
      public iOSAppInBackgroundBehavior appInBackgroundBehavior;
      public iOSBackgroundMode backgroundModes;
      public bool forceHardShadowsOnMetal;
      public bool allowHTTPDownload;
      public string appleDeveloperTeamID;
      public SystemGestureDeferMode deferSystemGesturesMode;
      public string iOSManualProvisioningProfileID;
      public ProvisioningProfileType tvOSManualProvisioningProfileType;
      public ProvisioningProfileType iOSManualProvisioningProfileType;
      public bool appleEnableAutomaticSigning;
      public string cameraUsageDescription;
      public string locationUsageDescription;
      public string microphoneUsageDescription;
      public string tvOSManualProvisioningProfileID;
      public iOSStatusBarStyle statusBarStyle;
      public bool requiresFullScreen;
      public bool requiresPersistentWiFi;
      public string applicationDisplayName;
      public iOSShowActivityIndicatorOnLoading showActivityIndicatorOnLoading;
      public bool disableDepthAndStencilBuffers;
      public string buildNumber;
      public iOSSdkVersion sdkVersion;
      public string targetOSVersionString;
      public iOSTargetDevice targetDevice;
      public bool prerenderedIcon;
      public ScriptCallOptimizationLevel scriptCallOptimization;
      public bool useOnDemandResources;
    }
  }
}
