using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DawaAddress;

public sealed record DatafordelerUnitAddressGraphqlResponse
{
    [JsonPropertyName("DAR_Adresse")]
    public required DarAdresse DarAdresse { get; init; }
}

public sealed record DarAdresse
{
    [JsonPropertyName("pageInfo")]
    public required DatafordelerUnitAddressGraphqlPageInfo PageInfo { get; init; }

    [JsonPropertyName("nodes")]
    public required IReadOnlyList<AddressNode> Nodes { get; init; }
}

public sealed record DatafordelerUnitAddressGraphqlPageInfo
{
    [JsonPropertyName("endCursor")]
    public required string EndCursor { get; init; }

    [JsonPropertyName("hasNextPage")]
    public required bool HasNextPage { get; init; }
}

public sealed record AddressNode
{
    [JsonPropertyName("adressebetegnelse")]
    public required string Adressebetegnelse { get; init; }

    [JsonPropertyName("forretningshaendelse")]
    public required string Forretningshaendelse { get; init; }

    [JsonPropertyName("etagebetegnelse")]
    public required string Etagebetegnelse { get; init; }

    [JsonPropertyName("doerbetegnelse")]
    public required string Doerbetegnelse { get; init; }

    [JsonPropertyName("doerpunkt")]
    public required object? Doerpunkt { get; init; }

    [JsonPropertyName("datafordelerRowVersion")]
    public required int DatafordelerRowVersion { get; init; }

    [JsonPropertyName("datafordelerRowId")]
    public required Guid DatafordelerRowId { get; init; }

    [JsonPropertyName("datafordelerRegisterImportSequenceNumber")]
    public required int DatafordelerRegisterImportSequenceNumber { get; init; }

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

    [JsonPropertyName("id_namespace")]
    public required string IdNamespace { get; init; }

    [JsonPropertyName("id_lokalId")]
    public required Guid IdLokalId { get; init; }

    [JsonPropertyName("husnummer")]
    public required Guid Husnummer { get; init; }

    [JsonPropertyName("forretningsproces")]
    public required string Forretningsproces { get; init; }

    [JsonPropertyName("forretningsomraade")]
    public required string Forretningsomraade { get; init; }

    [JsonPropertyName("datafordelerOpdateringstid")]
    public required DateTimeOffset DatafordelerOpdateringstid { get; init; }

    [JsonPropertyName("bygning")]
    public required object? Bygning { get; init; }
}
