using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DawaAddress;

public class DataForsyningenClient
{
    private const string _baseAddress = "https://api.dataforsyningen.dk/replikering";
    private readonly HttpClient _httpClient;

    public DataForsyningenClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<DawaTransaction>> GetAllTransactionsAfter(ulong transactionId, CancellationToken cancellationToken = default)
    {
        // We increment the transactionId so we only get the transactions after that transaction id,
        // Otherwise it would be included in the results.
        var uri = new Uri($"{_baseAddress}/transaktioner?txidfra={transactionId + 1}");

        using var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        return await JsonSerializer.DeserializeAsync<List<DawaTransaction>>(stream, options: null, cancellationToken).ConfigureAwait(false) ??
            throw new DawaEmptyResultException("Retrieved empty result from DAWA.");
    }

    public async Task<DawaTransaction> GetLatestTransactionAsync(CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{_baseAddress}/senestetransaktion");

        using var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        return await JsonSerializer.DeserializeAsync<DawaTransaction>(stream, options: null, cancellationToken).ConfigureAwait(false) ??
            throw new DawaEmptyResultException("Retrieved empty result from DAWA.");
    }

    public async IAsyncEnumerable<DawaAccessAddress> GetAllAccessAddresses(
        ulong tId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{_baseAddress}/udtraek?entitet=adgangsadresse&ndjson&txid={tId}");
        await foreach (var dawaEntity in StreamDawaJsonLine<DawaAccessAddress>(uri, cancellationToken).ConfigureAwait(false))
        {
            yield return dawaEntity;
        }
    }

    public async IAsyncEnumerable<DawaUnitAddress> GetAllUnitAddresses(
        ulong tId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{_baseAddress}/udtraek?entitet=adresse&ndjson&txid={tId}");
        await foreach (var dawaEntity in StreamDawaJsonLine<DawaUnitAddress>(uri, cancellationToken).ConfigureAwait(false))
        {
            yield return dawaEntity;
        }
    }

    public async IAsyncEnumerable<DawaRoad> GetAllRoadsAsync(
        ulong tId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{_baseAddress}/udtraek?entitet=navngivenvej&ndjson&txid={tId}");
        await foreach (var dawaEntity in StreamDawaJsonLine<DawaRoad>(uri, cancellationToken).ConfigureAwait(false))
        {
            yield return dawaEntity;
        }
    }

    public async IAsyncEnumerable<DawaPostCode> GetAllPostCodesAsync(
        ulong tId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{_baseAddress}/udtraek?entitet=postnummer&ndjson&txid={tId}");
        await foreach (var dawaEntity in StreamDawaJsonLine<DawaPostCode>(uri, cancellationToken).ConfigureAwait(false))
        {
            yield return dawaEntity;
        }
    }

    public async IAsyncEnumerable<NamedRoadMunicipalDistrict> GetAllNamedRoadMunicipalDistrictsAsync(
        ulong tId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var uri = new Uri($"{_baseAddress}/udtraek?entitet=dar_navngivenvejkommunedel_aktuel&ndjson&txid={tId}");
        await foreach (var dawaEntity in StreamDawaJsonLine<NamedRoadMunicipalDistrict>(uri, cancellationToken).ConfigureAwait(false))
        {
            yield return dawaEntity;
        }
    }

    public async IAsyncEnumerable<DawaEntityChange<DawaAccessAddress>> GetChangesAccessAddressAsync(
        ulong fromTransactionId,
        ulong toTransactionId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var uri = new Uri(@$"{_baseAddress}/haendelser?entitet=adgangsadresse&ndjson&txidfra={fromTransactionId}&txidtil={toTransactionId}");
        await foreach (var dawaEntity in StreamDawaJsonLine<DawaEntityChange<DawaAccessAddress>>(uri, cancellationToken).ConfigureAwait(false))
        {
            yield return dawaEntity;
        }
    }

    public async IAsyncEnumerable<DawaEntityChange<DawaUnitAddress>> GetChangesUnitAddressAsync(
        ulong fromTransactionId,
        ulong toTransactionId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var uri = new Uri(@$"{_baseAddress}/haendelser?entitet=adresse&ndjson&txidfra={fromTransactionId}&txidtil={toTransactionId}");
        await foreach (var dawaEntity in StreamDawaJsonLine<DawaEntityChange<DawaUnitAddress>>(uri, cancellationToken).ConfigureAwait(false))
        {
            yield return dawaEntity;
        }
    }

    public async IAsyncEnumerable<DawaEntityChange<DawaRoad>> GetChangesRoadsAsync(
        ulong fromTransactionId,
        ulong toTransactionId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var uri = new Uri(@$"{_baseAddress}/haendelser?entitet=navngivenvej&ndjson&txidfra={fromTransactionId}&txidtil={toTransactionId}");
        await foreach (var dawaEntity in StreamDawaJsonLine<DawaEntityChange<DawaRoad>>(uri, cancellationToken).ConfigureAwait(false))
        {
            yield return dawaEntity;
        }
    }

    public async IAsyncEnumerable<DawaEntityChange<DawaPostCode>> GetChangesPostCodesAsync(
        ulong fromTransactionId,
        ulong toTransactionId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var uri = new Uri(@$"{_baseAddress}/haendelser?entitet=postnummer&ndjson&txidfra={fromTransactionId}&txidtil={toTransactionId}");
        await foreach (var dawaEntity in StreamDawaJsonLine<DawaEntityChange<DawaPostCode>>(uri, cancellationToken).ConfigureAwait(false))
        {
            yield return dawaEntity;
        }
    }

    public async IAsyncEnumerable<DawaEntityChange<NamedRoadMunicipalDistrict>> GetChangesNamedRoadMunicipalDistrictAsync(
        ulong fromTransactionId,
        ulong toTransactionId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var uri = new Uri(@$"{_baseAddress}/haendelser?entitet=dar_navngivenvejkommunedel_aktuel&ndjson&txidfra={fromTransactionId}&txidtil={toTransactionId}");
        await foreach (var dawaEntity in StreamDawaJsonLine<DawaEntityChange<NamedRoadMunicipalDistrict>>(uri, cancellationToken).ConfigureAwait(false))
        {
            yield return dawaEntity;
        }
    }

    private async IAsyncEnumerable<T> StreamDawaJsonLine<T>(Uri uri, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var streamReader = new StreamReader(stream);

        string? line = null;
        while ((line = await streamReader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) is not null)
        {
            yield return JsonSerializer.Deserialize<T>(line)
                ?? throw new DawaEmptyResultException("Received empty value from DAWA.");
        }
    }
}
