using System.Text.Json.Serialization;

namespace DawaAddress;

public enum DawaPostCodeStatus
{
    Active,
    Discontinued,
}

public record DawaPostCode
{
    [JsonPropertyName("nr")]
    public string Number { get; init; }

    [JsonPropertyName("navn")]
    public string Name { get; init; }

    [JsonPropertyName("darstatus")]
    public DawaPostCodeStatus Status { get; init; }

    [JsonPropertyName("oprettet")]
    public DateTime Created { get; init; }

    [JsonPropertyName("ændret")]
    public DateTime? Updated { get; init; }

    [JsonConstructor]
    public DawaPostCode(
        string name,
        string number,
        DawaPostCodeStatus status,
        DateTime created,
        DateTime? updated)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(number))
        {
            throw new ArgumentNullException(nameof(name));
        }

        Name = name;
        Number = number;
        Status = status;
        Created = created;
        Updated = updated;
    }
}
