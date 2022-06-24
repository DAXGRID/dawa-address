using System.Text.Json;

[assembly: CLSCompliant(false)]
namespace DawaAddress;

public class DawaClient
{
    private const string _baseAddress = "https://api.dataforsyningen.dk/replikering";
    private readonly HttpClient _httpClient;

    public DawaClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DawaTransaction> GetLatestTransactionAsync()
    {
        var transactionUrl = new Uri($"{_baseAddress}/senestetransaktion");

        using var response = await _httpClient
                      .GetAsync(transactionUrl, HttpCompletionOption.ResponseHeadersRead)
                      .ConfigureAwait(false);

        using var stream = await response.Content
                      .ReadAsStreamAsync()
                      .ConfigureAwait(false);

        var result = await JsonSerializer
            .DeserializeAsync<DawaTransaction>(stream)
            .ConfigureAwait(false);

        return result ??
            throw new DawaEmptyResultException("Retrieved empty result from DAWA.");
    }

    public async IAsyncEnumerable<DawaAccessAddress> GetAllAccessAddresses(ulong tId)
    {
        var accessAddressUrl = new Uri($"{_baseAddress}/udtraek?entitet=adgangsadresse&ndjson&txid={tId}");
        using var response = await _httpClient
                      .GetAsync(accessAddressUrl, HttpCompletionOption.ResponseHeadersRead)
                      .ConfigureAwait(false);

        using var stream = await response.Content
                      .ReadAsStreamAsync()
                      .ConfigureAwait(false);

        using var streamReader = new StreamReader(stream);

        string? line = null;
        while ((line = await streamReader.ReadLineAsync().ConfigureAwait(false)) is not null)
        {
            yield return JsonSerializer.Deserialize<DawaAccessAddress>(line)
                ?? throw new DawaEmptyResultException("Received empty value from DAWA.");
        }
    }

    public async IAsyncEnumerable<DawaRoad> GetRoadsAsync(ulong transactionId)
    {
        var postNumberUrl = new Uri($"{_baseAddress}/udtraek?entitet=navngivenvej&txid={transactionId}");
        using var response = await _httpClient
                      .GetAsync(postNumberUrl, HttpCompletionOption.ResponseHeadersRead)
                      .ConfigureAwait(false);

        using var stream = await response.Content
                      .ReadAsStreamAsync()
                      .ConfigureAwait(false);

        var roadsStream = JsonSerializer.DeserializeAsyncEnumerable<DawaRoad>(stream);
        await foreach (var road in roadsStream.ConfigureAwait(false))
        {
            yield return road ??
                throw new DawaEmptyResultException("Received empty value from DAWA.");
        }
    }

    public async IAsyncEnumerable<DawaPostCode> GetPostCodesAsync(ulong transactionId)
    {
        var postNumberUrl = new Uri($"{_baseAddress}/udtraek?entitet=postnummer&txid={transactionId}");

        using var response = await _httpClient
                      .GetAsync(postNumberUrl, HttpCompletionOption.ResponseHeadersRead)
                      .ConfigureAwait(false);

        using var stream = await response.Content
                      .ReadAsStreamAsync()
                      .ConfigureAwait(false);

        var postCodeStream = JsonSerializer.DeserializeAsyncEnumerable<DawaPostCode>(stream);
        await foreach (var road in postCodeStream.ConfigureAwait(false))
        {
            yield return road ??
                throw new DawaEmptyResultException("Received empty value from DAWA.");
        }
    }
}
