# DAWA Address

Provides a simple streaming interface to the DAWA Address API.
It does simple translation from Danish to English and enables access to both full-load and the changes API.

You can get the NuGet package [here](https://www.nuget.org/packages/DawaAddress).

## Get latest transaction

```c#
var httpClient = new HttpClient();
var client = new DawaClient(httpClient);

var transaction = await client.GetLatestTransactionAsync();
```

## Get all access addresses

```c#
var httpClient = new HttpClient();
var client = new DawaClient(httpClient);

var transaction = await client.GetLatestTransactionAsync();

await foreach (var accessAddress in client.GetAllAccessAddresses(transaction.Id))
{
    // Do your logic here
}
```

## Get all access addresses

```c#
var httpClient = new HttpClient();
var client = new DawaClient(httpClient);

var transaction = await client.GetLatestTransactionAsync();

await foreach (var unitAddress in client.GetAllUnitAddresses(transaction.Id))
{
    // Do your logic here
}
```

## Get all roads

```c#
var httpClient = new HttpClient();
var client = new DawaClient(httpClient);

var transaction = await client.GetLatestTransactionAsync();

await foreach (var accessAddress in client.GetAllRoadsAsync(transaction.Id))
{
    // Do your logic here
}
```

## Get all post codes

```c#
var httpClient = new HttpClient();
var client = new DawaClient(httpClient);

var transaction = await client.GetLatestTransactionAsync();

await foreach (var postCode in client.GetAllPostCodesAsync(transaction.Id))
{
    // Do your logic here
}
```

## Get all named road municipal districts

```c#
var httpClient = new HttpClient();
var client = new DawaClient(httpClient);

var transaction = await client.GetLatestTransactionAsync();

await foreach (var postCode in client.GetAllNamedRoadMunicipalDistrictAsync(transaction.Id))
{
    // Do your logic here
}
```


## Get access address changes

```C#
var httpClient = new HttpClient();
var client = new DawaClient(httpClient);

var transaction = await client.GetLatestTransactionAsync();
// You should use your own fromTransactionId here.
var fromTransactionId = transaction.Id - 1000;
var toTransactionId = transaction.Id;

await foreach (var change in client.GetChangesAccessAddressAsync(fromTransactionId, toTransactionId))
{
    // Do your logic here
}
```

## Get unit address changes

```C#
var httpClient = new HttpClient();
var client = new DawaClient(httpClient);

var transaction = await client.GetLatestTransactionAsync();
// You should use your own fromTransactionId here.
var fromTransactionId = transaction.Id - 1000;
var toTransactionId = transaction.Id;

await foreach (var change in client.GetChangesUnitAddressAsync(fromTransactionId, toTransactionId))
{
    // Do your logic here
}
```

## Get road code changes

```C#
var httpClient = new HttpClient();
var client = new DawaClient(httpClient);

var transaction = await client.GetLatestTransactionAsync();
// You should use your own fromTransactionId here.
var fromTransactionId = transaction.Id - 1000;
var toTransactionId = transaction.Id;

await foreach (var change in client.GetChangesRoadsAsync(fromTransactionId, toTransactionId))
{
    // Do your logic here
}
```

## Get post code changes

```C#
var httpClient = new HttpClient();
var client = new DawaClient(httpClient);

var transaction = await client.GetLatestTransactionAsync();
// You should use your own fromTransactionId here.
var fromTransactionId = transaction.Id - 1000;
var toTransactionId = transaction.Id;

await foreach (var change in client.GetChangesPostCodesAsync(fromTransactionId, toTransactionId))
{
    // Do your logic here
}
```

## Get named road municipal district changes

```C#
var httpClient = new HttpClient();
var client = new DawaClient(httpClient);

var transaction = await client.GetLatestTransactionAsync();
// You should use your own fromTransactionId here.
var fromTransactionId = transaction.Id - 1000;
var toTransactionId = transaction.Id;

await foreach (var change in client.GetChangesNamedRoadMunicipalDistrictAsync(fromTransactionId, toTransactionId))
{
    // Do your logic here
}
```
