function New-PiraeusVnetDemo()  
{	
    param ([string]$SubscriptionName, [string]$VNetName, [string]$SubnetName, [string]$ResourceGroupName, [string]$Email, [string]$Dns, [string]$Location, [string]$StorageAcctName, [string]$AppID, [string]$Password)
    
    $apiKey1 = NewRandomKey(16)
	$apiKey2 = NewRandomKey(16)
	$apiCodes = $apiKey1 + ";" + $apiKey2
	$apiSymmetricKey = NewRandomKey(32)
	$symmetricKey = NewRandomKey(32)
	
	if($StorageAcctName.Length -eq 0)
	{
		$storageAcctName = NewRandomStorageAcctName
    }
    else
    {
		$storageAcctName = $StorageAcctName
    }

	$config = Get-Content -Raw -Path "./deploy.json" | ConvertFrom-Json	
	$config.storageAcctName = $storageAcctName
	$config.resourceGroupName = $ResourceGroupName
	$config.subscriptionNameOrId = $SubscriptionName
	$config.location = $Location
	$config.email = $Email
	$config.dnsName = $Dns
	$config.apiSymmetricKey = $apiSymmetricKey
	$config.apiSecurityCodes = $apiCodes
	$config.symmetricKey = $symmetricKey
	$config.apiIssuer = "http://$Dns.$Location.cloudapp.azure.com/mgmt"
	$config.apiAudience = $config.apiIssuer
	$config.tokenType = "JWT"
	$config.identityClaimType = "http://$Dns.$Location.cloudapp.azure.com/name"
	$config.issuer = "http://$Dns.$Location.cloudapp.azure.com/"
	$config.audience = $config.issuer
	$config.coapAuthority = "http://$Dns.$Location.cloudapp.azure.com"
	$config.frontendVMSize = "Standard_D2s_v3"
	$config.orleansVMSize  = "Standard_D4s_v3"
	$config.nodeCount = 1
	$config.clusterName = "piraeusvnetcluster"
	
	$email = $config.email
	$dnsName = $config.dnsName
	$location = $config.location
	$resourceGroupName = $config.resourceGroupName
	$subscriptionNameOrId = $config.subscriptionNameOrId
	$clusterName = $config.clusterName
	$nodeCount = $config.nodeCount
	$apiIssuer = $config.apiIssuer			
	$apiAudience = $config.apiAudience
	$apiSymmetricKey = $config.apiSymmetricKey
	$apiSecurityCodes = $config.apiSecurityCodes
	$identityClaimType = $config.identityClaimType
	$issuer = $config.issuer
	$audience = $config.audience
	$symmetricKey = $config.symmetricKey
	$tokenType = $config.tokenType
	$coapAuthority = $config.coapAuthority
	$frontendVMSize = $config.frontendVMSize
	$orleansVMSize = $config.orleansVMSize
	$appId = $config.appId
	$pwd = $config.pwd
	
	
	
	if($AppID.Length -ne 0)
	{
		$appId = $AppID
	}
	
	if($Password.Length -ne 0)
	{
		$pwd = $Password
	}
	
	$step = 1

	CleanUpK8Deployment "$clusterName" "$resourceGroupName"
	
	#Delete the AKS cluster if exists
	$clusterLine = az aks list --query "[?contains(name, '$clusterName')]" --output table
	if($clusterLine.Length -gt 0)
	{
		Write-Host "-- Step $step - Deleting old AKS cluster $clusterName" -ForegroundColor Green
		az aks delete --name $clusterName --resource-group $resourceGroupName --yes
		$step++
	}
	
	#read the file and see if service principal exists
	#if not create one and update the file
	
	
	if($config.appId -eq $null -or $config.appId.Length -eq 0)
	{
		#create the service principal
		Write-Host "-- Step $step - Creating service principal" -ForegroundColor Green
		$creds = az ad sp create-for-rbac  --skip-assignment
		$v1 = $creds[1].Replace(",","").Replace(":","=").Replace(" ","").Replace('"',"")
		$sd1 = ConvertFrom-StringData -StringData $v1
		$newAppId = $sd1.Values[0]
		$v2 = $creds[4].Replace(",","").Replace(":","=").Replace(" ","").Replace('"',"")
		$sd2 = ConvertFrom-StringData -StringData $v2
		$newPwd = $sd2.Values[0]
		$step++
		$config.appId = $null
		$config.pwd = $null		
		$config.appId = "$newAppId"
		$config.pwd = "$newPwd"	

		Write-Host "Service Principal Application ID $appId" -ForegroundColor Cyan
		Write-Host "Service Principal Password $pwd" -ForegroundColor Cyan
	}

	$dateTimeString = Get-Date -Format "MM-dd-yyyyTHH-mm-ss"
	$filename = "./deploy-" + $dateTimeString + ".json"
	$config | ConvertTo-Json -depth 100 | Out-File $filename
	
	
	#Set the subscription 
	Write-Host "-- Step $step - Setting subscription  $subscriptionNameOrId" -ForegroundColor Green
	az account set --subscription "$subscriptionNameOrId"
	if($LASTEXITCODE -ne 0)
	{
		Write-Host "Subscription - $subscriptionNameOrId could not be set" -ForegroundColor Yellow
		Write-Host "Exiting script" -ForegroundColor Yellow
		return;
	}
	$step++
	
	#check if the Resource Group exists
	$rgoutcome = az group exists --name $resourceGroupName
	
	if($rgoutcome -eq "false")
	{
		Write-Host "-- Step $step - Create resource group '$ResourceGroupName'" -ForegroundColor Green
		az group create --name $resourceGroupName --location $location 
		$step++
	}

    az network vnet create --resource-group $ResourceGroupName --name $VNetName --address-prefixes 192.168.0.0/16 --subnet-name $SubnetName --subnet-prefix 192.168.1.0/24
    


	$VNET_ID=$(az network vnet show --resource-group $ResourceGroupName --name $VNetName --query id -o tsv)
    $SUBNET_ID=$(az network vnet subnet show --resource-group $ResourceGroupName --vnet-name $VNetName --name $SubnetName --query id -o tsv)

    az role assignment create --assignee $AppID --scope $VNET_ID --role Contributor

    

	
	#Check if the Orleans storage account exists
	$saLine= az storage account check-name --name $storageAcctName
	if(-Not $saLine[2].Contains("true"))
	{
		Write-Host "-- Step $step - Cleaning up orleans storage account for new deployment" -ForegroundColor Green
		#$tempCS1 = az storage account show-connection-string --name $storageAcctName --resource-group $resourceGroupName
		az storage container delete --name grainstate  --account-name $storageAcctName 		
		az storage table delete --name OrleansSiloInstances --account-name $storageAcctName 
		az storage table delete --name 'MetricsHourPrimaryTransactionsBlob' --account-name $storageAcctName 
		az storage table delete --name 'MetricsHourPrimaryTransactionsFile' --account-name $storageAcctName 
		az storage table delete --name 'MetricsHourPrimaryTransactionsQueue' --account-name $storageAcctName 
		az storage table delete --name 'MetricsHourPrimaryTransactionsTable' --account-name $storageAcctName
	    #Write-Host "-- Step $step - Deleting orleans storage account" -ForegroundColor Green
		#az storage account delete --name $storageAcctName --resource-group $resourceGroupName --yes
		$step++
	}
	else
	{
		#create the storage account
		Write-Host "-- Step $step - Creating orleans storage account" -ForegroundColor Green
		az storage account create --location $location --name $storageAcctName --resource-group $resourceGroupName --sku "Standard_LRS"
		$step++
	} 
	
	 #Check if the Audit storage account exists
	$auditStorageAcctName = $storageAcctName + "audit"
	$asaLine= az storage account check-name --name $auditStorageAcctName 
	if(-Not $asaLine[2].Contains("true"))
	{
		#delete the storage account
		#Write-Host "-- Step $step - Deleting audit storage account" -ForegroundColor Green
		#az storage account delete --name $auditStorageAcctName --resource-group $resourceGroupName --yes
		Write-Host "-- Step $step - Cleaning up audit storage account for new deployment" -ForegroundColor Green
		az storage table delete --name 'MetricsHourPrimaryTransactionsBlob' --account-name $auditStorageAcctName 
		az storage table delete --name 'MetricsHourPrimaryTransactionsFile' --account-name $auditStorageAcctName 
		az storage table delete --name 'MetricsHourPrimaryTransactionsQueue' --account-name $auditStorageAcctName 
		az storage table delete --name 'MetricsHourPrimaryTransactionsTable' --account-name $auditStorageAcctName
		$step++
	}
	else
	{	
		#create the storage account
		Write-Host "-- Step $step - Creating audit storage account" -ForegroundColor Green
		az storage account create --location $location --name $auditStorageAcctName --resource-group $resourceGroupName --sku "Standard_LRS"
		$step++
	}
	
	#Get the credentials for the storage accounts
	$dcs = az storage account show-connection-string --name $storageAcctName --resource-group $resourceGroupName
	$vs1 = $dcs.Replace(",","").Replace(":","=").Replace(" ","").Replace('"',"").Replace("{","").Replace("}","").Trim()
	$ts1 = $vs1 -split "connectionString="
	$dataConnectionString = $ts1[2]  

	$adcs = az storage account show-connection-string --name $auditStorageAcctName --resource-group $resourceGroupName
	$vsa1 = $adcs.Replace(",","").Replace(":","=").Replace(" ","").Replace('"',"").Replace("{","").Replace("}","").Trim()
	$tsa1 = $vsa1 -split "connectionString="
	$auditConnectionString = $tsa1[2] 
	
	
	#create AKS cluster
	Write-Host "-- Step $step - Create AKS cluster" -ForegroundColor Green

    az aks create --resource-group $resourceGroupName --name $clusterName --node-count $nodeCount --network-plugin kubenet --service-cidr 10.0.0.0/16 --dns-service-ip 10.0.0.10 --pod-cidr 10.244.0.0/16 --docker-bridge-address 172.17.0.1/16 --vnet-subnet-id $SUBNET_ID --service-principal $AppID --client-secret $Password --node-vm-size $frontendVMSize 

	
	$step++

	#get AKS credentials
	Write-Host "-- Step $step - Get AKS credentials" -ForegroundColor Green
	GetAksCredentials $resourceGroupName $clusterName
	$step++
	
	#apply RBAC
	Write-Host "-- Step $step - Apply kubectl RBAC" -ForegroundColor Green
	ApplyYaml "./helm-rbac.yaml" "kube-system"
	$step++
	
	#initialize tiller with helm
	Write-Host "-- Step $step - Intialize tiller" -ForegroundColor Green
	helm init --service-account tiller
	Write-Host "...waiting 45 seconds for Tiller to start" -ForegroundColor Yellow
	Start-Sleep -Seconds 45
	$step++
	
	Write-Host "-- Step $step - Label existing node in cluster pool=nodepool1" -ForegroundColor Green
	SetNodeLabel "nodepool1" "pool" "nodepool1"
	$step++
	
	Write-Host "-- Step $step - Creating namespace for cert-manager" -ForegroundColor Green
	kubectl create namespace cert-manager
	$step++

	Write-Host "-- Step $step - Disabling validation on cert-manager" -ForegroundColor Green
	kubectl label namespace cert-manager certmanager.k8s.io/disable-validation="true"
	$step++

	Write-Host "-- Step $step - Getting cert-manager CRDs" -ForegroundColor Green
	#ApplyYaml "https://raw.githubusercontent.com/jetstack/cert-manager/release-0.11/deploy/manifests/00-crds.yaml" "cert-manager"
	kubectl apply -f "https://raw.githubusercontent.com/jetstack/cert-manager/release-0.11/deploy/manifests/00-crds.yaml" -n "cert-manager" --validate=false
	$step++
	
	Write-Host "-- Step $step - Adding Jetstack Helm repository " -ForegroundColor Green
	helm repo add jetstack https://charts.jetstack.io
	helm repo update

	Write-Host "-- Step $step - Installing cert-manager" -ForegroundColor Green
	helm install --name cert-manager --namespace cert-manager --version v0.11.0 --set ingressShim.extraArgs='{--default-issuer-name=letsencrypt-prod,--default-issuer-kind=ClusterIssuer}' jetstack/cert-manager --set webhook.enabled=true           
	Write-Host "Wait 45 seconds for cert-manager to initialize" -ForegroundColor Yellow
	Start-Sleep -Seconds 45
	$step++

	Write-Host "-- Step $step - Applying the certificate issuer" -ForegroundColor Green
	Copy-Item -Path "./issuer.yaml" -Destination "./issuer-copy.yaml"
	UpdateYaml -newValue $email -matchString "EMAILREF" -filename "./issuer-copy.yaml"            
	kubectl apply -f ./issuer-copy.yaml -n kube-system
	Write-Host "Wait 30 seconds for issuer to initialize"
	Start-Sleep -Seconds 30
	Remove-Item -Path "./issuer-copy.yaml"
	$step++

	Write-Host "-- Step $step - Adding NGINX Helm repository" -ForegroundColor Green
	helm repo update NGINX 
	$step++

	Write-Host "-- Step $step - Installing NGINX ingress controller" -ForegroundColor Green
	InstallNGINX
	#helm install stable/nginx-ingress --namespace kube-system --set controller.replicaCount=1
	Write-Host "Wait 45 seconds for nginx to initialize"
	Start-Sleep -Seconds 45
	$step++

	Write-Host "-- Step $step - NGINX ingress controller's external IP" -ForegroundColor Green
	$IP = GetExternalIP            
	Write-Host "Got external IP = $IP" -ForegroundColor Yellow
 
	# Get the resource-id of the public ip
	#$PUBLICIPID=$(az network public-ip list --query "[?ipAddress!=null]|[?contains(ipAddress, '$IP')].[id]" --output tsv)
	#Write-Host "PublicIPID = $PUBLICIPID" -ForegroundColor Yellow
	#$step++

	#update the azure network with the public IP ID
	#Write-Host "-- Step $step - Update Azure Network with Public IP ID" -ForegroundColor Green
	#if($subscriptionNameOrId.Length -ne 0)
	#{
	  #az network public-ip update --ids $PUBLICIPID --dns-name $dnsName --subscription $subscriptionNameOrId
	#}
	#else
	#{
	#  az network public-ip update --ids $PUBLICIPID --dns-name $dnsName
	#}
	#$step++
	
	
	#update and apply certificate with DNS and location
	Write-Host "-- Step $step - Update and apply the certificate" -ForegroundColor Green
	Copy-Item -Path "./certificate.yaml" -Destination "./certificate-copy.yaml"
	UpdateYaml -newValue $dnsName -matchString "INGRESSDNS" -filename "./certificate-copy.yaml"
	UpdateYaml -newValue $location -matchString "LOCATION" -filename "./certificate-copy.yaml"
	ApplyYaml "./certificate-copy.yaml" -n "cert-manager"
	Remove-Item -Path "./certificate-copy.yaml"
	$step++

	#add orleans VM to a new node pool
	Write-Host "-- Step $step - Adding Orleans node to node pool" -ForegroundColor Green
	az aks nodepool add --resource-group $resourceGroupName --cluster-name $clusterName --name "nodepool2" --node-count $nodeCount --node-vm-size $orleansVMSize
	Write-Host "Waiting 60 seconds for node to initialize"
	Start-Sleep -Seconds 60
	$step++
	
	#label the new node pool
	Write-Host "-- Step $step - Label orleans node in cluster pool=nodepool2" -ForegroundColor Green
	SetNodeLabel "nodepool2" "pool" "nodepool2"
	$step++

	
	

	Write-Host ("K8 api services online...let's deploy and finish up") -ForegroundColor Green


	#apply the piraeus silo helm chart
	Write-Host "-- Step $step - Deploying helm chart for piraeus-silo" -ForegroundColor Green
	helm install ./piraeus-silo --name piraeus-silo --namespace kube-system --set dataConnectionString=$dataConnectionString
	if($LASTEXITCODE -ne 0 )
	{
		WaitForApiServices
		helm install ./piraeus-silo --name piraeus-silo --namespace kube-system --set dataConnectionString=$dataConnectionString
	}
	$step++

	$tagain = $false
	Write-Host "-- Step $step - Deploying helm chart for piraeus management api" -ForegroundColor Green
	helm install ./piraeus-mgmt-api --namespace kube-system --set dataConnectionString="$dataConnectionString"  --set managementApiIssuer="$apiIssuer" --set managementApiAudience="$apiAudience" --set managmentApiSymmetricKey="$apiSymmetricKey" --set managementApiSecurityCodes="$apiSecurityCodes"
	if($LASTEXITCODE -ne 0 )
	{
		WaitForApiServices
		helm install ./piraeus-mgmt-api --namespace kube-system --set dataConnectionString="$dataConnectionString"  --set managementApiIssuer="$apiIssuer" --set managementApiAudience="$apiAudience" --set managmentApiSymmetricKey="$apiSymmetricKey" --set managementApiSecurityCodes="$apiSecurityCodes"
	}
	
	$step++
	
	Write-Host "-- Step $step - Deploying helm chart for piraeus front end" -ForegroundColor Green
	helm install ./piraeus-websocket --namespace kube-system --set dataConnectionString="$dataConnectionString" --set auditConnectionString="$auditConnectionString" --set clientIdentityNameClaimType="$identityClaimType" --set clientIssuer="$issuer" --set clientAudience="$audience" --set clientTokenType="$tokenType" --set clientSymmetricKey="$symmetricKey" --set coapAuthority="$coapAuthority" 
	if($LASTEXITCODE -ne 0 )
	{
		WaitForApiServices
		helm install ./piraeus-websocket --namespace kube-system --set dataConnectionString="$dataConnectionString" --set auditConnectionString="$auditConnectionString" --set clientIdentityNameClaimType="$identityClaimType" --set clientIssuer="$issuer" --set clientAudience="$audience" --set clientTokenType="$tokenType" --set clientSymmetricKey="$symmetricKey" --set coapAuthority="$coapAuthority" 
	}
	$step++
	
	#update the ingress controller with the routing data and dns
	Write-Host "-- Step $step - Apply update to NGINX ingress controller" -ForegroundColor Green
	Copy-Item -Path "./ingress.yaml" -Destination "./ingress-copy.yaml"
	UpdateYaml -newValue $dnsName -matchString "INGRESSDNS" -filename "./ingress-copy.yaml"
	UpdateYaml -newValue $location -matchString "LOCATION" -filename "./ingress-copy.yaml"
	ApplyYaml "./ingress-copy.yaml" "kube-system"
	Remove-Item -Path "./ingress-copy.yaml"
	$step++
	
	Write-Host "---- OUTPUTS -----" -ForegroundColor Cyan
	Write-Host "Hostname - $dnsName.$location.cloudapp.azure.com" -ForegroundColor Magenta
	Write-Host "Application ID - $appId" -ForegroundColor Magenta
	Write-Host "Password - $pwd" -ForegroundColor Magenta
	Write-Host "-------------------" -ForegroundColor Cyan
	Write-Host ""
	
	Write-Host "Waiting 60 seconds for new containers to start before configuring demo." -Foreground Yellow
	Start-Sleep -Seconds 60
	
	Write-Host "-- Step $step - Running Sample Configuration" -ForegroundColor Green
	Write-Host "---- Running Sample Configuration ---" -Foreground Cyan
	NewSampleConfig $Dns $Location $apiKey1
	$step++
	
   Write-Host "-- Step $step - Writing file for MQTT client configuration" -ForegroundColor Green   
   $config.email = $null
   $config.storageAcctName = $null
   $config.resourceGroupName = $null
   $config.subscriptionNameOrId = $null
   $config.appId = $null
   $config.pwd = $null
   $config.clusterName = $null
   $config.nodeCount = $null
   $config.apiIssuer = $null
   $config.apiAudience = $null
   $config.apiSymmetricKey = $null
   $config.apiSecurityCodes = $null
   $config.tokenType = $null
   $config.coapAuthority = $null
   $config.frontendVMSize = $null
   $config.orleansVMSize = $null
   
   $config | ConvertTo-Json -depth 100 | Out-File "./../src/Samples.Mqtt.Client/config.json"
   
   Write-Host "---- OUTPUTS -----" -ForegroundColor Cyan
   Write-Host "Hostname - $dnsName.$location.cloudapp.azure.com" -ForegroundColor Magenta
   Write-Host "Application ID - $appId" -ForegroundColor Magenta
   Write-Host "Password - $pwd" -ForegroundColor Magenta
   Write-Host "-------------------" -ForegroundColor Cyan
   Write-Host""
   Write-Host "Done :-)  Dare Mighty Things" -ForegroundColor Cyan
	
}

