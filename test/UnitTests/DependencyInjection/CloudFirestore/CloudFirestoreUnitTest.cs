using HealthChecks.Gcp.CloudFirestore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.CloudFirestore
{
    public class cloud_firestore_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("cloud firestore", typeof(CloudFirestoreHealthCheck), builder => builder.AddCloudFirestore(
                setup => setup.RequiredCollections = new string[] { }));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-cloud-firestore-group", typeof(CloudFirestoreHealthCheck), builder => builder.AddCloudFirestore(
                   setup => setup.RequiredCollections = new string[] { }, name: "my-cloud-firestore-group"));
        }
    }
}
