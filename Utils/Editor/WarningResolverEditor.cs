using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Utils.Editor
{
  public class WarningResolverEditor
  {
    [MenuItem("Tools/Resolve Warnings")]
    private static void Resolve()
    {
      var logBuffer = EditorGUIUtility.systemCopyBuffer;
      if (!string.IsNullOrWhiteSpace(logBuffer))
      {
        var resolvers = new Dictionary<string, IWarningResolver>
        {
          {"CS0109", new CS0109()},
          {"CS0108", new CS0108()},
          {"CS0649", new CS0649()},
          {"CS0114", new CS0114()},
          {"CS0219", new CS0219()}
        };

        var builder = new CodeBuilder();
        var linesReader = new StringReader(logBuffer);
        string logLine;
        while ((logLine = linesReader.ReadLine()) != null)
        {
          var warning = builder.GetWarning(logLine);
          if (warning != null)
          {
            IWarningResolver resolver;
            if (resolvers.TryGetValue(warning.Code, out resolver))
            {
              if (resolver.Resolve(warning))
              {
                warning.Line.File.SetDirty();
              }
            }
          }
        }

        var files = builder.Resolve();

        foreach (var fileText in files)
        {
          File.WriteAllText(fileText.Path, fileText.Text);
        }

        Debug.Log("Warning files resolved: " + files.Length);
      }
    }

    class CodeBuilder
    {
      private static readonly Regex MatchLogLine =
        new Regex("\\(\\d+,\\d+\\,\\d+\\,\\d+\\): warning CS\\d+:", RegexOptions.Singleline);

      private static readonly Regex MatchCode = new Regex("warning CS\\d+:", RegexOptions.Singleline);
      private static readonly Regex MatchPosition = new Regex("\\(\\d+,\\d+\\,\\d+\\,\\d+\\)", RegexOptions.Singleline);

      private readonly Dictionary<string, WarningFile> _fileMap = new Dictionary<string, WarningFile>();

      public Warning GetWarning(string line)
      {
        if (!string.IsNullOrWhiteSpace(line))
        {
          if (line.Contains("Assets"))
          {
            var match = MatchLogLine.Match(line);
            if (match.Success)
            {
              var filePath = line.Substring(0, match.Index).Replace('\\', '/');
              var assetPath = Application.dataPath.Replace('\\', '/');
              var fIndex = filePath.LastIndexOf(assetPath);
              if (fIndex != -1)
              {
                filePath = "Assets" + filePath.Substring(fIndex + assetPath.Length,
                             filePath.Length - (fIndex + assetPath.Length));
              }

              if (File.Exists(filePath))
              {
                WarningFile warningFile;
                if (!_fileMap.TryGetValue(filePath, out warningFile))
                {
                  _fileMap[filePath] = warningFile = new WarningFile(filePath);
                }

                var matchCode = MatchCode.Match(match.Value).Value;
                var codeStartWith = "warning CS";
                var code = "CS" + matchCode.Substring(codeStartWith.Length,
                             matchCode.Length - 1 - codeStartWith.Length);
                var matchPosition = MatchPosition.Match(match.Value).Value;
                var linePos = matchPosition.Trim('(', ')').Split(',');
                var startLine = int.Parse(linePos[0].Trim()) - 1;
                var startPos = int.Parse(linePos[1].Trim()) - 1;
                var endLine = int.Parse(linePos[2].Trim()) - 1;
                var endPos = int.Parse(linePos[3].Trim()) - 1;

                var warningLine = warningFile.Line(startLine);
                var warning = warningLine.Warinig(code, startPos, endPos, line);

                return warning;
              }
            }
          }
        }

        return null;
      }

      public FileText[] Resolve()
      {
        var result = new List<FileText>();
        foreach (var warningFile in _fileMap.Values)
        {
          if (warningFile.IsDirty)
          {
            var lines = warningFile.Lines;
            var fileBuilder = new StringBuilder();
            for (var i = 0; i < lines.Length; i++)
            {
              if (warningFile.HasWarning(i))
              {
                var warningLine = warningFile.Line(i);

                foreach (var line in warningLine.Top)
                {
                  fileBuilder.AppendLine(line);
                }

                //left-right
                {
                  var line = lines[i];

                  if (warningLine.Removes.Count != 0)
                  {
                    var chars = new char[line.Length];
                    for (int charIndex = 0; charIndex < line.Length; charIndex++)
                    {
                      if (warningLine.Removes.Any(r => charIndex >= r.StartPosition && charIndex <= r.EndPosition))
                      {
                        chars[charIndex] = ' ';
                      }
                      else
                      {
                        chars[charIndex] = line[charIndex];
                      }
                    }

                    //var newLine = "";
                    //foreach (var c in chars)
                    //{
                    //  if (c != (char)0)
                    //  {
                    //    newLine += c;
                    //  }
                    //}

                    line = new string(chars);
                  }

                  if (warningLine.Inserts.Count != 0)
                  {
                    var chars = new string[line.Length];
                    for (int charIndex = 0; charIndex < line.Length; charIndex++)
                    {
                      if (warningLine.Inserts.ContainsKey(charIndex))
                      {
                        chars[charIndex] = warningLine.Inserts[charIndex].Text + line[charIndex];
                      }
                      else
                      {
                        chars[charIndex] = line[charIndex].ToString();
                      }
                    }

                    line = string.Join("", chars);
                  }

                  if (!line.Contains("/*"))
                  {
                    //left
                    if (warningLine.Left.Count != 0)
                    {
                      line = line.Insert(warningLine.StartPosition, string.Join(" ", warningLine.Left) + " ");
                    }

                    //right
                    if (warningLine.Right.Count != 0)
                    {
                      line = line.TrimEnd();
                      var lastPosition = line.Length;
                      var commentIndex = line.IndexOf("//", StringComparison.InvariantCulture);
                      if (commentIndex != -1)
                      {
                        lastPosition = commentIndex;
                      }

                      line = line.Insert(lastPosition, " " + string.Join(" ", warningLine.Right));
                    }
                  }

                  fileBuilder.AppendLine(line);
                }

                foreach (var line in warningLine.Bottom)
                {
                  fileBuilder.AppendLine(line);
                }
              }
              else
              {
                if (i == lines.Length - 1)
                {
                  fileBuilder.Append(lines[i]);
                }
                else
                {
                  fileBuilder.AppendLine(lines[i]);
                }
              }
            }

            result.Add(new FileText(warningFile.FilePath, fileBuilder.ToString()));
          }
        }

        return result.ToArray();
      }
    }

    class FileText
    {
      public string Path { get; private set; }
      public string Text { get; private set; }

      public FileText(string path, string text)
      {
        Path = path;
        Text = text;
      }
    }

    class WarningFile
    {
      private Dictionary<int, WarningLine> _lines = new Dictionary<int, WarningLine>();

      public string Text { get; private set; }
      public string[] Lines { get; private set; }
      public string FilePath { get; private set; }
      public bool IsDirty { get; private set; }

      public WarningFile(string filePath)
      {
        FilePath = filePath;
        Text = File.ReadAllText(filePath);
        Lines = File.ReadAllLines(filePath);
      }

      public bool HasWarning(int lineIndex)
      {
        return _lines.ContainsKey(lineIndex);
      }

      public WarningLine Line(int lineIndex)
      {
        WarningLine line;
        if (!_lines.TryGetValue(lineIndex, out line))
        {
          _lines[lineIndex] = line = new WarningLine(this, lineIndex);
        }

        return line;
      }

      public void SetDirty()
      {
        IsDirty = true;
      }
    }

    class Line
    {
      private readonly string[] _fileLines;
      private readonly int _index;

      public Line(string[] fileLines, int index)
      {
        _fileLines = fileLines;
        _index = index;
      }

      public Line Prev
      {
        get
        {
          if (_index == 0) return null;
          return new Line(_fileLines, _index - 1);
        }
      }

      public Line Next
      {
        get
        {
          if (_index == _fileLines.Length - 1) return null;
          return new Line(_fileLines, _index + 1);
        }
      }

      public string Text
      {
        get { return _fileLines[_index]; }
      }
    }

    class Insert
    {
      public string Text = "";
    }

    class Range
    {
      public int StartPosition { get; private set; }
      public int EndPosition { get; private set; }

      public Range(int startPosition, int endPosition)
      {
        StartPosition = startPosition;
        EndPosition = endPosition;
      }
    }

    class WarningLine
    {
      private readonly Dictionary<string, Warning> _warnings = new Dictionary<string, Warning>();

      public WarningFile File { get; private set; }
      public int LineIndex { get; private set; }
      public int StartPosition { get; private set; }
      public string StartSpace { get; private set; }
      public Line Line { get; private set; }

      public HashSet<string> DisableWarnings { get; private set; }
      public Dictionary<int, Insert> Inserts { get; private set; }
      public List<Range> Removes { get; private set; }

      public LinkedList<string> Top { get; private set; }
      public LinkedList<string> Bottom { get; private set; }
      public LinkedList<string> Left { get; private set; }
      public LinkedList<string> Right { get; private set; }

      public string Text
      {
        get { return Line.Text; }
      }

      public bool HasNewKeyword { get; private set; }

      public void SetNewKeyword()
      {
        HasNewKeyword = true;
      }

      public WarningLine(WarningFile file, int lineIndex)
      {
        Top = new LinkedList<string>();
        Bottom = new LinkedList<string>();
        Left = new LinkedList<string>();
        Right = new LinkedList<string>();
        Inserts = new Dictionary<int, Insert>();
        DisableWarnings = new HashSet<string>();
        Removes = new List<Range>();

        File = file;
        LineIndex = lineIndex;
        Line = new Line(file.Lines, lineIndex);

        var text = Line.Text;
        var initLen = text.Length;
        var currLen = text.TrimStart().Length;
        StartPosition = initLen - currLen;
        StartSpace = text.Substring(0, StartPosition);
      }

      public Insert Insert(int position)
      {
        Insert insert;
        if (!Inserts.TryGetValue(position, out insert))
        {
          Inserts[position] = insert = new Insert();
        }

        return insert;
      }

      public Warning Warinig(string code, int startPosition, int endPosition, string source)
      {
        Warning warning;
        if (!_warnings.TryGetValue(code, out warning))
        {
          warning = new Warning(code, this, startPosition, endPosition, source);
          _warnings[code] = warning;
        }

        return warning;
      }
    }

    class Warning
    {
      public string Code { get; private set; }
      public WarningLine Line { get; private set; }
      public int StartPosition { get; private set; }
      public int EndPosition { get; private set; }
      public string Source { get; private set; }
      public string MemeberName { get; private set; }

      public Warning(string code, WarningLine line, int startPosition, int endPosition, string source)
      {
        Code = code;
        Line = line;
        StartPosition = startPosition;
        EndPosition = endPosition;
        Source = source;
        MemeberName = line.Line.Text.Substring(startPosition, endPosition - startPosition);
      }

      public bool AddNewKeyword()
      {
        if (!Line.HasNewKeyword)
        {
          var text = Line.Text;
          if (!text.Contains(" new "))
          {
            var c0 = text.IndexOf("//");
            var c1 = text.IndexOf("/*");
            if ((c0 < StartPosition && c0 != -1) || (c1 < StartPosition && c1 != -1))
            {
              return false;
            }

            var generics = 0;
            var arrays = 0;
            var pos = 0; // 0-readspace, 1-readType, 2-readSpace

            for (int i = StartPosition; i >= 0; i--)
            {
              var ch = text[i];
              if (ch == '>') generics++;
              if (ch == '<') generics--;
              if (ch == ']') arrays++;
              if (ch == '[') arrays--;

              if (pos == 0 && ch == ' ')
              {
                pos = 1;
              }
              else if (pos == 1 && ch == ' ' && generics == 0 && arrays == 0)
              {
                Line.Insert(i).Text += " new";
                Line.SetNewKeyword();
                return true;
              }
            }
          }
        }

        return false;
      }

      public bool RemoveNewKeyword()
      {
        var key = " new ";
        var index = Line.Text.IndexOf(key);
        if (index != -1 && index < StartPosition)
        {
          Line.Removes.Add(new Range(index, index + (key.Length - 2)));
          return true;
        }

        return false;
      }

      public bool DisableWarning(string code)
      {
        var result = Line.DisableWarnings.Add(code);
        if (result)
        {
          Line.Top.AddLast("#pragma warning disable " + code);
          Line.Bottom.AddFirst("#pragma warning restore " + code);
        }

        return result;
      }
    }

    interface IWarningResolver
    {
      bool Resolve(Warning warning);
    }

    class CS0109 : IWarningResolver
    {
      public bool Resolve(Warning warning)
      {
        return warning.RemoveNewKeyword();
      }
    }

    class CS0108 : IWarningResolver
    {
      public bool Resolve(Warning warning)
      {
        return warning.AddNewKeyword();
      }
    }

    class CS0114 : IWarningResolver
    {
      public bool Resolve(Warning warning)
      {
        return warning.AddNewKeyword();
      }
    }

    class CS0219 : IWarningResolver
    {
      public bool Resolve(Warning warning)
      {
        return warning.DisableWarning("219");
      }
    }

    class CS0649 : IWarningResolver
    {
      private static Regex MatchWarning =
        new Regex(": warning CS0649: Field \'[$_A-Za-z][$_A-Za-z0-9]*\\.[$_A-Za-z][$_A-Za-z0-9]*\'",
          RegexOptions.Singleline);

      private static Regex MatchClass = new Regex("\'([$_A-Za-z][$_A-Za-z0-9]*\\.)+", RegexOptions.Singleline);

      public bool Resolve(Warning warning)
      {
        var matchWarning = MatchWarning.Match(warning.Source);
        if (matchWarning.Success)
        {
          var matchClass = MatchClass.Match(matchWarning.Value);
          if (matchClass.Success)
          {
            var className = matchClass.Value.TrimStart('\'').TrimEnd('.');
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(warning.Line.File.FilePath);
            Type clazz;
            if (script != null && (clazz = script.GetClass()) != null)
            {
              var fieldInfo = clazz.GetField(warning.MemeberName,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
              if (fieldInfo != null && !fieldInfo.IsStatic)
              {
                if (fieldInfo.IsPublic || fieldInfo.GetCustomAttribute<SerializeField>() != null)
                {
                  return warning.DisableWarning("649");
                }
              }
            }
            else
            {
              var ok = warning.Line.Text.IndexOf("public ") != -1 && warning.Line.Text.IndexOf("public ") < warning.StartPosition;
              ok = ok || warning.Line.Text.Contains("[SerializeField]");
              ok = ok || warning.Line.Line.Prev != null && warning.Line.Line.Prev.Text.Contains("[SerializeField]");
              if (ok)
              {
                return warning.DisableWarning("649");
              }
            }
          }
        }

        return false;
      }
    }
  }
}
