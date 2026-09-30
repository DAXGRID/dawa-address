using System.Text.Json.Serialization;

namespace DawaAddress;

public sealed record DatafordelerJordStykkeFileServer
{
    [JsonPropertyName("id_lokalId")]
    public required string IdLokalId { get; set; }

    [JsonPropertyName("matrikelnummer")]
    public required string MatrikelNummer { get; set; }
}
