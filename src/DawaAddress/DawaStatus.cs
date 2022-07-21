using System.ComponentModel;

namespace DawaAddress;

// We disable CA1008 because it can be confusing to consumers to
// have to check for None, since that is invalid.
#pragma warning disable CA1008
public enum DawaStatus
{
    [Description("Active")]
    Active = 1,
    [Description("Canceled")]
    Canceled = 2,
    [Description("Pending")]
    Pending = 3,
    [Description("Discontinued")]
    Discontinued = 4
}
#pragma warning restore CA1008
