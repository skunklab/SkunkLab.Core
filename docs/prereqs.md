## Prerequisites for Samples
=============  
  
The following software must be installed on your local machine to run the AKS  deployment of Piraeus and the samples.
  
**[Helm](https://github.com/helm/helm)** v2.12.1 or later   
**[Kubectl](https://kubernetes.io/docs/tasks/tools/install-kubectl)** Client v1.10.11, Server v1.12.7   
**[Powershell](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-windows?view=powershell-6)** v6.2 or later (Powershell Core)    
**[Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest)** v2.0.61 or later 

Install the Azure CLI preview for AKS support for multiple node pools as follows:

 1. Add the Azure CLI extension for the preview
*az extension add --name aks-preview*

 2. Register the multiple node pool feature provider
 

    ```az feature register --name MultiAgentpoolPreview --namespace Microsoft.ContainerService```
    
    ```az feature register --name VMSSPreview --namespace Microsoft.ContainerService``` 
    
  It takes a few minutes for the status to show Registered . You can check on the registration       status using the az feature list command:  

    az feature list -o table --query "[?contains(name, 'Microsoft.ContainerService/MultiAgentpoolPreview')].{Name:name,State:properties.state}" az feature list -o table --query "[?contains(name, 'Microsoft.ContainerService/VMSSPreview')].{Name:name,State:properties.state}


 3. When ready, refresh the registration of the Microsoft.ContainerService resource provider using the az provider register command:
 

    ```az provider register --namespace Microsoft.ContainerService```

