using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using Utils.BuildPipeline.Builders;

namespace Utils.BuildPipeline
{
  public class BuilderBootstrap
  {
    private readonly Dictionary<BuildTarget, IBuilder> _mapBuilders;
    private readonly HashSet<string> _scenes;
    private readonly BuilderProcessorsProvider _processorsProvider;
    private ILogger _logger;
    private bool _defaultBuilderProvider;
    private string _additionalHelp;

    public BuilderBootstrap()
    {
      _mapBuilders = new Dictionary<BuildTarget, IBuilder>();
      _scenes = new HashSet<string>();
      _processorsProvider = new BuilderProcessorsProvider();
    }

    public BuilderBootstrap SetDefaultBuilders()
    {
      _defaultBuilderProvider = true;
      return this;
    }

    public BuilderBootstrap SetBuilder(BuildTarget target, IBuilder builder)
    {
      Assert.IsNotNull(builder);
      _defaultBuilderProvider = false;
      _mapBuilders[target] = builder;
      return this;
    }

    public BuilderBootstrap AddScene(string path)
    {
      Assert.IsNotNull(path);
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
      Assert.IsNotNull(processor);
      _processorsProvider.Add(processor);
      return this;
    }

    public BuilderBootstrap AddProcessor(BuildTarget target, IBuilderProcessor processor)
    {
      Assert.IsNotNull(processor);
      _processorsProvider.Add(target, processor);
      return this;
    }

    public BuilderBootstrap SetLogger(ILogger logger)
    {
      Assert.IsNotNull(logger);
      _logger = logger;
      return this;
    }

    public BuilderBootstrap SetAdditionalHelp(string additionalHelp)
    {
      _additionalHelp = additionalHelp;
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
      return new Builder(provider, _scenes.ToArray(), _logger, _processorsProvider, _additionalHelp);
    }

    private class BuilderProvider : IBuildersProvider
    {
      private readonly Dictionary<BuildTarget, IBuilder> _map;

      public BuilderProvider(Dictionary<BuildTarget, IBuilder> map)
      {
        _map = new Dictionary<BuildTarget, IBuilder>(map);
      }

      public BuildTarget[] AvailableTargets
      {
        get { return _map.Keys.ToArray(); }
      }

      public IBuilder Get(BuildTarget target)
      {
        return _map[target];
      }
    }
  }
}
