using System.Text.Json.Serialization;

namespace DawaAddress;

public record DawaTransaction
{
    [JsonPropertyName("txid")]
    public ulong Id { get; init; }

    [JsonConstructor]
    public DawaTransaction(ulong Id)
    {
        if (Id == 0)
        {
            throw new ArgumentException("Cannot be 0.", nameof(Id));
        }

        this.Id = Id;
    }
}
