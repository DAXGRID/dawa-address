namespace DawaAddress.Tests;

public class DawaClientTest
{
    [Fact]
    public async Task Get_all_access_addresses()
    {
        var httpClient = new HttpClient();
        var client = new DatafordelerClient(httpClient);

        var accessAddresses = new List<DawaAccessAddress>();
        await foreach (var accessAddress in client.GetAllAccessAddresses())
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
}
