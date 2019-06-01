
Deploying Piraeus on Azure Kubernetes Service (AKS)
---

Introduction
============

Using AKS as a cloud deployment platform for Piraeus provides significant
benefits in terms of management, scalability, fault tolerance, and simplicity.
However, deployments to Kubernetes (K8) are necessarily simple in themselves, as
they require multiple tools to be installed, e.g., Powershell Core, Helm, Azure
CLI, and the Kubernetes CLI (kubectl). We have provided a Powershell script and
several Helm charts which are used to orchestrate the deployment process on AKS,
in the /kubernetes folder.

The script has been tested with the prereqs. However, versions of the tools used
are being updated at a rapid pace and there is no guarantee that an update may
not have a breaking change or a bug, which is already happened once. Please
enter an issue into git if this occurs.

Before, we get the details of the deploy script, we will examine the parameters
of the deployment script. To load the script, open Powershell Core and navigate
to the /kubernetes folder in the source and type . ./piraeusdeploy.ps1. You can
execute the deployment in Powershell Core by typing New-PiraeusDeploy
\<parameter-list\> (below).

| **Parameter**        | **Optional** | **Description**                                                                                                                                                                                                                                            |
|----------------------|--------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Email                | N            | Email which is tied to Let’s Encrypt publishing of certificates.                                                                                                                                                                                           |
| DnsName              | N            | DNS name of the AKS deployment in the form \<dnsname\>.\<location\>.cloudapp.azure.com                                                                                                                                                                     |
| Location             | N            | The Azure region for the deployment, e.g., “eastus”.                                                                                                                                                                                                       |
| StorageAcctName      | N            | The storage account name for the Orleans provider. Note: The storage account name with append “audit” to a 2nd storage account used for optional Piraeus auditing.                                                                                         |
| ResourceGroupName    | N            | The name of the Azure Resource Group used for the deployment.                                                                                                                                                                                              |
| SubscriptionNameOrId | N            | The subscription name or ID used for the deployment.                                                                                                                                                                                                       |
| AppId                | Y            | Optional app id used when a service principal has already been created. If app id is omitted a new service principal will be created for the deployment.                                                                                                   |
| Pwd                  | Y            | Optional password used when a service principal has already been created. If pwd is omitted a new service principal will be created for the deployment.                                                                                                    |
| ClusterName          | Y            | The name of the AKS cluster. Default is “piraeuscluster”.                                                                                                                                                                                                  |
| NodeCount            | Y            | The number of nodes in the deployment. The default is 1. Note: Even if 1 node is deployed, it can be scaled with Kubernetes later.                                                                                                                         |
| ApiIssuer            | Y            | Name of the security token issuer for the Piraeus Management API. The default is “http://skunklab.io/mgmt”.                                                                                                                                                |
| ApiAudience          | Y            | Name of the security token audience for the Piraeus Management API. The default is “http://skunklab.io/mgmt”.                                                                                                                                              |
| ApiSymmetricKey      | Y            | The base64 encode 256 bit (32 bytes) symmetric key used to sign and verify the security token used by the Piraeus Management API. The default is “//////////////////////////////////////////8=", but deployments should use their own unique key.          |
| ApiSecurityCodes     | Y            | A semi-colon delimited list of security codes. A security code is used to obtain a security token to be used on each call to the Piraeus Management API. You should used unique and secret security codes for a deployment, e.g., “awe5J9wBm4;YW4dd5Fzp43” |
| IdentityClaimType    | Y            | A claim type that uniquely identifies a caller in the security token used to connect to a Piraeus Gateway. The default is “http://skunklab.io/name”.                                                                                                       |
| Issuer               | Y            | The issuer of the security token used to connect to a Piraeus Gateway. The default is “http://skunklab.io/”.                                                                                                                                               |
| Audience             | Y            | The audience of the security token used to connect to a Piraeus Gateway. The default is “http://skunklab.io/”.                                                                                                                                             |
| SymmetricKey         | Y            | The symmetric key used to sign and verify the security token used to connect to a Piraeus Gateway. The default is “//////////////////////////////////////////8=", but deployments should use their own unique key.                                         |
| TokenType            | Y            | The type of security token used to connect to a Piraeus Gateway. The default is “JWT”. Other options are “SWT” and “X509”. We highly recommend “JWT”.                                                                                                      |
| CoapAuthority        | Y            | The CoAP authority used for CoAP protocol messages in the form (coap \| coaps)://\<authority\>/… The default is “skunklab.io”.                                                                                                                             |
| FrontendVMSize       | Y            | The VM size of the node is nodepool1, i.e., the node pool that contains the ingress controller and Piraeus Gateways. The default is Standard_D2s_v3 (2 core).                                                                                              |
| OrleansVMSize        | Y            | The VM size of the node is nodepool2, i.e., the node pool that contains Piraeus/Orleans cluster. The default is Standard_D4s_v3 (4 core).                                                                                                                  |

