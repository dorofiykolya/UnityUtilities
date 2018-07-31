using System;
using System.Collections.Generic;

namespace Utils
{
  public class Lifetime
  {
    public class Definition
    {
      private Definition(string id)
      {
        Id = id;
        Lifetime = new Lifetime();
      }

      public string Id { get; private set; }
      public bool IsTerminated { get; private set; }
      public Lifetime Lifetime { get; private set; }

      public void Terminate()
      {
        if (!IsTerminated)
        {
          IsTerminated = true;
          Lifetime.Terminate();
        }
      }

      public static Definition Define(Lifetime lifetime, string id = "defaultName")
      {
        var definition = new Definition(id);
        lifetime.AddDefinition(definition);
        return definition;
      }

      public static Definition Intersection(params Lifetime[] lifetimes)
      {
        var definition = Define(Eternal);
        foreach (var lifetime in lifetimes)
        {
          lifetime.AddDefinition(definition);
        }
        return definition;
      }
    }

    public static readonly Lifetime Eternal = new Lifetime();

    private static int _instances;

    private readonly List<Action> _actions = new List<Action>();
    private readonly int _id;
    private bool _terminated;

    public static Definition Define(Lifetime lifetime, string id = "defaultName")
    {
      return Definition.Define(lifetime, id);
    }

    public static Definition Intersection(params Lifetime[] lifetimes)
    {
      return Definition.Intersection(lifetimes);
    }

    public Lifetime()
    {
      _id = ++_instances;
    }

    public int Id
    {
      get { return _id; }
    }

    public bool IsTerminated
    {
      get { return _terminated; }
    }

    public void AddAction(Action action)
    {
      if (_actions.Contains(action))
      {
        throw new ArgumentException();
      }
      _actions.Add(action);
    }

    public void AddBracket(Action onOpen, Action onTerminate)
    {
      if (!_terminated)
      {
        AddAction(onTerminate);
        onOpen();
      }
    }

    public Definition DefineNested(string name = "defaultName")
    {
      return Define(this, name);
    }

    private void AddDefinition(Definition definition)
    {
      if (!_actions.Contains(definition.Terminate))
      {
        _actions.Add(definition.Terminate);
        definition.Lifetime.AddAction(() =>
        {
          _actions.Remove(definition.Terminate);
        });
        if (IsTerminated || definition.IsTerminated)
        {
          definition.Terminate();
        }
      }
    }

    private void Terminate()
    {
      _terminated = true;
      var array = _actions.ToArray();
      for (var i = array.Length - 1; i >= 0; i--)
      {
        var action = array[i];
        action();
      }

      _actions.Clear();
    }
  }

  public class Lifetime<T>
  {
    public class Definition
    {
      private Definition(string id)
      {
        Id = id;
        Lifetime = new Lifetime<T>();
      }

      public string Id { get; private set; }
      public bool IsTerminated { get; private set; }
      public Lifetime<T> Lifetime { get; private set; }

      public void Terminate(T value)
      {
        if (!IsTerminated)
        {
          IsTerminated = true;
          Lifetime.Terminate(value);
        }
      }

      public static Definition Define(Lifetime<T> lifetime, string id = "defaultName")
      {
        var definition = new Definition(id);
        lifetime.AddAction(definition.Terminate);
        return definition;
      }

      public static Definition Intersection(params Lifetime<T>[] lifetimes)
      {
        var definition = Define(Eternal);
        foreach (var lifetime in lifetimes)
        {
          lifetime.AddAction(definition.Terminate);
        }
        return definition;
      }
    }

    public static readonly Lifetime<T> Eternal = new Lifetime<T>();

    private readonly List<Action<T>> _actions = new List<Action<T>>();
    private bool _terminated;
    private Lifetime _lifetime;

    public static Definition Define(Lifetime<T> lifetime, string id = "defaultName")
    {
      return Definition.Define(lifetime, id);
    }

    public static Definition Intersection(params Lifetime<T>[] lifetimes)
    {
      return Definition.Intersection(lifetimes);
    }

    public void AddAction(Action<T> action)
    {
      if (_actions.Contains(action))
      {
        throw new ArgumentException();
      }
      _actions.Add(action);
    }

    public void AddBracket(Action onOpen, Action<T> onTerminate)
    {
      if (!_terminated)
      {
        AddAction(onTerminate);
        onOpen();
      }
    }

    public Lifetime LifeTime
    {
      get
      {
        if (_lifetime == null)
        {
          var definition = Lifetime.Define(Lifetime.Eternal);
          AddAction(res => definition.Terminate());
          _lifetime = definition.Lifetime;
        }
        return _lifetime;
      }
    }

    private void Terminate(T value)
    {
      _terminated = true;
      var array = _actions.ToArray();
      for (var i = array.Length - 1; i >= 0; i--)
      {
        var action = array[i];
        action(value);
      }

      _actions.Clear();
    }
  }
}
