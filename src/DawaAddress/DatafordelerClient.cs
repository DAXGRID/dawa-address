using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
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

    public async IAsyncEnumerable<DawaAccessAddress> GetAllAccessAddresses(
        DateTime toDate,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var fromDate = DateTime.MinValue;
        const int pageSize = 5000;
        var page = 1;
        const int status = 3;

        var wktReader = new WKTReader();

        while (true)
        {
            var accessAddressResourcePath = BuildResourcePath(_baseAddress, "Husnummer", DateTime.MinValue, DateTime.UtcNow, pageSize, page, status);
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
                yield return Map(dawaAddress, wktReader);
            }

            if (dawaAccessAddresses.Length < pageSize)
            {
                break;
            }

            page++;
        }
    }

    public async IAsyncEnumerable<DawaUnitAddress> GetAllUnitAddresses(
        DateTime toDate,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var fromDate = DateTime.MinValue;
        const int pageSize = 200;
        var page = 1;
        const int status = 3;

        while (true)
        {
            var resourcePath = BuildResourcePath(_baseAddress, "Adresse", DateTime.MinValue, DateTime.UtcNow, pageSize, page, status);
            var response = await _httpClient
                .GetAsync(
                    resourcePath,
                    cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var resources = await response.Content
                .ReadFromJsonAsync<DatafordelerUnitAddress[]>(cancellationToken).ConfigureAwait(false);

            if (resources is null)
            {
                throw new InvalidOperationException(
                    $"Received NULL when trying to get DAWA unit address from path: '{resourcePath}'.");
            }

            foreach (var resource in resources)
            {
                yield return Map(resource);
            }

            if (resources.Length < pageSize)
            {
                break;
            }

            page++;
        }
    }

    public async IAsyncEnumerable<DawaRoad> GetAllRoadsAsync(
        DateTime toDate,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var fromDate = DateTime.MinValue;
        const int pageSize = 200;
        var page = 1;
        const int status = 3;

        while (true)
        {
            var resourcePath = BuildResourcePath(_baseAddress, "Navngivenvej", DateTime.MinValue, DateTime.UtcNow, pageSize, page, status, false);

            var response = await _httpClient
                .GetAsync(
                    resourcePath,
                    cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var resources = await response.Content
                .ReadFromJsonAsync<DatafordelerRoad[]>(cancellationToken).ConfigureAwait(false);

            if (resources is null)
            {
                throw new InvalidOperationException(
                    $"Received NULL when trying to get DAWA roads from path: '{resourcePath}'.");
            }

            foreach (var resource in resources)
            {
                yield return Map(resource);
            }

            if (resources.Length < pageSize)
            {
                break;
            }

            page++;
        }
    }

    public async IAsyncEnumerable<DawaPostCode> GetAllPostCodesAsync(
        DateTime toDate,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var fromDate = DateTime.MinValue;
        const int pageSize = 200;
        var page = 1;
        const int status = 3;

        while (true)
        {
            var resourcePath = BuildResourcePath(_baseAddress, "postnummer", DateTime.MinValue, DateTime.UtcNow, pageSize, page, status);
            var response = await _httpClient.GetAsync(resourcePath, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var datafordelerPostCodes = await response.Content.ReadFromJsonAsync<DatafordelerPostCode[]>(cancellationToken).ConfigureAwait(false);

            if (datafordelerPostCodes is null)
            {
                throw new InvalidOperationException(
                    $"Received NULL when trying to get DAWA post codes from path: '{resourcePath}'.");
            }

            foreach (var datafordelerPostCode in datafordelerPostCodes)
            {
                yield return Map(datafordelerPostCode);
            }

            if (datafordelerPostCodes.Length < pageSize)
            {
                break;
            }

            page++;
        }
    }

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

    private static Uri BuildResourcePath(
        string baseUrl,
        string entityType,
        DateTime daftTimestampFrom,
        DateTime daftTimestampTo,
        int pageSize,
        int page,
        int status,
        bool includeNestedData = true)
    {
        var uri = $"{baseUrl}/{entityType}?DAFTimestampFra={daftTimestampFrom.ToUniversalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}&DAFTimestampTil={daftTimestampTo.ToUniversalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}&pagesize={pageSize}&page={page}&status={status}&Format=JSON";

        if (!includeNestedData)
        {
            uri += "&meddybde=false";
        }

        return new Uri(uri);
    }

    private static DawaUnitAddress Map(DatafordelerUnitAddress datafordelerUnitAddress)
    {
        return new DawaUnitAddress
        {
            Id = Guid.Parse(datafordelerUnitAddress.IdLokalId),
            AccessAddressId = Guid.Parse(datafordelerUnitAddress.Husnummer.IdLokalId),
            Created = datafordelerUnitAddress.VirkningFra,
            Updated = datafordelerUnitAddress.DatafordelerOpdateringstid,
            FloorName = datafordelerUnitAddress.Etagebetegnelse,
            Status = DawaStatus.Active,
            SuitName = datafordelerUnitAddress.Drbetegnelse
        };
    }

    private static DawaAccessAddress Map(DatafordelerAccessAddress datafordelerAccessAddress, WKTReader wktReader)
    {
        var point = (Point)wktReader.Read(datafordelerAccessAddress.Adgangspunkt.Position);

        return new DawaAccessAddress
        {
            Created = datafordelerAccessAddress.VirkningFra,
            Id = Guid.Parse(datafordelerAccessAddress.IdLokalId),
            EastCoordinate = point.X,
            NorthCoordinate = point.Y,
            HouseNumber = datafordelerAccessAddress.Husnummertekst,
            LocationUpdated = datafordelerAccessAddress.Adgangspunkt.DatafordelerOpdateringstid,
            MunicipalCode = datafordelerAccessAddress.Kommuneinddeling.Id,
            Updated = datafordelerAccessAddress.DatafordelerOpdateringstid,
            RoadCode = datafordelerAccessAddress.Vejmidte.Split("-").Last(),
            Status = DawaStatus.Active,
            PlotId = datafordelerAccessAddress.Jordstykke,
            PostDistrictCode = datafordelerAccessAddress.Postnummer.Postnr,
            RoadId = Guid.Parse(datafordelerAccessAddress.NavngivenVej.IdLokalId),
            SupplementaryTownName = datafordelerAccessAddress.Sogneinddeling.Navn
        };
    }

    private static DawaPostCode Map(DatafordelerPostCode datafordelerPostCode)
    {
        return new DawaPostCode(datafordelerPostCode.Navn, datafordelerPostCode.Postnr);
    }

    private static DawaRoad Map(DatafordelerRoad datafordelerRoad)
    {
        return new DawaRoad
        {
            Id = Guid.Parse(datafordelerRoad.IdLokalId),
            Created = datafordelerRoad.VirkningFra,
            Updated = datafordelerRoad.DatafordelerOpdateringstid,
            Name = datafordelerRoad.Vejnavn,
            Status = DawaRoadStatus.Effective
        };
    }
}
