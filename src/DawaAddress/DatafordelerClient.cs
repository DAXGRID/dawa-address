using GraphQL;
using GraphQL.Client.Http;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.ComponentModel;
using System.Globalization;
using System.IO.Compression;
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

public sealed class DatafordelerClient : IDisposable
{
    private const string _baseAddress = "https://services.datafordeler.dk/DAR/DAR/3.0.0/rest";
    private const string _baseAddressApi = "https://api.datafordeler.dk";
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private readonly GraphQLHttpClient _graphqlClient;

    public DatafordelerClient(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromMinutes(30);
        _apiKey = apiKey;
        _graphqlClient = new GraphQLHttpClient(
            $"https://graphql.datafordeler.dk/flexibleCurrent/v1?apikey={_apiKey}",
            new GraphQL.Client.Serializer.SystemTextJson.SystemTextJsonSerializer());
    }

    public async Task<(int generationNumber, DateTime dateTime)?> LatestGenerationNumberCurrentTotalDownloadAsync(
        CancellationToken cancellationToken = default)
    {
        var resources = await LatestGenerationFileResourcesCurrentTotalDownloadAsync(cancellationToken).ConfigureAwait(false);
        var resourcesGroupedByEntityName = resources
            .GroupBy(x => x.EntityName);

        var generationNumbers = new List<(int generationNumber, DateTime? timeStamp)>();
        foreach (var resourceByEntityName in resourcesGroupedByEntityName)
        {
            var resource = resourceByEntityName.OrderByDescending(x => x.GenerationNumber).First();

            generationNumbers.Add(new(resource.GenerationNumber, resource.PointInTime));
        }

        if (generationNumbers.Select(x => x.generationNumber).Distinct().Count() == 1)
        {
            var result = generationNumbers.First();

            // The dataset from datafordeleren is really bad so some of the generation timestamps are null values.
            if (result.timeStamp is null)
            {
                return null;
            }
            else
            {
                return (result.generationNumber, result.timeStamp.Value);
            }
        }
        else
        {
            return null;
        }
    }

    public async Task<IEnumerable<DatafordelerFile>> LatestGenerationFileResourcesAsync(
        CancellationToken cancellationToken = default)
    {
        var resourcePath = new Uri($"{_baseAddressApi}/FileDownloads/GetAvailableFileDownloads?Register=DAR&format=JSON&apikey={_apiKey}");

        var response = await _httpClient.GetAsync(resourcePath, cancellationToken).ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var resources = await response.Content.ReadFromJsonAsync<DatafordelerFileResponse>(cancellationToken).ConfigureAwait(false);

        if (resources is null)
        {
            throw new InvalidOperationException($"Received NULL when trying to get resouces from path: '{resourcePath}'.");
        }

        return resources
            .AvailableFileDownloads
            .Where(x => x.ContainedFileFormat == "json")
            .Where(x => x.Version == "3");
    }

    public async Task<IEnumerable<DatafordelerFile>> LatestGenerationFileResourcesCurrentTotalDownloadAsync(
        CancellationToken cancellationToken = default)
    {
        var resources = await LatestGenerationFileResourcesAsync(cancellationToken).ConfigureAwait(false);
        return resources
            .Where(x => x.TypeOfDownload == "TotalDownload")
            .Where(x => x.TypeOfData == "Current")
            .OrderByDescending(x => x.GenerationNumber);
    }

