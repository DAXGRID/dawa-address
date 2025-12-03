namespace DawaAddress.Tests;

public class DataForsyningenClientTest
{
    [Fact]
    public async Task Get_latest_transaction()
    {
        var httpClient = new HttpClient();
        var client = new DataForsyningenClient(httpClient);

        var transaction = await client.GetLatestTransactionAsync();

        transaction.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Get_all_transactions_after_specfic_transaction_id()
    {
        var httpClient = new HttpClient();
        var client = new DataForsyningenClient(httpClient);

        var transactions = await client.GetAllTransactionsAfter(3958188);

        transactions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Get_all_access_addresses()
    {
        var httpClient = new HttpClient();
        var client = new DataForsyningenClient(httpClient);

        var transaction = await client.GetLatestTransactionAsync();

        var accessAddresses = new List<DawaAccessAddress>();
        await foreach (var accessAddress in client.GetAllAccessAddresses(transaction.Id))
        {
            // We only want to test on the first 10000
            // otherwise it would take too long.
            if (accessAddresses.Count == 10000)
            {
                break;
            }

            accessAddresses.Add(accessAddress);
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
        var client = new DataForsyningenClient(httpClient);

        var transaction = await client.GetLatestTransactionAsync();

        var unitAddresses = new List<DawaUnitAddress>();
        await foreach (var accessAddress in client.GetAllUnitAddresses(transaction.Id))
        {
            // We only want to test on the first 10000
            // otherwise it would take too long.
            if (unitAddresses.Count == 10000)
            {
                break;
            }

            unitAddresses.Add(accessAddress);
        }

        unitAddresses
            .Should()
            .HaveCount(10000);

        unitAddresses.Select(x => x.Id)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        unitAddresses.Select(x => x.Created)
            .Should()
            .AllSatisfy(x => x.Should().BeAfter(new()));

        unitAddresses
            .Select(x => x.Updated)
            .Should()
            .AllSatisfy(x => x.Should().BeAfter(new()));

        unitAddresses.Select(x => x.FloorName).Where(x => !string.IsNullOrWhiteSpace(x))
            .Should()
            .HaveCountGreaterThan(0);

        unitAddresses.Select(x => x.SuitName).Where(x => !string.IsNullOrWhiteSpace(x))
            .Should()
            .HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task Get_access_address_changes()
    {
        var httpClient = new HttpClient();
        var client = new DataForsyningenClient(httpClient);

        var transaction = await client.GetLatestTransactionAsync();

        var result = new List<DawaEntityChange<DawaAccessAddress>>();
        await foreach (var change in client.GetChangesAccessAddressAsync(transaction.Id - 100000, transaction.Id))
        {
            if (result.Count == 1000)
            {
                break;
            }

            result.Add(change);
        }

        result.Should().HaveCountGreaterThan(0);
        result.Select(x => x.Id).Should().AllSatisfy(x => x.Should().BeGreaterThan(0));
        result.Select(x => x.Data).Should().AllSatisfy(x => x.Should().NotBeNull());
        result.Select(x => x.ChangeTime).Should().AllSatisfy(x => x.Should().BeAfter(default));


        // We just test a few of them since the mapping is already tested in the full import.
        result.Select(x => x.Data.Id).Should().AllSatisfy(x => x.Should().NotBeEmpty());
        result.Select(x => x.Data.HouseNumber).Should().AllSatisfy(x => x.Should().NotBeEmpty());
    }

    [Fact]
    public async Task Get_unit_address_changes()
    {
        var httpClient = new HttpClient();
        var client = new DataForsyningenClient(httpClient);

        var transaction = await client.GetLatestTransactionAsync();

        var result = new List<DawaEntityChange<DawaUnitAddress>>();
        await foreach (var change in client.GetChangesUnitAddressAsync(0, transaction.Id))
        {
            if (result.Count == 1000)
            {
                break;
            }

            result.Add(change);
        }

        result.Should().HaveCountGreaterThan(0);
        result.Select(x => x.Id).Should().AllSatisfy(x => x.Should().BeGreaterThan(0));
        result.Select(x => x.Data).Should().AllSatisfy(x => x.Should().NotBeNull());
        result.Select(x => x.ChangeTime).Should().AllSatisfy(x => x.Should().BeAfter(default));

        // We just test a few of them since the mapping is already tested in the full import.
        result.Select(x => x.Data.Id).Should().AllSatisfy(x => x.Should().NotBeEmpty());
        result.Select(x => x.Data.AccessAddressId).Should().AllSatisfy(x => x.Should().NotBeEmpty());
    }

    [Fact]
    public async Task Get_all_roads()
    {
        var httpClient = new HttpClient();
        var client = new DataForsyningenClient(httpClient);

        var transaction = await client.GetLatestTransactionAsync();

        var result = new List<DawaRoad>();
        await foreach (var road in client.GetAllRoadsAsync(transaction.Id))
        {
            // We do
            if (result.Count == 1000)
            {
                break;
            }

            result.Add(road);
        }

        result.Should().HaveCount(1000);
        result.Select(x => x.Id.Should().NotBeEmpty());
        result.Select(x => x.Name.Should().NotBeNullOrWhiteSpace());
        result.Select(x => x.Created.Should().BeAfter(new()));
        result.Select(x => x.Updated.Should().BeAfter(new()));
        result
            .Select(x => x.Status)
            .Should()
            .AllSatisfy(x => x
                       .Should()
                       .BeOneOf(DawaRoadStatus.Effective, DawaRoadStatus.Temporary));
    }

    [Fact]
    public async Task Get_roads_changes()
    {
        var httpClient = new HttpClient();
        var client = new DataForsyningenClient(httpClient);

        var transaction = await client.GetLatestTransactionAsync();

        var result = new List<DawaEntityChange<DawaRoad>>();
        await foreach (var change in client.GetChangesRoadsAsync(0, transaction.Id))
        {
            if (result.Count == 1000)
            {
                break;
            }

            result.Add(change);
        }

        result.Should().HaveCountGreaterThan(0);
        result.Select(x => x.Id).Should().AllSatisfy(x => x.Should().BeGreaterThan(0));
        result.Select(x => x.Data).Should().AllSatisfy(x => x.Should().NotBeNull());
        result.Select(x => x.ChangeTime).Should().AllSatisfy(x => x.Should().BeAfter(default));
        result.Select(x => x.Data).Should().AllSatisfy(x => x.Should().NotBeNull());
        result.Select(x => x.Data.Id).Should().AllSatisfy(x => x.Should().NotBeEmpty());
        result.Select(x => x.Data.Name).Should().AllSatisfy(x => x.Should().NotBeEmpty());
        result.Select(x => x.Data.Created)
            .Should().AllSatisfy(x => x.Should().BeAfter(default));
        result.Select(x => x.Data.Updated)
            .Should().AllSatisfy(x => x.Should().BeAfter(default));
        result
            .Select(x => x.Data.Status)
            .Should()
            .AllSatisfy(x => x
                        .Should()
                        .BeOneOf(DawaRoadStatus.Effective, DawaRoadStatus.Temporary));
    }

    [Fact]
    public async Task Get_all_post_codes()
    {
        var httpClient = new HttpClient();
        var client = new DataForsyningenClient(httpClient);

        var transaction = await client.GetLatestTransactionAsync();

        var result = new List<DawaPostCode>();
        await foreach (var road in client.GetAllPostCodesAsync(transaction.Id))
        {
            if (result.Count == 1000)
            {
                break;
            }

            result.Add(road);
        }

        result.Should().HaveCount(1000);
        result.Select(x => x.Number).Should().NotBeNullOrEmpty();
        result.Select(x => x.Name).Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Get_post_codes_changes()
    {
        var httpClient = new HttpClient();
        var client = new DataForsyningenClient(httpClient);

        var transaction = await client.GetLatestTransactionAsync();

        var result = new List<DawaEntityChange<DawaPostCode>>();
        await foreach (var change in client.GetChangesPostCodesAsync(0, transaction.Id))
        {
            if (result.Count == 1000)
            {
                break;
            }

            result.Add(change);
        }

        result.Should().HaveCountGreaterThan(0);
        result.Select(x => x.Id).Should().AllSatisfy(x => x.Should().BeGreaterThan(0));
        result.Select(x => x.Data).Should().AllSatisfy(x => x.Should().NotBeNull());
        result.Select(x => x.ChangeTime).Should().AllSatisfy(x => x.Should().BeAfter(default));

        // We just test a few of them since the mapping is already tested in the full import.
        result.Select(x => x.Data.Name).Should().AllSatisfy(x => x.Should().NotBeEmpty());
        result.Select(x => x.Data.Number).Should().AllSatisfy(x => x.Should().NotBeNullOrWhiteSpace());
    }

    [Fact]
    public async Task Get_all_named_road_municipal_districts()
    {
        var httpClient = new HttpClient();
        var client = new DataForsyningenClient(httpClient);

        var transaction = await client.GetLatestTransactionAsync();

        var result = new List<NamedRoadMunicipalDistrict>();
        await foreach (var road in client.GetAllNamedRoadMunicipalDistrictsAsync(transaction.Id))
        {
            // We do
            if (result.Count == 1000)
            {
                break;
            }

            result.Add(road);
        }

        result.Should().HaveCount(1000);
        result.Select(x => x.Id.Should().NotBeEmpty());
        result.Select(x => x.MunicipalityCode.Should().NotBeNullOrWhiteSpace());
        result.Select(x => x.NamedRoadId.Should().NotBeEmpty());
    }

    [Fact]
    public async Task Get_named_municipal_districts_changes()
    {
        var httpClient = new HttpClient();
        var client = new DataForsyningenClient(httpClient);

        var transaction = await client.GetLatestTransactionAsync();

        var result = new List<DawaEntityChange<NamedRoadMunicipalDistrict>>();
        await foreach (var change in client.GetChangesNamedRoadMunicipalDistrictAsync(0, transaction.Id))
        {
            if (result.Count == 1000)
            {
                break;
            }

            result.Add(change);
        }

        result.Should().HaveCountGreaterThan(0);
        result.Select(x => x.Id).Should().AllSatisfy(x => x.Should().BeGreaterThan(0));
        result.Select(x => x.Data).Should().AllSatisfy(x => x.Should().NotBeNull());
        result.Select(x => x.ChangeTime).Should().AllSatisfy(x => x.Should().BeAfter(default));

        // We just test a few of them since the mapping is already tested in the full import.
        result.Select(x => x.Data.Id).Should().AllSatisfy(x => x.Should().NotBeEmpty());
        result.Select(x => x.Data.NamedRoadId).Should().AllSatisfy(x => x.Should().NotBeEmpty());
    }
}