function KubeApply()
{
	param([string]$filename, [string]$ns)
	$looper = $true
    while($looper)
    {    
		kubectl apply -f $filename -n $ns
		if($LASTEXITCODE -ne 0)
        {
            Write-Host "Waiting 30 to re-apply file..." -ForegroundColor Yellow
            Start-Sleep -Seconds 30
        }
        else
        {
			$looper = $false
        }
	}
}

function InstallNGINX()
{
	$looper = $true
    while($looper)
    {
		try
		{
			helm install stable/nginx-ingress --namespace kube-system --set controller.replicaCount=1
			if($LASTEXITCODE -ne 0 )
            {
				Write-Host "Error installing NGINX, waiting 20 seconds to try install NGINX again..." -ForegroundColor Yellow
				Start-Sleep -Seconds 20
            }
            else
            {
				$looper = $false
            }			
		}
		catch
		{
			Write-Host "Waiting 20 seconds to try install NGINX again..." -ForegroundColor Yellow
            Start-Sleep -Seconds 20
		}
	}
}

function WaitForApiServices()
{
	$v = kubectl get apiservice
	$ft = $true
	while(([string]$v).IndexOf("False") -ne -1)
	{
		if($ft)
		{
			Write-Host("K8 metrics-server and/or cert-manager-webhook is offline right now. We'll keep waiting until they are online") -ForegroundColor Yellow
			$ft = $false
		}
		else
		{
			Write-Host("Waiting 60 secs for the K8 apiservices to come back online, yuck...") -ForegroundColor Yellow
		}
		Start-Sleep -Seconds 60
		$v = kubectl get apiservice
	}
}



