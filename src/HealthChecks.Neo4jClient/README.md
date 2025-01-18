# Neo4j Health check

This healthCheck checks the status of a database in a neo4j DBMS using the BOLT protocol to establish a connection

# Sample

Using options class
```csharp
var options = new Neo4jClientHealthCheckOptions("bolt://localhost:7687", "neo4j", "neo4j", realm: null);

services.AddHealthChecks()
  .AddNeo4jClient(options);
```

Using client from service provider
```csharp
var graphClient = new BoltGraphClient("bolt://localhost:7687", "neo4j", "neo4j");
services.AddSingleton<IGraphClient>(graphClient);

services.AddHealthChecks()
  .AddNeo4jClient(_ => _.GetRequiredService<IGraphClient>());
```
