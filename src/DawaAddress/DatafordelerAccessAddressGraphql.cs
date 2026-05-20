using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace DawaAddress;

public record DatafordelerAccessAddressGraphqlResponse
{
    [JsonPropertyName("DAR_Husnummer")]
    public required DarHusnummer DarHusnummer { get; set; }
}

public record DarHusnummer
{
    [JsonPropertyName("pageInfo")]
    public required DatafordelerAccessAddressGraphqlPageInfo PageInfo { get; set; }

    [JsonPropertyName("nodes")]
    public required IReadOnlyList<HusnummerNode> Nodes { get; set; }
}

public record DatafordelerAccessAddressGraphqlPageInfo
{
    [JsonPropertyName("endCursor")]
    public required string EndCursor { get; set; }

    [JsonPropertyName("hasNextPage")]
    public required bool HasNextPage { get; set; }
}

public record HusnummerNode
{
    [JsonPropertyName("adgangsadressebetegnelse")]
    public required string Adgangsadressebetegnelse { get; set; }

    [JsonPropertyName("virkningTil")]
    public required DateTime? VirkningTil { get; set; }

    [JsonPropertyName("virkningsaktoer")]
    public required string Virkningsaktoer { get; set; }

    [JsonPropertyName("virkningFra")]
    public required DateTime VirkningFra { get; set; }

    [JsonPropertyName("vejpunkt")]
    public required string Vejpunkt { get; set; }

    [JsonPropertyName("vejmidte")]
    public required string Vejmidte { get; set; }

    [JsonPropertyName("supplerendeBynavn")]
    public required string? SupplerendeBynavn { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("sogneinddeling")]
    public required string Sogneinddeling { get; set; }

    [JsonPropertyName("registreringTil")]
    public required DateTime? RegistreringTil { get; set; }

    [JsonPropertyName("registreringsaktoer")]
    public required string Registreringsaktoer { get; set; }

    [JsonPropertyName("registreringFra")]
    public required DateTime RegistreringFra { get; set; }

    [JsonPropertyName("placeretPaaForeloebigtJordstykke")]
    public required string? PlaceretPaaForeloebigtJordstykke { get; set; }

    [JsonPropertyName("navngivenVej")]
    public required string NavngivenVej { get; set; }

    [JsonPropertyName("menighedsraadsafstemningsomraade")]
    public required string Menighedsraadsafstemningsomraade { get; set; }

    [JsonPropertyName("kommuneinddeling")]
    public required string Kommuneinddeling { get; set; }

    [JsonPropertyName("jordstykke")]
    public required string? Jordstykke { get; set; }

    [JsonPropertyName("id_namespace")]
    public required string IdNamespace { get; set; }

    [JsonPropertyName("id_lokalId")]
    public required string IdLokalId { get; set; }

    [JsonPropertyName("husnummertekst")]
    public required string Husnummertekst { get; set; }

    [JsonPropertyName("geoDanmarkBygning")]
    public required string? GeoDanmarkBygning { get; set; }

    [JsonPropertyName("forretningsproces")]
    public required string Forretningsproces { get; set; }

    [JsonPropertyName("forretningsomraade")]
    public required string Forretningsomraade { get; set; }

    [JsonPropertyName("forretningshaendelse")]
    public required string Forretningshaendelse { get; set; }

    [JsonPropertyName("datafordelerRowId")]
    public required string DatafordelerRowId { get; set; }

    [JsonPropertyName("datafordelerRegisterImportSequenceNumber")]
    public required int DatafordelerRegisterImportSequenceNumber { get; set; }

    [JsonPropertyName("datafordelerRowVersion")]
    public required int DatafordelerRowVersion { get; set; }

    [JsonPropertyName("datafordelerOpdateringstid")]
    public required DateTime DatafordelerOpdateringstid { get; set; }

    [JsonPropertyName("afstemningsomraade")]
    public required string Afstemningsomraade { get; set; }

    [JsonPropertyName("adgangTilTekniskAnlaeg")]
    public required string? AdgangTilTekniskAnlaeg { get; set; }

    [JsonPropertyName("adgangTilBygning")]
    public required string? AdgangTilBygning { get; set; }

    [JsonPropertyName("adgangspunkt")]
    public required string Adgangspunkt { get; set; }

    [JsonPropertyName("husnummerHoererTilIPostnummer")]
    public required HusnummerHoererTilPostnummer HusnummerHoererTilPostnummer { get; set; }

    [JsonPropertyName("HusnummerHarAdgangspunkt")]
    public required HusnummerHarAdgangspunkt HusnummerHarAdgangspunkt { get; set; }

    [JsonPropertyName("husnummerLiggerISogneInddeling")]
    public required HusnummerLiggerISogneInddeling? HusnummerLiggerISogneInddeling { get; set; }
}

public record HusnummerHoererTilPostnummer
{
    [JsonPropertyName("id_lokalId")]
    public required string IdLokalId { get; set; }

    [JsonPropertyName("navn")]
    public required string Navn { get; set; }

    [JsonPropertyName("postnr")]
    public required string Postnr { get; set; }
}

public record HusnummerHarAdgangspunkt
{
    [JsonPropertyName("position")]
    public required Position Position { get; set; }

    [JsonPropertyName("virkningFra")]
    public required DateTime VirkningFra { get; set; }
}

public record Position
{
    [JsonPropertyName("wkt")]
    public required string Wkt { get; set; }
}

public record HusnummerLiggerISogneInddeling
{
    [JsonPropertyName("id_lokalId")]
    public required string IdLokalId { get; set; }

    [JsonPropertyName("navn")]
    public required string Navn { get; set; }
}
