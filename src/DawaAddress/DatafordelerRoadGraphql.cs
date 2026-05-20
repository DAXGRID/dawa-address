using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DawaAddress;

public sealed record DatafordelerRoadGraphqlResponse
{
    [JsonPropertyName("DAR_NavngivenVej")]
    public required DarNavngivenVej DarNavngivenVej { get; init; }
}

public sealed record DarNavngivenVej
{
    [JsonPropertyName("pageInfo")]
    public required DatafordelerRoadGraphqlPageInfo PageInfo { get; init; }

    [JsonPropertyName("nodes")]
    public required IReadOnlyList<NavngivenVejNode> Nodes { get; init; }
}

public sealed record DatafordelerRoadGraphqlPageInfo
{
    [JsonPropertyName("endCursor")]
    public required string EndCursor { get; init; }

    [JsonPropertyName("hasNextPage")]
    public required bool HasNextPage { get; init; }
}

public sealed record NavngivenVejNode
{
    [JsonPropertyName("virkningTil")]
    public required DateTime? VirkningTil { get; init; }

    [JsonPropertyName("virkningFra")]
    public required DateTime VirkningFra { get; init; }

    [JsonPropertyName("virkningsaktoer")]
    public required string Virkningsaktoer { get; init; }

    [JsonPropertyName("vejnavn")]
    public required string Vejnavn { get; init; }

    [JsonPropertyName("vejadresseringsnavn")]
    public required string Vejadresseringsnavn { get; init; }

    [JsonPropertyName("udtaltVejnavn")]
    public required string UdtaltVejnavn { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("registreringTil")]
    public required DateTime? RegistreringTil { get; init; }

    [JsonPropertyName("registreringsaktoer")]
    public required string Registreringsaktoer { get; init; }

    [JsonPropertyName("registreringFra")]
    public required DateTime RegistreringFra { get; init; }

    [JsonPropertyName("id_namespace")]
    public required string IdNamespace { get; init; }

    [JsonPropertyName("id_lokalId")]
    public required Guid IdLokalId { get; init; }

    [JsonPropertyName("forretningsproces")]
    public required string Forretningsproces { get; init; }

    [JsonPropertyName("forretningsomraade")]
    public required string Forretningsomraade { get; init; }

    [JsonPropertyName("forretningshaendelse")]
    public required string Forretningshaendelse { get; init; }

    [JsonPropertyName("datafordelerRowVersion")]
    public required int DatafordelerRowVersion { get; init; }

    [JsonPropertyName("datafordelerRowId")]
    public required Guid DatafordelerRowId { get; init; }

    [JsonPropertyName("datafordelerRegisterImportSequenceNumber")]
    public required int DatafordelerRegisterImportSequenceNumber { get; init; }

    [JsonPropertyName("datafordelerOpdateringstid")]
    public required DateTime DatafordelerOpdateringstid { get; init; }

    [JsonPropertyName("beskrivelse")]
    public required string? Beskrivelse { get; init; }

    [JsonPropertyName("administreresAfKommune")]
    public required string AdministreresAfKommune { get; init; }
}
