using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.Assertions;

namespace Utils.BuildPipeline
{
  public class BuilderProcessorsProvider
  {
    private readonly HashSet<IBuilderProcessor> _processors = new HashSet<IBuilderProcessor>();
    private readonly Dictionary<BuildTarget, HashSet<IBuilderProcessor>> _map = new Dictionary<BuildTarget, HashSet<IBuilderProcessor>>();

    public void Add(IBuilderProcessor processor)
    {
      Assert.IsNotNull(processor);
      _processors.Add(processor);
    }

    public void Add(BuildTarget target, IBuilderProcessor processor)
    {
      HashSet<IBuilderProcessor> value;
      if (!_map.TryGetValue(target, out value))
      {
        _map[target] = value = new HashSet<IBuilderProcessor>();
      }

      value.Add(processor);
    }

    public IBuilderProcessor[] Get(BuildTarget buildTarget)
    {
      HashSet<IBuilderProcessor> value;
      if (!_map.TryGetValue(buildTarget, out value))
      {
        value = new HashSet<IBuilderProcessor>();
      }
      var result = new HashSet<IBuilderProcessor>(value.Concat(_processors));
      return result.ToArray();
    }

    public IBuilderProcessor[] Processors
    {
      get
      {
        var result = new HashSet<IBuilderProcessor>(_processors.Concat(_map.Values.SelectMany(s => s)));
        return result.ToArray();
      }
    }
  }
}
