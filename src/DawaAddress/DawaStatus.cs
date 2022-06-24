using System.ComponentModel;

namespace DawaAddress;

public enum DawaStatus
{
    [Description("None")]
    None = 0,
    [Description("Active")]
    Active = 1,
    [Description("Canceled")]
    Canceled = 2,
    [Description("Pending")]
    Pending = 3,
    [Description("Discontinued")]
    Discontinued = 4
}
