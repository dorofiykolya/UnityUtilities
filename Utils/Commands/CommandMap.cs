using System;
using System.Collections.Generic;
using Injection;

namespace Utils.Commands
{
  public class CommandMap
  {
    private readonly Dictionary<Type, Container> _map;
    private readonly Lifetime _lifetime;
    private readonly IInjector _injector;

    public CommandMap(Lifetime lifetime, IInjector injector)
    {
      _map = new Dictionary<Type, Container>();
      _lifetime = lifetime;
      _injector = new Injector(injector);
      _lifetime.AddAction(((IDisposable)_injector).Dispose);
    }

    public ICommandMapper Map<TMessage>() where TMessage : IMessage
    {
      Container container;
      if (!_map.TryGetValue(typeof(TMessage), out container))
      {
        var lifetime = Lifetime.Define(_lifetime);
        var mapper = new CommandMapper(lifetime.Lifetime, typeof(TMessage), _injector);

        _map[typeof(TMessage)] = container = new Container
        {
          Lifetime = lifetime,
          Mapper = mapper
        };

        lifetime.Lifetime.AddAction(() =>
        {
          _map.Remove(typeof(TMessage));
        });
      }
      return container.Mapper;
    }

    public void Tell(object message)
    {
      Container container;
      if (_map.TryGetValue(message.GetType(), out container))
      {
        container.Mapper.Tell(message);
      }
    }

    private class Container
    {
      public Lifetime.Definition Lifetime;
      public CommandMapper Mapper;
    }
  }
}
