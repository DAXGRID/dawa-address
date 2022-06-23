using System.Text.Json;
using System.Text.Json.Serialization;

[assembly: CLSCompliant(false)]
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

public class DawaClient
{
    private const string _dawaBaseAddress = "https://api.dataforsyningen.dk/replikering";
    private readonly HttpClient _httpClient;

    public DawaClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DawaTransaction> GetLatestTransactionAsync()
    {
        var transactionUrl = new Uri($"{_dawaBaseAddress}/senestetransaktion");

        using var response = await _httpClient
                      .GetAsync(transactionUrl, HttpCompletionOption.ResponseHeadersRead)
                      .ConfigureAwait(false);

        using var stream = await response.Content
                      .ReadAsStreamAsync()
                      .ConfigureAwait(false);

        var result = await JsonSerializer
            .DeserializeAsync<DawaTransaction>(stream).ConfigureAwait(false);

        return result ??
            throw new DawaEmptyResultException("Could not retrieve dawa transaction id.");
    }
}
