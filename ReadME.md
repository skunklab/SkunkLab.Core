
# Piraeus
## Introduction
Getting the right information to the right place at the right time is a difficult task in highly distributed environments.  Piraeus simplifies how heterogenous subsystems can interact statically, dynamically, and organically using an open-systems approach to real-time communications. Simplicity is the key where standard channels and protocols are supported with no coupling between subsystems.  The technology utilizes Microsoft Orleans to facilate on-demand routes for information delivery and Claims Authorization Policy Language (CAPL) for fine-grain access control between senders and receivers of messages.  The low latency and linearly scalable technology means you can build distributed systems, even complex systems, with simplicity and have real-time communications that scale.

The technology is designed to run on docker containers and the getting started sample show you how to get up and running in minutes on Azure AKS.

- For Management API using PowerShell v6 see [here](/docs/MgmtApi.md)

- For custom production deployments see [here](/docs/deployconfig.md)

![Architecture](/docs/arch.jpg)
[Deployment Details](/docs/deploydetail.md)
## Getting Started

 1. Clone the source
 2. Ensure the [prereqs](/docs/prereqs.md) are installed. 
 3. Deploy Piraeus to Azure AKS
 4. Configure Piraeus
 5. Run a sample client
 
 ### Deploy Piraeus to Azure AKS
 
 1. Open a command prompt and navigate to the /kubernetes folder 
 2. Type *pwsh* to get a powershell v6 command prompt 
 3. Open the deploy.json file in notepad and update the following parameters in the file 
 - *Email* - Your email address, i.e., required for Let's Encrypt certificate issuance
- *DnsName* - The DNS name for the deployment, e.g., "growlingdog"
- *Location* - The Azure data center location, e.g., "eastus"
- *StorageAcctName* - A name for the Azure storage account
- *ResourceGroupName* - Name of the resource group to deploy in your Azure subscription

4.  Load the PowerShell deployment script by typing
 ```. ./piraeusdeploy.ps1 ``` 
 5. Start the deployment with the following command and completing the desired custom parameters
> New-PiraeusDeploy -File "deploy.json"
> 

FQDN of the Piraeus deployment will be:

```<dns>.<location>.cloudapp.azure.com```

### Configure Piraeus

 1. In the /kubernetes folder and using a Powershell 6 prompt load the SampleConfig script
 
 ``` . ./Sample.Config.ps1```
 
 2. Run the sample configuration using the same DNS and Location parameters used in the deployment to AKS
 
 ``` New-SampleConfig *Dns* *Location*```
 
 3. The first time you run the Sample Configuration you will be prompted to load the Piraeus.Module.Core powershell module.  Select "Y" the first time you run the script to load the module.
 4. You should see the metadata output to the console when the script completes for resource-a and resource-b

### Run the Sample Client

 1. Open the Samples.Mqtt.Client project in Visual Studio 2017. 
 2. Right-click the project and select Debug and Start New Instance from the menu.  This will launch the client console app.  Follow the instructions in console window entering the information in the image below using your FQDN, e.g., growlingdog.eastus.cloudapp.azure.com.
 3. Right-click the project again and Debug and Start new instance from the menu.  This will launch a 2nd client console app.  Follow the instructions in the console window and enter in the information in image below using your FQDN.
 4. Send messages to and from both client apps and feel welcome to open more clients if you like.

