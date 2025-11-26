using System.Globalization;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DawaAddress;

public class DatafordelerClient
{
    private const string _baseAddress = "https://services.datafordeler.dk/DAR/DAR/3.0.0/rest";
    private readonly HttpClient _httpClient;

    public DatafordelerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private static DawaAccessAddress Map(DatafordelerAccessAddress datafordelerAccessAddress)
    {
        return new DawaAccessAddress
        {
            Created = datafordelerAccessAddress.RegistreringFra,
            Id = Guid.Parse(datafordelerAccessAddress.IdLokalId),
            EastCoordinate = 0,
            NorthCoordinate = 0,
            HouseNumber = "0",
            LocationUpdated = null,
            MunicipalCode = "0",
            Updated = DateTime.UtcNow,
            Status = DawaStatus.Active,
            PlotId = "",
            RoadCode = "",
            PostDistrictCode = "",
            RoadId = Guid.NewGuid(),
            SupplementaryTownName = ""
        };
    }

    public async IAsyncEnumerable<DawaAccessAddress> GetAllAccessAddresses(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var fromDate = DateTime.MinValue;
        var toDate = DateTime.UtcNow;
        const int pageSize = 200;
        var page = 1;
        const int status = 3;

        while (true)
        {
            var accessAddressResourcePath = BuildResourcePath(_baseAddress, "Husnummer", DateTime.MinValue, DateTime.UtcNow, pageSize, page, status);
            Console.WriteLine(accessAddressResourcePath);
            var response = await _httpClient.GetAsync(accessAddressResourcePath, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var dawaAccessAddresses = await response.Content.ReadFromJsonAsync<DatafordelerAccessAddress[]>(cancellationToken).ConfigureAwait(false);

            if (dawaAccessAddresses is null)
            {
                throw new InvalidOperationException(
                    $"Received NULL when trying to get DAWA Access addresses from path: '{accessAddressResourcePath}'.");
            }

            foreach (var dawaAddress in dawaAccessAddresses)
            {
                yield return Map(dawaAddress);
            }

            page++;
        }
    }

    // public async IAsyncEnumerable<DawaUnitAddress> GetAllUnitAddresses(
    //     ulong tId,
    //     [EnumeratorCancellation] CancellationToken cancellationToken = default)
    // {
    //     throw new NotImplementedException();
    // }

    // public async IAsyncEnumerable<DawaRoad> GetAllRoadsAsync(
    //     ulong tId,
    //     [EnumeratorCancellation] CancellationToken cancellationToken = default)
    // {
    //     throw new NotImplementedException();
    // }

    // public async IAsyncEnumerable<DawaPostCode> GetAllPostCodesAsync(
    //     ulong tId,
    //     [EnumeratorCancellation] CancellationToken cancellationToken = default)
    // {
    //     throw new NotImplementedException();
    // }

    // public async IAsyncEnumerable<NamedRoadMunicipalDistrict> GetAllNamedRoadMunicipalDistrictsAsync(
    //     ulong tId,
    //     [EnumeratorCancellation] CancellationToken cancellationToken = default)
    // {
    //     throw new NotImplementedException();
    // }

    // public async IAsyncEnumerable<DawaEntityChange<DawaAccessAddress>> GetChangesAccessAddressAsync(
    //     ulong fromTransactionId,
    //     ulong toTransactionId,
    //     [EnumeratorCancellation] CancellationToken cancellationToken = default)
    // {
    //     throw new NotImplementedException();
    // }

    // public async IAsyncEnumerable<DawaEntityChange<DawaUnitAddress>> GetChangesUnitAddressAsync(
    //     ulong fromTransactionId,
    //     ulong toTransactionId,
    //     [EnumeratorCancellation] CancellationToken cancellationToken = default)
    // {
    //     throw new NotImplementedException();
    // }

    // public async IAsyncEnumerable<DawaEntityChange<DawaRoad>> GetChangesRoadsAsync(
    //     ulong fromTransactionId,
    //     ulong toTransactionId,
    //     [EnumeratorCancellation] CancellationToken cancellationToken = default)
    // {
    //     throw new NotImplementedException();
    // }

    // public async IAsyncEnumerable<DawaEntityChange<DawaPostCode>> GetChangesPostCodesAsync(
    //     ulong fromTransactionId,
    //     ulong toTransactionId,
    //     [EnumeratorCancellation] CancellationToken cancellationToken = default)
    // {
    //     throw new NotImplementedException();
    // }

    // public async IAsyncEnumerable<DawaEntityChange<NamedRoadMunicipalDistrict>> GetChangesNamedRoadMunicipalDistrictAsync(
    //     ulong fromTransactionId,
    //     ulong toTransactionId,
    //     [EnumeratorCancellation] CancellationToken cancellationToken = default)
    // {
    //     throw new NotImplementedException();
    // }

    private static Uri BuildResourcePath(string baseUrl, string entityType, DateTime daftTimestampFrom, DateTime daftTimestampTo, int pageSize, int page, int status)
    {
        return new Uri($"{baseUrl}/{entityType}?DAFTimestampFra={daftTimestampFrom.ToUniversalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}&DAFTimestampTil={daftTimestampTo.ToUniversalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}&pagesize={pageSize}&page={page}&status={status}&MedDybde=FALSE&Format=JSON");
    }
}
