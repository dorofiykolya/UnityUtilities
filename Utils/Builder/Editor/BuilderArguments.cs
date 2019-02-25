using UnityEditor;
using UnityEngine.iOS;

namespace Utils.BuildPipeline
{
  public class BuilderArguments
  {
    [Key(true, typeof(BuildTarget), RequiredValue = true, Description = "build target")]
    public const string BuildTarget = "buildTarget";

    [Key(true, RequiredValue = true, Description = "output path (file or folder)")]
    public const string Out = "out";

    [Key(true, RequiredValue = true, Description = "build version (0.0.buildNumber)")]
    public const string BuildVersion = "buildVersion";

    [Key(true, RequiredValue = true, Description = "build number (int)")]
    public const string BuildNumber = "buildNumber";

    [Key(false, Description = "define DEBUG else define RELEASE")]
    public const string Debug = "debug";

    [Key(false, Description = "verbose logs")]
    public const string Verbose = "verbose";

    [Key(false, Description = "Remove unused Engine code from your build (IL2CPP-only).")]
    public const string StripEngineCode = "stripEngineCode";

    [Key(false, typeof(ScriptingImplementation), Description = "Sets the scripting framework for a BuildTargetPlatformGroup.")]
    public const string ScriptingBackend = "scriptingBackend";

    public class Android
    {
      [Key(true, RequiredValue = true, Description = "Android keystore name. (Name or path to keystore file)")]
      public const string KeystoreName = "keystoreName";

      [Key(true, RequiredValue = true, Description = "Android keystore password")]
      public const string KeystorePass = "keystorePass";

      [Key(true, RequiredValue = true, Description = "Android key alias name.")]
      public const string KeyaliasName = "keyaliasName";

      [Key(true, RequiredValue = true, Description = "Android key alias password.")]
      public const string KeyaliasPass = "keyaliasPass";

      [Key(false, Description = "Use APK Expansion Files.")]
      public const string UseAPKExpansionFiles = "useAPKExpansionFiles";

      [Key(false, typeof(AndroidPreferredInstallLocation), Description = "Preferred application install location.")]
      public const string PreferredInstallLocation = "preferredInstallLocation";

      [Key(false, Description = "Publish the build as a game rather than a regular application. This option affects devices running Android 5.0 Lollipop and later")]
      public const string AndroidIsGame = "androidIsGame";

      [Key(false, Description = "Force SD card permission.")]
      public const string ForceSDCardPermission = "forceSDCardPermission";

      [Key(false, Description = "Force internet permission flag.")]
      public const string ForceInternetPermission = "forceInternetPermission";

      [Key(false, Description = "Disable Depth and Stencil Buffers.")]
      public const string DisableDepthAndStencilBuffers = "disableDepthAndStencilBuffers";

      [Key(false, typeof(AndroidArchitecture), Description = "A set of CPU architectures for the Android build target. (enum flag, example use: \"targetArchitectures ARMv7,ARM64\")")]
      public const string TargetArchitectures = "targetArchitectures";

      [Key(false, Description = "Create a separate APK for each CPU architecture.")]
      public const string BuildApkPerCpuArchitecture = "buildApkPerCpuArchitecture";

      [Key(false, typeof(MobileTextureSubtarget), Description = "Android platform options.")]
      public const string AndroidBuildSubtarget = "androidBuildSubtarget";

      [Key(false, typeof(AndroidBlitType), Description = "Choose how content is drawn to the screen.")]
      public const string BlitType = "blitType";

      [Key(false, typeof(AndroidSdkVersions), Description = "The target API level of your application.")]
      public const string TargetSdkVersion = "targetSdkVersion";

      [Key(false, typeof(AndroidSdkVersions), Description = "The target API level of your application.")]
      public const string MinSdkVersion = "minSdkVersion";

      [Key(false, Description = "Maximum aspect ratio which is supported by the application. (float)")]
      public const string MaxAspectRatio = "maxAspectRatio";
    }

    public class IOS
    {
      [Key(false, Description = "Specifies whether the home button should be hidden in the iOS build of this application.")]
      public const string HideHomeButton = "hideHomeButton";

      [Key(false, typeof(iOSAppInBackgroundBehavior), Description = "Application behavior when entering background.")]
      public const string AppInBackgroundBehavior = "appInBackgroundBehavior";

      [Key(false, typeof(iOSBackgroundMode), Description = "Supported background execution modes (when appInBackgroundBehavior is set to '--appInBackgroundBehavior Custom').")]
      public const string BackgroundModes = "backgroundModes";

      [Key(false, Description = "Should hard shadows be enforced when running on (mobile) Metal.")]
      public const string ForceHardShadowsOnMetal = "forceHardShadowsOnMetal";

      [Key(false, Description = "Should insecure HTTP downloads be allowed?")]
      public const string AllowHTTPDownload = "allowHTTPDownload";

