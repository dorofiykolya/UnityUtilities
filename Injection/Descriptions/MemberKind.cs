using System;

namespace Injection
{
  [Flags]
  public enum MemberKind
  {
    Field = 1,
    Property = 2,
    Method = 4,
    Constructor = 8,
    All = Field | Property | Method | Constructor
  }
}
