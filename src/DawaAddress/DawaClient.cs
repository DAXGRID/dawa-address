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

        using var response = await _httpClient.GetAsync(transactionUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        var result = await JsonSerializer.DeserializeAsync<DawaTransaction>(stream).ConfigureAwait(false);

        return result ?? throw new DawaEmptyResultException("Retrieved empty result from DAWA.");
    }

    public async IAsyncEnumerable<DawaAccessAddress> GetAllAccessAddresses(ulong tId)
    {
        var accessAddressUrl = new Uri($"{_baseAddress}/udtraek?entitet=adgangsadresse&ndjson&txid={tId}");

        using var response = await _httpClient.GetAsync(accessAddressUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        using var streamReader = new StreamReader(stream);

        string? line = null;
        while ((line = await streamReader.ReadLineAsync().ConfigureAwait(false)) is not null)
        {
            yield return JsonSerializer.Deserialize<DawaAccessAddress>(line)
                ?? throw new DawaEmptyResultException("Received empty value from DAWA.");
        }
    }

    public async IAsyncEnumerable<DawaUnitAddress> GetAllUnitAddresses(ulong tId)
    {
        var uri = new Uri($"{_baseAddress}/udtraek?entitet=adresse&ndjson&txid={tId}");

        using var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        using var streamReader = new StreamReader(stream);

        string? line = null;
        while ((line = await streamReader.ReadLineAsync().ConfigureAwait(false)) is not null)
        {
            yield return JsonSerializer.Deserialize<DawaUnitAddress>(line)
                ?? throw new DawaEmptyResultException("Retrieved empty value from DAWA.");
        }
    }

    public async IAsyncEnumerable<DawaEntityChange<DawaAccessAddress>> GetChangesAccessAddressAsync(ulong fromTransactionId, ulong toTransactionId)
    {
        var url = new Uri(@$"{_baseAddress}/haendelser?entitet=adgangsadresse&txidfra={fromTransactionId}&txidtil={toTransactionId}");

        using var accessAddressResponse = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        using var stream = await accessAddressResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);

        await foreach (var change in JsonSerializer.DeserializeAsyncEnumerable<DawaEntityChange<DawaAccessAddress>>(stream).ConfigureAwait(false))
        {
            yield return change ?? throw new DawaEmptyResultException("Received empty value from DAWA.");
        }
    }

    public async IAsyncEnumerable<DawaEntityChange<DawaUnitAddress>> GetChangesUnitAddressAsync(ulong fromTransactionId, ulong toTransactionId)
    {
        var url = new Uri(@$"{_baseAddress}/haendelser?entitet=adresse&txidfra={fromTransactionId}&txidtil={toTransactionId}");

        using var accessAddressResponse = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        using var stream = await accessAddressResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);

        await foreach (var change in JsonSerializer.DeserializeAsyncEnumerable<DawaEntityChange<DawaUnitAddress>>(stream).ConfigureAwait(false))
        {
            yield return change ?? throw new DawaEmptyResultException("Received empty value from DAWA.");
        }
    }

    public async IAsyncEnumerable<DawaEntityChange<DawaRoad>> GetChangesRoadsAsync(ulong fromTransactionId, ulong toTransactionId)
    {
        var url = new Uri(@$"{_baseAddress}/haendelser?entitet=navngivenvej&txidfra={fromTransactionId}&txidtil={toTransactionId}");

        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        var roadsStream = JsonSerializer.DeserializeAsyncEnumerable<DawaEntityChange<DawaRoad>>(stream);
        await foreach (var road in roadsStream.ConfigureAwait(false))
        {
            yield return road ?? throw new DawaEmptyResultException("Received empty value from DAWA.");
        }
    }

    public async IAsyncEnumerable<DawaRoad> GetAllRoadsAsync(ulong tId)
    {
        var postNumberUrl = new Uri($"{_baseAddress}/udtraek?entitet=navngivenvej&txid={tId}");

        using var response = await _httpClient.GetAsync(postNumberUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        var roadsStream = JsonSerializer.DeserializeAsyncEnumerable<DawaRoad>(stream);
        await foreach (var road in roadsStream.ConfigureAwait(false))
        {
            yield return road ?? throw new DawaEmptyResultException("Received empty value from DAWA.");
        }
    }

    public async IAsyncEnumerable<DawaPostCode> GetAllPostCodesAsync(ulong tId)
    {
        var postNumberUrl = new Uri($"{_baseAddress}/udtraek?entitet=postnummer&txid={tId}");

        using var response = await _httpClient.GetAsync(postNumberUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        await foreach (var postCode in JsonSerializer.DeserializeAsyncEnumerable<DawaPostCode>(stream).ConfigureAwait(false))
        {
            yield return postCode ?? throw new DawaEmptyResultException("Received empty value from DAWA.");
        }
    }

    public async IAsyncEnumerable<DawaEntityChange<DawaPostCode>> GetChangesPostCodesAsync(ulong fromTransactionId, ulong toTransactionId)
    {
        var url = new Uri(@$"{_baseAddress}/haendelser?entitet=postnummer&txidfra={fromTransactionId}&txidtil={toTransactionId}");

        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        await foreach (var postCodeChange in JsonSerializer.DeserializeAsyncEnumerable<DawaEntityChange<DawaPostCode>>(stream).ConfigureAwait(false))
        {
            yield return postCodeChange ?? throw new DawaEmptyResultException("Received empty value from DAWA.");
        }
    }
}
