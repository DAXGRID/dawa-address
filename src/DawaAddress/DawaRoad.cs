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
    public required Guid Id { get; init; }

    [JsonPropertyName("navn")]
    public required string Name { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter<DawaRoadStatus>))]
    [JsonPropertyName("darstatus")]
    public required DawaRoadStatus Status { get; init; }

    [JsonPropertyName("oprettet")]
    public required DateTime Created { get; init; }

    [JsonPropertyName("ændret")]
    public required DateTime Updated { get; init; }

    public DawaRoad() {}

    [JsonConstructor]
    public DawaRoad(
        Guid id,
        string name,
        DawaRoadStatus status,
        DateTime created,
        DateTime updated)
    {
        Id = id;
        Name = name;
        Status = status;
        Created = created;
        Updated = updated;
    }
}
