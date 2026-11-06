# Neo4j Health Check

This health check verifies the ability to communicate with [Neo4j](https://www.neo4j.com/).
It uses the provided [IDriver](https://neo4j.com/docs/api/dotnet-driver/6.3/api/Neo4j.Driver.IDriver.html).

### Defaults

By default, the `IDriver` instance is resolved from service provider. 

```csharp
void Configure(IServiceCollection services)
{
    services
        .AddSingleton<IDriver>(GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password")))
        .AddHealthChecks()
        .AddNeo4j();
}
```

### Customization

You can optionally provide a custom client factory, set a custom name, failure status, tags, or timeout:

```csharp
void Configure(IServiceCollection services)
{

    services
        .AddSingleton<IDriver>(GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password")))
        .AddHealthChecks()
        .AddNeo4j(name: "custom-neo4j");
}
```
