[![License](https://img.shields.io/github/license/Xabaril/AspNetCore.Diagnostics.HealthChecks)](LICENSE)
[![codecov](https://codecov.io/github/Xabaril/AspNetCore.Diagnostics.HealthChecks/coverage.svg?branch=master)](https://codecov.io/github/Xabaril/AspNetCore.Diagnostics.HealthChecks?branch=master)
[![GitHub Release Date](https://img.shields.io/github/release-date/Xabaril/AspNetCore.Diagnostics.HealthChecks?label=released)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/releases)
[![GitHub commits since latest release (by date)](https://img.shields.io/github/commits-since/Xabaril/AspNetCore.Diagnostics.HealthChecks/latest?label=new+commits)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/commits/master)
![Size](https://img.shields.io/github/repo-size/Xabaril/AspNetCore.Diagnostics.HealthChecks)

[![GitHub contributors](https://img.shields.io/github/contributors/Xabaril/AspNetCore.Diagnostics.HealthChecks)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/contributors)
![Activity](https://img.shields.io/github/commit-activity/w/Xabaril/AspNetCore.Diagnostics.HealthChecks)
![Activity](https://img.shields.io/github/commit-activity/m/Xabaril/AspNetCore.Diagnostics.HealthChecks)
![Activity](https://img.shields.io/github/commit-activity/y/Xabaril/AspNetCore.Diagnostics.HealthChecks)

# AspNetCore.Diagnostics.HealthChecks

This repository offers a wide collection of **ASP.NET Core** Health Check packages for widely used services and platforms.

**ASP.NET Core** versions supported: 8.0, 7.0, 6.0, 5.0, 3.1, 3.0 and 2.2

# Sections

## Previous versions documentation

- [NetCore 3.1](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/blob/netcore-3.1/README.md)
- [NetCore 3.0](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/blob/netcore-3.0/README.md)
- [NetCore 2.2](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/blob/netcore-2.2/README.md)

## HealthChecks

- [Health Checks](#Health-Checks)
- [Health Checks Push Results](#HealthCheck-push-results)

## HealthChecks UI

- [UI](#HealthCheckUI)
- [UI Storage Providers](#UI-Storage-Providers)
- [UI Database Migrations](#UI-Database-Migrations)
- [History Timeline](#Health-status-history-timeline)
- [Configuration](#Configuration)
- [Webhooks and Failure Notifications](#Webhooks-and-Failure-Notifications)
- [HttpClient and HttpMessageHandler Configuration](#UI-Configure-HttpClient-and-HttpMessageHandler-for-Api-and-Webhooks-endpoints)

## HealthChecks UI and Kubernetes

- [Kubernetes Operator](#UI-Kubernetes-Operator)
- [Kubernetes automatic services discovery](#UI-Kubernetes-automatic-services-discovery)

## HealthChecks and Devops

- [Releases Gates for Azure DevOps Pipelines](#HealthChecks-as-Release-Gates-for-Azure-DevOps-Pipelines)

## HealthChecks Tutorials

- [Tutorials, Demos and walkthroughs](#tutorials-demos-and-walkthroughs-on-aspnet-core-healthchecks)

## Docker images

HealthChecks repo provides following images:

| Image | Downloads | Latest | Issues |
|------|--------|---|---|
| UI | ![ui pulls](https://img.shields.io/docker/pulls/xabarilcoding/healthchecksui.svg?label=downloads) | ![ui version](https://img.shields.io/docker/v/xabarilcoding/healthchecksui?label=docker&logo=dsd&sort=date) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/ui)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/ui)
| K8s operator | ![k8s pulls](https://img.shields.io/docker/pulls/xabarilcoding/healthchecksui-k8s-operator.svg?label=downloads) | ![k8s version](https://img.shields.io/docker/v/xabarilcoding/healthchecksui-k8s-operator?label=docker&logo=dsd&sort=date)

## Health Checks

HealthChecks packages include health checks for:

| Package                    | Downloads                                                                                                                                                                     | NuGet Latest | Issues | Notes                                                                  |
| -------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------ | ------ | ---------------------------------------------------------------------- |
| ApplicationStatus          | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.ApplicationStatus)](https://www.nuget.org/packages/AspNetCore.HealthChecks.ApplicationStatus)               | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.ApplicationStatus)](https://www.nuget.org/packages/AspNetCore.HealthChecks.ApplicationStatus) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/applicationstatus)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/applicationstatus)
| ArangoDB                   | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.ArangoDb)](https://www.nuget.org/packages/AspNetCore.HealthChecks.ArangoDb)                                 | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.ArangoDb)](https://www.nuget.org/packages/AspNetCore.HealthChecks.ArangoDb) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/arrangodb)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/arrangodb)
| Amazon S3                  | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Aws.S3)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Aws.S3)                                     | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Aws.S3)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Aws.S3) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/aws)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/aws)
| Amazon Secrets Manager     | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Aws.SecretsManager)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Aws.SecretsManager)             | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Aws.SecretsManager)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Aws.SecretsManager) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/aws)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/aws)
| Amazon SNS                 | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Aws.Sns)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Aws.Sns)                                   | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Aws.Sns)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Aws.Sns) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/aws)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/aws)
| Amazon SQS                 | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Aws.Sqs)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Aws.Sqs)                                   | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Aws.Sqs)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Aws.Sqs) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/aws)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/aws)
| Amazon Systems Manager     | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Aws.SystemsManager)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Aws.SystemsManager)             | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Aws.SystemsManager)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Aws.SystemsManager) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/aws)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/aws)
| Azure Application Insights | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.AzureApplicationInsights)](https://www.nuget.org/packages/AspNetCore.HealthChecks.AzureApplicationInsights) | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.AzureApplicationInsights)](https://www.nuget.org/packages/AspNetCore.HealthChecks.AzureApplicationInsights) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/applicationinsights)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/applicationinsights)
| Azure Tables               | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Azure.Data.Tables)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Azure.Data.Tables)               | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Azure.Data.Tables)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Azure.Data.Tables) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/azure)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/azure)
| Azure IoT Hub              | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Azure.IoTHub)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Azure.IoTHub)                         | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Azure.IoTHub)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Azure.IoTHub) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/azure)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/azure) |
| Azure Key Vault Secrets    | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Azure.KeyVault.Secrets)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Azure.KeyVault.Secrets)     | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Azure.KeyVault.Secrets)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Azure.KeyVault.Secrets) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/azure)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/azure) |
| Azure Event Hubs           | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Azure.Messaging.EventHubs)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Azure.Messaging.EventHubs) | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Azure.Messaging.EventHubs)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Azure.Messaging.EventHubs) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/azure)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/azure) |
| Azure Blob Storage         | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Azure.Storage.Blobs)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Azure.Storage.Blobs)           | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Azure.Storage.Blobs)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Azure.Storage.Blobs) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/azure)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/azure) |
| Azure File Storage         | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Azure.Storage.Files.Shares)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Azure.Storage.Files.Shares) | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Azure.Storage.Files.Shares)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Azure.Storage.Files.Shares) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/azure)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/azure) |
| Azure Queue Storage        | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Azure.Storage.Queues)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Azure.Storage.Queues)         | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Azure.Storage.Queues)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Azure.Storage.Queues) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/azure)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/azure) |
| Azure DigitalTwin          | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.AzureDigitalTwin)](https://www.nuget.org/packages/AspNetCore.HealthChecks.AzureDigitalTwin)                 | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.AzureDigitalTwin)](https://www.nuget.org/packages/AspNetCore.HealthChecks.AzureDigitalTwin) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/azure)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/azure) | Subscription status, models and instances                              |
| Azure Key Vault            | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.AzureKeyVault)](https://www.nuget.org/packages/AspNetCore.HealthChecks.AzureKeyVault)                       | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.AzureKeyVault)](https://www.nuget.org/packages/AspNetCore.HealthChecks.AzureKeyVault) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/azure)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/azure)
| Azure Search               | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.AzureSearch)](https://www.nuget.org/packages/AspNetCore.HealthChecks.AzureSearch)                           | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.AzureSearch)](https://www.nuget.org/packages/AspNetCore.HealthChecks.AzureSearch) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/azure)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/azure)
| Azure Service Bus          | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.AzureServiceBus)](https://www.nuget.org/packages/AspNetCore.HealthChecks.AzureServiceBus)                   | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.AzureServiceBus)](https://www.nuget.org/packages/AspNetCore.HealthChecks.AzureServiceBus) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/azure)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/azure) | Queue and Topics                                             |
| Consul                     | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Consul)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Consul)                                     | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Consul)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Consul) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/consul)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/consul)
| CosmosDb                   | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.CosmosDb)](https://www.nuget.org/packages/AspNetCore.HealthChecks.CosmosDb)                                 | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.CosmosDb)](https://www.nuget.org/packages/AspNetCore.HealthChecks.CosmosDb) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/cosmosdb)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/cosmosdb) | CosmosDb and Azure Table
| Dapr                       | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Dapr)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Dapr)                                         | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Dapr)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Dapr) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/dapr)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/dapr)
| Azure DocumentDb           | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.DocumentDb)](https://www.nuget.org/packages/AspNetCore.HealthChecks.DocumentDb)                             | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.DocumentDb)](https://www.nuget.org/packages/AspNetCore.HealthChecks.DocumentDb) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/documentdb)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/documentdb)
| Amazon DynamoDb            | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.DynamoDb)](https://www.nuget.org/packages/AspNetCore.HealthChecks.DynamoDb)                                 | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.DynamoDb)](https://www.nuget.org/packages/AspNetCore.HealthChecks.DynamoDb) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/dynamodb)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/dynamodb)
| Elasticsearch              | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Elasticsearch)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Elasticsearch)                       | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Elasticsearch)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Elasticsearch) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/elasticsearch)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/elasticsearch)
| EventStore                 | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.EventStore)](https://www.nuget.org/packages/AspNetCore.HealthChecks.EventStore)                             | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.EventStore)](https://www.nuget.org/packages/AspNetCore.HealthChecks.EventStore) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/eventstore)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/eventstore) | [TCP EventStore](https://github.com/EventStore/EventStoreDB-Client-Dotnet-Legacy)
| EventStore gRPC            | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.EventStore.gRPC)](https://www.nuget.org/packages/AspNetCore.HealthChecks.EventStore.gRPC)                   | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.EventStore.gRPC)](https://www.nuget.org/packages/AspNetCore.HealthChecks.EventStore.gRPC) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/eventstore)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/eventstore) | [gRPC EventStore](https://github.com/EventStore/EventStore-Client-Dotnet)
| Google Cloud Firestore     | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Gcp.CloudFirestore)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Gcp.CloudFirestore)             | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Gcp.CloudFirestore)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Gcp.CloudFirestore) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/cloudfirestore)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/cloudfirestore)
| Gremlin                    | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Gremlin)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Gremlin)                                   | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Gremlin)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Gremlin) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/gremlin)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/gremlin)
| Hangfire                   | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Hangfire)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Hangfire)                                 | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Hangfire)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Hangfire) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/hangfire)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/hangfire)
| IbmMQ                      | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.IbmMQ)](https://www.nuget.org/packages/AspNetCore.HealthChecks.IbmMQ)                                       | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.IbmMQ)](https://www.nuget.org/packages/AspNetCore.HealthChecks.IbmMQ) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/ibmmq)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/ibmmq)
| InfluxDB                   | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.InfluxDB)](https://www.nuget.org/packages/AspNetCore.HealthChecks.InfluxDB)                                 | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.InfluxDB)](https://www.nuget.org/packages/AspNetCore.HealthChecks.InfluxDB) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/influxdb)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/influxdb)
| Kafka                      | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Kafka)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Kafka)                                       | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Kafka)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Kafka) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/kafka)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/kafka)
| Kubernetes                 | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Kubernetes)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Kubernetes)                             | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Kubernetes)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Kubernetes) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/kubernetes)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/kubernetes)
| MongoDB                    | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.MongoDb)](https://www.nuget.org/packages/AspNetCore.HealthChecks.MongoDb)                                   | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.MongoDb)](https://www.nuget.org/packages/AspNetCore.HealthChecks.MongoDb) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/mongodb)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/mongodb)
| MySql                      | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.MySql)](https://www.nuget.org/packages/AspNetCore.HealthChecks.MySql)                                       | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.MySql)](https://www.nuget.org/packages/AspNetCore.HealthChecks.MySql) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/mysql)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/mysql)
| Nats                       | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Nats)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Nats)                                         | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Nats)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Nats) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/nats)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/nats) | NATS, messaging, message-bus, pubsub                                   |
| Network                    | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Network)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Network)                                   | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Network)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Network) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/network)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/network) | Ftp, SFtp, Dns, Tcp port, Smtp, Imap, Ssl                              |
| Postgres                   | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.NpgSql)](https://www.nuget.org/packages/AspNetCore.HealthChecks.NpgSql)                                     | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.NpgSql)](https://www.nuget.org/packages/AspNetCore.HealthChecks.NpgSql) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/npgsql)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/npgsql)
| Pulsar                      | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Pulsar)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Pulsar)                                       | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Pulsar)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Pulsar) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/Pulsar)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/pulsar)
| Identity Server            | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.OpenIdConnectServer)](https://www.nuget.org/packages/AspNetCore.HealthChecks.OpenIdConnectServer)           | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.OpenIdConnectServer)](https://www.nuget.org/packages/AspNetCore.HealthChecks.OpenIdConnectServer) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/openidconnect)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/openidconnect)
| Oracle                     | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Oracle)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Oracle)                                     | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Oracle)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Oracle) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/oracle)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/oracle)
| RabbitMQ                   | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.RabbitMQ)](https://www.nuget.org/packages/AspNetCore.HealthChecks.RabbitMQ)                                 | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.RabbitMQ)](https://www.nuget.org/packages/AspNetCore.HealthChecks.RabbitMQ) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/rabbitmq)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/rabbitmq)
| RavenDB                    | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.RavenDB)](https://www.nuget.org/packages/AspNetCore.HealthChecks.RavenDB)                                   | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.RavenDB)](https://www.nuget.org/packages/AspNetCore.HealthChecks.RavenDB) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/ravendb)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/ravendb)
| Redis                      | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Redis)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Redis)                                       | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Redis)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Redis) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/redis)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/redis)
| SendGrid                   | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.SendGrid)](https://www.nuget.org/packages/AspNetCore.HealthChecks.SendGrid)                                 | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.SendGrid)](https://www.nuget.org/packages/AspNetCore.HealthChecks.SendGrid) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/sendgrid)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/sendgrid)
| SignalR                    | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.SignalR)](https://www.nuget.org/packages/AspNetCore.HealthChecks.SignalR)                                   | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.SignalR)](https://www.nuget.org/packages/AspNetCore.HealthChecks.SignalR) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/signalr)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/signalr)
| Solr                       | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Solr)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Solr)                                         | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Solr)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Solr) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/solr)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/solr)
| Sqlite                     | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Sqlite)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Sqlite)                                     | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Sqlite)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Sqlite) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/sqlite)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/sqlite)
| Sql Server                 | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.SqlServer)](https://www.nuget.org/packages/AspNetCore.HealthChecks.SqlServer)                               | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.SqlServer)](https://www.nuget.org/packages/AspNetCore.HealthChecks.SqlServer) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/sqlserver)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/sqlserver)
| System                     | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.System)](https://www.nuget.org/packages/AspNetCore.HealthChecks.System)                                     | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.System)](https://www.nuget.org/packages/AspNetCore.HealthChecks.System) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/system)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/system) | Disk Storage, Folder, File, Private Memory, Virtual Memory, Process, Windows Service |
| Uris                       | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Uris)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Uris)                                         | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Uris)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Uris) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/uris)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/uris) | Single uri and uri groups                                              |

