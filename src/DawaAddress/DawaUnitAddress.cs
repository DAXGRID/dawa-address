using System.Text.Json.Serialization;

namespace DawaAddress;

public record DawaUnitAddress
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }
    [JsonPropertyName("adgangsadresseid")]
    public Guid AccessAddressId { get; init; }
    [JsonPropertyName("status")]
    public DawaStatus Status { get; init; }
    [JsonPropertyName("etage")]
    public string? FloorName { get; init; }
    [JsonPropertyName("dør")]
    public string? SuitName { get; init; }
    [JsonPropertyName("oprettet")]
    public DateTime Created { get; init; }
    [JsonPropertyName("ændret")]
    public DateTime? Updated { get; init; }

    [JsonConstructor]
    public DawaUnitAddress(
        Guid id,
        Guid accessAddressId,
        DawaStatus status,
        string? floorName,
        string? suitName,
        DateTime created,
        DateTime? updated)
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
