using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DawaAddress;

public record DawaUnitAddress
{
    [JsonPropertyName("id")]
    public string Id { get; init; }
    [JsonPropertyName("adgangsadresseid")]
    public string AccessAddressId { get; init; }
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
        string id,
        string accessAddressId,
        DawaStatus status,
        string? floorName,
        string? suitName,
        DateTime created,
        DateTime? updated)
    {
        if (status == DawaStatus.None)
        {
            throw new InvalidEnumArgumentException($"{nameof(DawaStatus)} is required.");
        }

        Id = id;
        AccessAddressId = accessAddressId;
        Status = status;
        FloorName = floorName;
        SuitName = suitName;
        Created = created;
        Updated = updated;
    }
}
