namespace DawaAddress.Tests;

public class DatafordelerClientTest
{
    [Fact]
    public async Task Get_all_access_addresses_active_pending_from_file()
    {
        var httpClient = new HttpClient();
        var client = new DatafordelerClient(httpClient);
        var fromDate = DateTime.MinValue;
        var toDate = DateTime.UtcNow;

        var accessAddresses = new List<DawaAccessAddress>();
        await foreach (var accessAddress in client.GetAllAccessAddressesAsync(new() { DawaStatus.Active, DawaStatus.Pending }))
        {
            accessAddresses.Add(accessAddress);

            if (accessAddresses.Count == 100_000)
            {
                break;
            }
        }

        accessAddresses
            .Should()
            .HaveCount(100_000);

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

        accessAddresses.Select(x => x.Status)
            .Should()
            .AllSatisfy(x => x.Should().Match(x => x == DawaStatus.Active || x == DawaStatus.Pending));
    }

    [Fact]
    public async Task Get_all_access_addresses_active()
    {
        var httpClient = new HttpClient();
        var client = new DatafordelerClient(httpClient);
        var fromDate = DateTime.MinValue;
        var toDate = DateTime.UtcNow;

        var accessAddresses = new List<DawaAccessAddress>();
        await foreach (var accessAddress in client.GetAllAccessAddressesAsync(fromDate, toDate, DatafordelerAccessAddressStatus.Active))
        {
            accessAddresses.Add(accessAddress);

            if (accessAddresses.Count == 1000)
            {
                break;
            }
        }

        accessAddresses
            .Should()
            .HaveCount(1000);

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

        accessAddresses.Select(x => x.Status)
            .Should()
            .AllBeEquivalentTo(DawaStatus.Active);
    }

    [Fact]
    public async Task Get_all_access_addresses_pending()
    {
        var httpClient = new HttpClient();
        var client = new DatafordelerClient(httpClient);
        var fromDate = DateTime.MinValue;
        var toDate = DateTime.UtcNow;

        var accessAddresses = new List<DawaAccessAddress>();
        await foreach (var accessAddress in client.GetAllAccessAddressesAsync(fromDate, toDate, DatafordelerAccessAddressStatus.Pending))
        {
            accessAddresses.Add(accessAddress);

            if (accessAddresses.Count == 400)
            {
                break;
            }
        }

        accessAddresses
            .Should()
            .HaveCountGreaterThan(1);

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

        accessAddresses.Select(x => x.Status)
            .Should()
            .AllBeEquivalentTo(DawaStatus.Pending);
    }

