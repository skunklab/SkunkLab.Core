Azure Blob Storage Subscription
===============================

Adds a subscription as a static route to a π-system to write information to
Azure Blob Storage. If the BlobType is “Append”, then the optional Filename
parameter must be used as it indicates the file to append. If the BlobType is
other than “Append”, the blob is written with a filename of a random GUID with
an extension consistent with the content type, e.g., .json, .txt, .xml. If the
content type is application/octet-stream, no file extension is added. The
Container field is omitted, the blob is written the root of the blob storage
account. The NumClients field has a default of 1, i.e., 1 blob storage client is
used to write files. If very high frequency data is written to many files, then
increasing NumClient \> 1, can be advantageous.

| **Parameter**     | **Optional** | **Definition**                                                                                                                      |
|-------------------|--------------|-------------------------------------------------------------------------------------------------------------------------------------|
| ServiceUrl        | N            | Url of the Piraeus Management API service, e.g., https://\<dns\>.\<location\>.cloudapp.azure.com                                    |
| SecurityToken     | N            | Security token acquired from the Management API using a security code.                                                              |
| ResourceUriString | N            | The π-system URI identifier associated with a specific event.                                                                       |
| Account           | N            | The Azure Blob storage account name.                                                                                                |
| Key               | N            | The Azure Blob storage key.                                                                                                         |
| Container         | Y            | Optional container name to store the file(s) in Azure Blob Storage.                                                                 |
| BlobType          | N            | The type of Azure Blob to be used, i.e., one of (Block, Append, Page).                                                              |
| NumClients        | Y            | The number Azure Blob clients associated with deployment. For high frequency writes multiple clients can be used. The default is 1. |
| Description       | Y            | An optional description of the subscription, which is useful if querying subscriptions for a π-system from the management API.      |
| Filename          | Y            | A filename which is an optional parameter, but used when the BlobType is “Append”, i.e., appending to a single file.                |

**Example**

\`\`\`

\$url = "http://piraeus.eastus.cloudapp.azure.com"  
\$code = "12345678"  
\$token = Get-PiraeusManagementToken -ServiceUrl \$url -Key \$code

\$piSystem = “http://skunklab.io/test/resource-a”  
\$account = “mystorageacct”  
\$container = “myfiles”  
\$filename = “myappendfile.txt”  
\$key =  
\$description = “Test Azure Blob storage subscription”

Add-PiraeusBlobStorageSubscription -ServiceUrl \$url -SecurityToken \$token
`-ResourceUriString $piSystem`  
-Account \$account `-Container $container`  
-BlobType Append \`  
-Filename \$filename

\`\`\`
