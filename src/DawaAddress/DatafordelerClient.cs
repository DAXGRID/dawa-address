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
        var wktReader = new WKTReader();
        await foreach (var x in GetAllAsync<DatafordelerAccessAddress, DawaAccessAddress>("Husnummer", DateTime.MinValue, toDate, true, (DatafordelerAccessAddress x) => { return MapAccessAddress(x, wktReader); }, cancellationToken).ConfigureAwait(false))
        {
            yield return x;
        }
    }

    public async IAsyncEnumerable<DawaUnitAddress> GetAllUnitAddresses(
        DateTime toDate,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var x in GetAllAsync<DatafordelerUnitAddress, DawaUnitAddress>("Adresse", DateTime.MinValue, toDate, false, MapUnitAddress, cancellationToken).ConfigureAwait(false))
        {
            yield return x;
        }
    }

    public async IAsyncEnumerable<DawaRoad> GetAllRoadsAsync(
        DateTime toDate,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var x in GetAllAsync<DatafordelerRoad, DawaRoad>("Navngivenvej", DateTime.MinValue, toDate, false, MapRoad, cancellationToken).ConfigureAwait(false))
        {
            yield return x;
        }
    }

    public async IAsyncEnumerable<DawaPostCode> GetAllPostCodesAsync(
        DateTime toDate,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var x in GetAllAsync<DatafordelerPostCode, DawaPostCode>("postnummer", DateTime.MinValue, toDate, true, MapPostCode, cancellationToken).ConfigureAwait(false))
        {
            yield return x;
        }
    }

    public async IAsyncEnumerable<NamedRoadMunicipalDistrict> GetAllNamedRoadMunicipalDistrictsAsync(
        DateTime toDate,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var x in GetAllAsync<DatafordelerNamedRoadMunicipalDistrict, NamedRoadMunicipalDistrict>("NavngivenvejKommunedel", DateTime.MinValue, toDate, false, MapNamedRoadMunicipalDistrict, cancellationToken).ConfigureAwait(false))
        {
            yield return x;
        }
    }

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

    private static DawaUnitAddress MapUnitAddress(DatafordelerUnitAddress datafordelerUnitAddress)
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

    private static DawaAccessAddress MapAccessAddress(DatafordelerAccessAddress datafordelerAccessAddress, WKTReader wktReader)
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

    private static DawaPostCode MapPostCode(DatafordelerPostCode datafordelerPostCode)
    {
        return new DawaPostCode(datafordelerPostCode.Navn, datafordelerPostCode.Postnr);
    }

    private static DawaRoad MapRoad(DatafordelerRoad datafordelerRoad)
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

    private static NamedRoadMunicipalDistrict MapNamedRoadMunicipalDistrict(DatafordelerNamedRoadMunicipalDistrict datafordelerNamedRoadMunicipalDistrict)
    {
        return new NamedRoadMunicipalDistrict
        {
            Id = Guid.Parse(datafordelerNamedRoadMunicipalDistrict.IdLokalId),
            Status = NamedRoadMunicipalDistrictStatus.Active,
            MunicipalityCode = datafordelerNamedRoadMunicipalDistrict.Kommune,
            NamedRoadId = Guid.Parse(datafordelerNamedRoadMunicipalDistrict.NavngivenVej.IdLokalId),
            RoadCode = datafordelerNamedRoadMunicipalDistrict.Vejkode
        };
    }

    private async IAsyncEnumerable<T2> GetAllAsync<T1, T2>(
        string resourceName,
        DateTime fromDate, 
        DateTime toDate,
        bool includeNestedData,
        Func<T1, T2> fMap,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        const int pageSize = 200;
        var page = 1;
        const int status = 3;

        while (true)
        {
            var resourcePath = BuildResourcePath(_baseAddress, resourceName, DateTime.MinValue, DateTime.UtcNow, pageSize, page, status, includeNestedData);
            var response = await _httpClient.GetAsync(resourcePath, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var resources = await response.Content.ReadFromJsonAsync<T1[]>(cancellationToken).ConfigureAwait(false);

            if (resources is null)
            {
                throw new InvalidOperationException($"Received NULL when trying to get {resourceName} codes from path: '{resourcePath}'.");
            }

            foreach (var resource in resources)
            {
                yield return fMap(resource);
            }

            if (resources.Length < pageSize)
            {
                break;
            }

            page++;
        }
    }
}
