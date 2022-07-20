using System.Text.Json.Serialization;

namespace DawaAddress;

public record DawaRoad
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }
    [JsonPropertyName("navn")]
    public string Name { get; init; }
    [JsonPropertyName("darstatus")]
    public string DarStatus { get; init; }

    public DawaRoad(Guid id, string name, string darStatus)
    {
        Id = id;
        Name = name;
        DarStatus = darStatus;
    }
}