The following are the details of the Piraeus deployment script, which deploys

-   2 nodes in 2 node pools

-   NGINX ingress controller and Piraeus Web socket gateway in nodepool1

-   Piraeus/Orleans cluster in nodepool2

-   Using Let’s Encrypt and Cert-Manager to get production level certificate for
    encrypted channels to the ingress controller.

The following are the details of the deployment script, we may be helpful if you
need to debug a faulty deployment.

Sets the Azure subscription to be used for the deployment.

| az account set --subscription \$SubscriptionNameOrId |
|------------------------------------------------------|


Determines if the Azure Resource Group exists or not for the deployment.

| \$rgoutcome = az group exists --name \$ResourceGroupName |
|----------------------------------------------------------|


If the Resource *does not exist* if will be created an Azure Region specified by
\$Location, e.g., “eastus”.

| az group create --name \$ResourceGroupName --location \$Location |
|------------------------------------------------------------------|


Attempts to find is the AKS cluster exists by querying the cluster name.

| \$clusterLine = az aks list --query "[?contains(name, '\$ClusterName')]" --output table |
|-----------------------------------------------------------------------------------------|


If the AKS cluster already exists, it will be deleted.

| az aks delete --name \$ClusterName --resource-group \$ResourceGroupName --yes |
|-------------------------------------------------------------------------------|


If the Service Principal parameters are omitted as input into the script, i.e.,
\$AppId and \$Pwd, a new Service Principal will be created.
```

                $creds = az ad sp create-for-rbac  --skip-assignment
                $v1 = $creds[1].Replace(",","").Replace(":","=").Replace(" ","").Replace('"',"")
                $sd1 = ConvertFrom-StringData -StringData $v1
                $appId = $sd1.Values[0]
                $v2 = $creds[4].Replace(",","").Replace(":","=").Replace(" ","").Replace('"',"")
                $sd2 = ConvertFrom-StringData -StringData $v2
                $pwd = $sd2.Values[0]
```
The storage account name is checked to see if it already exists.```
```
 $saLine= az storage account check-name --name $StorageAcctName 
```


If the storage account does not exist, then a new storage account is created.
```
az storage account create --location $Location --name $StorageAcctName --resource-group \$ResourceGroupName --sku "Standard_LRS" 
```


The audit storage account name is checked to see if it already exists.
```
$auditStorageAcctName = $StorageAcctName + "audit" 
$asaLine= az storage account check-name --name $auditStorageAcctName 
```

If the audit storage account does not exist, then a new storage account is
created.
```
az storage account create --location $Location --name $auditStorageAcctName --resource-group $ResourceGroupName --sku "Standard_LRS" ```
```

Both storage account connection strings are obtained.
```
$dcs = az storage account show-connection-string --name $StorageAcctName --resource-group $ResourceGroupName 
$vs1 = $dcs.Replace(",","").Replace(":","=").Replace(" ","").Replace('"',"").Replace("{","").Replace("}","").Trim() 
$ts1 = $vs1 -split "connectionString=" 
$dataConnectionString = $ts1[2] \$adcs = az storage account show-connection-string --name $auditStorageAcctName --resource-group $ResourceGroupName 
$vsa1 = $adcs.Replace(",","").Replace(":","=").Replace(" ","").Replace('"',"").Replace("{","").Replace("}","").Trim() 
$tsa1 = $vsa1 -split "connectionString=" $auditConnectionString = $tsa1[2] 
```


The AKS cluster is created.
```
az aks create --resource-group $ResourceGroupName --name $ClusterName --node-count $NodeCount --service-principal $appId --client-secret $pwd --node-vm-size $FrontendVMSize --enable-vmss 
```


The AKS credentials are obtained
```
GetAksCredentials $ResourceGroupName $ClusterName 
```

The Helm RBAC yaml file is applied.
```
ApplyYaml "./helm-rbac.yaml" 
```

Helm is initialized with the tiller service account.
```
helm init --service-account tiller 
```