> We support netcoreapp 2.2, 3.0 and 3.1. Please use package versions 2.2.X, 3.0.X and 3.1.X to target different versions.

```PowerShell
Install-Package AspNetCore.HealthChecks.ApplicationStatus
Install-Package AspNetCore.HealthChecks.ArangoDb
Install-Package AspNetCore.HealthChecks.Aws.S3
Install-Package AspNetCore.HealthChecks.Aws.SecretsManager
Install-Package AspNetCore.HealthChecks.Aws.Sns
Install-Package AspNetCore.HealthChecks.Aws.Sqs
Install-Package AspNetCore.HealthChecks.Aws.SystemsManager
Install-Package AspNetCore.HealthChecks.Azure.Data.Tables
Install-Package AspNetCore.HealthChecks.Azure.IoTHub
Install-Package AspNetCore.HealthChecks.Azure.KeyVault.Secrets
Install-Package AspNetCore.HealthChecks.Azure.Messaging.EventHubs
Install-Package AspNetCore.HealthChecks.Azure.Storage.Blobs
Install-Package AspNetCore.HealthChecks.Azure.Storage.Files.Shares
Install-Package AspNetCore.HealthChecks.Azure.Storage.Queues
Install-Package AspNetCore.HealthChecks.AzureApplicationInsights
Install-Package AspNetCore.HealthChecks.AzureDigitalTwin
Install-Package AspNetCore.HealthChecks.AzureKeyVault
Install-Package AspNetCore.HealthChecks.AzureSearch
Install-Package AspNetCore.HealthChecks.AzureServiceBus
Install-Package AspNetCore.HealthChecks.AzureStorage
Install-Package AspNetCore.HealthChecks.Consul
Install-Package AspNetCore.HealthChecks.CosmosDb
Install-Package AspNetCore.HealthChecks.Dapr
Install-Package AspNetCore.HealthChecks.DocumentDb
Install-Package AspNetCore.HealthChecks.DynamoDB
Install-Package AspNetCore.HealthChecks.Elasticsearch
Install-Package AspNetCore.HealthChecks.EventStore
Install-Package AspNetCore.HealthChecks.EventStore.gRPC
Install-Package AspNetCore.HealthChecks.Gcp.CloudFirestore
Install-Package AspNetCore.HealthChecks.Gremlin
Install-Package AspNetCore.HealthChecks.Hangfire
Install-Package AspNetCore.HealthChecks.IbmMQ
Install-Package AspNetCore.HealthChecks.InfluxDB
Install-Package AspNetCore.HealthChecks.Kafka
Install-Package AspNetCore.HealthChecks.Kubernetes
Install-Package AspNetCore.HealthChecks.MongoDb
Install-Package AspNetCore.HealthChecks.MySql
Install-Package AspNetCore.HealthChecks.Nats
Install-Package AspNetCore.HealthChecks.Network
Install-Package AspNetCore.HealthChecks.Npgsql
Install-Package AspNetCore.HealthChecks.Pulsar
Install-Package AspNetCore.HealthChecks.OpenIdConnectServer
Install-Package AspNetCore.HealthChecks.Oracle
Install-Package AspNetCore.HealthChecks.RabbitMQ
Install-Package AspNetCore.HealthChecks.RavenDB
Install-Package AspNetCore.HealthChecks.Redis
Install-Package AspNetCore.HealthChecks.SendGrid
Install-Package AspNetCore.HealthChecks.SignalR
Install-Package AspNetCore.HealthChecks.Solr
Install-Package AspNetCore.HealthChecks.SqLite
Install-Package AspNetCore.HealthChecks.SqlServer
Install-Package AspNetCore.HealthChecks.System
Install-Package AspNetCore.HealthChecks.Uris
```

