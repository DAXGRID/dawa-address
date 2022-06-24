using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DawaAddress;

public enum Status
{
    [Description("None")]
    None = 0,
    [Description("Active")]
    Active = 1,
    [Description("Canceled")]
    Canceled = 2,
    [Description("Pending")]
    Pending = 3,
    [Description("Discontinued")]
    Discontinued = 4
}

public record DawaAccessAddress
{
    [JsonPropertyName("id")]
    public string Id { get; init; }
    [JsonPropertyName("oprettet")]
    public DateTime Created { get; init; }
    [JsonPropertyName("ændret")]
    public DateTime Updated { get; init; }
    [JsonPropertyName("kommunekode")]
    public string MunicipalCode { get; init; }
    [JsonPropertyName("status")]
    public Status Status { get; init; }
    [JsonPropertyName("vejkode")]
    public string RoadCode { get; init; }
    [JsonPropertyName("husnr")]
    public string HouseNumber { get; init; }
    [JsonPropertyName("postnr")]
    public string PostDistrictCode { get; init; }
    [JsonPropertyName("etrs89koordinat_øst")]
    public double? EastCoordinate { get; init; }
    [JsonPropertyName("etrs89koordinat_nord")]
    public double? NorthCoordinate { get; init; }
    [JsonPropertyName("adressepunktændringsdato")]
    public DateTime? LocationUpdated { get; init; }
    [JsonPropertyName("supplerendebynavn")]
    public string TownName { get; init; }
    [JsonPropertyName("matrikelnr")]
    public string PlotId { get; init; }
    [JsonPropertyName("navngivenvej_id")]
    public string RoadId { get; init; }

    [JsonConstructor]
    public DawaAccessAddress(
        string id,
        DateTime created,
        DateTime updated,
        string municipalCode,
        Status status,
        string roadCode,
        string houseNumber,
        string postDistrictCode,
        double? eastCoordinate,
        double? northCoordinate,
        DateTime? locationUpdated,
        string townName,
        string plotId,
        string roadId)
    {
        if (status == Status.None)
        {
            throw new InvalidEnumArgumentException(nameof(status));
        }

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
        TownName = townName;
        PlotId = plotId;
        RoadId = roadId;
        Status = status;
    }
}
