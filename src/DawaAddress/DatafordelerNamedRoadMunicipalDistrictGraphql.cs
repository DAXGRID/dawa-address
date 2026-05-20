using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DawaAddress;

public sealed record DatafordelerNamedRoadMunicipalDistrictGraphqlResponse
{
    [JsonPropertyName("DAR_NavngivenVejKommunedel")]
    public required DarNavngivenVejKommunedel DarNavngivenVejKommunedel { get; init; }
}

public sealed record DarNavngivenVejKommunedel
{
    [JsonPropertyName("pageInfo")]
    public required DatafordelerNamedRoadMunicipalDistrictPageInfo PageInfo { get; init; }

    [JsonPropertyName("nodes")]
    public required IReadOnlyList<NavngivenVejKommunedelNode> Nodes { get; init; }
}

public sealed record DatafordelerNamedRoadMunicipalDistrictPageInfo
{
    [JsonPropertyName("endCursor")]
    public required string EndCursor { get; init; }

    [JsonPropertyName("hasNextPage")]
    public required bool HasNextPage { get; init; }
}

public sealed record NavngivenVejKommunedelNode
{
    [JsonPropertyName("virkningTil")]
    public required DateTime? VirkningTil { get; init; }

    [JsonPropertyName("virkningsaktoer")]
    public required string Virkningsaktoer { get; init; }

    [JsonPropertyName("vejkode")]
    public required string Vejkode { get; init; }

    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("virkningFra")]
    public required DateTime VirkningFra { get; init; }

    [JsonPropertyName("registreringTil")]
    public required DateTime? RegistreringTil { get; init; }

    [JsonPropertyName("registreringsaktoer")]
    public required string Registreringsaktoer { get; init; }

    [JsonPropertyName("registreringFra")]
    public required DateTime RegistreringFra { get; init; }

    [JsonPropertyName("navngivenVej")]
    public required Guid NavngivenVej { get; init; }

    [JsonPropertyName("kommune")]
    public required string Kommune { get; init; }

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

    [JsonPropertyName("datafordelerRegisterImportSequenceNumber")]
    public required int DatafordelerRegisterImportSequenceNumber { get; init; }

    [JsonPropertyName("datafordelerRowId")]
    public required Guid DatafordelerRowId { get; init; }

    [JsonPropertyName("datafordelerOpdateringstid")]
    public required DateTime DatafordelerOpdateringstid { get; init; }
}
