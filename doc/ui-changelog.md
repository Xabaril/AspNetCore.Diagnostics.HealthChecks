**5.0.0**

- HealthChecks packages, UI, UI Docker Image and K8s Operator updated to framework .NET 5.0
- Updated UI Docker Image Azure App Configuration legacy package and added environment variable for cache expiration (AAC_CacheExpiration)
- Operator fixes (Fix regression and add reconnection on watch error)
- Bumped third party package versions (more to come in the next release)
- Added docker compose and operator samples
- Removed OIDC integration inside the UI (A sample about how to protect the UI with OIDC has been added to samples folder)

**3.1.3**

- Fixed ui-settings middleware not respecting relative paths #642 (Thanks to @bogataj)

**3.1.2**

- Updated React and other package.json library versions
- Added Health Checks Execution tags #194
- The UI no longer has a inteval configuration button. The polling interval is configured in the backend using setup.SetEvaluationTimeInSeconds(seconds);
- Added Start / Stop polling button #597
- Added UI settings middleware
- Added Request Limiting Middleware and setup method to configure max api active requests with setup.SetApiMaxActiveRequests(value);
- Improved healthcheck table styling for better rows alignment. #607
- Added animation for execution history panel
- Fonts and icons are now embedded in the javascript bundle for users running the UI in no-internet connection environments #607
- Customize Header Text using setup.SetHeaderText(text) - Default is Health Checks Status #562
- Fixed incorrect name length in HealthCheckExecutionHistories #577
- Updated Storage Providers EF Core migrations

**3.1.1**

- [Breaking Change] The UI no longer uses sqlite internally. A storage provider must be configured using HealthChecksUIBuilder storage package extensions.
- Added new storage providers (SqlServer, InMemory, Sqlite, MySql and PostreSQL)
- Added new storage providers [configuration environment variables for docker image](./ui-docker.md)
- UI now reacts to services updates (health checks path / scheme) and removals from k8s operator
- Bugfix - The UI now refreshes correctly when the k8s operator removes last endpoint
- UI preview5 now allows updating stored configurations in startup #516
- UI preview6 changes default webhook description message and enumerates failing liveness names
- UI preview7 adds the ability to register API custom delegating handlers and webhooks message content escaping

  **3.1.0**

- Updated UI and dependencies to NetStandard 2.1

**3.0.11**

- Fix issue #449 (Kubernetes Discovery service was not considering configured namespaces)

- Extended Webhook notifications customization with new functions. They user can now configure if a payload should be notified, and customize [[FAILURE]] and [[DESCRIPTIONS]] bookmarks messages by providing custom user functions.

**3.0.10**

- Improved k8s discovery service Thanks @ggmaresca:

  - New health, scheme and port annotations and better compatibility with ipv4 / ipv6 addresses.
  - Customizable namespaces

- Added docker image push capabilities for Kubernetes Operator with a new protected endpoint

- Allow custom style sheets configuration to use absolute paths

**3.0.8**

- Fix absolute uris in Unix systems

**3.0.7**

- Expose Actions to configure UI Api and Webhooks http client

**3.0.6**

- Configurable AsidePanel initial state

**3.0.5**

- Protect all assembly resources endpoints and add Authn and Authz demo
