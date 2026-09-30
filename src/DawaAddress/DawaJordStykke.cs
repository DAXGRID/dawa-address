using System.Text.Json.Serialization;

namespace DawaAddress;

public sealed record DawaJordStykke
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("matrikkelnummer")]
    public required string MatrikkelNummer { get; init; }
}

