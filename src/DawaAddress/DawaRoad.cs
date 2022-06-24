using System.Text.Json.Serialization;

namespace DawaAddress;

public record DawaRoad
{
    [JsonPropertyName("id")]
    public string Id { get; init; }
    [JsonPropertyName("navn")]
    public string Name { get; init; }

    public DawaRoad(string id, string name)
    {
        Id = id;
        Name = name;
    }
}
