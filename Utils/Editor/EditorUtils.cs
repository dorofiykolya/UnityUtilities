using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Utils.Editor
{
  public class EditorUtils
  {
    struct ColorInfo
    {
      public Color color;
      public Color backgroundColor;
      public Color contentColor;
    }

    private static readonly Stack<ColorInfo> _colorsStack = new Stack<ColorInfo>();
    private static readonly Stack<bool> _enableStack = new Stack<bool>();

    public static void PushEnable()
    {
      _enableStack.Push(GUI.enabled);
    }

    public static void PushEnable(bool newValue, bool inherited = true)
    {
      _enableStack.Push(GUI.enabled);
      GUI.enabled = newValue && GUI.enabled;
    }

    public static void PopEnable()
    {
      if (_enableStack.Count != 0)
      {
        GUI.enabled = _enableStack.Pop();
      }
    }

    public static void PushColor()
    {
      _colorsStack.Push(new ColorInfo
      {
        color = GUI.color,
        backgroundColor = GUI.backgroundColor,
        contentColor = GUI.contentColor,
      });
    }

    public static void PopColor()
    {
      if (_colorsStack.Count != 0)
      {
        var colorInfo = _colorsStack.Pop();
        GUI.color = colorInfo.color;
        GUI.backgroundColor = colorInfo.backgroundColor;
        GUI.contentColor = colorInfo.contentColor;
      }
    }

    public static void Header(string text, float space = 0f, bool enable = true)
    {
      var lastBackgroundColor = GUI.backgroundColor;
      if (!enable) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);

      GUILayout.BeginHorizontal();
      text = "<b><size=11>" + text + "</size></b>";
      GUILayout.Toggle(true, "☼ " + text, "dragtab", GUILayout.MinWidth(20f));
      GUILayout.EndHorizontal();
      GUILayout.Space(space);

      GUI.backgroundColor = lastBackgroundColor;
      if (!enable) GUILayout.Space(space);
    }

    public static void BeginVertical(bool selected = true, float space = 2f)
    {
      GUILayout.BeginHorizontal();
      if (selected) EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
      else EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
      GUILayout.BeginVertical();
      GUILayout.Space(space);
    }

    public static bool FoldoutHeader(string text, bool enable, float space = 3f)
    {
      var lastBackgroundColor = GUI.backgroundColor;
      if (!enable) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);

      GUILayout.BeginHorizontal();
      text = "<b><size=11>" + text + "</size></b>";

      if (enable)
      {
        text = "\u25BC " + text;
      }
      else
      {
        text = "\u25BA " + text;
      }

      if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) enable = !enable;
      GUILayout.EndHorizontal();

      GUI.backgroundColor = lastBackgroundColor;
      if (!enable) GUILayout.Space(space);

      return enable;
    }

    public static void EndVertical(float space = 3f)
    {
      GUILayout.Space(3f);
      GUILayout.EndVertical();
      EditorGUILayout.EndHorizontal();
      GUILayout.Space(3f);
      GUILayout.EndHorizontal();
      GUILayout.Space(space);
    }

    public static void BeginHorizontal(bool selected = true, float space = 2f)
    {
      GUILayout.BeginVertical();
      GUILayout.Space(space);
      if (selected) EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
      else EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
      GUILayout.Space(space);
    }

    public static void EndHorizontal(float space = 3f)
    {
      GUILayout.Space(space);
      EditorGUILayout.EndHorizontal();
      GUILayout.Space(space);
      GUILayout.EndVertical();
      GUILayout.Space(space);
    }

    public static int UpDownArrows(GUIContent label, int value, GUIStyle labelStyle, GUIStyle upArrow, GUIStyle downArrow)
    {
      GUILayout.BeginHorizontal();
      GUILayout.Space(EditorGUI.indentLevel * 10);
      GUILayout.Label(label, labelStyle, GUILayout.Width(170));

      if (downArrow == null || upArrow == null)
      {
        upArrow = GUI.skin.FindStyle("Button");
        downArrow = upArrow;
      }

      if (GUILayout.Button("", upArrow, GUILayout.Width(16), GUILayout.Height(12)))
      {
        value++;
      }
      if (GUILayout.Button("", downArrow, GUILayout.Width(16), GUILayout.Height(12)))
      {
        value--;
      }
      GUILayout.Space(100);
      GUILayout.EndHorizontal();
      return value;
    }

    private static readonly Dictionary<UnityEditor.Editor, FadeEditorGUILayout> _fades = new Dictionary<UnityEditor.Editor, FadeEditorGUILayout>();

    public static FadeEditorGUILayout.FadeArea BeginFade(UnityEditor.Editor editor, bool open)
    {
      FadeEditorGUILayout result;
      if (!_fades.TryGetValue(editor, out result))
      {
        _fades[editor] = result = new FadeEditorGUILayout { typeIndex = _fades.Count };
      }
      FadeEditorGUILayout.editor = editor;
      return result.BeginFadeArea(open, editor.GetType().FullName + result.typeIndex);
    }

    public static FadeEditorGUILayout.FadeArea BeginFade(UnityEditor.Editor editor, bool open, string label, string id)
    {
      FadeEditorGUILayout result;
      if (!_fades.TryGetValue(editor, out result))
      {
        _fades[editor] = result = new FadeEditorGUILayout();
      }
      FadeEditorGUILayout.editor = editor;
      return result.BeginFadeArea(open, label, id);
    }

    public static FadeEditorGUILayout.FadeArea BeginFade(UnityEditor.Editor editor, bool open, string id)
    {
      FadeEditorGUILayout result;
      if (!_fades.TryGetValue(editor, out result))
      {
        _fades[editor] = result = new FadeEditorGUILayout();
      }
      FadeEditorGUILayout.editor = editor;
      return result.BeginFadeArea(open, id);
    }

    public static void EndFade(UnityEditor.Editor editor)
    {
      FadeEditorGUILayout result;
      if (_fades.TryGetValue(editor, out result))
      {
        result.EndFadeArea();
      }
    }

    public static void DisposeFade(UnityEditor.Editor editor)
    {
      FadeEditorGUILayout result;
      if (_fades.TryGetValue(editor, out result))
      {
        _fades.Remove(editor);
      }
    }

    public static FadeEditorGUILayout.FadeArea BeginVerticalFade(UnityEditor.Editor editor, bool open, string id)
    {
      FadeEditorGUILayout result;
      if (!_fades.TryGetValue(editor, out result))
      {
        _fades[editor] = result = new FadeEditorGUILayout();
      }
      FadeEditorGUILayout.editor = editor;
      var area = result.BeginFadeArea(open, id);
      BeginVertical();
      return area;
    }

    public static void EndVerticalFade(UnityEditor.Editor editor)
    {
      FadeEditorGUILayout result;
      if (_fades.TryGetValue(editor, out result))
      {
        EndVertical();
        result.EndFadeArea();
      }
    }

    public class Styles
    {
      public static readonly string AboutWIndowLicenseLabel = "AboutWIndowLicenseLabel";
      public static readonly string AC_LeftArrow = "AC LeftArrow";
      public static readonly string AC_RightArrow = "AC RightArrow";
      public static readonly string AnimationCurveEditorBackground = "AnimationCurveEditorBackground";
      public static readonly string AnimationEventBackground = "AnimationEventBackground";
      public static readonly string AnimationEventTooltip = "AnimationEventTooltip";
      public static readonly string AnimationEventTooltipArrow = "AnimationEventTooltipArrow";
      public static readonly string AnimationKeyframeBackground = "AnimationKeyframeBackground";
      public static readonly string AnimationRowEven = "AnimationRowEven";
      public static readonly string AnimationRowOdd = "AnimationRowOdd";
      public static readonly string AnimationSelectionTextField = "AnimationSelectionTextField";
      public static readonly string AnimationTimelineTick = "AnimationTimelineTick";
      public static readonly string AnimPropDropdown = "AnimPropDropdown";
      public static readonly string AppToolbar = "AppToolbar";
      public static readonly string AS_TextArea = "AS TextArea";
      public static readonly string BoldLabel = "BoldLabel";
      public static readonly string BoldToggle = "BoldToggle";
      public static readonly string ButtonLeft = "ButtonLeft";
      public static readonly string ButtonMid = "ButtonMid";
      public static readonly string ButtonRight = "ButtonRight";
      public static readonly string CN_Box = "CN Box";
      public static readonly string CN_CountBadge = "CN CountBadge";
      public static readonly string CN_EntryBackEven = "CN EntryBackEven";
      public static readonly string CN_EntryBackOdd = "CN EntryBackOdd";
      public static readonly string CN_EntryError = "CN EntryError";
      public static readonly string CN_EntryInfo = "CN EntryInfo";
      public static readonly string CN_EntryWarn = "CN EntryWarn";
      public static readonly string CN_Message = "CN Message";
      public static readonly string CN_StatusError = "CN StatusError";
      public static readonly string CN_StatusInfo = "CN StatusInfo";
      public static readonly string CN_StatusWarn = "CN StatusWarn";
      public static readonly string ColorField = "ColorField";
      public static readonly string ColorPicker2DThumb = "ColorPicker2DThumb";
      public static readonly string ColorPickerBackground = "ColorPickerBackground";
      public static readonly string ColorPickerBox = "ColorPickerBox";
      public static readonly string ColorPickerHorizThumb = "ColorPickerHorizThumb";
      public static readonly string ColorPickerVertThumb = "ColorPickerVertThumb";
      public static readonly string Command = "Command";
      public static readonly string CommandLeft = "CommandLeft";
      public static readonly string CommandMid = "CommandMid";
      public static readonly string CommandRight = "CommandRight";
      public static readonly string ControlLabel = "ControlLabel";
      public static readonly string CurveEditorLabelTickmarks = "CurveEditorLabelTickmarks";
      public static readonly string debug_layout_box = "debug_layout_box";
      public static readonly string dockarea = "dockarea";
      public static readonly string dockareaOverlay = "dockareaOverlay";
      public static readonly string dockareaStandalone = "dockareaStandalone";
      public static readonly string dragtab = "dragtab";
      public static readonly string dragtabbright = "dragtabbright";
      public static readonly string dragtabdropwindow = "dragtabdropwindow";
      public static readonly string DropDown = "DropDown";
      public static readonly string DropDownButton = "DropDownButton";
      public static readonly string ErrorLabel = "ErrorLabel";
      public static readonly string ExposablePopupItem = "ExposablePopupItem";
      public static readonly string ExposablePopupMenu = "ExposablePopupMenu";
      public static readonly string EyeDropperHorizontalLine = "EyeDropperHorizontalLine";
      public static readonly string EyeDropperPickedPixel = "EyeDropperPickedPixel";
      public static readonly string EyeDropperVerticalLine = "EyeDropperVerticalLine";
      public static readonly string flow_background = "flow background";
      public static readonly string flow_navbar_back = "flow navbar back";
      public static readonly string flow_navbar_button = "flow navbar button";
      public static readonly string flow_navbar_separator = "flow navbar separator";
      public static readonly string flow_node_0 = "flow node 0";
      public static readonly string flow_node_0_on = "flow node 0 on";
      public static readonly string flow_node_1 = "flow node 1";
      public static readonly string flow_node_1_on = "flow node 1 on";
      public static readonly string flow_node_2 = "flow node 2";
      public static readonly string flow_node_2_on = "flow node 2 on";
      public static readonly string flow_node_3 = "flow node 3";
      public static readonly string flow_node_3_on = "flow node 3 on";
      public static readonly string flow_node_4 = "flow node 4";
      public static readonly string flow_node_4_on = "flow node 4 on";
      public static readonly string flow_node_5 = "flow node 5";
      public static readonly string flow_node_5_on = "flow node 5 on";
      public static readonly string flow_node_6 = "flow node 6";
      public static readonly string flow_node_6_on = "flow node 6 on";
      public static readonly string flow_node_hex_0 = "flow node hex 0";
      public static readonly string flow_node_hex_0_on = "flow node hex 0 on";
      public static readonly string flow_node_hex_1 = "flow node hex 1";
      public static readonly string flow_node_hex_1_on = "flow node hex 1 on";
      public static readonly string flow_node_hex_2 = "flow node hex 2";
      public static readonly string flow_node_hex_2_on = "flow node hex 2 on";
      public static readonly string flow_node_hex_3 = "flow node hex 3";
      public static readonly string flow_node_hex_3_on = "flow node hex 3 on";
      public static readonly string flow_node_hex_4 = "flow node hex 4";
      public static readonly string flow_node_hex_4_on = "flow node hex 4 on";
      public static readonly string flow_node_hex_5 = "flow node hex 5";
      public static readonly string flow_node_hex_5_on = "flow node hex 5 on";
      public static readonly string flow_node_hex_6 = "flow node hex 6";
      public static readonly string flow_node_hex_6_on = "flow node hex 6 on";
      public static readonly string flow_node_titlebar = "flow node titlebar";
      public static readonly string flow_overlay_area_left = "flow overlay area left";
      public static readonly string flow_overlay_area_right = "flow overlay area right";
      public static readonly string flow_overlay_box = "flow overlay box";
      public static readonly string flow_overlay_foldout = "flow overlay foldout";
      public static readonly string flow_overlay_header_lower_left = "flow overlay header lower left";
      public static readonly string flow_overlay_header_lower_right = "flow overlay header lower right";
      public static readonly string flow_overlay_header_upper_left = "flow overlay header upper left";
      public static readonly string flow_overlay_header_upper_right = "flow overlay header upper right";
      public static readonly string flow_shader_in_0 = "flow shader in 0";
      public static readonly string flow_shader_in_1 = "flow shader in 1";
      public static readonly string flow_shader_in_2 = "flow shader in 2";
      public static readonly string flow_shader_in_3 = "flow shader in 3";
      public static readonly string flow_shader_in_4 = "flow shader in 4";
      public static readonly string flow_shader_in_5 = "flow shader in 5";
      public static readonly string flow_shader_node_0 = "flow shader node 0";
      public static readonly string flow_shader_node_0_on = "flow shader node 0 on";
      public static readonly string flow_shader_out_0 = "flow shader out 0";
      public static readonly string flow_shader_out_1 = "flow shader out 1";
      public static readonly string flow_shader_out_2 = "flow shader out 2";
      public static readonly string flow_shader_out_3 = "flow shader out 3";
      public static readonly string flow_shader_out_4 = "flow shader out 4";
      public static readonly string flow_shader_out_5 = "flow shader out 5";
      public static readonly string flow_target_in = "flow target in";
      public static readonly string flow_triggerPin_in = "flow triggerPin in";
      public static readonly string flow_triggerPin_out = "flow triggerPin out";
      public static readonly string flow_var_0 = "flow var 0";
      public static readonly string flow_var_0_on = "flow var 0 on";
      public static readonly string flow_var_1 = "flow var 1";
      public static readonly string flow_var_1_on = "flow var 1 on";
      public static readonly string flow_var_2 = "flow var 2";
      public static readonly string flow_var_2_on = "flow var 2 on";
      public static readonly string flow_var_3 = "flow var 3";
      public static readonly string flow_var_3_on = "flow var 3 on";
      public static readonly string flow_var_4 = "flow var 4";
      public static readonly string flow_var_4_on = "flow var 4 on";
      public static readonly string flow_var_5 = "flow var 5";
      public static readonly string flow_var_5_on = "flow var 5 on";
      public static readonly string flow_var_6 = "flow var 6";
      public static readonly string flow_var_6_on = "flow var 6 on";
      public static readonly string flow_varPin_in = "flow varPin in";
      public static readonly string flow_varPin_out = "flow varPin out";
      public static readonly string flow_varPin_tooltip = "flow varPin tooltip";
      public static readonly string Foldout = "Foldout";
      public static readonly string FoldOutPreDrop = "FoldOutPreDrop";
      public static readonly string GameViewBackground = "GameViewBackground";
      public static readonly string Grad_Down_Swatch = "Grad Down Swatch";
      public static readonly string Grad_Down_Swatch_Overlay = "Grad Down Swatch Overlay";
      public static readonly string Grad_Up_Swatch = "Grad Up Swatch";
      public static readonly string Grad_Up_Swatch_Overlay = "Grad Up Swatch Overlay";
      public static readonly string grey_border = "grey_border";
      public static readonly string GridList = "GridList";
      public static readonly string GridListText = "GridListText";
      public static readonly string GridToggle = "GridToggle";
      public static readonly string GroupBox = "GroupBox";
      public static readonly string GUIEditor_BreadcrumbLeft = "GUIEditor.BreadcrumbLeft";
      public static readonly string GUIEditor_BreadcrumbMid = "GUIEditor.BreadcrumbMid";
      public static readonly string GV_Gizmo_DropDown = "GV Gizmo DropDown";
      public static readonly string HeaderLabel = "HeaderLabel";
      public static readonly string HelpBox = "HelpBox";
      public static readonly string Hi_Label = "Hi Label";
      public static readonly string HorizontalMinMaxScrollbarThumb = "HorizontalMinMaxScrollbarThumb";
      public static readonly string hostview = "hostview";
      public static readonly string IN_BigTitle = "IN BigTitle";
      public static readonly string IN_BigTitle_Inner = "IN BigTitle Inner";
      public static readonly string IN_ColorField = "IN ColorField";
      public static readonly string IN_DropDown = "IN DropDown";
      public static readonly string IN_Foldout = "IN Foldout";
      public static readonly string IN_FoldoutStatic = "IN FoldoutStatic";
      public static readonly string IN_Label = "IN Label";
      public static readonly string IN_LockButton = "IN LockButton";
      public static readonly string IN_ObjectField = "IN ObjectField";
      public static readonly string IN_Popup = "IN Popup";
      public static readonly string IN_SelectedLine = "IN SelectedLine";
      public static readonly string IN_TextField = "IN TextField";
      public static readonly string IN_ThumbnailSelection = "IN ThumbnailSelection";
      public static readonly string IN_ThumbnailShadow = "IN ThumbnailShadow";
      public static readonly string IN_Title = "IN Title";
      public static readonly string IN_TitleText = "IN TitleText";
      public static readonly string IN_Toggle = "IN Toggle";
      public static readonly string InnerShadowBg = "InnerShadowBg";
      public static readonly string InvisibleButton = "InvisibleButton";
      public static readonly string LargeButton = "LargeButton";
      public static readonly string LargeButtonLeft = "LargeButtonLeft";
      public static readonly string LargeButtonMid = "LargeButtonMid";
      public static readonly string LargeButtonRight = "LargeButtonRight";
      public static readonly string LargeDropDown = "LargeDropDown";
      public static readonly string LargeLabel = "LargeLabel";
      public static readonly string LargePopup = "LargePopup";
      public static readonly string LargeTextField = "LargeTextField";
      public static readonly string LightmapEditorSelectedHighlight = "LightmapEditorSelectedHighlight";
      public static readonly string ListToggle = "ListToggle";
      public static readonly string LockedHeaderBackground = "LockedHeaderBackground";
      public static readonly string LockedHeaderButton = "LockedHeaderButton";
      public static readonly string LockedHeaderLabel = "LockedHeaderLabel";
      public static readonly string LODBlackBox = "LODBlackBox";
      public static readonly string LODCameraLine = "LODCameraLine";
      public static readonly string LODLevelNotifyText = "LODLevelNotifyText";
      public static readonly string LODRendererAddButton = "LODRendererAddButton";
      public static readonly string LODRendererButton = "LODRendererButton";
      public static readonly string LODRendererRemove = "LODRendererRemove";
      public static readonly string LODRenderersText = "LODRenderersText";
      public static readonly string LODSceneText = "LODSceneText";
      public static readonly string LODSliderBG = "LODSliderBG";
      public static readonly string LODSliderRange = "LODSliderRange";
      public static readonly string LODSliderRangeSelected = "LODSliderRangeSelected";
      public static readonly string LODSliderText = "LODSliderText";
      public static readonly string LODSliderTextSelected = "LODSliderTextSelected";
      public static readonly string MeBlendBackground = "MeBlendBackground";
      public static readonly string MeBlendPosition = "MeBlendPosition";
      public static readonly string MeBlendTriangleLeft = "MeBlendTriangleLeft";
      public static readonly string MeBlendTriangleRight = "MeBlendTriangleRight";
      public static readonly string MeLivePlayBackground = "MeLivePlayBackground";
      public static readonly string MeLivePlayBar = "MeLivePlayBar";
      public static readonly string MeTimeLabel = "MeTimeLabel";
      public static readonly string MeTransBGOver = "MeTransBGOver";
      public static readonly string MeTransitionBack = "MeTransitionBack";
      public static readonly string MeTransitionBlock = "MeTransitionBlock";
      public static readonly string MeTransitionHandleLeft = "MeTransitionHandleLeft";
      public static readonly string MeTransitionHandleLeftPrev = "MeTransitionHandleLeftPrev";
      public static readonly string MeTransitionHandleRight = "MeTransitionHandleRight";
      public static readonly string MeTransitionHead = "MeTransitionHead";
      public static readonly string MeTransitionSelect = "MeTransitionSelect";
      public static readonly string MeTransitionSelectHead = "MeTransitionSelectHead";
      public static readonly string MeTransOff2On = "MeTransOff2On";
      public static readonly string MeTransOffLeft = "MeTransOffLeft";
      public static readonly string MeTransOffRight = "MeTransOffRight";
      public static readonly string MeTransOn2Off = "MeTransOn2Off";
      public static readonly string MeTransOnLeft = "MeTransOnLeft";
      public static readonly string MeTransOnRight = "MeTransOnRight";
      public static readonly string MeTransPlayhead = "MeTransPlayhead";
      public static readonly string MiniBoldLabel = "MiniBoldLabel";
      public static readonly string minibutton = "minibutton";
      public static readonly string minibuttonleft = "minibuttonleft";
      public static readonly string minibuttonmid = "minibuttonmid";
      public static readonly string minibuttonright = "minibuttonright";
      public static readonly string MiniLabel = "MiniLabel";
      public static readonly string MiniLabelRight = "MiniLabelRight";
      public static readonly string MiniMinMaxSliderHorizontal = "MiniMinMaxSliderHorizontal";
      public static readonly string MiniMinMaxSliderVertical = "MiniMinMaxSliderVertical";
      public static readonly string MiniPopup = "MiniPopup";
      public static readonly string MiniPullDown = "MiniPullDown";
      public static readonly string MiniPullDownLeft = "MiniPullDownLeft";
      public static readonly string MiniTextField = "MiniTextField";
      public static readonly string MiniToolbarButton = "MiniToolbarButton";
      public static readonly string MiniToolbarButtonLeft = "MiniToolbarButtonLeft";
      public static readonly string MiniToolbarPopup = "MiniToolbarPopup";
      public static readonly string MinMaxHorizontalSliderThumb = "MinMaxHorizontalSliderThumb";
      public static readonly string NotificationBackground = "NotificationBackground";
      public static readonly string NotificationText = "NotificationText";
      public static readonly string ObjectField = "ObjectField";
      public static readonly string ObjectFieldThumb = "ObjectFieldThumb";
      public static readonly string ObjectFieldThumbOverlay = "ObjectFieldThumbOverlay";
      public static readonly string ObjectFieldThumbOverlay2 = "ObjectFieldThumbOverlay2";
      public static readonly string ObjectPickerBackground = "ObjectPickerBackground";
      public static readonly string ObjectPickerGroupHeader = "ObjectPickerGroupHeader";
      public static readonly string ObjectPickerLargeStatus = "ObjectPickerLargeStatus";
      public static readonly string ObjectPickerPreviewBackground = "ObjectPickerPreviewBackground";
      public static readonly string ObjectPickerResultsEven = "ObjectPickerResultsEven";
      public static readonly string ObjectPickerResultsGrid = "ObjectPickerResultsGrid";
      public static readonly string ObjectPickerResultsGridLabel = "ObjectPickerResultsGridLabel";
      public static readonly string ObjectPickerResultsOdd = "ObjectPickerResultsOdd";
      public static readonly string ObjectPickerSmallStatus = "ObjectPickerSmallStatus";
      public static readonly string ObjectPickerTab = "ObjectPickerTab";
      public static readonly string ObjectPickerToolbar = "ObjectPickerToolbar";
      public static readonly string OL_box = "OL box";
      public static readonly string OL_box_NoExpand = "OL box NoExpand";
      public static readonly string OL_Elem = "OL Elem";
      public static readonly string OL_EntryBackEven = "OL EntryBackEven";
      public static readonly string OL_EntryBackOdd = "OL EntryBackOdd";
      public static readonly string OL_header = "OL header";
      public static readonly string OL_Label = "OL Label";
      public static readonly string OL_Minus = "OL Minus";
      public static readonly string OL_Plus = "OL Plus";
      public static readonly string OL_TextField = "OL TextField";
      public static readonly string OL_Title = "OL Title";
      public static readonly string OL_Title_TextRight = "OL Title TextRight";
      public static readonly string OL_Titleleft = "OL Titleleft";
      public static readonly string OL_Titlemid = "OL Titlemid";
      public static readonly string OL_Titleright = "OL Titleright";
      public static readonly string OL_Toggle = "OL Toggle";
      public static readonly string OL_ToggleWhite = "OL ToggleWhite";
      public static readonly string PaneOptions = "PaneOptions";
      public static readonly string PlayerSettingsLevel = "PlayerSettingsLevel";
      public static readonly string PlayerSettingsPlatform = "PlayerSettingsPlatform";
      public static readonly string Popup = "Popup";
      public static readonly string PopupBackground = "PopupBackground";
      public static readonly string PopupCurveDropdown = "PopupCurveDropdown";
      public static readonly string PopupCurveEditorBackground = "PopupCurveEditorBackground";
      public static readonly string PopupCurveEditorSwatch = "PopupCurveEditorSwatch";
      public static readonly string PopupCurveSwatchBackground = "PopupCurveSwatchBackground";
      public static readonly string PR_DigDownArrow = "PR DigDownArrow";
      public static readonly string PR_Insertion = "PR Insertion";
      public static readonly string PR_Label = "PR Label";
      public static readonly string PR_Ping = "PR Ping";
      public static readonly string PR_TextField = "PR TextField";
      public static readonly string PreBackground = "PreBackground";
      public static readonly string PreButton = "PreButton";
      public static readonly string PreferencesKeysElement = "PreferencesKeysElement";
      public static readonly string PreferencesSection = "PreferencesSection";
      public static readonly string PreferencesSectionBox = "PreferencesSectionBox";
      public static readonly string PreHorizontalScrollbar = "PreHorizontalScrollbar";
      public static readonly string PreHorizontalScrollbarThumb = "PreHorizontalScrollbarThumb";
      public static readonly string PreLabel = "PreLabel";
      public static readonly string PreOverlayLabel = "PreOverlayLabel";
      public static readonly string PreSlider = "PreSlider";
      public static readonly string PreSliderThumb = "PreSliderThumb";
      public static readonly string PreToolbar = "PreToolbar";
      public static readonly string PreToolbar2 = "PreToolbar2";
      public static readonly string PreVerticalScrollbar = "PreVerticalScrollbar";
      public static readonly string PreVerticalScrollbarThumb = "PreVerticalScrollbarThumb";
      public static readonly string ProfilerBadge = "ProfilerBadge";
      public static readonly string ProfilerLeftPane = "ProfilerLeftPane";
      public static readonly string ProfilerLeftPaneOverlay = "ProfilerLeftPaneOverlay";
      public static readonly string ProfilerPaneLeftBackground = "ProfilerPaneLeftBackground";
      public static readonly string ProfilerPaneSubLabel = "ProfilerPaneSubLabel";
      public static readonly string ProfilerRightPane = "ProfilerRightPane";
      public static readonly string ProfilerScrollviewBackground = "ProfilerScrollviewBackground";
      public static readonly string ProfilerSelectedLabel = "ProfilerSelectedLabel";
      public static readonly string ProgressBarBack = "ProgressBarBack";
      public static readonly string ProgressBarBar = "ProgressBarBar";
      public static readonly string ProgressBarText = "ProgressBarText";
      public static readonly string ProjectBrowserBottomBarBg = "ProjectBrowserBottomBarBg";
      public static readonly string ProjectBrowserGridLabel = "ProjectBrowserGridLabel";
      public static readonly string ProjectBrowserHeaderBgMiddle = "ProjectBrowserHeaderBgMiddle";
      public static readonly string ProjectBrowserHeaderBgTop = "ProjectBrowserHeaderBgTop";
      public static readonly string ProjectBrowserIconAreaBg = "ProjectBrowserIconAreaBg";
      public static readonly string ProjectBrowserIconDropShadow = "ProjectBrowserIconDropShadow";
      public static readonly string ProjectBrowserPreviewBg = "ProjectBrowserPreviewBg";
      public static readonly string ProjectBrowserSubAssetBg = "ProjectBrowserSubAssetBg";
      public static readonly string ProjectBrowserSubAssetBgCloseEnded = "ProjectBrowserSubAssetBgCloseEnded";
      public static readonly string ProjectBrowserSubAssetBgDivider = "ProjectBrowserSubAssetBgDivider";
      public static readonly string ProjectBrowserSubAssetBgMiddle = "ProjectBrowserSubAssetBgMiddle";
      public static readonly string ProjectBrowserSubAssetBgOpenEnded = "ProjectBrowserSubAssetBgOpenEnded";
      public static readonly string ProjectBrowserSubAssetExpandBtn = "ProjectBrowserSubAssetExpandBtn";
      public static readonly string ProjectBrowserTopBarBg = "ProjectBrowserTopBarBg";
      public static readonly string QualitySettingsDefault = "QualitySettingsDefault";
      public static readonly string Radio = "Radio";
      public static readonly string RightLabel = "RightLabel";
      public static readonly string RL_Background = "RL Background";
      public static readonly string RL_DragHandle = "RL DragHandle";
      public static readonly string RL_Element = "RL Element";
      public static readonly string RL_Footer = "RL Footer";
      public static readonly string RL_FooterButton = "RL FooterButton";
      public static readonly string RL_Header = "RL Header";
      public static readonly string SC_ViewAxisLabel = "SC ViewAxisLabel";
      public static readonly string SC_ViewLabel = "SC ViewLabel";
      public static readonly string SceneViewOverlayTransparentBackground = "SceneViewOverlayTransparentBackground";
      public static readonly string ScriptText = "ScriptText";
      public static readonly string SearchCancelButton = "SearchCancelButton";
      public static readonly string SearchCancelButtonEmpty = "SearchCancelButtonEmpty";
      public static readonly string SearchModeFilter = "SearchModeFilter";
      public static readonly string SearchTextField = "SearchTextField";
      public static readonly string SelectionRect = "SelectionRect";
      public static readonly string ServerChangeCount = "ServerChangeCount";
      public static readonly string ServerUpdateChangeset = "ServerUpdateChangeset";
      public static readonly string ServerUpdateChangesetOn = "ServerUpdateChangesetOn";
      public static readonly string ServerUpdateInfo = "ServerUpdateInfo";
      public static readonly string ServerUpdateLog = "ServerUpdateLog";
      public static readonly string ShurikenCheckMark = "ShurikenCheckMark";
      public static readonly string ShurikenEffectBg = "ShurikenEffectBg";
      public static readonly string ShurikenEmitterTitle = "ShurikenEmitterTitle";
      public static readonly string ShurikenLabel = "ShurikenLabel";
      public static readonly string ShurikenLine = "ShurikenLine";
      public static readonly string ShurikenMinus = "ShurikenMinus";
      public static readonly string ShurikenModuleBg = "ShurikenModuleBg";
      public static readonly string ShurikenModuleTitle = "ShurikenModuleTitle";
      public static readonly string ShurikenObjectField = "ShurikenObjectField";
      public static readonly string ShurikenPlus = "ShurikenPlus";
      public static readonly string ShurikenPopUp = "ShurikenPopUp";
      public static readonly string ShurikenToggle = "ShurikenToggle";
      public static readonly string ShurikenValue = "ShurikenValue";
      public static readonly string SimplePopup = "SimplePopup";
      public static readonly string SliderMixed = "SliderMixed";
      public static readonly string StaticDropdown = "StaticDropdown";
      public static readonly string sv_iconselector_back = "sv_iconselector_back";
      public static readonly string sv_iconselector_button = "sv_iconselector_button";
      public static readonly string sv_iconselector_labelselection = "sv_iconselector_labelselection";
      public static readonly string sv_iconselector_selection = "sv_iconselector_selection";
      public static readonly string sv_iconselector_sep = "sv_iconselector_sep";
      public static readonly string sv_label_0 = "sv_label_0";
      public static readonly string sv_label_1 = "sv_label_1";
      public static readonly string sv_label_2 = "sv_label_2";
      public static readonly string sv_label_3 = "sv_label_3";
      public static readonly string sv_label_4 = "sv_label_4";
      public static readonly string sv_label_5 = "sv_label_5";
      public static readonly string sv_label_6 = "sv_label_6";
      public static readonly string sv_label_7 = "sv_label_7";
      public static readonly string TabWindowBackground = "TabWindowBackground";
      public static readonly string Tag_MenuItem = "Tag MenuItem";
      public static readonly string Tag_TextField = "Tag TextField";
      public static readonly string Tag_TextField_Button = "Tag TextField Button";
      public static readonly string Tag_TextField_Empty = "Tag TextField Empty";
      public static readonly string TE_NodeBackground = "TE NodeBackground";
      public static readonly string TE_NodeBox = "TE NodeBox";
      public static readonly string TE_NodeBoxSelected = "TE NodeBoxSelected";
      public static readonly string TE_NodeLabelBot = "TE NodeLabelBot";
      public static readonly string TE_NodeLabelTop = "TE NodeLabelTop";
      public static readonly string TE_PinLabel = "TE PinLabel";
      public static readonly string TE_Toolbar = "TE Toolbar";
      public static readonly string TE_toolbarbutton = "TE toolbarbutton";
      public static readonly string TE_ToolbarDropDown = "TE ToolbarDropDown";
      public static readonly string TimeScrubber = "TimeScrubber";
      public static readonly string TimeScrubberButton = "TimeScrubberButton";
      public static readonly string TL_BaseStateLogicBarOverlay = "TL BaseStateLogicBarOverlay";
      public static readonly string TL_EndPoint = "TL EndPoint";
      public static readonly string TL_InPoint = "TL InPoint";
      public static readonly string TL_ItemTitle = "TL ItemTitle";
      public static readonly string TL_LeftColumn = "TL LeftColumn";
      public static readonly string TL_LeftItem = "TL LeftItem";
      public static readonly string TL_LogicBar_0 = "TL LogicBar 0";
      public static readonly string TL_LogicBar_1 = "TL LogicBar 1";
      public static readonly string TL_LogicBar_parentgrey = "TL LogicBar parentgrey";
      public static readonly string TL_LoopSection = "TL LoopSection";
      public static readonly string TL_OutPoint = "TL OutPoint";
      public static readonly string TL_Playhead = "TL Playhead";
      public static readonly string TL_Range_Overlay = "TL Range Overlay";
      public static readonly string TL_RightLine = "TL RightLine";
      public static readonly string TL_Selection_H1 = "TL Selection H1";
      public static readonly string TL_Selection_H2 = "TL Selection H2";
      public static readonly string TL_SelectionBarCloseButton = "TL SelectionBarCloseButton";
      public static readonly string TL_SelectionBarPreview = "TL SelectionBarPreview";
      public static readonly string TL_SelectionBarText = "TL SelectionBarText";
      public static readonly string TL_SelectionButton = "TL SelectionButton";
      public static readonly string TL_SelectionButton_PreDropGlow = "TL SelectionButton PreDropGlow";
      public static readonly string TL_SelectionButtonName = "TL SelectionButtonName";
      public static readonly string TL_SelectionButtonNew = "TL SelectionButtonNew";
      public static readonly string TL_tab_left = "TL tab left";
      public static readonly string TL_tab_mid = "TL tab mid";
      public static readonly string TL_tab_plus_left = "TL tab plus left";
      public static readonly string TL_tab_plus_right = "TL tab plus right";
      public static readonly string TL_tab_right = "TL tab right";
      public static readonly string ToggleMixed = "ToggleMixed";
      public static readonly string Toolbar = "Toolbar";
      public static readonly string toolbarbutton = "toolbarbutton";
      public static readonly string ToolbarDropDown = "ToolbarDropDown";
      public static readonly string ToolbarPopup = "ToolbarPopup";
      public static readonly string ToolbarSeachCancelButton = "ToolbarSeachCancelButton";
      public static readonly string ToolbarSeachCancelButtonEmpty = "ToolbarSeachCancelButtonEmpty";
      public static readonly string ToolbarSeachTextField = "ToolbarSeachTextField";
      public static readonly string ToolbarSeachTextFieldPopup = "ToolbarSeachTextFieldPopup";
      public static readonly string ToolbarSearchField = "ToolbarSearchField";
      public static readonly string ToolbarTextField = "ToolbarTextField";
      public static readonly string Tooltip = "Tooltip";
      public static readonly string U2D_createRect = "U2D.createRect";
      public static readonly string U2D_dragDot = "U2D.dragDot";
      public static readonly string U2D_dragDotDimmed = "U2D.dragDotDimmed";
      public static readonly string VCS_StickyNote = "VCS_StickyNote";
      public static readonly string VCS_StickyNoteArrow = "VCS_StickyNoteArrow";
      public static readonly string VCS_StickyNoteLabel = "VCS_StickyNoteLabel";
      public static readonly string VCS_StickyNoteP4 = "VCS_StickyNoteP4";
      public static readonly string VerticalMinMaxScrollbarThumb = "VerticalMinMaxScrollbarThumb";
      public static readonly string VisibilityToggle = "VisibilityToggle";
      public static readonly string WhiteBoldLabel = "WhiteBoldLabel";
      public static readonly string WhiteLabel = "WhiteLabel";
      public static readonly string WhiteLargeLabel = "WhiteLargeLabel";
      public static readonly string WhiteMiniLabel = "WhiteMiniLabel";
      public static readonly string WinBtnCloseActiveMac = "WinBtnCloseActiveMac";
      public static readonly string WinBtnCloseMac = "WinBtnCloseMac";
      public static readonly string WinBtnCloseWin = "WinBtnCloseWin";
      public static readonly string WinBtnInactiveMac = "WinBtnInactiveMac";
      public static readonly string WinBtnMaxActiveMac = "WinBtnMaxActiveMac";
      public static readonly string WinBtnMaxMac = "WinBtnMaxMac";
      public static readonly string WinBtnMaxWin = "WinBtnMaxWin";
      public static readonly string WinBtnMinActiveMac = "WinBtnMinActiveMac";
      public static readonly string WinBtnMinMac = "WinBtnMinMac";
      public static readonly string WinBtnMinWin = "WinBtnMinWin";
      public static readonly string WindowBackground = "WindowBackground";
      public static readonly string WindowBottomResize = "WindowBottomResize";
      public static readonly string WindowResizeMac = "WindowResizeMac";
      public static readonly string Wizard_Box = "Wizard Box";
      public static readonly string Wizard_Error = "Wizard Error";
      public static readonly string WordWrapLabel = "WordWrapLabel";
      public static readonly string WordWrappedLabel = "WordWrappedLabel";
      public static readonly string WordWrappedMiniLabel = "WordWrappedMiniLabel";
      public static readonly string WrappedLabel = "WrappedLabel";
    }

  }
}