function GetAksCredentials()
{
    param([string]$rgn, [string]$cn)

    $looper = $true
    while($looper)
    {
        try
        {         
            az aks get-credentials --resource-group $rgn --name $cn            
            $looper = $false
        }
        catch
        {
            Write-Host "Waiting 30 seconds to try get aks credentials again..." -ForegroundColor Yellow
            Start-Sleep -Seconds 30
        }    
    }
}



function GetExternalIP()
{
    $looper = $TRUE
    while($looper)
    {   $externalIP = ""                  
        $lineValue = kubectl get service -l app=nginx-ingress --namespace kube-system
        
        Write-Host "Last Exit Code for get external ip $LASTEXITCODE" -ForegroundColor White
        if($LASTEXITCODE -ne 0 )
        {
            Write-Host "Try get external ip...waiting 30 seconds" -ForegroundColor Yellow
            Start-Sleep -Seconds 30
        }  
        elseif($lineValue.Length -gt 0)
        {
            $line = $lineValue[1]
            $lineout = $line -split '\s+'
            $externalIP = $lineout[3]              
        }
        
              
        if($externalIP -eq "<pending>")
        {        
            Write-Host "External IP is pending...waiting 30 seconds" -ForegroundColor Yellow
            Start-Sleep -Seconds 30
        }
        elseif($externalIP.Length -eq 0)
        {
            Write-Host "External IP is zero length...waiting 30 seconds" -ForegroundColor Yellow
            Start-Sleep -Seconds 30
        }
        else
        {
			$looper = $FALSE
            Write-Host "External IP is $externalIP" -ForegroundColor Magenta
            return $externalIP
        }
    }
}