    [Fact]
    public async Task Get_all_access_addresses_four_days()
    {
        var httpClient = new HttpClient();
        var client = new DatafordelerClient(httpClient);
        var fromDate = DateTime.UtcNow.AddDays(-4);
        var toDate = DateTime.UtcNow;

        var accessAddresses = new List<DawaAccessAddress>();
        await foreach (var accessAddress in client.GetAllAccessAddressesAsync(fromDate, toDate))
        {
            accessAddresses.Add(accessAddress);

            if (accessAddresses.Count == 1000)
            {
                break;
            }
        }

        accessAddresses
            .Should()
            .HaveCountGreaterThan(1);

        accessAddresses.Select(x => x.Id)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        accessAddresses.Select(x => x.Updated.Date)
            .Should()
            .AllSatisfy(x => x.Should().BeOnOrAfter(fromDate.Date));

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
    public async Task Get_all_unit_addresses_from_file_active_and_temporary()
    {
        var httpClient = new HttpClient();
        var client = new DatafordelerClient(httpClient);

        var unitAddresses = new List<DawaUnitAddress>();
        await foreach (var unitAddress in client.GetAllUnitAddressesAsync(new() { DawaStatus.Active, DawaStatus.Pending }))
        {
            unitAddresses.Add(unitAddress);

            if (unitAddresses.Count == 100_000)
            {
                break;
            }
       }

        unitAddresses
            .Should()
            .HaveCount(100_000);

        unitAddresses.Select(x => x.AccessAddressId)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        unitAddresses.Select(x => x.Id)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        unitAddresses.Select(x => x.Status)
            .Should()
            .AllSatisfy(x => x.Should().Match(x => x == DawaStatus.Active || x == DawaStatus.Pending));
    }

    [Fact]
    public async Task Get_all_unit_addresses_active()
    {
        var httpClient = new HttpClient();
        var client = new DatafordelerClient(httpClient);
        var fromDate = DateTime.MinValue;
        var toDate = DateTime.UtcNow;

        var unitAddresses = new List<DawaUnitAddress>();
        await foreach (var unitAddress in client.GetAllUnitAddressesAsync(fromDate, toDate, DatafordelerUnitAddressStatus.Active))
        {
            unitAddresses.Add(unitAddress);

            if (unitAddresses.Count == 1000)
            {
                break;
            }
        }

        unitAddresses
            .Should()
            .HaveCount(1000);

        unitAddresses.Select(x => x.AccessAddressId)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        unitAddresses.Select(x => x.Id)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        unitAddresses.Select(x => x.Status)
            .Should()
            .AllBeEquivalentTo(DawaStatus.Active);
    }

    [Fact]
    public async Task Get_all_unit_addresses_pending()
    {
        var httpClient = new HttpClient();
        var client = new DatafordelerClient(httpClient);
        var fromDate = DateTime.MinValue;
        var toDate = DateTime.UtcNow;

        var unitAddresses = new List<DawaUnitAddress>();
        await foreach (var unitAddress in client.GetAllUnitAddressesAsync(fromDate, toDate, DatafordelerUnitAddressStatus.Pending))
        {
            unitAddresses.Add(unitAddress);

            if (unitAddresses.Count == 1000)
            {
                break;
            }
        }

        unitAddresses
            .Should()
            .HaveCount(1000);

        unitAddresses.Select(x => x.AccessAddressId)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        unitAddresses.Select(x => x.Id)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        unitAddresses.Select(x => x.Status)
            .Should()
            .AllBeEquivalentTo(DawaStatus.Pending);
    }

    [Fact]
    public async Task Get_post_codes()
    {
        var httpClient = new HttpClient();
        var client = new DatafordelerClient(httpClient);

        var postCodes = new List<DawaPostCode>();
        await foreach (var postCode in client.GetAllPostCodesAsync())
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

        postCodes.Select(x => x.Status)
            .Should()
            .AllSatisfy(x => x.Should().Be(DawaPostCodeStatus.Active));

        postCodes.Select(x => x.Created)
            .Should()
            .AllSatisfy(x => x.Should().BeAfter(new DateTime()));

        postCodes.Select(x => x.Updated)
            .Should()
            .AllSatisfy(x => x.Should().BeAfter(new DateTime()));
    }

    [Fact]
    public async Task Get_all_roads_active_from_file()
    {
        var httpClient = new HttpClient();
        var client = new DatafordelerClient(httpClient);

        var resources = new List<DawaRoad>();
        await foreach (var resource in client.GetAllRoadsAsync(new() { DawaRoadStatus.Effective, DawaRoadStatus.Temporary }))
        {
            resources.Add(resource);
        }

        resources
             .Should()
             .HaveCountGreaterThan(1000);

        resources.Select(x => x.Name)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        resources.Select(x => x.Id)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        resources.Select(x => x.Status)
            .Should()
            .AllSatisfy(x => x.Should().Match(x => x == DawaRoadStatus.Effective || x == DawaRoadStatus.Temporary));

        resources.Select(x => x.Created)
            .Should()
            .AllSatisfy(x => x.Should().BeAfter(new DateTime()));
    }

    [Fact]
    public async Task Get_roads_active()
    {
        var httpClient = new HttpClient();
        var client = new DatafordelerClient(httpClient);
        var fromDate = DateTime.MinValue;
        var toDate = DateTime.UtcNow;

        var resources = new List<DawaRoad>();
        await foreach (var resource in client.GetAllRoadsAsync(fromDate, toDate, DatafordelerRoadStatus.Active))
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
    public async Task Get_roads_temporary()
    {
        var httpClient = new HttpClient();
        var client = new DatafordelerClient(httpClient);
        var fromDate = DateTime.MinValue;
        var toDate = DateTime.UtcNow;

        var resources = new List<DawaRoad>();
        await foreach (var resource in client.GetAllRoadsAsync(fromDate, toDate, DatafordelerRoadStatus.Temporary))
        {
            resources.Add(resource);

            if (resources.Count == 1000)
            {
                break;
            }
        }

        resources
             .Should()
             .HaveCountGreaterThan(1);

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
    public async Task Get_named_road_municipal_districts_active()
    {
        var httpClient = new HttpClient();
        var client = new DatafordelerClient(httpClient);

        var fromDate = DateTime.MinValue;
        var toDate = DateTime.UtcNow;

        var namedRoadMunicipalDistricts = new List<NamedRoadMunicipalDistrict>();
        await foreach (var namedRoadMunicipalDistrict in client.GetAllNamedRoadMunicipalDistrictsAsync(fromDate, toDate, DatafordelerNamedRoadMunicipalDistrictStatus.Active))
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
