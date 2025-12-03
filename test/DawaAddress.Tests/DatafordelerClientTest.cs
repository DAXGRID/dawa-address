namespace DawaAddress.Tests;

public class DatafordelerClientTest
{
    [Fact]
    public async Task Get_all_access_addresses()
    {
        var httpClient = new HttpClient();
        var client = new DatafordelerClient(httpClient);
        var fromDate = DateTime.MinValue;
        var toDate = DateTime.UtcNow;

        var accessAddresses = new List<DawaAccessAddress>();
        await foreach (var accessAddress in client.GetAllAccessAddresses(fromDate, toDate))
        {
            accessAddresses.Add(accessAddress);

            if (accessAddresses.Count == 10000)
            {
                break;
            }
        }

        accessAddresses
            .Should()
            .HaveCount(10000);

        accessAddresses.Select(x => x.Id)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        accessAddresses.Select(x => x.HouseNumber)
            .Should()
            .AllSatisfy(x => x.Should().NotBeNullOrWhiteSpace());

        accessAddresses.Select(x => x.MunicipalCode)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        accessAddresses.Select(x => x.Created)
            .Should()
            .AllSatisfy(x => x.Should().BeAfter(new()));

        accessAddresses
            .Select(x => x.Updated)
            .Should()
            .AllSatisfy(x => x.Should().BeAfter(new()));

        accessAddresses.Select(x => x.RoadId)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        accessAddresses.Select(x => x.PostDistrictCode)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        accessAddresses.Select(x => x.NorthCoordinate)
            .Should()
            .AllSatisfy(x => x.Should().BeGreaterThan(0));

        accessAddresses.Select(x => x.EastCoordinate)
            .Should()
            .AllSatisfy(x => x.Should().BeGreaterThan(0))
            .Should();

        accessAddresses.Select(x => x.PlotId).Where(x => !string.IsNullOrWhiteSpace(x))
            .Should()
            .HaveCountGreaterThan(0);

        accessAddresses.Select(x => x.SupplementaryTownName).Where(x => !string.IsNullOrWhiteSpace(x))
            .Should()
            .HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task Get_all_unit_addresses()
    {
        var httpClient = new HttpClient();
        var client = new DatafordelerClient(httpClient);
        var fromDate = DateTime.MinValue;
        var toDate = DateTime.UtcNow;

        var unitAddresses = new List<DawaUnitAddress>();
        await foreach (var unitAddress in client.GetAllUnitAddresses(fromDate, toDate))
        {
            unitAddresses.Add(unitAddress);

            if (unitAddresses.Count == 10000)
            {
                break;
            }
        }

        unitAddresses
            .Should()
            .HaveCount(10000);

        unitAddresses.Select(x => x.AccessAddressId)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        unitAddresses.Select(x => x.Id)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());
    }

    [Fact]
    public async Task Get_post_codes()
    {
        var httpClient = new HttpClient();
        var client = new DatafordelerClient(httpClient);
        var fromDate = DateTime.MinValue;
        var toDate = DateTime.UtcNow;

        var postCodes = new List<DawaPostCode>();
        await foreach (var postCode in client.GetAllPostCodesAsync(fromDate, toDate))
        {
            postCodes.Add(postCode);

            if (postCodes.Count == 1000)
            {
                break;
            }
        }

        postCodes
             .Should()
             .HaveCount(1000);

        postCodes.Select(x => x.Number)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        postCodes.Select(x => x.Name)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());
    }

    [Fact]
    public async Task Get_roads()
    {
        var httpClient = new HttpClient();
        var client = new DatafordelerClient(httpClient);
        var fromDate = DateTime.MinValue;
        var toDate = DateTime.UtcNow;

        var resources = new List<DawaRoad>();
        await foreach (var resource in client.GetAllRoadsAsync(fromDate, toDate))
        {
            resources.Add(resource);

            if (resources.Count == 1000)
            {
                break;
            }
        }

        resources
             .Should()
             .HaveCount(1000);

        resources.Select(x => x.Name)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        resources.Select(x => x.Id)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        resources.Select(x => x.Created)
            .Should()
            .AllSatisfy(x => x.Should().BeAfter(new DateTime()));
    }

    [Fact]
    public async Task Get_named_road_municipal_districts()
    {
        var httpClient = new HttpClient();
        var client = new DatafordelerClient(httpClient);

        var fromDate = DateTime.MinValue;
        var toDate = DateTime.UtcNow;

        var namedRoadMunicipalDistricts = new List<NamedRoadMunicipalDistrict>();
        await foreach (var namedRoadMunicipalDistrict in client.GetAllNamedRoadMunicipalDistrictsAsync(fromDate, toDate))
        {
            namedRoadMunicipalDistricts.Add(namedRoadMunicipalDistrict);

            if (namedRoadMunicipalDistricts.Count == 1000)
            {
                break;
            }
        }

        namedRoadMunicipalDistricts
             .Should()
             .HaveCount(1000);

        namedRoadMunicipalDistricts.Select(x => x.Id)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        namedRoadMunicipalDistricts.Select(x => x.RoadCode)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        namedRoadMunicipalDistricts.Select(x => x.MunicipalityCode)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        namedRoadMunicipalDistricts.Select(x => x.NamedRoadId)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());
    }
}
