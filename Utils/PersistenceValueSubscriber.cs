using System;
using Utils.Persistences;

namespace Utils
{
  public class PersistenceValueSubscriber<T> : ValueSubscriber<T> where T : IEquatable<T>
  {
    public PersistenceValueSubscriber(Lifetime lifetime, Persistence persistence, T defaultValue) : base(lifetime, defaultValue)
    {
      Initialize(lifetime, persistence, defaultValue);
    }

    private void Initialize(Lifetime lifetime, Persistence persistence, T defaultValue)
    {
      var type = typeof(T);
      if (Array.IndexOf(Persistence.AvailableTypes, type) != -1)
      {
        persistence.DefaultValue = defaultValue;
        Current = persistence.GetValue<T>();
        SubscribeOnChange(lifetime, value =>
        {
          persistence.SetValue<T>(value.Current);
        });
      }
      else
      {
        throw new ArgumentException("type not supported: " + typeof(T).FullName);
      }
    }
  }
}
