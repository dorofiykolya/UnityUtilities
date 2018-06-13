using System;

namespace Utils.Commands
{
  public interface ICommandMapper
  {
    Lifetime RegisterCommand(Func<Lifetime, ICommand> factory, bool oneTime = false);
  }
}
