using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace DawaAddress;

public enum DawaRoadStatus
{
    [EnumMember(Value = "foreløbig")]
    Temporary,
    [EnumMember(Value = "gældende")]
    Effective,
}

public record DawaRoad
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }
    [JsonPropertyName("navn")]
    public string Name { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter<DawaRoadStatus>))]
    [JsonPropertyName("darstatus")]
    public DawaRoadStatus Status { get; init; }

    public DawaRoad(Guid id, string name, DawaRoadStatus status)
    {
        Id = id;
        Name = name;
        Status = status;
    }
}
