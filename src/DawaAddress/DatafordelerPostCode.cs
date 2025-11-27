using System.Text.Json.Serialization;

namespace DawaAddress;

public class DatafordelerPostCode
{
    [JsonPropertyName("datafordelerOpdateringstid")]
    public required DateTime DatafordelerOpdateringstid { get; set; }

    [JsonPropertyName("forretningshændelse")]
    public required string Forretningshndelse { get; set; }

    [JsonPropertyName("forretningsområde")]
    public required string Forretningsomrde { get; set; }

    [JsonPropertyName("forretningsproces")]
    public required string Forretningsproces { get; set; }

    [JsonPropertyName("id_lokalId")]
    public required string IdLokalId { get; set; }

    [JsonPropertyName("id_namespace")]
    public required string IdNamespace { get; set; }

    [JsonPropertyName("navn")]
    public required string Navn { get; set; }

    [JsonPropertyName("postnr")]
    public required string Postnr { get; set; }

    [JsonPropertyName("postnummerinddeling")]
    public required string Postnummerinddeling { get; set; }

    [JsonPropertyName("registreringFra")]
    public required DateTime RegistreringFra { get; set; }

    [JsonPropertyName("registreringsaktør")]
    public required string Registreringsaktr { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("virkningFra")]
    public required DateTime VirkningFra { get; set; }

    [JsonPropertyName("virkningsaktør")]
    public required string Virkningsaktr { get; set; }
}