      [Key(false, RequiredValue = true, Description = "Set this property with your Apple Developer Team ID. You can find this on the Apple Developer website under <a href=\"https:developer.apple.comaccount#membership\"> Account > Membership </a> . This sets the Team ID for the generated Xcode project, allowing developers to use the Build and Run functionality. An Apple Developer Team ID must be set here for automatic signing of your app.")]
      public const string AppleDeveloperTeamID = "appleDeveloperTeamID";

      [Key(false, typeof(SystemGestureDeferMode), Description = "Defer system gestures until the second swipe on specific edges.")]
      public const string DeferSystemGesturesMode = "deferSystemGesturesMode";

      [Key(false, RequiredValue = true, Description = "A provisioning profile Universally Unique Identifier (UUID) that Xcode will use to build your iOS app in Manual Signing mode.")]
      public const string IOSManualProvisioningProfileID = "iOSManualProvisioningProfileID";

      [Key(false, typeof(ProvisioningProfileType), Description = "A ProvisioningProfileType that will be set when building a tvOS Xcode project.")]
      public const string TvOSManualProvisioningProfileType = "tvOSManualProvisioningProfileType";

      [Key(false, typeof(ProvisioningProfileType), Description = "A ProvisioningProfileType that will be set when building an iOS Xcode project.")]
      public const string IOSManualProvisioningProfileType = "iOSManualProvisioningProfileType";

      [Key(false, Description = "Set this property to true for Xcode to attempt to automatically sign your app based on your appleDeveloperTeamID.")]
      public const string AppleEnableAutomaticSigning = "appleEnableAutomaticSigning";

      [Key(false, RequiredValue = true, Description = "Describes the reason for access to the user's camera.")]
      public const string CameraUsageDescription = "cameraUsageDescription";

      [Key(false, RequiredValue = true, Description = "Describes the reason for access to the user's location data.")]
      public const string LocationUsageDescription = "locationUsageDescription";

      [Key(false, RequiredValue = true, Description = "Describes the reason for access to the user's microphone.")]
      public const string MicrophoneUsageDescription = "microphoneUsageDescription";

      [Key(false, RequiredValue = true, Description = "A provisioning profile Universally Unique Identifier (UUID) that Xcode will use to build your tvOS app in Manual Signing mode.")]
      public const string TvOSManualProvisioningProfileID = "tvOSManualProvisioningProfileID";

      [Key(false, typeof(iOSStatusBarStyle), Description = "Status bar style.")]
      public const string StatusBarStyle = "statusBarStyle";

      [Key(false, Description = "RequiresFullScreen maps to Apple's plist build setting UIRequiresFullScreen, which is used to opt out of being eligible to participate in Slide Over and Split View for iOS 9.0 multitasking.")]
      public const string RequiresFullScreen = "requiresFullScreen";

      [Key(false, Description = "Application requires persistent WiFi.")]
      public const string RequiresPersistentWiFi = "requiresPersistentWiFi";

      [Key(false, Description = "iOS application display name.")]
      public const string ApplicationDisplayName = "applicationDisplayName";

      [Key(false, typeof(iOSShowActivityIndicatorOnLoading), Description = "Application should show ActivityIndicator when loading.")]
      public const string ShowActivityIndicatorOnLoading = "showActivityIndicatorOnLoading";

      [Key(false, Description = "Disable Depth and Stencil Buffers.")]
      public const string DisableDepthAndStencilBuffers = "disableDepthAndStencilBuffers";

      [Key(false, typeof(iOSSdkVersion), Description = "Active iOS SDK version used for build.")]
      public const string SdkVersion = "sdkVersion";

      [Key(false, RequiredValue = true, Description = "Deployment minimal version of iOS.")]
      public const string TargetOSVersionString = "targetOSVersionString";

      [Key(false, typeof(iOSTargetDevice), Description = "Targeted device.")]
      public const string TargetDevice = "targetDevice";

      [Key(false, Description = "Icon is prerendered.")]
      public const string PrerenderedIcon = "prerenderedIcon";

      [Key(false, typeof(ScriptCallOptimizationLevel), Description = "Script calling optimization.")]
      public const string ScriptCallOptimization = "scriptCallOptimization";

      [Key(false, Description = "Indicates whether application will use On Demand Resources (ODR) API.")]
      public const string UseOnDemandResources = "useOnDemandResources";

      [Key(false, typeof(iOSLaunchScreenType), Description = "Sets the mode which will be used to generate the app's launch screen Interface Builder (.xib) file for iPad.")]
      public const string IPadLaunchScreenType = "iPadLaunchScreenType";

      [Key(false, typeof(iOSLaunchScreenType), Description = "Sets the mode which will be used to generate the app's launch screen Interface Builder (.xib) file for iPhone.")]
      public const string IPhoneLaunchScreenType = "iPhoneLaunchScreenType";
    }

    public class WebGL
    {

    }

    public class Standalone
    {

    }
  }
}
