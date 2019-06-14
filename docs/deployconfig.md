Piraeus Deployment and Configuration
====================================

The Piraeus deployment is performed using Powershell Core, i.e., Powershell v6.2
or greater.

1.  Open a Visual Studio command prompt and navigate to the /kubernetes folder.

2.  Type pwsh to open Powershell Core

3.  Type . ./piraeusdeploy.ps1 to load the Powershell deployment script for
    Piraeus.

4.  Edit and save the deploy.json file in the /kubernetes folder (see below file
    parameters and descriptions).

5.  Deploy Piraeus by typing New-PiraeusDeploy -File “deploy.json”

>   Notes:

1.  You can use Powershell script, “randonsymmetrickey.ps1”, to generate random
    256-bit base64 encoded keys. Generate 2 of these keys, 1 or the
    “apiSymmetricKey” value and another for the “symmetricKey” value in the
    deploy.json file.

2.  You can use the Powershell script, “randomapikey.ps1” to generate a random
    Piraeus Management API key to use in the “apiSecurityCodes” value in the
    deploy.json file.

>   Below is a list of the parameters and definitions for the
>   /Piraeus/kubernetes/deploy.json.

| Parameter            | Description                                                                                                                                                                                                                                                                                                           |
|----------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| subscriptionNameOrId | The subscription name or guid identifier where Piraeus is to be deployed.                                                                                                                                                                                                                                             |
| resourceGroupName    | The name of the resource group where the Piraeus is to be deployed. The script will create the Resource Group if it does not already exist.                                                                                                                                                                           |
| location             | The Azure region, e.g., “eastus”, where the deployment will be made.                                                                                                                                                                                                                                                  |
| dnsName              | The DNS name for the Piraeus deployment. The FQDN will be \<dnsName\>.\<location\>.cloudapp.azure.com                                                                                                                                                                                                                 |
| email                | Your email address. This will be used by Let’s Encrypt to warning you of expiring certificates.                                                                                                                                                                                                                       |
| storageAcctName      | The name of the storage account used to maintain cluster info, metrics, and grain state for Orleans. If the storage account does not exist it will be created. An addition storage account will be created with the word “audit” appended. This storage account can be used by Piraeus for user and message auditing. |
| appId                | The application ID of a previously created Service Principal that can be used for the deployment. If the appId is null or empty, a new Service Principal will be created for the deployment.                                                                                                                          |
| pwd                  | The password of a previously created Service Principal that can be used for the deployment. If the pwd is null or empty, a new Service Principal will be created for the deployment.                                                                                                                                  |
| clusterName          | The name of the Piraeus AKS cluster.                                                                                                                                                                                                                                                                                  |
| nodeCount            | The number of nodes (VMs) created by in each node pool (there are 2 node pools) in the Piraeus cluster.                                                                                                                                                                                                               |
| apiIssuer            | The issuer URI, e.g., “http://skunklab.io/mgmt”, used in creating the security token used by the Piraeus Management API.                                                                                                                                                                                              |
| apiAudience          | The audience URI, e.g., “http://skunklab.io/mgmt”, used in creating the security token used by the Piraeus Management API.                                                                                                                                                                                            |
| apiSymmetricKey      | A base64 encoded 256-bit (32-byte) symmetric key used to sign and verify Piraeus Management security tokens.                                                                                                                                                                                                          |
| apiSecurityCodes     | A list of semi-colon delimited security codes used to obtain a security token for the Piraeus Management API. The codes should be at least 8 characters and composed of randomly ordered upper and lower alphabet (English-US) and numbers [0-9].                                                                     |
| identityClaimType    | The claim type that uniquely identities a user, e.g., “http://skunklab.io/name”                                                                                                                                                                                                                                       |
| issuer               | The issuer URI for a Piraeus security tokens used to connect to a gateway, e.g., “http://skunklab.io/”                                                                                                                                                                                                                |
| audience             | The audience URI for a Piraeus security token used to connect to a gateway, e.g., “http://skunklab.io/”                                                                                                                                                                                                               |
| symmetricKey         | A base64 encoded 256-bit (32-byte) symmetric key used to sign and verify Piraeus security tokens used to connect to a gateway.                                                                                                                                                                                        |
| tokenType            | The token type of the Piraeus security token (use JWT)                                                                                                                                                                                                                                                                |
| coapAuthority        | The authority used in a CoAP URI, e.g., “skunklab.io”                                                                                                                                                                                                                                                                 |
| frontendVMSize       | The Azure VM size for the nodes in the front end node pool, e.g., “Standard_D2s_v3”                                                                                                                                                                                                                                   |
| orleansVMSize        | The Azure VM size for the nodes in the Orleans cluster node pool, e.g., “Standard_D4s_v3” or larger.                                                                                                                                                                                                                  |