#---- functions ----
function SetNodeLabel
{
    param([string]$nodeMatchValue, [string]$key, [string]$value)
    
    $looper = $true
    while($looper)
    {    
        $nodes = kubectl get nodes
        if($LASTEXITCODE -ne 0)
        {
            Write-Host "Waiting 10 seconds to get nodes from kubectl..." -ForegroundColor Yellow
            Start-Sleep -Seconds 10
        }
        else
        {
            foreach($node in $nodes)
            {
               $nodeVal = $node.Split(" ")[0]
               if($nodeVal.Contains($nodeMatchValue))
               {
		            kubectl label nodes $nodeVal "$key=$value"
                    if($LASTEXITCODE -ne 0)
                    {
                        Write-Host "Set node label failed. Waiting 10 seconds to try again..." -ForegroundColor Yellow
                        Start-Sleep -Seconds 10
                    }
                    else
                    {
                        $looper = $false
                    }
               }
            }
        }
    }
}

function ApplyYaml
{
    param([string]$file, [string]$ns)

    $looper = $true
    while($looper)
    {
        kubectl apply -f $file -n $ns
        if($LASTEXITCODE -ne 0)
        {
            Write-Host "kubectl apply failed for $file. Waiting 10 seconds to try again..." -ForegroundColor Yellow
            Start-Sleep -Seconds 10
        }
        else
        {
            $looper = $false
        }
    }
}

