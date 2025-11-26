using System.Text.Json.Serialization;

namespace DawaAddress;

public class Afstemningsområde
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
}

public class Kommuneinddeling
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
}

public class Menighedsrådsafstemningsområde
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
}

public class NavngivenVej
{
    [JsonPropertyName("id_lokalId")]
    public required string IdLokalId { get; set; }
}

public class Postnummer
{
    [JsonPropertyName("id_lokalId")]
    public required string IdLokalId { get; set; }
}

public class DatafordelerAccessAddress
{
    [JsonPropertyName("datafordelerOpdateringstid")]
    public required DateTime DatafordelerOpdateringstid { get; set; }

    [JsonPropertyName("adgangsadressebetegnelse")]
    public required string Adgangsadressebetegnelse { get; set; }

    [JsonPropertyName("adgangTilBygning")]
    public string? AdgangTilBygning { get; set; }

    [JsonPropertyName("afstemningsområde")]
    public required Afstemningsområde Afstemningsomrde { get; set; }

    [JsonPropertyName("forretningshændelse")]
    public required string Forretningshndelse { get; set; }

    [JsonPropertyName("forretningsområde")]
    public required string Forretningsomrde { get; set; }

    [JsonPropertyName("forretningsproces")]
    public required string Forretningsproces { get; set; }

    [JsonPropertyName("geoDanmarkBygning")]
    public string? GeoDanmarkBygning { get; set; }

    [JsonPropertyName("husnummerretning")]
    public required string Husnummerretning { get; set; }

    [JsonPropertyName("husnummertekst")]
    public required string Husnummertekst { get; set; }

    [JsonPropertyName("id_lokalId")]
    public required string IdLokalId { get; set; }

    [JsonPropertyName("id_namespace")]
    public required string IdNamespace { get; set; }

    [JsonPropertyName("jordstykke")]
    public string? Jordstykke { get; set; }

    [JsonPropertyName("kommuneinddeling")]
    public required Kommuneinddeling Kommuneinddeling { get; set; }

    [JsonPropertyName("menighedsrådsafstemningsområde")]
    public Menighedsrådsafstemningsområde? Menighedsrdsafstemningsomrde { get; set; }

    [JsonPropertyName("registreringFra")]
    public required DateTime RegistreringFra { get; set; }

    [JsonPropertyName("registreringsaktør")]
    public required string Registreringsaktr { get; set; }

    [JsonPropertyName("sogneinddeling")]
    public required Sogneinddeling Sogneinddeling { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("vejmidte")]
    public required string Vejmidte { get; set; }

    [JsonPropertyName("virkningFra")]
    public required DateTime VirkningFra { get; set; }

    [JsonPropertyName("virkningsaktør")]
    public required string Virkningsaktr { get; set; }

    [JsonPropertyName("navngivenVej")]
    public required NavngivenVej NavngivenVej { get; set; }

    [JsonPropertyName("postnummer")]
    public required Postnummer Postnummer { get; set; }
}

public class Sogneinddeling
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
}