    public async Task<DatafordelerFile> LatestGenerationFileResourceCurrentTotalDownloadAsync(
        string resourceName,
        CancellationToken cancellationToken = default)
    {
        var resources = await LatestGenerationFileResourcesCurrentTotalDownloadAsync(cancellationToken).ConfigureAwait(false);
        return resources
            .Where(x => x.EntityName == resourceName)
            // This is done because sometimes there can be multiple total downloads with a subset.
            // Don't ask me why, an exaple is:
            // Full:
            // DAR_V3_NavngivenVej_TotalDownload_json_Current_636.zip
            // Subsets:
            // DAR_V3_Adressepunkt_0766_TotalDownload_json_Current_636.zip
            // DAR_V3_Adressepunkt_0787_TotalDownload_json_Current_636.zip
            .Where(x => x.FileName.StartsWith($"DAR_V3_{resourceName}_TotalDownload_json_Current_", StringComparison.CurrentCultureIgnoreCase))
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

        var postalCodeLookup = new Dictionary<Guid, DawaPostCode>();
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

        const int count = 200;
        string? after = null;

        dynamic whereCondition = status is not null
            ? new
            {
                virkningFra = new
                {
                    gte = fromDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                },
                status = new
                {
                    eq = ((int)status).ToString(CultureInfo.InvariantCulture)
                }
            }
            : new
            {
                virkningFra = new
                {
                    gte = fromDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                }
            };

        while (true)
        {
            var request = new GraphQLRequest
            {
                Query = @"
        query ($virkningstid: DafDateTime! $first: Int! $after: String $where: DAR_HusnummerFilterInput) {
          DAR_Husnummer(virkningstid: $virkningstid first: $first after: $after where: $where) {
            pageInfo {
              endCursor
              hasNextPage
            }
            nodes {
              adgangsadressebetegnelse
              virkningTil
              virkningsaktoer
              virkningFra
              vejpunkt
              vejmidte
              supplerendeBynavn
              status
              sogneinddeling
              registreringTil
              registreringsaktoer
              registreringFra
              placeretPaaForeloebigtJordstykke
              navngivenVej
              menighedsraadsafstemningsomraade
              kommuneinddeling
              jordstykke
              id_namespace
              id_lokalId
              husnummertekst
              geoDanmarkBygning
              forretningsproces
              forretningsomraade
              forretningshaendelse
              datafordelerRowId
              datafordelerRegisterImportSequenceNumber
              datafordelerRowVersion
              datafordelerOpdateringstid
              afstemningsomraade
              adgangTilTekniskAnlaeg
              adgangTilBygning
              adgangspunkt
              husnummerHoererTilIPostnummer {
                id_lokalId
                navn
                postnr
              }
              HusnummerHarAdgangspunkt {
                position {
                  wkt
                }
                virkningFra
              }
              husnummerLiggerISogneInddeling {
                id_lokalId
                navn
              }
            }
          }
        }",
                Variables = new
                {
                    virkningstid = toDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                    first = count,
                    after = after,
                    where = whereCondition
                }
            };

            var response = await _graphqlClient.SendQueryAsync<DatafordelerAccessAddressGraphqlResponse>(request, cancellationToken).ConfigureAwait(false);

            if (response.Errors?.Length > 0)
            {
                Console.WriteLine(JsonSerializer.Serialize(response.Errors));
            }

            foreach (var datafordelerAccessAddress in response.Data.DarHusnummer.Nodes)
            {
                var mapped = MapAccessAddress(datafordelerAccessAddress, wktReader);

                // It might be NULL if the address is invalid.
                if (mapped is null)
                {
                    continue;
                }

                yield return mapped;
            }

            if (!response.Data.DarHusnummer.PageInfo.HasNextPage)
            {
                break;
            }

            after = response.Data.DarHusnummer.PageInfo!.EndCursor;
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
        const int count = 200;
        string? after = null;

        dynamic whereCondition = status is not null
            ? new
            {
                virkningFra = new
                {
                    gte = fromDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                },
                status = new
                {
                    eq = ((int)status).ToString(CultureInfo.InvariantCulture)
                }
            }
            : new
            {
                virkningFra = new
                {
                    gte = fromDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                }
            };

        while (true)
        {
            var request = new GraphQLRequest
            {
                Query = @"
        query ($virkningstid: DafDateTime! $first: Int! $after: String $where: DAR_AdresseFilterInput) {
          DAR_Adresse(virkningstid: $virkningstid first: $first after: $after where: $where) {
            pageInfo {
              endCursor
              hasNextPage
            }
            nodes {
              adressebetegnelse
              forretningshaendelse
              etagebetegnelse
              doerbetegnelse
              doerpunkt
              datafordelerRowVersion
              datafordelerRowId
              datafordelerRegisterImportSequenceNumber
              virkningTil
              virkningsaktoer
              virkningFra
              status
              registreringTil
              registreringsaktoer
              registreringFra
              id_namespace
              id_lokalId
              husnummer
              forretningsproces
              forretningsomraade
              datafordelerOpdateringstid
              bygning
            }
          }
        }",
                Variables = new
                {
                    virkningstid = toDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                    first = count,
                    after = after,
                    where = whereCondition
                }
            };

            var response = await _graphqlClient.SendQueryAsync<DatafordelerUnitAddressGraphqlResponse>(request, cancellationToken).ConfigureAwait(false);

            if (response.Errors?.Length > 0)
            {
                Console.WriteLine(JsonSerializer.Serialize(response.Errors));
            }

            foreach (var datafordelerUnitAddress in response.Data.DarAdresse.Nodes)
            {
                yield return MapUnitAddress(datafordelerUnitAddress);
            }

            if (!response.Data.DarAdresse.PageInfo.HasNextPage)
            {
                break;
            }

            after = response.Data.DarAdresse.PageInfo!.EndCursor;
        }
    }

    public async IAsyncEnumerable<DawaRoad> GetAllRoadsAsync(HashSet<DawaRoadStatus> includeStatuses, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(includeStatuses);

        await foreach (var x in GetAllFromFileAsync<DatafordelerRoadFileServer, DawaRoad>(
                           "NavngivenVej",
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
        const int count = 200;
        string? after = null;

        dynamic whereCondition = status is not null
            ? new
            {
                virkningFra = new
                {
                    gte = fromDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                },
                status = new
                {
                    eq = ((int)status).ToString(CultureInfo.InvariantCulture)
                }
            }
            : new
            {
                virkningFra = new
                {
                    gte = fromDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                }
            };

        while (true)
        {
            var request = new GraphQLRequest
            {
                Query = @"
        query ($virkningstid: DafDateTime! $first: Int! $after: String $where: DAR_NavngivenVejFilterInput) {
          DAR_NavngivenVej(virkningstid: $virkningstid first: $first after: $after where: $where) {
            pageInfo {
              endCursor
              hasNextPage
            }
            nodes {
               virkningTil
              virkningFra
              virkningsaktoer
              vejnavn
              vejadresseringsnavn
              udtaltVejnavn
              status
              registreringTil
              registreringsaktoer
              registreringFra
              id_namespace
              id_lokalId
              forretningsproces
              forretningsomraade
              forretningshaendelse
              datafordelerRowVersion
              datafordelerRowId
              datafordelerRegisterImportSequenceNumber
              datafordelerOpdateringstid
              beskrivelse
              administreresAfKommune
            }
          }
        }",
                Variables = new
                {
                    virkningstid = toDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                    first = count,
                    after = after,
                    where = whereCondition
                }
            };

            var response = await _graphqlClient.SendQueryAsync<DatafordelerRoadGraphqlResponse>(request, cancellationToken).ConfigureAwait(false);

            if (response.Errors?.Length > 0)
            {
                Console.WriteLine(JsonSerializer.Serialize(response.Errors));
            }

            foreach (var datafordelerRoad in response.Data.DarNavngivenVej.Nodes)
            {
                yield return MapRoad(datafordelerRoad);
            }

            if (!response.Data.DarNavngivenVej.PageInfo.HasNextPage)
            {
                break;
            }

            after = response.Data.DarNavngivenVej.PageInfo!.EndCursor;
        }
    }

    public async IAsyncEnumerable<DawaPostCode> GetAllPostCodesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var x in GetAllFromFileAsync<DatafordelerPostCodeFileServer, DawaPostCode>(
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
        const int count = 200;
        string? after = null;

        dynamic whereCondition = status is not null
            ? new
            {
                virkningFra = new
                {
                    gte = fromDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                },
                status = new
                {
                    eq = ((int)status).ToString(CultureInfo.InvariantCulture)
                }
            }
            : new
            {
                virkningFra = new
                {
                    gte = fromDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                }
            };

        while (true)
        {
            var request = new GraphQLRequest
            {
                Query = @"
        query ($virkningstid: DafDateTime! $first: Int! $after: String $where: DAR_PostnummerFilterInput) {
          DAR_Postnummer(virkningstid: $virkningstid first: $first after: $after where: $where) {
            pageInfo {
              endCursor
              hasNextPage
            }
            nodes {
              datafordelerOpdateringstid
              datafordelerRegisterImportSequenceNumber
              datafordelerRowId
              datafordelerRowVersion
              forretningshaendelse
              forretningsomraade
              forretningsproces
              virkningTil
              virkningsaktoer
              virkningFra
              status
              registreringTil
              registreringsaktoer
              registreringFra
              postnummerinddeling
              postnr
              navn
              id_namespace
              id_lokalId
            }
          }
        }",
                Variables = new
                {
                    virkningstid = toDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                    first = count,
                    after = after,
                    where = whereCondition
                }
            };

            var response = await _graphqlClient.SendQueryAsync<DatafordelerPostCodeGraphqlResponse>(request, cancellationToken).ConfigureAwait(false);

            if (response.Errors?.Length > 0)
            {
                Console.WriteLine(JsonSerializer.Serialize(response.Errors));
            }

            foreach (var datafordelerPostCode in response.Data.DarPostnummer.Nodes)
            {
                yield return MapPostCode(datafordelerPostCode);
            }

            if (!response.Data.DarPostnummer.PageInfo.HasNextPage)
            {
                break;
            }

            after = response.Data.DarPostnummer.PageInfo!.EndCursor;
        }
    }

    public async IAsyncEnumerable<NamedRoadMunicipalDistrict> GetAllNamedRoadMunicipalDistrictsAsync(
        DateTime fromDate,
        DateTime toDate,
        DatafordelerNamedRoadMunicipalDistrictStatus? status = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        const int count = 200;
        string? after = null;

        dynamic whereCondition = status is not null
            ? new
            {
                virkningFra = new
                {
                    gte = fromDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                },
                status = new
                {
                    eq = ((int)status).ToString(CultureInfo.InvariantCulture)
                }
            }
            : new
            {
                virkningFra = new
                {
                    gte = fromDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                }
            };

        while (true)
        {
            var request = new GraphQLRequest
            {
                Query = @"
        query ($virkningstid: DafDateTime! $first: Int! $after: String $where: DAR_NavngivenVejKommunedelFilterInput) {
          DAR_NavngivenVejKommunedel(virkningstid: $virkningstid first: $first after: $after where: $where) {
            pageInfo {
              endCursor
              hasNextPage
            }
            nodes {
              virkningTil
              virkningsaktoer
              vejkode
              status
              virkningFra
              registreringTil
              registreringsaktoer
              registreringFra
              navngivenVej
              kommune
              id_namespace
              id_lokalId
              forretningsproces
              forretningsomraade
              forretningshaendelse
              datafordelerRowVersion
              datafordelerRegisterImportSequenceNumber
              datafordelerRowId
              datafordelerOpdateringstid
            }
          }
        }",
                Variables = new
                {
                    virkningstid = toDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                    first = count,
                    after = after,
                    where = whereCondition
                }
            };

            var response = await _graphqlClient.SendQueryAsync<DatafordelerNamedRoadMunicipalDistrictGraphqlResponse>(request, cancellationToken).ConfigureAwait(false);

            if (response.Errors?.Length > 0)
            {
                Console.WriteLine(JsonSerializer.Serialize(response.Errors));
            }

            foreach (var datafordelerNamedRoadMunicipalDistrict in response.Data.DarNavngivenVejKommunedel.Nodes)
            {
                yield return MapNamedRoadMunicipalDistrict(datafordelerNamedRoadMunicipalDistrict);
            }

            if (!response.Data.DarNavngivenVejKommunedel.PageInfo.HasNextPage)
            {
                break;
            }

            after = response.Data.DarNavngivenVejKommunedel.PageInfo!.EndCursor;
        }
    }

    private static Uri BuildResourcePathFileDownload(
        string baseUri,
        string fileName,
        string apiKey)
    {
        return new Uri($"{baseUri}/FileDownloads/GetFile?filename={fileName}&apikey={apiKey}");
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
            Id = datafordelerUnitAddress.IdLokalId,
            AccessAddressId = Guid.Parse(datafordelerUnitAddress.Husnummer),
            Created = datafordelerUnitAddress.VirkningFra,
            Updated = datafordelerUnitAddress.VirkningFra,
            FloorName = datafordelerUnitAddress.Etagebetegnelse,
            Status = MapUnitAddressStatus(datafordelerUnitAddress.Status),
            SuitName = datafordelerUnitAddress.Drbetegnelse
        };
    }

    private static DawaUnitAddress MapUnitAddress(AddressNode datafordelerUnitAddress)
    {
        return new DawaUnitAddress
        {
            Id = datafordelerUnitAddress.IdLokalId,
            AccessAddressId = datafordelerUnitAddress.Husnummer,
            Created = datafordelerUnitAddress.VirkningFra,
            Updated = datafordelerUnitAddress.VirkningFra,
            FloorName = datafordelerUnitAddress.Etagebetegnelse,
            Status = MapUnitAddressStatus(datafordelerUnitAddress.Status),
            SuitName = datafordelerUnitAddress.Doerbetegnelse
        };
    }

    private static DawaAccessAddress? MapAccessAddress(HusnummerNode from, WKTReader wktReader)
    {
        // In some weird cases they have no reference and that is an invalid address, so we cannot map it.
        if (from.NavngivenVej is null)
        {
            return null;
        }

        var point = (Point)wktReader.Read(from.HusnummerHarAdgangspunkt.Position.Wkt);

        return new DawaAccessAddress
        {
            Created = from.VirkningFra,
            Id = Guid.Parse(from.IdLokalId),
            EastCoordinate = point.X,
            NorthCoordinate = point.Y,
            HouseNumber = string.IsNullOrWhiteSpace(from.Husnummertekst) ? "?" : from.Husnummertekst,
            LocationUpdated = from.HusnummerHarAdgangspunkt.VirkningFra,
            MunicipalCode = from.Kommuneinddeling,
            Updated = from.VirkningFra,
            RoadCode = from.Vejmidte.Split("-").Last(),
            Status = MapAccessAddressStatus(from.Status),
            PlotId = from.Jordstykke,
            PostDistrictCode = from.HusnummerHoererTilPostnummer.Postnr,
            RoadId = Guid.Parse(from.NavngivenVej),
            SupplementaryTownName = from.SupplerendeBynavn
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
            Console.WriteLine($"Could not map access address with id: '{datafordelerAccessAddress.IdLokalId}' since it does not contain the required reference 'NavngivenVej'.");
            return null;
        }

        // This is done because their data is invalid and can reference things that do not exist.
        if (!adgangsPunktLookup.TryGetValue(Guid.Parse(datafordelerAccessAddress.Adgangspunkt), out var adgangsPunkt))
        {
            Console.WriteLine($"Could not find adgangspunkt with id: '{datafordelerAccessAddress.Adgangspunkt}' on access address with id: '{datafordelerAccessAddress.IdLokalId}'.");
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
            LocationUpdated = adgangsPunkt.VirkningFra,
            MunicipalCode = datafordelerAccessAddress.Kommuneinddeling,
            Updated = datafordelerAccessAddress.VirkningFra,
            RoadCode = datafordelerAccessAddress.Vejmidte.Split("-").Last(),
            Status = MapAccessAddressStatus(datafordelerAccessAddress.Status),
            PlotId = datafordelerAccessAddress.Jordstykke,
            PostDistrictCode = postCode?.Number ?? "",
            RoadId = Guid.Parse(datafordelerAccessAddress.NavngivenVej),
            SupplementaryTownName = supplementaryTownName
        };
    }

    private static DawaPostCode MapPostCode(DatafordelerPostCodeFileServer datafordelerPostCode)
    {
        return new DawaPostCode(
            Guid.Parse(datafordelerPostCode.IdLokalId),
            datafordelerPostCode.Navn,
            datafordelerPostCode.Postnr,
            MapPostCodeStatus(datafordelerPostCode.Status),
            datafordelerPostCode.VirkningFra,
            datafordelerPostCode.VirkningFra
        );
    }

    private static DawaPostCode MapPostCode(PostnummerNode datafordelerPostCode)
    {
        return new DawaPostCode(
            datafordelerPostCode.IdLokalId,
            datafordelerPostCode.Navn,
            datafordelerPostCode.Postnr,
            MapPostCodeStatus(datafordelerPostCode.Status),
            datafordelerPostCode.VirkningFra,
            datafordelerPostCode.VirkningFra
        );
    }

    private static DawaRoad MapRoad(DatafordelerRoadFileServer datafordelerRoad)
    {
        return new DawaRoad
        {
            Id = Guid.Parse(datafordelerRoad.IdLokalId),
            Created = datafordelerRoad.VirkningFra,
            Updated = datafordelerRoad.VirkningFra,
            Name = datafordelerRoad.Vejnavn ?? "",
            Status = MapRoadStatus(datafordelerRoad.Status)
        };
    }

    private static DawaRoad MapRoad(NavngivenVejNode datafordelerRoad)
    {
        return new DawaRoad
        {
            Id = datafordelerRoad.IdLokalId,
            Created = datafordelerRoad.VirkningFra,
            Updated = datafordelerRoad.VirkningFra,
            Name = datafordelerRoad.Vejnavn ?? "",
            Status = MapRoadStatus(datafordelerRoad.Status)
        };
    }

    private static NamedRoadMunicipalDistrict MapNamedRoadMunicipalDistrict(DatafordelerNamedRoadMunicipalDistrictFileServerFileServer datafordelerNamedRoadMunicipalDistrict)
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

    private static NamedRoadMunicipalDistrict MapNamedRoadMunicipalDistrict(NavngivenVejKommunedelNode datafordelerNamedRoadMunicipalDistrict)
    {
        return new NamedRoadMunicipalDistrict
        {
            Id = datafordelerNamedRoadMunicipalDistrict.IdLokalId,
            Status = MapNamedRoadMunicipalDistrictStatus(datafordelerNamedRoadMunicipalDistrict.Status),
            MunicipalityCode = datafordelerNamedRoadMunicipalDistrict.Kommune,
            NamedRoadId = datafordelerNamedRoadMunicipalDistrict.NavngivenVej,
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

        try
        {
            var latestGenerationFile = await LatestGenerationFileResourceCurrentTotalDownloadAsync(
                resourceName, cancellationToken).ConfigureAwait(false);

            var uri = BuildResourcePathFileDownload(_baseAddressApi, latestGenerationFile.FileName, apiKey);

            var response = await _httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            using (var fs = new FileStream(tempFileNameZip, FileMode.Create))
            {
                await stream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
            }

            await ZipFile.ExtractToDirectoryAsync(tempFileNameZip, tempFileName, cancellationToken).ConfigureAwait(false);

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
        }
        finally
        {
            if (File.Exists(tempFileNameZip))
            {
                File.Delete(tempFileNameZip);
            }

            if (Directory.Exists(tempFileName))
            {
                Directory.Delete(tempFileName, true);
            }
        }
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

    public void Dispose()
    {
        if (_graphqlClient is not null)
        {
            _graphqlClient.Dispose();
        }
    }
}