Once the package is installed you can add the HealthCheck using the **AddXXX** `IServiceCollection` extension methods.

> We use [MyGet](https://www.myget.org/F/xabaril/api/v3/index.json) feed for preview versions of HealthChecks packages.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks()
        .AddSqlServer(Configuration["Data:ConnectionStrings:Sql"])
        .AddRedis(Configuration["Data:ConnectionStrings:Redis"]);
}
```

Each HealthCheck registration supports also name, tags, failure status and other optional parameters.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHealthChecks()
        .AddSqlServer(
            connectionString: Configuration["Data:ConnectionStrings:Sql"],
            healthQuery: "SELECT 1;",
            name: "sql",
            failureStatus: HealthStatus.Degraded,
            tags: new string[] { "db", "sql", "sqlserver" });
}
```

## HealthCheck push results

HealthChecks include a _push model_ to send HealthCheckReport results into configured consumers.
The project **AspNetCore.HealthChecks.Publisher.ApplicationInsights**, **AspNetCore.HealthChecks.Publisher.Datadog**,
**AspNetCore.HealthChecks.Publisher.Prometheus**, **AspNetCore.HealthChecks.Publisher.Seq** or
**AspNetCore.HealthChecks.Publisher.CloudWatch** define a consumers to send report results to
Application Insights, Datadog, Prometheus, Seq or CloudWatch.

