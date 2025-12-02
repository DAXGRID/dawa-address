using System.Text.Json.Serialization;

namespace DawaAddress;

public class DatafordelerHusnummer
{
    [JsonPropertyName("id_lokalId")]
    public required string IdLokalId { get; set; }
}

public class DatafordelerUnitAddress
{
    [JsonPropertyName("datafordelerOpdateringstid")]
    public required DateTime DatafordelerOpdateringstid { get; set; }

    [JsonPropertyName("adressebetegnelse")]
    public required string Adressebetegnelse { get; set; }

    [JsonPropertyName("dørbetegnelse")]
    public string? Drbetegnelse { get; set; }

    [JsonPropertyName("etagebetegnelse")]
    public string? Etagebetegnelse { get; set; }

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

    [JsonPropertyName("husnummer")]
    public required DatafordelerHusnummer Husnummer { get; set; }
}
