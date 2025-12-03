using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DawaAddress;

// We disable CA1008 because it can be confusing to consumers to
// have to check for None, since that is invalid.
#pragma warning disable CA1008

// 2 - foreløbige navngivne veje
// 3 - gældende navngivne veje
// 4 - nedlagte navngivne veje
// 5 - henlagte navngivne veje
public enum NamedRoadMunicipalDistrictStatus
{
    [Description("Temporary")]
    Temporary = 2,

    [Description("Active")]
    Active = 3,

    [Description("Discontinued")]
    Discontinued = 4,

    [Description("Canceled")]
    Canceled = 5
}

#pragma warning restore CA1008

// dar_navngivenvejkommunedel_aktuel
public record NamedRoadMunicipalDistrict
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("kommune")]
    public string? MunicipalityCode { get; init; }

    [JsonPropertyName("vejkode")]
    public string? RoadCode { get; init; }

    [JsonPropertyName("status")]
    public required NamedRoadMunicipalDistrictStatus Status { get; init; }

    [JsonPropertyName("navngivenvej_id")]
    public required Guid NamedRoadId { get; init; }

    public NamedRoadMunicipalDistrict() {}

    [JsonConstructor]
    public NamedRoadMunicipalDistrict(
        Guid id,
        string? municipalityCode,
        string? roadCode,
        NamedRoadMunicipalDistrictStatus status,
        Guid namedRoadId)
    {
        Id = id;
        MunicipalityCode = municipalityCode;
        RoadCode = roadCode;
        Status = status;
        NamedRoadId = namedRoadId;
    }
}
