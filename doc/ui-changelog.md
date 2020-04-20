**3.1.1**

- [Breaking Change] The UI no longer uses sqlite internally. A storage provider must be configured using HealthChecksUIBuilder storage package extensions.
- Added new storage providers (SqlServer, InMemory, Sqlite, MySql and PostreSQL)
- Added new storage providers [configuration environment variables for docker image](./ui-docker.md)
- UI now reacts to services updates (health checks path / scheme) and removals from k8s operator
- Bugfix - The UI now refreshes correctly when the k8s operator removes last endpoint

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
