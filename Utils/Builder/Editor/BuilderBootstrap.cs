using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Utils.BuildPipeline.Builders;

namespace Utils.BuildPipeline
{
  public class BuilderBootstrap
  {
    private readonly Dictionary<BuildTarget, IBuilder> _mapBuilders;
    private readonly HashSet<string> _scenes;
    private readonly HashSet<IBuilderProcessor> _processors;
    private ILogger _logger;
    private bool _defaultBuilderProvider;

    public BuilderBootstrap()
    {
      _mapBuilders = new Dictionary<BuildTarget, IBuilder>();
      _scenes = new HashSet<string>();
      _processors = new HashSet<IBuilderProcessor>();
    }

    public BuilderBootstrap SetDefaultBuilders()
    {
      _defaultBuilderProvider = true;
      return this;
    }

    public BuilderBootstrap SetBuilder(BuildTarget target, IBuilder builder)
    {
      _defaultBuilderProvider = false;
      _mapBuilders[target] = builder;
      return this;
    }

    public BuilderBootstrap AddScene(string path)
    {
      _scenes.Add(path);
      return this;
    }

    public BuilderBootstrap AddEnabledScenes()
    {
      foreach (var path in Builder.ScenePaths)
      {
        _scenes.Add(path);
      }

      return this;
    }

    public BuilderBootstrap AddProcessor(IBuilderProcessor processor)
    {
      _processors.Add(processor);
      return this;
    }

    public BuilderBootstrap SetLogger(ILogger logger)
    {
      _logger = logger;
      return this;
    }

    public Builder Create()
    {
      IBuildersProvider provider = null;
      if (_defaultBuilderProvider || _mapBuilders.Count == 0)
      {
        provider = new DefaultBuildersProvider();
      }
      else
      {
        provider = new BuilderProvider(_mapBuilders);
      }
      return new Builder(provider, _scenes.ToArray(), _logger, _processors.ToArray());
    }

    private class BuilderProvider : IBuildersProvider
    {
      private readonly Dictionary<BuildTarget, IBuilder> _map;

      public BuilderProvider(Dictionary<BuildTarget, IBuilder> map)
      {
        _map = new Dictionary<BuildTarget, IBuilder>(map);
      }

      public IBuilder Get(BuildTarget target)
      {
        return _map[target];
      }
    }
  }
}
