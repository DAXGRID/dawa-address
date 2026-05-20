using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DawaAddress;

public sealed record DatafordelerPostCodeGraphqlResponse
{
    [JsonPropertyName("DAR_Postnummer")]
    public required DarPostnummer DarPostnummer { get; init; }
}

public sealed record DarPostnummer
{
    [JsonPropertyName("pageInfo")]
    public required DatafordelerPostCodeGraphqlPageInfo PageInfo { get; init; }

    [JsonPropertyName("nodes")]
    public required IReadOnlyList<PostnummerNode> Nodes { get; init; }
}

public sealed record DatafordelerPostCodeGraphqlPageInfo
{
    [JsonPropertyName("endCursor")]
    public required string EndCursor { get; init; }

    [JsonPropertyName("hasNextPage")]
    public required bool HasNextPage { get; init; }
}

public sealed record PostnummerNode
{
    [JsonPropertyName("datafordelerOpdateringstid")]
    public required DateTime DatafordelerOpdateringstid { get; init; }

    [JsonPropertyName("datafordelerRegisterImportSequenceNumber")]
    public required int DatafordelerRegisterImportSequenceNumber { get; init; }

    [JsonPropertyName("datafordelerRowId")]
    public required Guid DatafordelerRowId { get; init; }

    [JsonPropertyName("datafordelerRowVersion")]
    public required int DatafordelerRowVersion { get; init; }

    [JsonPropertyName("forretningshaendelse")]
    public required string Forretningshaendelse { get; init; }

    [JsonPropertyName("forretningsomraade")]
    public required string Forretningsomraade { get; init; }

    [JsonPropertyName("forretningsproces")]
    public required string Forretningsproces { get; init; }

    [JsonPropertyName("virkningTil")]
    public required DateTime? VirkningTil { get; init; }

    [JsonPropertyName("virkningsaktoer")]
    public required string Virkningsaktoer { get; init; }

    [JsonPropertyName("virkningFra")]
    public required DateTime VirkningFra { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("registreringTil")]
    public required DateTimeOffset? RegistreringTil { get; init; }

    [JsonPropertyName("registreringsaktoer")]
    public required string Registreringsaktoer { get; init; }

    [JsonPropertyName("registreringFra")]
    public required DateTimeOffset RegistreringFra { get; init; }

    [JsonPropertyName("postnummerinddeling")]
    public required string Postnummerinddeling { get; init; }

    [JsonPropertyName("postnr")]
    public required string Postnr { get; init; }

    [JsonPropertyName("navn")]
    public required string Navn { get; init; }

    [JsonPropertyName("id_namespace")]
    public required string IdNamespace { get; init; }

    [JsonPropertyName("id_lokalId")]
    public required Guid IdLokalId { get; init; }
}
