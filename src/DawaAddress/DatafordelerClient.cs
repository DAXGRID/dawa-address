using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.ComponentModel;
using System.Globalization;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DawaAddress;

// We disable CA1008 because it can be confusing to consumers to
// have to check for None, since that is invalid.
#pragma warning disable CA1008
public enum DatafordelerAccessAddressStatus
{
    [Description("Foreløbige husnumre")]
    Pending = 2,
    [Description("Gældende husnumre")]
    Active = 3,
    [Description("Nedlagte husnumre")]
    Discontinued = 4,
    [Description("Henlagte husnumre")]
    Canceled = 5
}

public enum DatafordelerUnitAddressStatus
{
    [Description("Foreløbige adresser")]
    Pending = 2,
    [Description("Gældende adresser")]
    Active = 3,
    [Description("Nedlagte adresser")]
    Discontinued = 4,
    [Description("Henlagte adresser")]
    Canceled = 5
}

public enum DatafordelerPostCodeStatus
{
    [Description("Gældende postnummer")]
    Active = 3,
    [Description("Nedlagte nedlagt postnummer")]
    Discontinued = 4,
}

public enum DatafordelerRoadStatus
{
    [Description("Foreløbige navngivne veje")]
    Temporary = 2,
    [Description("Gældende navngivne veje")]
    Active = 3,
    [Description("Nedlagte navngivne veje")]
    Discontinued = 4,
    [Description("Henlagte navngivne veje")]
    Canceled = 5
}

public enum DatafordelerNamedRoadMunicipalDistrictStatus
{
    [Description("Gældende relation")]
    Active = 3,
    [Description("Nedlagt relation")]
    Discontinued = 4,
}
#pragma warning restore CA1008

public class DatafordelerClient
{
    private const string _baseAddress = "https://services.datafordeler.dk/DAR/DAR/3.0.0/rest";
    private readonly HttpClient _httpClient;

    public DatafordelerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async IAsyncEnumerable<DawaAccessAddress> GetAllAccessAddresses(
        DateTime fromDate,
        DateTime toDate,
        DatafordelerAccessAddressStatus? status = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var wktReader = new WKTReader();
        await foreach (var x in GetAllAsync<DatafordelerAccessAddress, DawaAccessAddress>(
                           "Husnummer",
                           fromDate,
                           toDate,
                           true,
                           (DatafordelerAccessAddress x) => { return MapAccessAddress(x, wktReader); }, (int?)status, cancellationToken)
                       .ConfigureAwait(false))
        {
            yield return x;
        }
    }

