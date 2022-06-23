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
    public async Task Get_roads()
    {
        var httpClient = new HttpClient();
        var client = new DawaClient(httpClient);

        var transaction = await client.GetLatestTransactionAsync();
        var roads = client.GetRoadsAsync(transaction.Id);

        await foreach (var road in roads)
        {
            road.Should().NotBeNull();
            road.Id.Should().NotBeNullOrWhiteSpace();
            // We just want to make sure it has results
            // so we break after first on.
            break;
        }
    }

    [Fact]
    public async Task Get_post_codes()
    {
        var httpClient = new HttpClient();
        var client = new DawaClient(httpClient);

        var transaction = await client.GetLatestTransactionAsync();
        var postCodes = client.GetPostCodesAsync(transaction.Id);

        await foreach (var road in postCodes)
        {
            road.Should().NotBeNull();
            road.Number.Should().NotBeNullOrWhiteSpace();
            road.Name.Should().NotBeNullOrWhiteSpace();
            // We just want to make sure it has results
            // so we break after first on.
            break;
        }
    }
}
