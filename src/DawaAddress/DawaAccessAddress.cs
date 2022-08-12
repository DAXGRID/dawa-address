using System.Text.Json.Serialization;

namespace DawaAddress;

public record DawaAccessAddress
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }
    [JsonPropertyName("oprettet")]
    public DateTime Created { get; init; }
    [JsonPropertyName("ændret")]
    public DateTime? Updated { get; init; }
    [JsonPropertyName("kommunekode")]
    public string MunicipalCode { get; init; }
    [JsonPropertyName("status")]
    public DawaStatus Status { get; init; }
    [JsonPropertyName("vejkode")]
    public string RoadCode { get; init; }
    [JsonPropertyName("husnr")]
    public string HouseNumber { get; init; }
    [JsonPropertyName("postnr")]
    public string PostDistrictCode { get; init; }
    [JsonPropertyName("etrs89koordinat_øst")]
    public double EastCoordinate { get; init; }
    [JsonPropertyName("etrs89koordinat_nord")]
    public double NorthCoordinate { get; init; }
    [JsonPropertyName("adressepunktændringsdato")]
    public DateTime? LocationUpdated { get; init; }
    [JsonPropertyName("supplerendebynavn")]
    public string? SupplementaryTownName { get; init; }
    [JsonPropertyName("matrikelnr")]
    public string? PlotId { get; init; }
    [JsonPropertyName("navngivenvej_id")]
    public Guid RoadId { get; init; }

    [JsonConstructor]
    public DawaAccessAddress(
        Guid id,
        DateTime created,
        DateTime? updated,
        string municipalCode,
        DawaStatus status,
        string roadCode,
        string houseNumber,
        string postDistrictCode,
        double eastCoordinate,
        double northCoordinate,
        DateTime? locationUpdated,
        string? supplementaryTownName,
        string? plotId,
        Guid roadId)
    {
        Id = id;
        Created = created;
        Updated = updated;
        MunicipalCode = municipalCode;
        RoadCode = roadCode;
        HouseNumber = houseNumber;
        PostDistrictCode = postDistrictCode;
        EastCoordinate = eastCoordinate;
        NorthCoordinate = northCoordinate;
        LocationUpdated = locationUpdated;
        SupplementaryTownName = supplementaryTownName;
        PlotId = plotId;
        RoadId = roadId;
        Status = status;
    }
}