function UpdateYaml()
{
    Param ([string]$newValue, [string]$matchString, [string]$filename)

    (Get-Content $filename) -replace $matchString,$newValue | out-file $filename -Encoding ascii
}

#---- end functions


function CleanUpK8Deployment()
{
	param([string]$cName, [string]$rgName)

	#Remove previous deployments from kubectl
	$cleanup = Read-Host "Clean up previous kubectl deployment [y/n] ? "
	if($cleanup.ToLowerInvariant() -eq "y")
	{
		$cleanupClusterName = Read-Host "Enter previous cluster name [Enter blank == $cName] "
		$cleanupResourceGroup = Read-Host "Enter previous resource group name [Enter blank == $rgName] "
		
		if($cleanupClusterName.Length -eq 0)
		{
			$cleanupClusterName = $cName
		}
		
		if($cleanupResourceGroup.Length -eq 0)
		{
			$cleanupResourceGroup = $rgName
		}
		
		$condition1 = "users.clusterUser_" + $cleanupResourceGroup + "_" + $cleanupClusterName
		$condition2 = "clusters." + $cleanupClusterName
		kubectl config unset $condition1
		kubectl config unset $condition2
	}
}

function NewRandomKey()	   
{
    param([int]$Length)
    
	$random = new-Object System.Random
	$buffer = [System.Byte[]]::new($Length)
	$random.NextBytes($buffer)
	$stringVar = [Convert]::ToBase64String($buffer)
    if($stringVar.Contains("+") -or $stringVar.Contains("/"))
    {
        return NewRandomKey($Length)
    }
    else
    {
        return $stringVar
    }
}


