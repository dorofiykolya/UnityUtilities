using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utils.Editor
{
    public class InspectorStyles
    {
        public static readonly string MiniToolbarButton = "MiniToolbarButton";
        public static readonly string MiniToolbarButtonLeft = "MiniToolbarButtonLeft";
        public static readonly string flow_background = "flow background";
        public static readonly string toolbarbutton = "toolbarbutton";
        public static readonly string minibutton = "minibutton";
        public static readonly string minibuttonleft = "minibuttonleft";
        public static readonly string minibuttonmid = "minibuttonmid";
        public static readonly string minibuttonright = "minibuttonright";
    }

    public class InspectorComponentsEditorWindow : EditorWindow
    {
        private static Object _target;
        private static Object[] _selectedObjects;
        private static bool[] _selectedObjectsFoldout;

        [MenuItem("Window/Inspector")]
        public static void Open()
        {
            GetWindow<InspectorComponentsEditorWindow>("Inspector").Show(true);
        }

        public void OnDisable()
        {
            if (EditorApplication.update != null)
            {
                EditorApplication.update -= Repaint;
            }
        }

        public void OnEnable()
        {
            EditorApplication.update += Repaint;
        }

        private bool IsEquals(Object[] first, Object[] second)
        {
            if (first != null && second != null && first.Length == second.Length)
            {
                for (int i = 0; i < first.Length; i++)
                {
                    if (first[i] != second[i]) return false;
                }
                return true;
            }
            return false;
        }

        private void OnGUI()
        {
            _target = EditorGUILayout.ObjectField("select object or component:", _target, typeof(Object), true);
            GUI.enabled = _target != null;
            if (GUILayout.Button("Inspect"))
            {
                InspectorEditorWindow.Open(_target, _target.ToString(), true, true);
            }
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            var objects = Selection.objects;
            if (!IsEquals(objects, _selectedObjects))
            {
                _selectedObjects = Selection.objects;
                _selectedObjectsFoldout = new bool[_selectedObjects.Length];
            }

            Object inspectTarget = null;
            var index = 0;
            foreach (var target in objects)
            {
                _selectedObjectsFoldout[index] = InspectorEditorHelper.FoldoutHeader(target.ToString(), _selectedObjectsFoldout[index]);
                if (_selectedObjectsFoldout[index])
                {
                    InspectorEditorHelper.BeginVertical();
                    if (GUILayout.Button("Inspect", InspectorStyles.minibuttonmid))
                    {
                        inspectTarget = target;
                        break;
                    }
                    var go = target as GameObject;
                    var stoped = false;
                    if (go != null)
                    {
                        GUILayout.Label("Inspect components:");
                        EditorGUILayout.BeginHorizontal(EditorStyles.label);
                        EditorGUILayout.Space();
                        EditorGUILayout.BeginVertical();
                        foreach (var component in go.GetComponents(typeof(Component)))
                        {
                            if (component != null)
                            {
                                if (GUILayout.Button(component.ToString(), InspectorStyles.minibuttonmid))
                                {
                                    inspectTarget = component;
                                    stoped = true;
                                    break;
                                }
                            }
                            else
                            {
                                GUILayout.Label("missing script", InspectorStyles.flow_background);
                            }
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }
                    InspectorEditorHelper.EndVertical();
                    if (stoped) break;
                }
                index++;
            }
            if (inspectTarget != null)
            {
                InspectorEditorWindow.Open(inspectTarget, inspectTarget.ToString(), true, true);
            }
        }
    }

    public class InspectorEditorWindow : EditorWindow
    {
        public static void Open(object target, string name = null, bool privateMembers = false, bool newInstance = false)
        {
            var window = newInstance ? CreateInstance<InspectorEditorWindow>() : GetWindow<InspectorEditorWindow>();
            window.titleContent = new GUIContent(target.ToString());
            window._target = target;
            window._name = name;
            window._privateMembers = privateMembers;
            window._path = null;
            window.Show(true);
        }

        private PathValue[] _path;
        private object _target;
        private SortBy _sortBy;
        private string _name;
        private bool _privateMembers;

        private void OnGUI()
        {
            if (_path == null)
            {
                _path = new[] { Inspect(_name ?? "root", _target, _privateMembers) };
            }
            GUI.enabled = _path != null || _path.Length == 0;
            DrawPath();
            ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);
            DrawContent();
            EditorGUILayout.EndScrollView();
            GUI.enabled = true;
        }

        private Vector2 ScrollPosition
        {
            get
            {
                if (_path != null && _path.Length != 0)
                {
                    return _path[_path.Length - 1].scroll;
                }
                return Vector2.zero;
            }
            set
            {
                if (_path != null && _path.Length != 0)
                {
                    var info = _path[_path.Length - 1];
                    info.scroll = value;
                    _path[_path.Length - 1] = info;
                }
            }
        }

        private static PathValue Inspect(string targetName, object target, bool privateMembers)
        {
            targetName = ObjectToString(targetName);

            if (target != null)
            {
                var type = target as Type ?? target.GetType();

                if (type.IsPrimitive || target is String)
                {
                    return new PathValue { name = targetName, value = target, message = ObjectToString(target) };
                }
                if (target is Array)
                {
                    var array = target as Array;
                    var path = new PathValue[array.Length];

                    if (array.Rank == 1)
                    {
                        for (int i = 0; i < array.Length; i++)
                        {
                            var message = ObjectToString(array, i);
                            path[i] = new PathValue
                            {
                                index = i,
                                name = i.ToString(),
                                message = message,
                                value = array.GetValue(i)
                            };
                        }
                    }
                    else if (array.Rank == 2)
                    {
                        for (int i = 0; i < array.GetLength(0); i++)
                        {
                            for (int j = 0; j < array.GetLength(1); j++)
                            {
                                var message = ObjectToString(array, i, j);
                                var index = i * array.GetLength(0) + j;
                                path[index] = new PathValue
                                {
                                    index = index,
                                    name = string.Concat(i, ",", j),
                                    message = message,
                                    value = array.GetValue(i, j)
                                };
                            }
                        }
                    }

                    return new PathValue { name = targetName, value = path, message = Convert.ToString(target) };
                }
                if (target is IDictionary)
                {
                    var dict = target as IDictionary;
                    var path = new PathValue[dict.Count];
                    var index = 0;
                    var keys = dict.Keys;
                    var keyList = new List<object>(keys.Count);
                    foreach (var key in keys)
                    {
                        keyList.Add(key);
                    }
                    keyList.Sort(new KeyComparer<object>());
                    foreach (var key in keyList)
                    {
                        var value = dict[key];
                        path[index] = new PathValue
                        {
                            index = index,
                            name = ObjectToString(key),
                            message = ObjectToString(value),
                            value = value
                        };
                        index++;
                    }
                    return new PathValue { name = targetName, value = path, message = Convert.ToString(target) };
                }
                if (target is ICollection)
                {
                    var array = target as ICollection;
                    var path = new PathValue[array.Count];
                    var index = 0;
                    foreach (var e in array)
                    {
                        path[index] = new PathValue
                        {
                            index = index,
                            name = index.ToString(),
                            message = ObjectToString(e),
                            value = e
                        };
                        index++;
                    }
                    return new PathValue { name = targetName, value = path, message = Convert.ToString(target) };
                }
                if (target is IEnumerator)
                {
                    var enumerator = target as IEnumerator;
                    var path = new List<PathValue>();
                    var index = 0;
                    while (enumerator.MoveNext())
                    {
                        var e = enumerator.Current;
                        path.Add(new PathValue
                        {
                            index = index,
                            name = index.ToString(),
                            message = ObjectToString(e),
                            value = e
                        });
                        index++;
                    }
                    return new PathValue { name = targetName, value = path, message = Convert.ToString(target) };
                }
                else
                {
                    FieldInfo[] fields = GetFields(type, privateMembers);
                    Array.Sort(fields, new MemberComparer<FieldInfo>());

                    BindingFlags propertyFlagsflags = BindingFlags.Instance | BindingFlags.Public |
                                                      BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.InvokeMethod;
                    if (privateMembers) propertyFlagsflags |= BindingFlags.NonPublic;

                    PropertyInfo[] property = type.GetProperties(propertyFlagsflags);
                    Array.Sort(property, new MemberComparer<PropertyInfo>());

                    MethodInfo[] methods = type.GetMethods(propertyFlagsflags).Where(m => m.GetGenericArguments().Length == 0 && m.GetParameters().Length == 0 && m.ReturnType != typeof(void)).ToArray();
                    Array.Sort(methods, new MemberComparer<MethodInfo>());

                    var enumerable = target as IEnumerable;
                    var component = target as Component;
                    var gameObject = target as GameObject;
                    var additional = 0;
                    if (enumerable != null) additional++;
                    if (component != null) additional += 2;
                    if (gameObject != null) additional += 2;
                    var path = new PathValue[fields.Length + property.Length + additional + methods.Length];
                    var index = 0;
                    foreach (var fieldInfo in fields)
                    {
                        object value;
                        try
                        {
                            value = fieldInfo.GetValue(target);
                        }
                        catch (Exception e)
                        {
                            value = string.Format("{0}: {1}", e.GetType().Name, e.Message);
                        }
                        path[index] = new PathValue
                        {
                            index = index,
                            name = ObjectToString(fieldInfo.Name),
                            message = ObjectToString(value),
                            value = value
                        };
                        index++;
                    }
                    foreach (var propertyInfo in property)
                    {
                        object value;
                        try
                        {
                            value = propertyInfo.GetValue(target, null);
                        }
                        catch (Exception e)
                        {
                            value = string.Format("{0}: {1}", e.GetType().Name, e.Message);
                        }
                        path[index] = new PathValue
                        {
                            index = index,
                            name = ObjectToString(propertyInfo.Name),
                            message = ObjectToString(value),
                            value = value
                        };
                        index++;
                    }
                    foreach (var methodInfo in methods)
                    {
                        object value;
                        try
                        {
                            value = methodInfo.Invoke(target, null);
                        }
                        catch (Exception e)
                        {
                            value = string.Format("{0}: {1}", e.GetType().Name, e.Message);
                        }
                        path[index] = new PathValue
                        {
                            index = index,
                            name = ObjectToString(methodInfo.Name) + "():" + methodInfo.ReturnType,
                            message = ObjectToString(value),
                            value = value
                        };
                        index++;
                    }
                    if (enumerable != null)
                    {
                        object value;
                        try
                        {
                            value = enumerable.GetEnumerator();
                        }
                        catch (Exception e)
                        {
                            value = string.Format("{0}: {1}", e.GetType().Name, e.Message);
                        }
                        path[index] = new PathValue
                        {
                            index = index,
                            name = ObjectToString("GetEnumerator()"),
                            message = ObjectToString(value),
                            value = value
                        };
                        index++;
                    }

                    if (component != null)
                    {
                        object value;
                        try
                        {
                            value = component.GetComponents(typeof(Component));
                        }
                        catch (Exception e)
                        {
                            value = string.Format("{0}: {1}", e.GetType().Name, e.Message);
                        }
                        path[index] = new PathValue
                        {
                            index = index,
                            name = ObjectToString("GetComponents()"),
                            message = ObjectToString(value),
                            value = value
                        };
                        index++;

                        try
                        {
                            value = component.GetComponentsInChildren(typeof(Component));
                        }
                        catch (Exception e)
                        {
                            value = string.Format("{0}: {1}", e.GetType().Name, e.Message);
                        }
                        path[index] = new PathValue
                        {
                            index = index,
                            name = ObjectToString("GetComponentsInChildren()"),
                            message = ObjectToString(value),
                            value = value
                        };
                        index++;
                    }

                    if (gameObject != null)
                    {
                        object value;
                        try
                        {
                            value = gameObject.GetComponents(typeof(Component));
                        }
                        catch (Exception e)
                        {
                            value = string.Format("{0}: {1}", e.GetType().Name, e.Message);
                        }
                        path[index] = new PathValue
                        {
                            index = index,
                            name = ObjectToString("GetComponents()"),
                            message = ObjectToString(value),
                            value = value
                        };
                        index++;

                        try
                        {
                            value = gameObject.GetComponentsInChildren(typeof(Component));
                        }
                        catch (Exception e)
                        {
                            value = string.Format("{0}: {1}", e.GetType().Name, e.Message);
                        }
                        path[index] = new PathValue
                        {
                            index = index,
                            name = ObjectToString("GetComponentsInChildren()"),
                            message = ObjectToString(value),
                            value = value
                        };
                        index++;
                    }

                    return new PathValue { name = targetName, value = path, message = ObjectToString(target) };
                }
            }
            return new PathValue();
        }

        private static FieldInfo[] GetFields(Type type, bool privateMembers)
        {
            BindingFlags fieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField |
                                      BindingFlags.SetField | BindingFlags.DeclaredOnly;
            if (privateMembers) fieldFlags |= BindingFlags.NonPublic;

            var result = new List<FieldInfo>();
            var target = type;
            while (target != null && target != typeof(object))
            {
                result.AddRange(target.GetFields(fieldFlags));
                target = target.BaseType;
            }
            return result.ToArray();
        }

        private void DrawPath()
        {
            float posX = 0f;
            var content = new GUIContent(_name);
            var size = EditorStyles.miniButtonMid.CalcSize(content);
            var rect = EditorGUILayout.GetControlRect(true, size.y);
            GUI.Box(rect, "");
            if (_path == null)
            {
                rect.x = 0;
                rect.width = size.x;
                rect.height = size.y;
                GUI.Button(rect, content, EditorStyles.miniButtonMid);
                return;
            }
            for (int i = 0; i < _path.Length; i++)
            {
                content.text = "\u25BA " + _path[i].name;
                size = EditorStyles.miniButtonMid.CalcSize(content);
                rect.x = posX;
                rect.width = size.x;
                rect.height = size.y;
                var lastColor = GUI.color;
                if (i == _path.Length - 1)
                {
                    GUI.color = Color.green;
                }
                else if (i == 0)
                {
                    GUI.color = Color.cyan;
                }
                var click = GUI.Button(rect, content, EditorStyles.miniButtonMid);
                GUI.color = lastColor;
                posX += size.x;
                if (click)
                {
                    Array.Resize(ref _path, i + 1);
                    if (_path.Length == 1)
                    {
                        _sortBy = SortBy.NAME_UP;
                    }
                    break;
                }
            }
        }

        private void DrawContentHeader()
        {
            float posX = 0f;
            var content = new GUIContent("");
            GUIStyle flow_background = InspectorStyles.flow_background;
            var size = flow_background.CalcSize(content);
            var rect = EditorGUILayout.GetControlRect(true, size.y);
            var totalRect = rect;
            rect.x = 0;
            rect.y -= 1;
            rect.width += 4;
            GUI.Box(rect, content, flow_background);

            GUIStyle toolbarbutton = InspectorStyles.toolbarbutton;
            size = toolbarbutton.CalcSize(content);
            rect = EditorGUILayout.GetControlRect(true, size.y);
            rect.x = 0;
            rect.y -= 2;
            rect.width += 4;
            GUI.Box(rect, content, toolbarbutton);

            content.text = "NAME ";
            if (_sortBy == SortBy.NAME_UP) content.text += "▲";
            if (_sortBy == SortBy.NAME_DOWN) content.text += "▼";
            rect.width = 300f;
            rect.x = posX + 1;
            posX = rect.xMax;
            if (GUI.Button(rect, content, InspectorStyles.MiniToolbarButton))
            {
                if (_sortBy == SortBy.NAME_DOWN) _sortBy = SortBy.NAME_UP;
                else if (_sortBy == SortBy.NAME_UP) _sortBy = SortBy.NAME_DOWN;
                else _sortBy = SortBy.NAME_UP;
            }

            content.text = "VALUE ";
            if (_sortBy == SortBy.VALUE_UP) content.text += "▲";
            if (_sortBy == SortBy.VALUE_DOWN) content.text += "▼";
            rect.width = totalRect.width - rect.xMax + 2;
            rect.x = posX + 1;
            if (GUI.Button(rect, content, InspectorStyles.MiniToolbarButton))
            {
                if (_sortBy == SortBy.VALUE_DOWN) _sortBy = SortBy.VALUE_UP;
                else if (_sortBy == SortBy.VALUE_UP) _sortBy = SortBy.VALUE_DOWN;
                else _sortBy = SortBy.VALUE_UP;
            }
        }

        private void DrawContent()
        {
            DrawContentHeader();
            EditorGUILayout.BeginVertical();
            DrawValueContent();
            EditorGUILayout.EndVertical();
        }

        private void DrawValueContent()
        {
            var current = _path.Last();
            var enumerable = current.value as IEnumerable<PathValue>;
            if (enumerable != null)
            {
                var sorted = Sort(enumerable, _sortBy);
                var index = true;

                var content = new GUIContent("");
                GUIStyle minibuttonmid = InspectorStyles.minibuttonmid;
                var size = minibuttonmid.CalcSize(content);
                var rect = EditorGUILayout.GetControlRect(true, size.y);
                var totalRect = rect;
                rect.x = 0f;
                rect.y -= 1f;
                rect.height += 3f;
                rect.width += 4f;

                SaveLastColor();

                var lastAlign = GUI.skin.label.alignment;

                foreach (var value in sorted)
                {
                    if (index) RestoreLastColor();
                    else
                    {
                        SaveLastColor();
                        GUI.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                    }
                    GUI.Box(rect, content, EditorStyles.helpBox);
                    RestoreLastColor();

                    var labelRect = rect;
                    labelRect.width = 120f;
                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                    labelRect.width = 300f;
                    if (GUI.Button(labelRect, value.name, EditorStyles.label))
                    {
                        AddPath(value);
                    }
                    var valueRect = labelRect;
                    valueRect.x = labelRect.xMax;
                    valueRect.width = totalRect.width - valueRect.x;
                    EditorGUI.TextField(valueRect, value.message, EditorStyles.label);
                    rect = EditorGUILayout.GetControlRect(true, size.y);
                    rect.x = 0;
                    rect.y -= 1;
                    rect.height += 3;
                    rect.width += 4;

                    index = !index;
                }
                GUI.skin.label.alignment = lastAlign;
                RestoreLastColor();
            }
        }

        private void SaveLastColor()
        {
            InspectorEditorHelper.PushColor();
        }

        private void RestoreLastColor()
        {
            InspectorEditorHelper.PopColor();
        }


        private void AddPath(PathValue value)
        {
            if (value.value != null && !value.value.GetType().IsPrimitive && !(value.value is string))
            {
                Array.Resize(ref _path, _path.Length + 1);
                _path[_path.Length - 1] = Inspect(value.name, value.value, _privateMembers);
                _sortBy = SortBy.NAME_UP;
            }
        }

        private static IEnumerable<PathValue> Sort(IEnumerable<PathValue> array, SortBy sortBy)
        {
            var list = new List<PathValue>(array);
            list.Sort((p1, p2) =>
            {
                switch (sortBy)
                {
                    case SortBy.NAME_UP:
                        if (p1.index != p2.index) return p1.index > p2.index ? 1 : -1;
                        return string.Compare(p1.name, p2.name);
                    case SortBy.NAME_DOWN:
                        if (p1.index != p2.index) return p1.index > p2.index ? -1 : 1;
                        return string.Compare(p2.name, p1.name);
                    case SortBy.VALUE_UP:
                        return string.Compare(p1.value.ToString(), p2.value.ToString());
                    case SortBy.VALUE_DOWN:
                        return string.Compare(p2.value.ToString(), p1.value.ToString());
                }
                return 0;
            });
            return list;
        }

        private static string ObjectToString(object value)
        {
            string result;
            try
            {
                result = Regex.Unescape(Convert.ToString(value));
            }
            catch (Exception e)
            {
                result = e.Message;
            }
            return result;
        }

        private static string ObjectToString(Array array, int index)
        {
            string result;
            try
            {
                result = Regex.Unescape(Convert.ToString(array.GetValue(index)));
            }
            catch (Exception e)
            {
                result = string.Concat("[Exception] ObjectToString array:", Convert.ToString(array), ", index:",
                    index.ToString(), ", ", e.Message);
            }
            return result;
        }

        private static string ObjectToString(Array array, int index, int index2)
        {
            string result;
            try
            {
                result = Regex.Unescape(Convert.ToString(array.GetValue(index, index2)));
            }
            catch (Exception e)
            {
                result = string.Concat("[Exception] ObjectToString array:", Convert.ToString(array), ", index:",
                    index.ToString(), ", index2:", index2.ToString(), ", ", e.Message);
            }
            return result;
        }

        private struct PathValue
        {
            public int index;
            public string name;
            public object value;
            public string message;
            public Vector2 scroll;
        }

        private enum SortBy
        {
            NAME_UP,
            NAME_DOWN,
            VALUE_UP,
            VALUE_DOWN,
        }

        public class MemberComparer<T> : IComparer<T> where T : MemberInfo
        {
            public int Compare(T x, T y)
            {
                return String.CompareOrdinal(x.Name, y.Name);
            }
        }

        public class KeyComparer<T> : IComparer<T>
        {
            public int Compare(T x, T y)
            {
                return String.CompareOrdinal(x.ToString(), y.ToString());
            }
        }
    }

    class InspectorEditorHelper
    {
        struct ColorInfo
        {
            public Color color;
            public Color backgroundColor;
            public Color contentColor;
        }

        private readonly static Stack<ColorInfo> _colorsStack = new Stack<ColorInfo>();

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

        public static void Header(string text)
        {
            GUILayout.BeginHorizontal();
            text = "<b><size=11>" + text + "</size></b>";
            GUILayout.Toggle(true, "☼ " + text, "dragtab", GUILayout.MinWidth(20f));
            GUILayout.EndHorizontal();
            GUILayout.Space(3f);
        }

        public static void BeginVertical(bool selected = true)
        {
            GUILayout.BeginHorizontal();
            if (selected) EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
            else EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
            GUILayout.BeginVertical();
            GUILayout.Space(2f);
        }

        public static bool FoldoutHeader(string text, bool enable)
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
            if (!enable) GUILayout.Space(3f);

            return enable;
        }

        public static void EndVertical()
        {
            GUILayout.Space(3f);
            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(3f);
            GUILayout.EndHorizontal();
            GUILayout.Space(3f);
        }

        public static void BeginHorizontal(bool selected = true)
        {
            GUILayout.BeginVertical();
            GUILayout.Space(2f);
            if (selected) EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
            else EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
            GUILayout.Space(2f);
        }

        public static void EndHorizontal()
        {
            GUILayout.Space(3f);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(3f);
            GUILayout.EndVertical();
            GUILayout.Space(3f);
        }
    }
}