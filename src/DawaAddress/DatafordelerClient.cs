using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.ComponentModel;
using System.Globalization;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.IO.Compression;

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
    [Description("Nedlagt postnummer")]
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
    private const string _baseAddressApi = "https://api.datafordeler.dk";
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public DatafordelerClient(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
    }

    public async Task<IEnumerable<DatafordelerFile>> LatestGenerationFileResourcesAsync(
        string resourceName,
        CancellationToken cancellationToken = default)
    {
        var resourcePath = new Uri($"{_baseAddressApi}/FileDownloads/GetAvailableFileDownloads?Register=DAR&format=JSON&apikey={_apiKey}");

        var response = await _httpClient.GetAsync(resourcePath, cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var resources = await response.Content.ReadFromJsonAsync<DatafordelerFileResponse>(cancellationToken).ConfigureAwait(false);

        if (resources is null)
        {
            throw new InvalidOperationException($"Received NULL when trying to get {resourceName} codes from path: '{resourcePath}'.");
        }

        return resources
            .AvailableFileDownloads
            .Where(x => x.EntityName == resourceName)
            .Where(x => x.ContainedFileFormat == "json")
            .Where(x => x.Version == "3");
    }

    public async Task<DatafordelerFile> LatestGenerationTotalDownloadFileResourceAsync(
        string resourceName,
        CancellationToken cancellationToken = default)
    {
        var resources = await LatestGenerationFileResourcesAsync(resourceName, cancellationToken).ConfigureAwait(false);

        return resources
            .Where(x => x.EntityName == resourceName)
            .Where(x => x.TypeOfDownload == "TotalDownload")
            .Where(x => x.TypeOfData == "Current")
            .OrderByDescending(x => x.GenerationNumber)
            .First();
    }

    public async IAsyncEnumerable<DawaAccessAddress> GetAllAccessAddressesAsync(HashSet<DawaStatus> includeStatuses, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(includeStatuses);
        var wktReader = new WKTReader();

        var adgangsPunktLookUp = new Dictionary<Guid, AdgangspunktFileServer>();
        await foreach (var x in GetAllFromFileAsync<AdgangspunktFileServer, AdgangspunktFileServer>(
                           "Adressepunkt",
                           _apiKey,
                           (AdgangspunktFileServer x) => { return x; },
                           cancellationToken)
                       .ConfigureAwait(false))
        {
            if (x is null)
            {
                continue;
            }

            adgangsPunktLookUp.Add(Guid.Parse(x.IdLokalId), x);
        }

        var sogneIndelingLookup = new Dictionary<Guid, SupplerendeByNavnFileServer>();
        await foreach (var x in GetAllFromFileAsync<SupplerendeByNavnFileServer, SupplerendeByNavnFileServer>(
                           "SupplerendeBynavn",
                           _apiKey,
                           (SupplerendeByNavnFileServer x) => { return x; },
                           cancellationToken)
                       .ConfigureAwait(false))
        {
            sogneIndelingLookup.Add(Guid.Parse(x.IdLokalId), x);
        }

        var postalCodeLookup  = new Dictionary<Guid, DawaPostCode>();
        await foreach (var postalCode in GetAllPostCodesAsync(cancellationToken).ConfigureAwait(false))
        {
            postalCodeLookup.Add(postalCode.Id, postalCode);
        }

        await foreach (var x in GetAllFromFileAsync<DatafordelerAccessAddressFileServer, DawaAccessAddress?>(
                           "Husnummer",
                           _apiKey,
                           (DatafordelerAccessAddressFileServer x) => { return MapAccessAddress(x, wktReader, adgangsPunktLookUp, postalCodeLookup, sogneIndelingLookup); },
                           cancellationToken)
                       .ConfigureAwait(false))
        {
            // It might be NULL if the address is invalid.
            if (x is null)
            {
                continue;
            }

            if (includeStatuses.Contains(x.Status))
            {
                yield return x;
            }
        }
    }

    public async IAsyncEnumerable<DawaAccessAddress> GetAllAccessAddressesAsync(
        DateTime fromDate,
        DateTime toDate,
        DatafordelerAccessAddressStatus? status = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var wktReader = new WKTReader();
        await foreach (var x in GetAllAsync<DatafordelerAccessAddressApi, DawaAccessAddress?>(
                           "Husnummer",
                           fromDate,
                           toDate,
                           true,
                           (DatafordelerAccessAddressApi x) => { return MapAccessAddress(x, wktReader); },
                           (int?)status, cancellationToken)
                       .ConfigureAwait(false))
        {
            // It might be NULL if the address is invalid.
            if (x is null)
            {
                continue;
            }

            yield return x;
        }
    }

    public async IAsyncEnumerable<DawaUnitAddress> GetAllUnitAddressesAsync(HashSet<DawaStatus> includeStatuses, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(includeStatuses);

        await foreach (var x in GetAllFromFileAsync<DatafordelerUnitAddressFileServer, DawaUnitAddress>(
                           "Adresse",
                           _apiKey,
                           MapUnitAddress,
                           cancellationToken)
                       .ConfigureAwait(false))
        {
            if (includeStatuses.Contains(x.Status))
            {
                yield return x;
            }
        }
    }

    public async IAsyncEnumerable<DawaUnitAddress> GetAllUnitAddressesAsync(
        DateTime fromDate,
        DateTime toDate,
        DatafordelerUnitAddressStatus? status = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var x in GetAllAsync<DatafordelerUnitAddressApi, DawaUnitAddress>(
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

    public async IAsyncEnumerable<DawaRoad> GetAllRoadsAsync(HashSet<DawaRoadStatus> includeStatuses, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(includeStatuses);

        await foreach (var x in GetAllFromFileAsync<DatafordelerRoad, DawaRoad>(
                           "Navngivenvej",
                           _apiKey,
                           MapRoad,
                           cancellationToken)
                       .ConfigureAwait(false))
        {
            if (includeStatuses.Contains(x.Status))
            {
                yield return x;
            }
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

    public async IAsyncEnumerable<DawaPostCode> GetAllPostCodesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var x in GetAllFromFileAsync<DatafordelerPostCode, DawaPostCode>(
                           "Postnummer",
                           _apiKey,
                           MapPostCode,
                           cancellationToken)
                       .ConfigureAwait(false))
        {
            yield return x;
        }
    }

    public async IAsyncEnumerable<DawaPostCode> GetAllPostCodesAsync(
        DateTime fromDate,
        DateTime toDate,
        DatafordelerPostCodeStatus? status = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var x in GetAllAsync<DatafordelerPostCode, DawaPostCode>(
                           "postnummer",
                           fromDate,
                           toDate,
                           true,
                           MapPostCode,
                           (int?)status,
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

    private static Uri BuildResourcePathFileDownload(
        string baseUri,
        string entityType,
        string apiKey)
    {
        return new Uri($"{baseUri}/FileDownloads/GetFile?Register=DAR&LatestTotalForEntity={entityType}&type=current&format=JSON&apikey={apiKey}");
    }

    private static Uri BuildResourcePath(
        string baseUrl,
        string entityType,
        DateTime daftTimestampFrom,
        DateTime? daftTimestampTo,
        int pageSize,
        int page,
        int? status,
        bool includeNestedData = true)
    {
        var uri = $"{baseUrl}/{entityType}?DAFTimestampFra={daftTimestampFrom.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)}&pagesize={pageSize}&page={page}&Format=JSON";

        if (!includeNestedData)
        {
            uri += "&meddybde=false";
        }

        if (status is not null)
        {
            uri += $"&status={status}";
        }

        if (daftTimestampTo is not null)
        {
            uri += $"&DAFTimestampTil={daftTimestampTo.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)}";
        }

        return new Uri(uri);
    }

    private static DawaUnitAddress MapUnitAddress(DatafordelerUnitAddressFileServer datafordelerUnitAddress)
    {
        return new DawaUnitAddress
        {
            Id = Guid.Parse(datafordelerUnitAddress.IdLokalId),
            AccessAddressId = Guid.Parse(datafordelerUnitAddress.Husnummer),
            Created = datafordelerUnitAddress.VirkningFra,
            Updated = datafordelerUnitAddress.DatafordelerOpdateringstid,
            FloorName = datafordelerUnitAddress.Etagebetegnelse,
            Status = MapUnitAddressStatus(datafordelerUnitAddress.Status),
            SuitName = datafordelerUnitAddress.Drbetegnelse
        };
    }

    private static DawaUnitAddress MapUnitAddress(DatafordelerUnitAddressApi datafordelerUnitAddress)
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

    private static DawaAccessAddress? MapAccessAddress(DatafordelerAccessAddressApi datafordelerAccessAddress, WKTReader wktReader)
    {
        // In some weird cases they have no reference and that is an invalid address, so we cannot map it.
        if (datafordelerAccessAddress.NavngivenVej is null)
        {
            return null;
        }

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
            SupplementaryTownName = datafordelerAccessAddress.SupplerendeBynavn?.Navn
        };
    }

    private static DawaAccessAddress? MapAccessAddress(
        DatafordelerAccessAddressFileServer datafordelerAccessAddress,
        WKTReader wktReader,
        Dictionary<Guid, AdgangspunktFileServer> adgangsPunktLookup,
        Dictionary<Guid, DawaPostCode> postalCodeLookup,
        Dictionary<Guid, SupplerendeByNavnFileServer> supplementaryTownNameLookUp)
    {
        // In some weird cases they have no reference and that is an invalid address, so we cannot map it.
        if (datafordelerAccessAddress.NavngivenVej is null)
        {
            Console.WriteLine($"Could not find adgangspunkt with id: '{datafordelerAccessAddress.Adgangspunkt}' on access address with id: '{datafordelerAccessAddress.IdLokalId}'.")
            return null;
        }

        // This is done because their data is invalid and can reference things that do not exist.
        if (!adgangsPunktLookup.TryGetValue(Guid.Parse(datafordelerAccessAddress.Adgangspunkt), out var adgangsPunkt))
        {
            Console.WriteLine($"Could not find adgangspunkt with id: '{datafordelerAccessAddress.Adgangspunkt}' on access address with id: '{datafordelerAccessAddress.IdLokalId}'.")
            return null;
        }

        var postCode = postalCodeLookup[Guid.Parse(datafordelerAccessAddress.Postnummer)];

        string? supplementaryTownName = null;
        if (datafordelerAccessAddress.SupplerendeBynavn is not null)
        {
            supplementaryTownName = supplementaryTownNameLookUp[Guid.Parse(datafordelerAccessAddress.SupplerendeBynavn)].Navn;
        }

        var point = (Point)wktReader.Read(adgangsPunkt.Position);

        return new DawaAccessAddress
        {
            Created = datafordelerAccessAddress.VirkningFra,
            Id = Guid.Parse(datafordelerAccessAddress.IdLokalId),
            EastCoordinate = point.X,
            NorthCoordinate = point.Y,
            HouseNumber = string.IsNullOrWhiteSpace(datafordelerAccessAddress.Husnummertekst) ? "?" : datafordelerAccessAddress.Husnummertekst,
            LocationUpdated = adgangsPunkt.DatafordelerOpdateringstid,
            MunicipalCode = datafordelerAccessAddress.Kommuneinddeling,
            Updated = datafordelerAccessAddress.DatafordelerOpdateringstid,
            RoadCode = datafordelerAccessAddress.Vejmidte.Split("-").Last(),
            Status = MapAccessAddressStatus(datafordelerAccessAddress.Status),
            PlotId = datafordelerAccessAddress.Jordstykke,
            PostDistrictCode = postCode?.Number ?? "",
            RoadId = Guid.Parse(datafordelerAccessAddress.NavngivenVej),
            SupplementaryTownName = supplementaryTownName
        };
    }

    private static DawaPostCode MapPostCode(DatafordelerPostCode datafordelerPostCode)
    {
        return new DawaPostCode(
            Guid.Parse(datafordelerPostCode.IdLokalId),
            datafordelerPostCode.Navn,
            datafordelerPostCode.Postnr,
            MapPostCodeStatus(datafordelerPostCode.Status),
            datafordelerPostCode.VirkningFra,
            datafordelerPostCode.DatafordelerOpdateringstid
        );
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
            "4" => DawaRoadStatus.Discontinued,
            "5" => DawaRoadStatus.Canceled,
            _ => throw new ArgumentException($"Could not convert: '{status}'.")
        };
    }

    private static DawaPostCodeStatus MapPostCodeStatus(string status)
    {
        return status switch
        {
            "3" => DawaPostCodeStatus.Active,
            "4" => DawaPostCodeStatus.Discontinued,
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

    private async IAsyncEnumerable<T2> GetAllFromFileAsync<T1, T2>(
        string resourceName,
        string apiKey,
        Func<T1, T2> fMap,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var tempFileName = $"{Path.GetTempPath()}/{Guid.NewGuid()}";
            var tempFileNameZip = $"{tempFileName}.zip";

            var uri = BuildResourcePathFileDownload(_baseAddressApi, resourceName, apiKey);
            var response = await _httpClient.GetStreamAsync(uri, cancellationToken).ConfigureAwait(false);

            using (var fs = new FileStream(tempFileNameZip, FileMode.Create))
            {
                await response.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
            }

            ZipFile.ExtractToDirectory(tempFileNameZip, tempFileName);

            var jsonfileName = Directory.EnumerateFiles(tempFileName, "*.json*", SearchOption.AllDirectories).First();

            using (var fs = new FileStream(jsonfileName, FileMode.Open))
            {
                var resources = JsonSerializer.DeserializeAsyncEnumerable<T1?>(fs, cancellationToken: cancellationToken);
                await foreach (var resource in resources.ConfigureAwait(false))
                {
                    if (resource is null)
                    {
                        throw new ArgumentException($"Could not deserialize JSON output from DAWA for {resourceName}.");
                    }

                    yield return fMap(resource);
                }
            }

            File.Delete(tempFileNameZip);
            Directory.Delete(tempFileName, true);
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
