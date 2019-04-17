Prerequisites

The following software must be installed on your local machine to run the AKS deployment of Piraeus.

1. Helm v2.12.1 or later https://github.com/helm/helm  If installed, type helm version to check the version.
2. Kubectl Client v1.10.11, Server v1.12.7 https://kubernetes.io/docs/tasks/tools/install-kubectl/ If installed type kubectl version to check the versions.
3. Powershell v6.2 or later (Powershell Core) https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-windows?view=powershell-6
4. Azure CLI  v2.0.61 or later https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest  If installed type az --version to check the version.

5. Add the AKS preview extension via Azure CLI

	az extension add --name aks-preview

6. Apply any updates if you have previously installed the extension 

	az extension update --name aks-preview

7. Register the multiple node pools feature

	az feature register --name MultiAgentpoolPreview --namespace Microsoft.ContainerService
	az feature register --name VMSSPreview --namespace Microsoft.ContainerService

8. Refresh the registration of the Microsoft.ContainerService
	az provider register --namespace Microsoft.ContainerService


Deployment

Step 1 – Deploy the Piraeus cluster on AKS
	Task 1: Open a command prompt
	Task 2: Type pwsh to start PowerShell Core v6.2
	Task 3: Navigate to the SkunkLab.Core/Kubernetes folder
	Task 4: : type . ./piraeusdeploy.ps1 to load the powershell script.  Note: dot space dot/piraeusdeploy.ps1
	Task 5: Run the sample deployment by typing
	New-PiraeusDeploy “email-address” “dns-name” “location” “storage-acct-name” “resource-group-name”   

	Example
	New-PiraeusDeploy "foo@gmail.com" testhack eastus mystorageacct myResourceGroup

#Note:  The FQDN will be "testhack.eastus.cloudapp.azure.com" and will be bound to the ACME certificate
#Note:  If you run the deployment again 
#       (1) Use a different dnsname (because it has already been used by Let's Encrypt/ACME
#       (2) Remove the previous deployment via kubectl, i.e., the interaction portion of the script
#       (3) The default clusterName is "piraeuscluster"

You can test the deployment by browsing to https://<dnsname>.<location>.cloudapp.azure.com/api/manage?code=12345678
You should see a JWT token in the browser



