using System.Text.Json;
using System.Text.Json.Serialization;

namespace DawaAddress;

public enum DawaEntityChangeOperation
{
    Insert,
    Update,
    Delete
}

public class DawaEntityChangeOperationConverter : JsonConverter<DawaEntityChangeOperation>
{
    public override DawaEntityChangeOperation Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        MapOperation(reader.GetString() ?? throw new ArgumentNullException(nameof(reader)));

    public override void Write(
        Utf8JsonWriter writer, DawaEntityChangeOperation value, JsonSerializerOptions options) =>
        throw new NotImplementedException();

    private static DawaEntityChangeOperation MapOperation(string operation) =>
        operation switch
        {
            "insert" => DawaEntityChangeOperation.Insert,
            "update" => DawaEntityChangeOperation.Update,
            "delete" => DawaEntityChangeOperation.Delete,
            _ => throw new ArgumentException($"Could not convert '{operation}'", nameof(operation)),
        };
}

public record DawaEntityChange<T>
{
    [JsonPropertyName("txid")]
    public ulong Id { get; init; }

    [JsonPropertyName("operation")]
    [JsonConverter(typeof(DawaEntityChangeOperationConverter))]
    public DawaEntityChangeOperation Operation { get; init; }

    [JsonPropertyName("sekvensnummer")]
    public ulong SequenceNumber { get; init; }

    [JsonPropertyName("tidspunkt")]
    public DateTime ChangeTime { get; init; }

    [JsonPropertyName("data")]
    public T Data { get; init; }

    [JsonConstructor]
    public DawaEntityChange(
        ulong id,
        DawaEntityChangeOperation operation,
        ulong sequenceNumber,
        DateTime changeTime,
        T data)
    {
        Id = id;
        Operation = operation;
        SequenceNumber = sequenceNumber;
        ChangeTime = changeTime;
        Data = data;
    }
}
