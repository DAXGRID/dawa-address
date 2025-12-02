using System.Text.Json.Serialization;

namespace DawaAddress;

public record DawaUnitAddress
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }

    [JsonPropertyName("adgangsadresseid")]
    public required Guid AccessAddressId { get; init; }

    [JsonPropertyName("status")]
    public required DawaStatus Status { get; init; }

    [JsonPropertyName("etage")]
    public string? FloorName { get; init; }

    [JsonPropertyName("dør")]
    public string? SuitName { get; init; }

    [JsonPropertyName("oprettet")]
    public required DateTime Created { get; init; }

    [JsonPropertyName("ændret")]
    public required DateTime Updated { get; init; }

    public DawaUnitAddress() { }

    [JsonConstructor]
    public DawaUnitAddress(
        Guid id,
        Guid accessAddressId,
        DawaStatus status,
        string? floorName,
        string? suitName,
        DateTime created,
        DateTime updated)
    {
        Id = id;
        AccessAddressId = accessAddressId;
        Status = status;
        FloorName = floorName;
        SuitName = suitName;
        Created = created;
        Updated = updated;
    }
}