function NewRandomStorageAcctName()
{
	$alpha = "abcdefghijklmnopqrstuvwxyz"
	$alpha2 = "0123456789"
	$array = $alpha.ToCharArray()
	$array2 = $alpha2.ToCharArray()
	$maxLength = 6
	$random = new-Object System.Random
	$randonString = ""
	$dummy = $null
	For ($i=0; $i -lt $maxLength; $i++)  
	{
		$index = $random.Next($alpha.Length)
		$randomString += $array[$index] 
	}
	
		
	$maxLength = 2
	For ($i=0; $i -lt $maxLength; $i++)  
	{
		$index = $random.Next($alpha2.Length)
		$randomString += $array2[$index] 
	}
	
	return $randomString
}


function NewSampleConfig()
{
    param([string]$DnsName, [string]$Location, [string]$Key)
    
    $authority = $DnsName.ToLower() + "." + $Location.ToLower() + ".cloudapp.azure.com"
    $url = "https://$authority"
    Write-Host "Using $url for management api" -ForegroundColor Yellow

    Import-Module "../src/Piraeus.Module.Core/bin/Release/netcoreapp2.2/Piraeus.Module.Core.dll"
    Write-Host "Module imported" -ForegroundColor Yellow

    #get a security token for the management API
    Write-Host "--- Get security token for Piraeus configuration ---" -Foreground Yellow
    $token = Get-PiraeusManagementToken -ServiceUrl $url -Key $Key
    while($LASTEXITCODE -ne 0)
    {
		Write-Host "--- Try get security token again...waiting 30 seconds" -ForegroundColor Yellow
		Start-Sleep -Seconds 30
		$token = Get-PiraeusManagementToken -ServiceUrl $url -Key $Key
    }
    
    Write-Host "--- Got security token, ready to configure Piraeus ---" -ForegroundColor Green
    
        
    Write-Host "---  INFORMATION ABOUT Sample Config ----" -ForegroundColor White
    Write-Host "The client demos create security tokens based on the selection of a 'Role', i.e., 'A' or 'B'" -ForegroundColor White
    Write-Host "The script will create 2 CAPL policies" -ForegroundColor White
    Write-Host "  (1) a client in role 'A' may transmit to 'resource-a' and subscribe to 'resource-b'" -ForegroundColor White
    Write-Host "  (2) a client in role 'B' may transmit to 'resource-b' and subscribe to 'resource-a'" -ForegroundColor White
    Write-Host "-----------------------------------------" -ForegroundColor White
    Write-Host ""
    Start-Sleep -Seconds 1
        
    #--------------- CAPL policy for users in role "A" ------------------------
    Write-Host "-- Building CAPL Authorization Policies ---" -ForegroundColor White
    Write-Host "  (1) Match Expression : Find a claim type in the security token" -ForegroundColor White
    Write-Host "  (2) Operation -- Binds a claim value from the matched claim type to perform an operation, e.g., Equals" -ForegroundColor White
    Write-Host "  (3) Rule -- Create a rule that binds a match expression and an operation" -ForegroundColor White
    Write-Host "  (4) Policy -- create a policy that is uniquely identifiable, that incorporates a Rule (or Logical Connective)" -ForegroundColor White
    Write-Host ""
    Start-Sleep -Seconds 1              

    #define the claim type to match to determines the client's role
    $authority = $DnsName.ToLower() + "." + $Location.ToLower() + "." + "cloudapp.azure.com"
    $matchClaimType = "http://$authority/role"

    #create a match expression of 'Literal' to match the role claim type
    $match = New-CaplMatch -Type Literal -ClaimType $matchClaimType -Required $true  

    #create an operation to check the match claim value is 'Equal' to "A"
    $operation_A = New-CaplOperation -Type Equal -Value "A"

    #create a rule to bind the match expression and operation
    $rule_A = New-CaplRule -Evaluates $true -MatchExpression $match -Operation $operation_A

    #define a unique identifier (as URI) for the policy
    $policyId_A = "http://$authority/policy/resource-a" 

    #create the policy for clients in role "A"
    $policy_A = New-CaplPolicy -PolicyID $policyId_A -EvaluationExpression $rule_A
    #-------------------End Policy for "B"------------------------------------
        
        
    #--------------- CAPL policy for users in role "B" ------------------------

    #create an operation to check the match claim value is 'Equal' to "B"
    $operation_B = New-CaplOperation -Type Equal -Value "B"

    #create a rule to bind the match expression and operation
    $rule_B = New-CaplRule -Evaluates $true -MatchExpression $match -Operation $operation_B

    #define a unique identifier (as URI) for the policy
    $policyId_B = "http://$authority/policy/resource-b" 

    #create the policy for users in role "A"
    $policy_B = New-CaplPolicy -PolicyID $policyId_B -EvaluationExpression $rule_B

    #-------------------End Policy for "B"------------------------------------

    # The policies are completed.  We need to add them to Piraeus

    Add-CaplPolicy -ServiceUrl $url -SecurityToken $token -Policy $policy_A 
    Add-CaplPolicy -ServiceUrl $url -SecurityToken $token -Policy $policy_B

    Write-Host "CAPL policies added to Piraeus" -ForegroundColor Yellow


    #Uniquely identify Piraeus resources by URI
    $resource_A = "http://$authority/resource-a"
    $resource_B = "http://$authority/resource-b"

    #Add the resources to Piraeus

    #Resource "A" lets users with role "A" send and users with role "B" subscribe to receive transmissions
    Add-PiraeusEventMetadata -ResourceUriString $resource_A -Enabled $true -RequireEncryptedChannel $false -PublishPolicyUriString $policyId_A -SubscribePolicyUriString $policyId_B -ServiceUrl $url -SecurityToken $token -Audit $false

    #Resource "B" lets users with role "B" send and users with role "A" subscribe to receive transmissions
    Add-PiraeusEventMetadata -ResourceUriString $resource_B -Enabled $true -RequireEncryptedChannel $false -PublishPolicyUriString $policyId_B -SubscribePolicyUriString $policyId_A -ServiceUrl $url -SecurityToken $token -Audit $false

    Write-Host "PI-System metadata added to Piraeus" -ForegroundColor Yellow
    Write-Host""


        
    #Quick check get the resource data and verify what was set
    Write-Host "----- PI-System $resource_A Metadata ----" -ForegroundColor Green
    Get-PiraeusEventMetadata -ResourceUriString $resource_A -ServiceUrl $url -SecurityToken $token
    

    Write-Host""


    Write-Host "----- PI-System $resource_B Metadata ----" -ForegroundColor Green
    Get-PiraeusEventMetadata -ResourceUriString $resource_B -ServiceUrl $url -SecurityToken $token
}
