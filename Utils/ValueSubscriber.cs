using System;

namespace Utils
{
  public class ValueSubscriber<T> where T : IEquatable<T>
  {
    private readonly Signal<ValueSubscriber<T>> _onChange;
    private T _current;
    private T _prev;

    public ValueSubscriber(Lifetime lifetime, T defaultValue = default(T))
    {
      _onChange = new Signal<ValueSubscriber<T>>(lifetime);
      _current = _prev = defaultValue;
    }

    public T Current
    {
      get { return _current; }
      set
      {
        var last = _current;
        if (!ReferenceEquals(last, value) && (ReferenceEquals(last, null) || !last.Equals(value)))
        {
          _prev = last;
          _current = value;
          _onChange.Fire(this);
        }
      }
    }

    public T Prev
    {
      get { return _prev; }
    }

    public void SubscribeOnChange(Lifetime lifetime, Action<ValueSubscriber<T>> listener)
    {
      _onChange.Subscribe(lifetime, listener);
    }
  }
}