    public async IAsyncEnumerable<DawaUnitAddress> GetAllUnitAddresses(
        DateTime fromDate,
        DateTime toDate,
        DatafordelerUnitAddressStatus? status = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var x in GetAllAsync<DatafordelerUnitAddress, DawaUnitAddress>(
                           "Adresse",
                           fromDate,
                           toDate,
                           false,
                           MapUnitAddress,
                           (int?)status, cancellationToken)
                       .ConfigureAwait(false))
        {
            yield return x;
        }
    }

    public async IAsyncEnumerable<DawaRoad> GetAllRoadsAsync(
        DateTime fromDate,
        DateTime toDate,
        DatafordelerRoadStatus? status = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var x in GetAllAsync<DatafordelerRoad, DawaRoad>(
                           "Navngivenvej",
                           fromDate,
                           toDate,
                           false,
                           MapRoad,
                           (int?)status,
                           cancellationToken)
                       .ConfigureAwait(false))
        {
            yield return x;
        }
    }

    public async IAsyncEnumerable<DawaPostCode> GetAllPostCodesAsync(
        DateTime fromDate,
        DateTime toDate,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var x in GetAllAsync<DatafordelerPostCode, DawaPostCode>(
                           "postnummer",
                           fromDate,
                           toDate,
                           true,
                           MapPostCode,
                           null,
                           cancellationToken)
                       .ConfigureAwait(false))
        {
            yield return x;
        }
    }

    public async IAsyncEnumerable<NamedRoadMunicipalDistrict> GetAllNamedRoadMunicipalDistrictsAsync(
        DateTime fromDate,
        DateTime toDate,
        DatafordelerNamedRoadMunicipalDistrictStatus? status = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var x in GetAllAsync<DatafordelerNamedRoadMunicipalDistrict, NamedRoadMunicipalDistrict>(
                           "NavngivenvejKommunedel",
                           fromDate,
                           toDate,
                           false,
                           MapNamedRoadMunicipalDistrict,
                           (int?)status,
                           cancellationToken)
                       .ConfigureAwait(false))
        {
            yield return x;
        }
    }

    private static Uri BuildResourcePath(
        string baseUrl,
        string entityType,
        DateTime daftTimestampFrom,
        DateTime daftTimestampTo,
        int pageSize,
        int page,
        int? status,
        bool includeNestedData = true)
    {
        var uri = $"{baseUrl}/{entityType}?DAFTimestampFra={daftTimestampFrom.ToUniversalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}&DAFTimestampTil={daftTimestampTo.ToUniversalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}&pagesize={pageSize}&page={page}&Format=JSON";

        if (!includeNestedData)
        {
            uri += "&meddybde=false";
        }

        if (status is not null)
        {
            uri += $"&status={status}";
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
            Status = MapUnitAddressStatus(datafordelerUnitAddress.Status),
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
            HouseNumber = string.IsNullOrWhiteSpace(datafordelerAccessAddress.Husnummertekst) ? "?" : datafordelerAccessAddress.Husnummertekst,
            LocationUpdated = datafordelerAccessAddress.Adgangspunkt.DatafordelerOpdateringstid,
            MunicipalCode = datafordelerAccessAddress.Kommuneinddeling.Id,
            Updated = datafordelerAccessAddress.DatafordelerOpdateringstid,
            RoadCode = datafordelerAccessAddress.Vejmidte.Split("-").Last(),
            Status = MapAccessAddressStatus(datafordelerAccessAddress.Status),
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
            Status = MapRoadStatus(datafordelerRoad.Status)
        };
    }

    private static NamedRoadMunicipalDistrict MapNamedRoadMunicipalDistrict(DatafordelerNamedRoadMunicipalDistrict datafordelerNamedRoadMunicipalDistrict)
    {
        return new NamedRoadMunicipalDistrict
        {
            Id = Guid.Parse(datafordelerNamedRoadMunicipalDistrict.IdLokalId),
            Status = MapNamedRoadMunicipalDistrictStatus(datafordelerNamedRoadMunicipalDistrict.Status),
            MunicipalityCode = datafordelerNamedRoadMunicipalDistrict.Kommune,
            NamedRoadId = Guid.Parse(datafordelerNamedRoadMunicipalDistrict.NavngivenVej.IdLokalId),
            RoadCode = datafordelerNamedRoadMunicipalDistrict.Vejkode
        };
    }

    private static DawaStatus MapAccessAddressStatus(string status)
    {
        return status switch
        {
            "2" => DawaStatus.Pending,
            "3" => DawaStatus.Active,
            "4" => DawaStatus.Discontinued,
            "5" => DawaStatus.Canceled,
            _ => throw new ArgumentException($"Could not convert {status}")
        };
    }

    private static DawaStatus MapUnitAddressStatus(string status)
    {
        return status switch
        {
            "2" => DawaStatus.Pending,
            "3" => DawaStatus.Active,
            "4" => DawaStatus.Discontinued,
            "5" => DawaStatus.Canceled,
            _ => throw new ArgumentException($"Could not convert {status}")
        };
    }

    private static DawaRoadStatus MapRoadStatus(string status)
    {
        return status switch
        {
            "2" => DawaRoadStatus.Temporary,
            "3" => DawaRoadStatus.Effective,
            _ => throw new ArgumentException($"Could not convert: '{status}'.")
        };
    }

    private static NamedRoadMunicipalDistrictStatus MapNamedRoadMunicipalDistrictStatus(string status)
    {
        return status switch
        {
            "2" => NamedRoadMunicipalDistrictStatus.Temporary,
            "3" => NamedRoadMunicipalDistrictStatus.Active,
            "4" => NamedRoadMunicipalDistrictStatus.Discontinued,
            "5" => NamedRoadMunicipalDistrictStatus.Canceled,
            _ => throw new ArgumentException($"Could not convert: '{status}'.")
        };
    }

    private async IAsyncEnumerable<T2> GetAllAsync<T1, T2>(
        string resourceName,
        DateTime fromDate,
        DateTime toDate,
        bool includeNestedData,
        Func<T1, T2> fMap,
        int? status = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        const int pageSize = 200;
        var page = 1;

        while (true)
        {
            var resourcePath = BuildResourcePath(_baseAddress, resourceName, fromDate, toDate, pageSize, page, status, includeNestedData);
            Console.WriteLine(resourcePath);

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