A node label is set on the current node pool for the deployment.
```
SetNodeLabel "nodepool1" "pool" "nodepool1" 
```


Validation disabled for cert-manager.
```
kubectl label namespace kube-system certmanager.k8s.io/disable-validation="true"
```

The Cert-Manager CRDs are applied.
```
ApplyYaml "https://raw.githubusercontent.com/jetstack/cert-manager/release-0.6/deploy/manifests/00-crds.yaml"
```

Cert-Manager is installed.
```
helm install --name cert-manager --namespace kube-system --set ingressShim.extraArgs='{--default-issuer-name=letsencrypt-prod,--default-issuer-kind=ClusterIssuer}' stable/cert-manager
```

The email address is updated in the issuer.yaml and applied, which is necessary
obtaining the production certificates from Let’s Encrypt.
```
Copy-Item -Path "./issuer.yaml" -Destination "./issuer-copy.yaml" 
UpdateYaml -newValue \$Email -matchString "EMAILREF" -filename "./issuer-copy.yaml" 
kubectl apply -f ./issuer-copy.yaml 
Remove-Item -Path "./issuer-copy.yaml" 
```

The NGINX ingress controller is added.
```
helm install stable/nginx-ingress --namespace kube-system --set controller.replicaCount=1 
```

The NGINX ingress controller’s external IP is acquired.
```
$IP = GetExternalIP 
```

The Public IP ID is acquired for use in setting azure networking.
```
$PUBLICIPID=$(az network public-ip list --query "[?ipAddress!=null]\|[?contains(ipAddress, '$IP')].[id]" --output tsv) 
```

The azure network is updated with the Public IP ID. This exposes the ingress
controller publicly.
```
az network public-ip update --ids $PUBLICIPID --dns-name $dnsName --subscription $SubscriptionNameOrId
```

The certificate information is updated with the DNS and Location and applied for
Let’s Encrypt.
```
Copy-Item -Path "./certificate.yaml" -Destination "./certificate-copy.yaml"
UpdateYaml -newValue $dnsName -matchString "INGRESSDNS" -filename "./certificate-copy.yaml" 
UpdateYaml -newValue $Location -matchString "LOCATION" -filename "./certificate-copy.yaml" 
ApplyYaml "./certificate-copy.yaml" Remove-Item -Path "./certificate-copy.yaml" 
```

A second node pool is added to the AKS cluster for Orleans.
```
az aks nodepool add --resource-group $ResourceGroupName --cluster-name $ClusterName --name "nodepool2" --node-count $NodeCount --node-vm-size $OrleansVMSize
```

The Orleans node pool is labeled.
```
SetNodeLabel "nodepool2" "pool" "nodepool2" 
```

The Helm chart for the Piraeus silo installs the application as a pod in the
second node pool.
```
helm install ./piraeus-silo --name piraeus-silo --namespace kube-system --set dataConnectionString=$dataConnectionString 
```

The Helm chart for the Piraeus Management API installs the application as a pod
in the first node pool.
```
helm install ./piraeus-mgmt-api --namespace kube-system --set dataConnectionString="$dataConnectionString" --set managementApiIssuer="$ApiIssuer" --set managementApiAudience="\$ApiAudience" --set managmentApiSymmetricKey="$ApiSymmetricKey" --set managementApiSecurityCodes="$ApiSecurityCodes" 
```

The Helm chart for the Piraeus Web socket gateway install the application as a
pod in the first node pool.
```
helm install ./piraeus-websocket --namespace kube-system --set dataConnectionString="$dataConnectionString" --set auditConnectionString="$auditConnectionString" --set clientIdentityNameClaimType="$IdentityClaimType" --set clientIssuer="$Issuer" --set clientAudience="$Audience" --set clientTokenType="$TokenType" --set clientSymmetricKey="$SymmetricKey" --set coapAuthority="$CoapAuthority"
```

The routing information for the ingress controller is updated and applied.
```
Copy-Item -Path "./ingress.yaml" -Destination "./ingress-copy.yaml" 
UpdateYaml -newValue \$dnsName -matchString "INGRESSDNS" -filename "./ingress-copy.yaml" 
UpdateYaml -newValue \$Location -matchString "LOCATION" -filename "./ingress-copy.yaml" 
ApplyYaml "./ingress-copy.yaml" 
Remove-Item -Path "./ingress-copy.yaml" 
```

![Deployment Infrastructure](/docs/parch.png)

