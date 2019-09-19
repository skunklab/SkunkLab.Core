
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
 
 ### Deploy Piraeus Demo
 
 1. Open a command prompt and navigate to the /kubernetes folder 
 2. Type *pwsh* to get a powershell v6 command prompt 
 3. Type . ./NewPiraeusDeploy.ps1
 4. Execute the following command New-PiraeusDemo with the following parameters
 -  *-SubscriptionName*  Name of Azure subscription to do the deployment.
 -  *-ResourceGroupName*  Name of the Resoure Group for the deployment.
 -  *-Email* Your email address, which is necessary for the Let's Encrypt certificates (limited 50 dns names per email address per week)
 -  *-Dns* Dns name for the deployment, which can be used only  1 time for each new deployment, e.g., "flyingdogs42"
 -  *-Location* Azure data center location, e.g., "eastus"
 -  *-StorageAcctName* The name of a storage account to be created.  *Warning*: If storage account already exists, it will be deleted and recreated and *you will loose all existing data*.

FQDN of the Piraeus deployment will be:
```<dns>.<location>.cloudapp.azure.com```

The sample will be automatically configured in Piraeus and a  configuration file will be written to Samples.Mqtt.Client project. 

Build the Samples.Mqtt.Client project, then run 2 Samples.Mqtt.Client instances. Use the "use file [y]" option when prompted.  Type in different client "names" in each of the 2 instances and select role "A" for one instance and "B" for the other when prompted.  Now, your 2 instances can communicate with each other.

