using Google.Cloud.Firestore;

namespace HealthChecks.Gcp.CloudFirestore
{
    /// <summary>
    /// Represent options for CloudFirestore.
    /// </summary>
    public class CloudFirestoreOptions
    {
        /// <summary>
        /// Firestore Cloud database object used in your application.
        /// </summary>
        public FirestoreDb FirestoreDatabase { get; set; }

        /// <summary>
        /// Specify the root collections that needs to exist in the database.
        /// </summary>
        public string[] RequiredCollections { get; set; }
    }
}
