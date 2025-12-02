using System.Text.Json.Serialization;

namespace DawaAddress;

public class DatafordelerRoad
{
    [JsonPropertyName("datafordelerOpdateringstid")]
    public required DateTime DatafordelerOpdateringstid { get; set; }

    [JsonPropertyName("id_lokalId")]
    public required string IdLokalId { get; set; }

    [JsonPropertyName("virkningFra")]
    public required DateTime VirkningFra { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("vejnavn")]
    public required string Vejnavn { get; set; }
}
