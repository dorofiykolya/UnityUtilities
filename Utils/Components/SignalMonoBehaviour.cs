using UnityEngine;
using System.Collections;

namespace Utils
{
  public class SignalMonoBehaviour : MonoBehaviour
  {
    private Lifetime.Definition _lifetime;
    private Signal _onUpdate;
    private Signal _onLateUpdate;
    private Signal _onFixedUpdate;
    private Signal _onAwake;
    private Signal _onEnable;
    private Signal _onDisable;
    private Signal _onStart;
    private Signal _onDestroy;

    public ISignalSubsribe UpdateSignal { get { return _onUpdate; } }
    public ISignalSubsribe LateUpdateSignal { get { return _onLateUpdate; } }
    public ISignalSubsribe FixedUpdateSignal { get { return _onFixedUpdate; } }
    public ISignalSubsribe AwakeSignal { get { return _onAwake; } }
    public ISignalSubsribe StartSignal { get { return _onStart; } }
    public ISignalSubsribe EnableSignal { get { return _onEnable; } }
    public ISignalSubsribe DisableSignal { get { return _onDisable; } }
    public ISignalSubsribe DestroySignal { get { return _onDestroy; } }


    private void Awake()
    {
      _lifetime = Lifetime.Define(Lifetime.Eternal);

      _onUpdate = new Signal(_lifetime.Lifetime);
      _onLateUpdate = new Signal(_lifetime.Lifetime);
      _onFixedUpdate = new Signal(_lifetime.Lifetime);

      _onAwake = new Signal(_lifetime.Lifetime);
      _onEnable = new Signal(_lifetime.Lifetime);
      _onDisable = new Signal(_lifetime.Lifetime);
      _onDestroy = new Signal(_lifetime.Lifetime);
      _onStart = new Signal(_lifetime.Lifetime);
      _onDestroy = new Signal(_lifetime.Lifetime);

      _onAwake.Fire();
    }

    private void FixedUpdate()
    {
      _onFixedUpdate.Fire();
    }

    private void LateUpdate()
    {
      _onLateUpdate.Fire();
    }

    private void OnDestroy()
    {
      _onDestroy.Fire();

      _lifetime.Terminate();
    }

    private void OnDisable()
    {
      _onDisable.Fire();
    }

    private void OnEnable()
    {
      _onEnable.Fire();
    }

    private void Start()
    {
      _onStart.Fire();
    }

    private void Update()
    {
      _onUpdate.Fire();
    }

  }
}