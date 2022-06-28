using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DawaAddress.Tests;

public class DawaClientTest
{
    [Fact]
    public async Task Get_latest_transaction()
    {
        var httpClient = new HttpClient();
        var client = new DawaClient(httpClient);

        var transaction = await client.GetLatestTransactionAsync();

        transaction.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Get_all_access_addresses()
    {
        var httpClient = new HttpClient();
        var client = new DawaClient(httpClient);

        var transaction = await client.GetLatestTransactionAsync();

        var accessAddresses = new List<DawaAccessAddress>();
        await foreach (var accessAddress in client.GetAllAccessAddresses(transaction.Id))
        {
            // We only want to test on the first 1000
            // otherwise it would take too long.
            if (accessAddresses.Count == 1000)
            {
                break;
            }

            accessAddresses.Add(accessAddress);
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

        accessAddresses.Select(x => x.RoadId)
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

        accessAddresses.Select(x => x.TownName).Where(x => !string.IsNullOrWhiteSpace(x))
            .Should()
            .HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task Get_all_unit_addresses()
    {
        var httpClient = new HttpClient();
        var client = new DawaClient(httpClient);

        var transaction = await client.GetLatestTransactionAsync();

        var unitAddresses = new List<DawaUnitAddress>();
        await foreach (var accessAddress in client.GetAllUnitAddresses(transaction.Id))
        {
            // We only want to test on the first 1000
            // otherwise it would take too long.
            if (unitAddresses.Count == 1000)
            {
                break;
            }

            unitAddresses.Add(accessAddress);
        }

        unitAddresses
            .Should()
            .HaveCount(1000);

        unitAddresses.Select(x => x.Id)
            .Should()
            .AllSatisfy(x => x.Should().NotBeEmpty());

        unitAddresses.Select(x => x.Created)
            .Should()
            .AllSatisfy(x => x.Should().BeAfter(new()));

        unitAddresses.Select(x => x.Status)
            .Should()
            .AllSatisfy(x => x.Should().NotBe(DawaStatus.None));

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
        var client = new DawaClient(httpClient);

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

        // We just test a few of them since the mapping is already tested in the full import.
        result.Select(x => x.Data.Id).Should().AllSatisfy(x => x.Should().NotBeEmpty());
        result.Select(x => x.Data.HouseNumber).Should().AllSatisfy(x => x.Should().NotBeEmpty());
    }

    [Fact]
    public async Task Get_unit_address_changes()
    {
        var httpClient = new HttpClient();
        var client = new DawaClient(httpClient);

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

        // We just test a few of them since the mapping is already tested in the full import.
        result.Select(x => x.Data.Id).Should().AllSatisfy(x => x.Should().NotBeEmpty());
        result.Select(x => x.Data.AccessAddressId).Should().AllSatisfy(x => x.Should().NotBeEmpty());
    }

    [Fact]
    public async Task Get_roads()
    {
        var httpClient = new HttpClient();
        var client = new DawaClient(httpClient);

        var transaction = await client.GetLatestTransactionAsync();

        var result = new List<DawaRoad>();
        await foreach (var road in client.GetRoadsAsync(transaction.Id))
        {
            // We do
            if (result.Count == 1000)
            {
                break;
            }

            result.Add(road);
        }

        result.Should().HaveCount(1000);
        result.Select(x => x.Id.Should().NotBeNullOrWhiteSpace());
        result.Select(x => x.Name.Should().NotBeNullOrWhiteSpace());
    }

    [Fact]
    public async Task Get_roads_changes()
    {
        var httpClient = new HttpClient();
        var client = new DawaClient(httpClient);

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

        // We just test a few of them since the mapping is already tested in the full import.
        result.Select(x => x.Data.Id).Should().AllSatisfy(x => x.Should().NotBeEmpty());
        result.Select(x => x.Data.Name).Should().AllSatisfy(x => x.Should().NotBeEmpty());
    }

    [Fact]
    public async Task Get_post_codes()
    {
        var httpClient = new HttpClient();
        var client = new DawaClient(httpClient);

        var transaction = await client.GetLatestTransactionAsync();

        var result = new List<DawaPostCode>();
        await foreach (var road in client.GetPostCodesAsync(transaction.Id))
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

    public async Task Get_post_codes_changes()
    {
        var httpClient = new HttpClient();
        var client = new DawaClient(httpClient);

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

        // We just test a few of them since the mapping is already tested in the full import.
        result.Select(x => x.Data.Name).Should().AllSatisfy(x => x.Should().NotBeEmpty());
        result.Select(x => x.Data.Number).Should().AllSatisfy(x => x.Should().NotBeNullOrWhiteSpace());
    }
}
