using System.Text.Json.Serialization;

namespace DawaAddress;

public record DatafordelerFileResponse
{
    [JsonPropertyName("availableFileDownloads")]
    public required IReadOnlyList<DatafordelerFile> AvailableFileDownloads { get; init; }
}

public record DatafordelerFile
{
    [JsonPropertyName("fileName")]
    public required string FileName { get; init; }

    [JsonPropertyName("register")]
    public required string Register { get; init; }

    [JsonPropertyName("entityName")]
    public required string EntityName { get; init; }

    [JsonPropertyName("typeOfDownload")]
    public required string TypeOfDownload { get; init; }

    [JsonPropertyName("typeOfData")]
    public required string TypeOfData { get; init; }

    [JsonPropertyName("generationNumber")]
    public required int GenerationNumber { get; init; }

    [JsonPropertyName("version")]
    public required string Version { get; init; }

    [JsonPropertyName("containedFileFormat")]
    public required string ContainedFileFormat { get; init; }
}