| Package              | Downloads                                                                                                                                                                               | NuGet Latest | Issues | Notes |
| -------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------ | ------ | ----- |
| Application Insights | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Publisher.ApplicationInsights)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Publisher.ApplicationInsights) | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Publisher.ApplicationInsights)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Publisher.ApplicationInsights) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/applicationinsights)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/applicationinsights)
| CloudWatch           | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Publisher.CloudWatch)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Publisher.CloudWatch)                   | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Publisher.CloudWatch)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Publisher.CloudWatch) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/cloudwatch)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/cloudwatch)
| Datadog              | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Publisher.Datadog)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Publisher.Datadog)                         | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Publisher.Datadog)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Publisher.Datadog) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/datadog)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/datadog)
| Prometheus Gateway   | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Publisher.Prometheus)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Publisher.Prometheus)                   | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Publisher.Prometheus)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Publisher.Prometheus) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/prometheus)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/prometheus) | **DEPRECATED** |
| Seq                  | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.Publisher.Seq)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Publisher.Seq)                                 | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.Publisher.Seq)](https://www.nuget.org/packages/AspNetCore.HealthChecks.Publisher.Seq) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/seq)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/seq)

Include the package in your project:

```powershell
install-package AspNetcore.HealthChecks.Publisher.ApplicationInsights
install-package AspNetcore.HealthChecks.Publisher.CloudWatch
install-package AspNetcore.HealthChecks.Publisher.Datadog
install-package AspNetcore.HealthChecks.Publisher.Prometheus
install-package AspNetcore.HealthChecks.Publisher.Seq
```

Add publisher[s] into the `IHealthCheckBuilder`:

```csharp
services
    .AddHealthChecks()
    .AddSqlServer(connectionString: Configuration["Data:ConnectionStrings:Sample"])
    .AddCheck<RandomHealthCheck>("random")
    .AddApplicationInsightsPublisher()
    .AddCloudWatchPublisher()
    .AddDatadogPublisher("myservice.healthchecks")
    .AddPrometheusGatewayPublisher();
```

## HealthChecks Prometheus Exporter

If you need an endpoint to consume from prometheus instead of using Prometheus Gateway you could install **AspNetCore.HealthChecks.Prometheus.Metrics**.

```powershell
install-package AspNetCore.HealthChecks.Prometheus.Metrics
```

Use the `ApplicationBuilder` extension method to add the endpoint with the metrics:

```csharp
// default endpoint: /healthmetrics
app.UseHealthChecksPrometheusExporter();

// You could customize the endpoint
app.UseHealthChecksPrometheusExporter("/my-health-metrics");

// Customize HTTP status code returned(prometheus will not read health metrics when a default HTTP 503 is returned)
app.UseHealthChecksPrometheusExporter("/my-health-metrics", options => options.ResultStatusCodes[HealthStatus.Unhealthy] = (int)HttpStatusCode.OK);
```

## HealthCheckUI

![HealthChecksUI](./doc/images/ui-home.png)

[UI Changelog](./doc/ui-changelog.md)

The project HealthChecks.UI is a minimal UI interface that stores and shows the health checks results from the configured HealthChecks URIs.

For UI, we provide the following packages:

| Package   | Downloads                                                                                                                                       | NuGet Latest | Issues | Notes |
| --------- | ----------------------------------------------------------------------------------------------------------------------------------------------- | ------------ | ------ | ------|
| UI        | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.UI)](https://www.nuget.org/packages/AspNetCore.HealthChecks.UI)               | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.UI)](https://www.nuget.org/packages/AspNetCore.HealthChecks.UI) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/ui)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/ui) | ASP.NET Core UI viewer of ASP.NET Core HealthChecks |
| UI.Client | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.UI.Client)](https://www.nuget.org/packages/AspNetCore.HealthChecks.UI.Client) | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.UI.Client)](https://www.nuget.org/packages/AspNetCore.HealthChecks.UI.Client) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/ui)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/ui) | Mandatory abstractions to work with HealthChecks.UI |
| UI.Core   | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.UI.Core)](https://www.nuget.org/packages/AspNetCore.HealthChecks.UI.Core)     | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.UI.Core)](https://www.nuget.org/packages/AspNetCore.HealthChecks.UI.Core) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/ui)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/ui) | |
| UI.Data   | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.UI.Data)](https://www.nuget.org/packages/AspNetCore.HealthChecks.UI.Data)     | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.UI.Data)](https://www.nuget.org/packages/AspNetCore.HealthChecks.UI.Data) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/ui)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/ui) | Data models and database context definition |

To integrate HealthChecks.UI in your project you just need to add the HealthChecks.UI services and middlewares available in the package: **AspNetCore.HealthChecks.UI**

```csharp
using HealthChecks.UI.Core;
using HealthChecks.UI.InMemory.Storage;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddHealthChecksUI()
            .AddInMemoryStorage();
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        app
            .UseRouting()
            .UseEndpoints(config => config.MapHealthChecksUI());
    }
}
```

This automatically registers a new interface on **/healthchecks-ui** where the SPA will be served.

> Optionally, `MapHealthChecksUI` can be configured to serve its health API, webhooks API and the front-end resources in
> different endpoints using the `MapHealthChecksUI(setup => { })` method overload. The default configured URLs for these endpoints
> can be found [here](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/blob/master/src/HealthChecks.UI/Configuration/Options.cs)

**Important note:** It is important to understand that the API endpoint that the UI serves is used by the frontend SPA to receive the result
of all processed checks. The health reports are collected by a background hosted service and the API endpoint served at /healthchecks-api by
default is the URL that the SPA queries.

Do not confuse this UI API endpoint with the endpoints we have to configure to declare the target APIs to be checked on the UI project in
the [appsettings HealthChecks configuration section](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/blob/master/samples/HealthChecks.UI.Sample/appsettings.json)

When we target applications to be tested and shown on the UI interface, those endpoints have to register the `UIResponseWriter` that is present
on the **AspNetCore.HealthChecks.UI.Client** as their [ResponseWriter in the HealthChecksOptions](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/blob/master/samples/HealthChecks.Sample/Startup.cs#L48) when configuring MapHealthChecks method.

### UI Polling interval

You can configure the polling interval in seconds for the UI inside the setup method. Default value is 10 seconds:

```csharp
.AddHealthChecksUI(setupSettings: setup =>
{
    setup.SetEvaluationTimeInSeconds(5); // Configures the UI to poll for healthchecks updates every 5 seconds
});
```

### UI API max active requests

You can configure max active requests to the HealthChecks UI backend api using the setup method. Default value is 3 active requests:

```csharp
.AddHealthChecksUI(setupSettings: setup =>
{
    setup.SetApiMaxActiveRequests(1);
    //Only one active request will be executed at a time.
    //All the excedent requests will result in 429 (Too many requests)
});
```

### UI Storage Providers

HealthChecks UI offers several storage providers, available as different nuget packages.

The current supported databases are:

| Package    | Downloads                                                                                                                                                               | NuGet Latest | Issues | Notes |
| ---------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------ | ------ | ----- |
| InMemory   | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.UI.InMemory.Storage)](https://www.nuget.org/packages/AspNetCore.HealthChecks.UI.InMemory.Storage)     | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.UI.InMemory.Storage)](https://www.nuget.org/packages/AspNetCore.HealthChecks.UI.InMemory.Storage) |
| SqlServer  | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.UI.SqlServer.Storage)](https://www.nuget.org/packages/AspNetCore.HealthChecks.UI.SqlServer.Storage)   | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.UI.SqlServer.Storage)](https://www.nuget.org/packages/AspNetCore.HealthChecks.UI.SqlServer.Storage) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/sqlserver)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/sqlserver)
| SQLite     | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.UI.SQLite.Storage)](https://www.nuget.org/packages/AspNetCore.HealthChecks.UI.SQLite.Storage)         | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.UI.SQLite.Storage)](https://www.nuget.org/packages/AspNetCore.HealthChecks.UI.SQLite.Storage) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/sqlite)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/sqlite)
| PostgreSQL | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.UI.PostgreSQL.Storage)](https://www.nuget.org/packages/AspNetCore.HealthChecks.UI.PostgreSQL.Storage) | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.UI.PostgreSQL.Storage)](https://www.nuget.org/packages/AspNetCore.HealthChecks.UI.PostgreSQL.Storage) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/npgsql)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/npgsql)
| MySql      | [![Nuget](https://img.shields.io/nuget/dt/AspNetCore.HealthChecks.UI.MySql.Storage)](https://www.nuget.org/packages/AspNetCore.HealthChecks.UI.MySql.Storage)           | [![Nuget](https://img.shields.io/nuget/v/AspNetCore.HealthChecks.UI.MySql.Storage)](https://www.nuget.org/packages/AspNetCore.HealthChecks.UI.MySql.Storage) | [![view](https://img.shields.io/github/issues/Xabaril/AspNetCore.Diagnostics.HealthChecks/mysql)](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/mysql)

All the storage providers are extensions of `HealthChecksUIBuilder`:

**InMemory**

```csharp
services
    .AddHealthChecksUI()
    .AddInMemoryStorage();
```

**Sql Server**

```csharp
services
    .AddHealthChecksUI()
    .AddSqlServerStorage("connectionString");
```

**Postgre SQL**

```csharp
services
    .AddHealthChecksUI()
    .AddPostgreSqlStorage("connectionString");
```

**MySql**

```csharp
services
    .AddHealthChecksUI()
    .AddMySqlStorage("connectionString");
```

**Sqlite**

```csharp
services
    .AddHealthChecksUI()
    .AddSqliteStorage($"Data Source=sqlite.db");
```

### UI Database Migrations

**Database Migrations** are enabled by default, if you need to disable migrations you can use the `AddHealthChecksUI` setup:

```csharp
services
    .AddHealthChecksUI(setup => setup.DisableDatabaseMigrations())
    .AddInMemoryStorage();
```

Or you can use `IConfiguration` providers, like json file or environment variables:

```json
"HealthChecksUI": {
  "DisableMigrations": true
}
```

### Health status history timeline

By clicking details button in the healthcheck row, you can preview the health status history timeline:

![Timeline](./doc/images/timeline.png)

**Note**: HealthChecks UI saves an execution history entry in the database whenever a HealthCheck status changes from Healthy to Unhealthy and viceversa.

This information is displayed in the status history timeline, but we do not perform purge or cleanup tasks in users' databases.
In order to limit the maximum history entries that are sent by the UI API middleware to the frontend, you can do a database cleanup or set the maximum history entries served by endpoint using:

```csharp
services.AddHealthChecksUI(setup =>
{
    // Set the maximum history entries by endpoint that will be served by the UI api middleware
    setup.MaximumHistoryEntriesPerEndpoint(50);
});
```

**HealthChecksUI** is also available as a _docker image_ You can read more about [HealthChecks UI Docker image](./doc/ui-docker.md).

### Configuration

By default, HealthChecks return a simple Status Code (200 or 503) without the HealthReport data. If you want the
HealthCheck-UI to show the HealthReport data from your HealthCheck, you can enable it by adding a specific `ResponseWriter`.

```csharp
app
    .UseRouting()
    .UseEndpoints(config =>
    {
        config.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
    });
```

> _WriteHealthCheckUIResponse_ is defined on HealthChecks.UI.Client nuget package.

To show these HealthChecks in HealthCheck-UI, they have to be configured through the **HealthCheck-UI** settings.

You can configure these Healthchecks and webhooks by using `IConfiguration` providers (appsettings, user secrets, env variables) or the `AddHealthChecksUI(setupSettings: setup => { })` method can be used too.

#### Sample 2: Configuration using appsettings.json

```json
{
  "HealthChecksUI": {
    "HealthChecks": [
      {
        "Name": "HTTP-Api-Basic",
        "Uri": "http://localhost:6457/healthz"
      }
    ],
    "Webhooks": [
      {
        "Name": "",
        "Uri": "",
        "Payload": "",
        "RestoredPayload": ""
      }
    ],
    "EvaluationTimeInSeconds": 10,
    "MinimumSecondsBetweenFailureNotifications": 60
  }
}
```

#### Sample 2: Configuration using setupSettings method:

```csharp
services
    .AddHealthChecksUI(setupSettings: setup =>
    {
       setup.AddHealthCheckEndpoint("endpoint1", "http://localhost:8001/healthz");
       setup.AddHealthCheckEndpoint("endpoint2", "http://remoteendpoint:9000/healthz");
       setup.AddWebhookNotification("webhook1", uri: "http://httpbin.org/status/200?code=ax3rt56s", payload: "{...}");
    })
    .AddSqlServer("connectionString");
```

**Note**: The previous configuration section was HealthChecks-UI, but due to incompatibilies with Azure Web App environment variables, the section has been moved to HealthChecksUI. The UI is retro compatible and it will check the new section first, and fallback to the old section if the new section has not been declared.

    1.- HealthChecks: The collection of health checks uris to evaluate.
    2.- EvaluationTimeInSeconds: Number of elapsed seconds between health checks.
    3.- Webhooks: If any health check returns a *Failure* result, this collections will be used to notify the error status. (Payload is the json payload and must be escaped. For more information see the notifications documentation section)
    4.- MinimumSecondsBetweenFailureNotifications: The minimum seconds between failure notifications to avoid receiver flooding.

```json
{
  "HealthChecksUI": {
    "HealthChecks": [
      {
        "Name": "HTTP-Api-Basic",
        "Uri": "http://localhost:6457/healthz"
      }
    ],
    "Webhooks": [
      {
        "Name": "",
        "Uri": "",
        "Payload": "",
        "RestoredPayload": ""
      }
    ],
    "EvaluationTimeInSeconds": 10,
    "MinimumSecondsBetweenFailureNotifications": 60
  }
}
```

### Using relative URLs in Health Checks and Webhooks configurations (UI 3.0.5 onwards)

If you are configuring the UI in the same process where the HealthChecks and Webhooks are listening, from version 3.0.5 onwards the UI can use relative URLs,
and it will automatically discover the listening endpoints by using server `IServerAddressesFeature`.

Sample:

```csharp
//Configuration sample with relative URL health checks and webhooks
services
    .AddHealthChecksUI(setupSettings: setup =>
    {
       setup.AddHealthCheckEndpoint("endpoint1", "/health-databases");
       setup.AddHealthCheckEndpoint("endpoint2", "health-messagebrokers");
       setup.AddWebhookNotification("webhook1", uri: "/notify", payload: "{...}");
    })
    .AddSqlServer("connectionString");
```

You can also use relative URLs when using `IConfiguration` providers like appsettings.json.

### Webhooks and Failure Notifications

If the **WebHooks** section is configured, HealthCheck-UI automatically posts a new notification into the webhook collection. HealthCheckUI uses a simple replace method for values in the webhook's **Payload** and **RestorePayload** properties. At this moment we support two bookmarks:

[[LIVENESS]] The name of the liveness that returns _Down_.

[[FAILURE]] A detail message with the failure.

[[DESCRIPTIONS]] Failure descriptions

Webhooks can be configured with configuration providers and also by code. Using code allows greater customization as you can setup you own user functions to customize output messages or configuring if a payload should be sent to a given webhook endpoint.

The [web hooks section](./doc/webhooks.md) contains more information and webhooks samples for Microsoft Teams, Azure Functions, Slack and more.

**Avoid Fail notification spam**

To prevent you from receiving several failure notifications from your application, a configuration was created to meet this scenario.

```csharp
services.AddHealthChecksUI(setup =>
{
    setup.SetNotifyUnHealthyOneTimeUntilChange(); // You will only receive one failure notification until the status changes.
});
```

## UI Style and branding customization

### Sample of dotnet styled UI

![HealthChecksUIBranding](./doc/images/ui-branding.png)

Since version 2.2.34, UI supports custom styles and branding by using a **custom style sheet** and **css variables**.
To add your custom styles sheet, use the UI setup method:

```csharp
app
    .UseRouting()
    .UseEndpoints(config =>
    {
        config.MapHealthChecksUI(setup =>
        {
            setup.AddCustomStylesheet("dotnet.css");
        });
    });
```

You can visit the section [custom styles and branding](./doc/styles-branding.md) to find source samples and get further information about custom css properties.

## UI Configure HttpClient and HttpMessageHandler for Api and Webhooks endpoints

If you need to configure a proxy, or set an authentication header, the UI allows you to configure the `HttpMessageHandler` and the `HttpClient` for the webhooks and healtheck api endpoints. You can also register custom delegating handlers for the API and WebHooks HTTP clients.

```csharp
services.AddHealthChecksUI(setupSettings: setup =>
{
    setup.ConfigureApiEndpointHttpclient((sp, client) =>
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "supertoken");
    })
    .UseApiEndpointHttpMessageHandler(sp =>
    {
        return new HttpClientHandler
        {
            Proxy = new WebProxy("http://proxy:8080")
        };
    })
    .UseApiEndpointDelegatingHandler<CustomDelegatingHandler>()
    .ConfigureWebhooksEndpointHttpclient((sp, client) =>
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "sampletoken");
    })
    .UseWebhookEndpointHttpMessageHandler(sp =>
    {
        return new HttpClientHandler()
        {
            Properties =
            {
                ["prop"] = "value"
            }
        };
    })
    .UseWebHooksEndpointDelegatingHandler<CustomDelegatingHandler2>();
})
.AddInMemoryStorage();
```

## UI Kubernetes Operator

If you are running your workloads in kubernetes, you can benefit from it and have your healthchecks environment ready and monitoring in seconds.

You can get for information in our [HealthChecks Operator docs](./doc/k8s-operator.md)

## UI Kubernetes automatic services discovery

<!-- ![k8s-discovery](./doc/images/k8s-discovery-service.png) -->

HealthChecks UI supports automatic discovery of k8s services exposing pods that have health checks endpoints. This means, you can benefit from it and avoid registering all the endpoints you want to check and let the UI discover them using the k8s api.

You can get more information [here](./doc/k8s-ui-discovery.md)

## HealthChecks as Release Gates for Azure DevOps Pipelines

HealthChecks can be used as [Release Gates for Azure DevOps](https://docs.microsoft.com/en-us/azure/devops/pipelines/release/approvals/gates?view=azure-devops) using this [Visual Studio Market place Extension](https://marketplace.visualstudio.com/items?itemName=luisfraile.vss-services-aspnetcorehealthcheck-extensions).

Check this [README](./extensions/README.md) on how to configure it.

## Protected HealthChecks.UI with OpendId Connect

There are some scenarios where you can find useful to restrict access for users on HealthChecks UI, maybe for users who belong to some role, based on some claim value etc.

We can leverage the ASP.NET Core Authentication/Authorization features to easily implement it. You can see a fully functional example using IdentityServer4 [here](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/tree/master/samples/HealthChecks.UI.Oidc) but you can use Azure AD, Auth0, Okta, etc.

Check this [README](./extensions/README.md) on how to configure it.

## Tutorials, demos and walkthroughs on ASP.NET Core HealthChecks

- [ASP.NET Core HealthChecks and Kubernetes Liveness / Readiness by Carlos Landeras](./doc/kubernetes-liveness.md)
- [ASP.NET Core HealthChecks, BeatPulse UI, Webhooks and Kubernetes Liveness / Readiness probes demos at SDN.nl live WebCast by Carlos Landeras](https://www.youtube.com/watch?v=kzRKGCmGbqo)
- [ASP.NET Core HealthChecks features video by @condrong](https://t.co/YriQ6cLWVm)
- [How to set up ASP.NET Core 2.2 Health Checks with BeatPulse's AspNetCore.Diagnostics.HealthChecks by Scott Hanselman](https://www.hanselman.com/blog/HowToSetUpASPNETCore22HealthChecksWithBeatPulsesAspNetCoreDiagnosticsHealthChecks.aspx)
- [ASP.NET Core HealthChecks announcement](https://t.co/47M9FBfpWF)
- [ASP.NET Core 2.2 HealthChecks Explained by Thomas Ardal](https://blog.elmah.io/asp-net-core-2-2-health-checks-explained/)
- [Health Monitoring on ASP.NET Core 2.2 / eShopOnContainers](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/monitor-app-health)

## Contributing

AspNetCore.Diagnostics.HealthChecks wouldn't be possible without the time and effort of its contributors.
The team is made up of Unai Zorrilla Castro [@unaizorrilla](https://github.com/unaizorrilla),
Luis Ruiz Pavón [@lurumad](https://github.com/lurumad), Carlos Landeras [@carloslanderas](https://github.com/carloslanderas),
Eduard Tomás [@eiximenis](https://github.com/eiximenis), Eva Crespo [@evacrespob](https://github.com/evacrespob) and
Ivan Maximov [@sungam3r](https://github.com/sungam3r).

Thanks to all the people who already contributed!

<a href="https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/graphs/contributors">
  <img src="https://contributors-img.web.app/image?repo=Xabaril/AspNetCore.Diagnostics.HealthChecks" />
</a>

If you want to contribute to the project and make it better, your help is very welcome.
You can contribute with helpful bug reports, features requests, submitting new features with pull requests and also
answering [questions](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/question).

1. Read and follow the [Don't push your pull requests](https://www.igvita.com/2011/12/19/dont-push-your-pull-requests/)
2. Follow the code guidelines and conventions.
3. New features are not only code, tests and documentation are also mandatory.
4. PRs with [`Ups for grabs`](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/Ups%20for%20grabs)
and [help wanted](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/labels/help%20wanted) tags are good candidates to contribute.
