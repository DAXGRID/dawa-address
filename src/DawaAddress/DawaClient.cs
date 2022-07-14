using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DawaAddress;

public class DawaClient
{
    private const string _baseAddress = "https://api.dataforsyningen.dk/replikering";
    private readonly HttpClient _httpClient;

    public DawaClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DawaTransaction> GetLatestTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transactionUrl = new Uri($"{_baseAddress}/senestetransaktion");

        using var response = await _httpClient.GetAsync(transactionUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        return await JsonSerializer.DeserializeAsync<DawaTransaction>(stream, options: null, cancellationToken).ConfigureAwait(false) ??
            throw new DawaEmptyResultException("Retrieved empty result from DAWA.");
    }

    public async IAsyncEnumerable<DawaAccessAddress> GetAllAccessAddresses(
        ulong tId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var accessAddressUrl = new Uri($"{_baseAddress}/udtraek?entitet=adgangsadresse&ndjson&txid={tId}");

        using var response = await _httpClient.GetAsync(accessAddressUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var streamReader = new StreamReader(stream);

        string? line = null;
        while ((line = await streamReader.ReadLineAsync().ConfigureAwait(false)) is not null)
        {
            yield return JsonSerializer.Deserialize<DawaAccessAddress>(line)
                ?? throw new DawaEmptyResultException("Received empty value from DAWA.");
        }
    }

    public async IAsyncEnumerable<DawaUnitAddress> GetAllUnitAddresses(
        ulong tId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{_baseAddress}/udtraek?entitet=adresse&ndjson&txid={tId}");

        using var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var streamReader = new StreamReader(stream);

        string? line = null;
        while ((line = await streamReader.ReadLineAsync().ConfigureAwait(false)) is not null)
        {
            yield return JsonSerializer.Deserialize<DawaUnitAddress>(line)
                ?? throw new DawaEmptyResultException("Retrieved empty value from DAWA.");
        }
    }

    public async IAsyncEnumerable<DawaEntityChange<DawaAccessAddress>> GetChangesAccessAddressAsync(
        ulong fromTransactionId,
        ulong toTransactionId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var url = new Uri(@$"{_baseAddress}/haendelser?entitet=adgangsadresse&txidfra={fromTransactionId}&txidtil={toTransactionId}");

        using var accessAddressResponse = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        using var stream = await accessAddressResponse.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        var changesAsyncEnumerable = JsonSerializer
            .DeserializeAsyncEnumerable<DawaEntityChange<DawaAccessAddress>>(stream, null, cancellationToken)
            .ConfigureAwait(false);

        await foreach (var change in changesAsyncEnumerable)
        {
            yield return change ?? throw new DawaEmptyResultException("Received empty value from DAWA.");
        }
    }

    public async IAsyncEnumerable<DawaEntityChange<DawaUnitAddress>> GetChangesUnitAddressAsync(
        ulong fromTransactionId,
        ulong toTransactionId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var url = new Uri(@$"{_baseAddress}/haendelser?entitet=adresse&txidfra={fromTransactionId}&txidtil={toTransactionId}");

        using var accessAddressResponse = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        using var stream = await accessAddressResponse.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        var changesAsyncEnumerable = JsonSerializer
            .DeserializeAsyncEnumerable<DawaEntityChange<DawaUnitAddress>>(stream, null, cancellationToken)
            .ConfigureAwait(false);

        await foreach (var change in changesAsyncEnumerable)
        {
            yield return change ?? throw new DawaEmptyResultException("Received empty value from DAWA.");
        }
    }

    public async IAsyncEnumerable<DawaEntityChange<DawaRoad>> GetChangesRoadsAsync(
        ulong fromTransactionId,
        ulong toTransactionId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var url = new Uri(@$"{_baseAddress}/haendelser?entitet=navngivenvej&txidfra={fromTransactionId}&txidtil={toTransactionId}");

        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        var changesAsyncEnumerable = JsonSerializer
            .DeserializeAsyncEnumerable<DawaEntityChange<DawaRoad>>(stream, null, cancellationToken)
            .ConfigureAwait(false);

        await foreach (var road in changesAsyncEnumerable)
        {
            yield return road ?? throw new DawaEmptyResultException("Received empty value from DAWA.");
        }
    }

    public async IAsyncEnumerable<DawaRoad> GetAllRoadsAsync(
        ulong tId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var postNumberUrl = new Uri($"{_baseAddress}/udtraek?entitet=navngivenvej&txid={tId}");

        using var response = await _httpClient.GetAsync(postNumberUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        var roadsAsyncEnumerable = JsonSerializer
            .DeserializeAsyncEnumerable<DawaRoad>(stream, null, cancellationToken)
            .ConfigureAwait(false);

        await foreach (var road in roadsAsyncEnumerable)
        {
            yield return road ?? throw new DawaEmptyResultException("Received empty value from DAWA.");
        }
    }

    public async IAsyncEnumerable<DawaPostCode> GetAllPostCodesAsync(
        ulong tId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var postNumberUrl = new Uri($"{_baseAddress}/udtraek?entitet=postnummer&txid={tId}");

        using var response = await _httpClient.GetAsync(postNumberUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        var postCodesAsyncEnumerable = JsonSerializer
            .DeserializeAsyncEnumerable<DawaPostCode>(stream, null, cancellationToken)
            .ConfigureAwait(false);

        await foreach (var postCode in postCodesAsyncEnumerable)
        {
            yield return postCode ?? throw new DawaEmptyResultException("Received empty value from DAWA.");
        }
    }

    public async IAsyncEnumerable<DawaEntityChange<DawaPostCode>> GetChangesPostCodesAsync(
        ulong fromTransactionId,
        ulong toTransactionId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var url = new Uri(@$"{_baseAddress}/haendelser?entitet=postnummer&txidfra={fromTransactionId}&txidtil={toTransactionId}");

        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        var changesAsyncEnumerable = JsonSerializer
            .DeserializeAsyncEnumerable<DawaEntityChange<DawaPostCode>>(stream, null, cancellationToken)
            .ConfigureAwait(false);

        await foreach (var postCodeChange in changesAsyncEnumerable)
        {
            yield return postCodeChange ?? throw new DawaEmptyResultException("Received empty value from DAWA.");
        }
    }
}
