using System.Text.Json.Serialization;

namespace DawaAddress;

public record DawaPostCode
{
    [JsonPropertyName("nr")]
    public string Number { get; init; }

    [JsonPropertyName("navn")]
    public string Name { get; init; }

    [JsonConstructor]
    public DawaPostCode(string name, string number)
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
    }
}
