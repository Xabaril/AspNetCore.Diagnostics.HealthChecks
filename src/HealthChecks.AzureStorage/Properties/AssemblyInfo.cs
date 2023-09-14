using System.Runtime.CompilerServices;
using HealthChecks.AzureStorage;

[assembly: TypeForwardedTo(typeof(AzureBlobStorageHealthCheck))]
[assembly: TypeForwardedTo(typeof(AzureBlobStorageHealthCheckOptions))]
[assembly: TypeForwardedTo(typeof(AzureQueueStorageHealthCheck))]
[assembly: TypeForwardedTo(typeof(AzureQueueStorageHealthCheckOptions))]
[assembly: TypeForwardedTo(typeof(AzureFileShareHealthCheck))]
[assembly: TypeForwardedTo(typeof(AzureFileShareHealthCheckOptions))]
