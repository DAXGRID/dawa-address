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
            // We only want to test on the first 10000
            // otherwise it would take too long.
            if (accessAddresses.Count == 10000)
            {
                break;
            }

            accessAddresses.Add(accessAddress);
        }


        using (var scope = new AssertionScope())
        {
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
    }

    [Fact]
    public async Task Get_roads()
    {
        var httpClient = new HttpClient();
        var client = new DawaClient(httpClient);

        var transaction = await client.GetLatestTransactionAsync();

        await foreach (var road in client.GetRoadsAsync(transaction.Id))
        {
            road.Should().NotBeNull();
            road.Id.Should().NotBeNullOrWhiteSpace();
            // We just want to make sure it has results
            // so we break after first one.
            break;
        }
    }

    [Fact]
    public async Task Get_post_codes()
    {
        var httpClient = new HttpClient();
        var client = new DawaClient(httpClient);

        var transaction = await client.GetLatestTransactionAsync();

        await foreach (var road in client.GetPostCodesAsync(transaction.Id))
        {
            road.Should().NotBeNull();
            road.Number.Should().NotBeNullOrWhiteSpace();
            road.Name.Should().NotBeNullOrWhiteSpace();
            // We just want to make sure it has results
            // so we break after first one.
            break;
        }
    }
}
