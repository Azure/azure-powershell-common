# Authentication Abstractions
Base abstrations library for all Azure PowerShell modules.

## Restrictions
**This library cannot have API or binary breaking changes**. It is required to be completely backward compatible, this means:
- Interfaces in this library cannot be changed.  New base functionality requires using the interface `Extensions` property, or defining a new interface
- For abstract and concrete classes, methods and properties may be added, but may not be changed or removed. 
## Target
NetStandard 2.0
## Dependencies
- Hyak.Commmon (1.0.0.0)
- Microsoft.Azure.Common (2.0.0.0)
- Microsoft.Rest.ClientRuntime (2.0.0.0)
- Newtonsoft.Json (10.0.0.0)
## Interfaces
### IAccessToken
Represents a renewable access token
### IAuthenticationFactory
Abstract factory for authentication classes compatible with .Net autorest-generated libraries.  Methods in this interface allow creating abstract self-renewing token-based credentials that could be applied to any client.  This is the primary mechanism for authenticating clients in track 2, or other non-management clients that use AAD or MSAL bearer token authentication.
### IAzureAccount
Abstract representation of a logged-in principal
### IAzureContext
Abstract representation of the default target for authentication and Azure PowerShell commands (Environment, Tenant, Account, Subscription)
### IAzureContextContainer
A dictionary of IAzureContext objects.  The default context container will contain all of the possible contexts for the logged-in identities (one context per tenant x subscription, for each logged-in principal).
### IAzureEnvironment
Abstract representation of a particular Azure Cloud.  Contains the endpoints, authentication resources (scopes), and domain name suffixes associated with a particular cloud.  There are built-in environments for the Azure public clouds, and users may create an environment for any on-premise, private, or air-gapped clouds.
### IAzureSession
### IAzureSubscription
Abstract representation of an ARM subscription.
### IAzureTenant
Abstract representation of an AAD Tenant
### IClientAction
Abstract representation of an action that can be performed on a client.  This is generally used to alter the shape or processing of all or some requests and responses processed by the client.
### IClientFactory
Abstract factory for .Net Autorest-generated clients.  This is the main entry point that cmdlets based on management clients will use to create and authenticate their clients.
### IDataStore
An abstract representation of a file store, used to abstract operations over the file system for items cached, written, or read by PowerShell cmdlets.  Used mainly to mock file operations in tests, but could also be used to store PowerShell files on some other medium (like blob storage).
### IExtensibleModel
Base interface for all model interfaces - provides a dictionary of extended properties that can be used by any model.  Extensions over this interface implemented in this library allow adding basic extension storage and lookup functions once and having them apply to all implementations of any interface in this library.  This is used as a mechanism for adding new data, or command-specific data into the metadata caches maintained by PowerShell.
### IExtensibleSettings
Base interface for serializable settings classes.  Interface allows a single extension implementation that applies to all implementing interfaces and classes.
### IFileProvider
Base abstraction for shared file data, especially used in IProfileProvider implementations- allows implementations that provide thread-safe and process-safe file access to shared files, or store files in memory.
### IHyakAuthenticationFactory
Abstract factory for authentication classes compatible with .Net Hyak generated clients
### IHyakClientAction
Abstract representation of a client configuration action for Hyak-generated clients
### IHyakClientFactory
Abstract factory for Hyak=generated clients
### IProfileProvider
Abstract collection of profile management methods, designed to be combined with an IFileProvider for any concrete implementation.  Specific implementations provide thread-safe access to shared files, or cache profiles in memory.
### IRenewableToken
An abstract type for renewable tokens that are compatible with the ITokenProvider interfaces in autorest-generated and other clients.  This is the primary mechanism for adapting authentication to Track 2 or non-autorest clients.
### IStorageContext
Abstract representation of an Azure Storage data plane target - provides a single aabstraction for data plane access regardless of authentication method, and regardless of the storage data plane version.
### IStorageContextProvider
Abstract representation of a factory for IStorageContext - allows using a single abstraction regardless of the implementation, which allows using this abstraction over multiple different versions of Azure Storage data and management plane APIs.

## Classes
### AzureAccount
Default implementation of IAzureAccount, includes extension property names for extension properties used in Azure Accounts.
### AzureContext
Default implementation of IAzureContext
### AzureEnvironment
Default implementation of IAzureEnvironment - a collection of properties that defines how to communicate with a particular Azure Cloud.
### AzurePSDataCollectionProfile
Serialization class for data collection settings
### AzureRmProfileProvider
Default ProfileProvider for AzureRM
### AzureSMProfileProvider
Default Profile provider for RDFE
### AzureSubscription
Default implementation of IAzureSubscription, including extension property names typically used with Subscriptions.
### AzureTenant
Default implementation fo IAzureTenant.
### DataCollectionController
Default implementation of client-side telemetry.
### DiskDataStore
Default implementation of IDataStore using the file system.

## Extensions
## Enumerations
### AzureModule
The kinds of AzureModule (in this case: Profile, ARM, RDFE)
### ContextSaveMode
The modes of context autosave (CurrentUser or Process)
## Static Classes
### AzureEnvironmentConstants
General constants defining the endpoints in the built-in Azure environments
### AzureSession
General constants used at runtime by common code, especially authentication
### EnvironmentName
General constants used in the built-in Azure environments
