function New-PiraeusDeploy()
{
    param ([string]$Email, [string]$DnsName, [string]$Location, [string]$StorageAcctName, [string]$ResourceGroupName, [string]$SubscriptionNameOrId, [string]$AppId = "", [string]$Pwd = "", [string]$ClusterName = "piraeuscluster", [int]$NodeCount = 1,
            [string]$ApiIssuer = "http://skunklab.io/mgmt", [string]$ApiAudience = "http://skunklab.io/mgmt", [string]$ApiSymmetricKey = "//////////////////////////////////////////8=", [string]$ApiSecurityCodes = "12345678;87654321",
            [string]$IdentityClaimType = "http://skunklab.io/name", [string]$Issuer = "http://skunklab.io/", [string]$Audience = "http://skunklab.io/", [string]$SymmetricKey = "//////////////////////////////////////////8=", 
            [string]$TokenType = "JWT", [string]$CoapAuthority = "skunklab.io", [string]$FrontendVMSize = "Standard_D2s_v3", [string]$OrleansVMSize = "Standard_D4s_v3")


            #Prereqs
            Write-Host "The following software must be installed on your local machine." -ForegroundColor Cyan
            Write-Host "-------------------------------------------------" -ForegroundColor Cyan
            Write-Host "Helm v2.12.1 or later [https://github.com/helm/helm]" -ForegroundColor Cyan
            Write-Host "Kubectl Client v1.10.11, Server v1.12.7 [https://kubernetes.io/docs/tasks/tools/install-kubectl]" -ForegroundColor Cyan
            Write-Host "Powershell v6.2 or later (Powershell Core) [https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-windows?view=powershell-6]" -ForegroundColor Cyan
            Write-Host "Azure CLI v2.0.61 or later [https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest]" -ForegroundColor Cyan
            Write-Host "-------------------------------------------------" -ForegroundColor Cyan
            Write-Host ""

            $continueScript = Read-Host "Do you want to continue [y/n] ? "
            if($continueScript.ToLowerInvariant() -ne "y")
            {
                Write-Host "Exiting script" -ForegroundColor Yellow
                return;
            }           


            #Remove previous deployments from kubectl
            $cleanup = Read-Host "Clean up previous kubectl deployment [y/n] ? "
            if($cleanup.ToLowerInvariant() -eq "y")
            {
				
                $clusterName = Read-Host "Enter previous cluster name [Enter blank == piraeuscluster] "
                $resourceGroup = Read-Host "Enter previous resource group name [Enter blank == myResourceGroup] "
                
                if($clusterName.Length -eq 0)
                {
					$clusterName = "piraeuscluster"
                }
                
                if($resourceGroup.Length -eq 0)
                {
					$resourceGroup = "myResourceGroup"
                }
                
                $condition1 = "users.clusterUser_" + $resourceGroup + "_" + $clusterName
                $condition2 = "clusters." + $clusterName
                kubectl config unset $condition1
                kubectl config unset $condition2
            }

            $step = 1
            #Set the subscription 
            Write-Host "Step $step - Setting subscription  $SubscriptionNameOrId" -ForegroundColor Green
            az account set --subscription $SubscriptionNameOrId
            if($LASTEXITCODE -ne 0)
            {
                Write-Host "Subscription - $SubscriptionNameOrId could not be set" -ForegroundColor Yellow
                Write-Host "Exiting script" -ForegroundColor Yellow
                return;
            }
            $step++


            #Check if the Resource Group exists
            $rgoutcome = az group exists --name $ResourceGroupName
            
            if($rgoutcome -eq "false")
            {
                Write-Host "Step $step - Create resource group '$ResourceGroupName'" -ForegroundColor Green
                az group create --name $ResourceGroupName --location $Location 
                $step++
            }

            #Delete the AKS cluster if exists
            $clusterLine = az aks list --query "[?contains(name, '$ClusterName')]" --output table

            if($clusterLine.Length -gt 0)
            {
                Write-Host "Step $step - Deleting old AKS cluster '$ClusterName'" -ForegroundColor Green
                az aks delete --name $ClusterName --resource-group $ResourceGroupName --yes
                $step++
            }

            if($AppId.Length -eq 0)
            {
                #create service principal
                Write-Host "-- Step $step - Creating service principal" -ForegroundColor Green
                $creds = az ad sp create-for-rbac  --skip-assignment
                $v1 = $creds[1].Replace(",","").Replace(":","=").Replace(" ","").Replace('"',"")
                $sd1 = ConvertFrom-StringData -StringData $v1
                $appId = $sd1.Values[0]
                $v2 = $creds[4].Replace(",","").Replace(":","=").Replace(" ","").Replace('"',"")
                $sd2 = ConvertFrom-StringData -StringData $v2
                $pwd = $sd2.Values[0]
                $step++

                Write-Host "Service Principal Application ID $appId" -ForegroundColor Cyan
                Write-Host "Service Principal Password $pwd" -ForegroundColor Cyan
            }
            else
            {
                $appId = $AppId
                $pwd = $Pwd
            }

            #Check if the Orleans storage account exists
            $saLine= az storage account check-name --name $StorageAcctName 
            if($saLine[2].Contains("true"))
            {
                #create the storage account
                Write-Host "-- Step $step - Creating orleans storage account" -ForegroundColor Green
                az storage account create --location $Location --name $StorageAcctName --resource-group $ResourceGroupName --sku "Standard_LRS"
                $step++
            } 

            #Check if the Audit storage account exists
            $auditStorageAcctName = $StorageAcctName + "audit"
            $asaLine= az storage account check-name --name $auditStorageAcctName 
            if($asaLine[2].Contains("true"))
            {
                #create the storage account
                Write-Host "-- Step $step - Creating audit storage account" -ForegroundColor Green
                az storage account create --location $Location --name $auditStorageAcctName --resource-group $ResourceGroupName --sku "Standard_LRS"
                $step++
            }

            #Get the credentials for the storage accounts

            $dcs = az storage account show-connection-string --name $StorageAcctName --resource-group $ResourceGroupName
            $vs1 = $dcs.Replace(",","").Replace(":","=").Replace(" ","").Replace('"',"").Replace("{","").Replace("}","").Trim()
            $ts1 = $vs1 -split "connectionString="
            $dataConnectionString = $ts1[2]  

            $adcs = az storage account show-connection-string --name $auditStorageAcctName --resource-group $ResourceGroupName
            $vsa1 = $adcs.Replace(",","").Replace(":","=").Replace(" ","").Replace('"',"").Replace("{","").Replace("}","").Trim()
            $tsa1 = $vsa1 -split "connectionString="
            $auditConnectionString = $tsa1[2] 

            #create AKS cluster
            Write-Host "-- Step $step - Create AKS cluster" -ForegroundColor Green
            az aks create --resource-group $ResourceGroupName --name $ClusterName --node-count $NodeCount --service-principal $appId --client-secret $pwd --node-vm-size $FrontendVMSize --enable-vmss 
            $step++


            #get AKS credentials
            Write-Host "-- Step $step - Get AKS credentials" -ForegroundColor Green
            GetAksCredentials $ResourceGroupName $ClusterName
            #az aks get-credentials --resource-group $ResourceGroupName --name $ClusterName
            $step++

            #apply RBAC
            Write-Host "-- Step $step - Apply kubectl RBAC" -ForegroundColor Green
            ApplyYaml "./helm-rbac.yaml"
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

            Write-Host "-- Step $step - Disabling validation on cert-manager" -ForegroundColor Green
            kubectl label namespace kube-system certmanager.k8s.io/disable-validation="true"
            $step++

            Write-Host "-- Step $step - Getting cert-manager CRDs" -ForegroundColor Green
            ApplyYaml "https://raw.githubusercontent.com/jetstack/cert-manager/release-0.6/deploy/manifests/00-crds.yaml"
            $step++

            Write-Host "-- Step $step - Installing cert-manager" -ForegroundColor Green
            helm install --name cert-manager --namespace kube-system --set ingressShim.extraArgs='{--default-issuer-name=letsencrypt-prod,--default-issuer-kind=ClusterIssuer}' stable/cert-manager
            Write-Host "Wait 45 seconds for cert-manager to initialize" -ForegroundColor Yellow
            Start-Sleep -Seconds 45
            $step++

            Write-Host "-- Step $step - Applying the certificate issuer" -ForegroundColor Green
            Copy-Item -Path "./issuer.yaml" -Destination "./issuer-copy.yaml"
            UpdateYaml -newValue $Email -matchString "EMAILREF" -filename "./issuer-copy.yaml"
            kubectl apply -f ./issuer-copy.yaml
            Write-Host "Wait 30 seconds for issuer to initialize"
            Start-Sleep -Seconds 30
            Remove-Item -Path "./issuer-copy.yaml"
            $step++

            Write-Host "-- Step $step - Installing NGINX ingress controller" -ForegroundColor Green
            helm install stable/nginx-ingress --namespace kube-system --set controller.replicaCount=1
            Write-Host "Wait 45 seconds for nginx to initialize"
            Start-Sleep -Seconds 45
            $step++

            Write-Host "-- Step $step - NGINX ingress controller's external IP" -ForegroundColor Green
            $IP = GetExternalIP            
            Write-Host "Got external IP = $IP" -ForegroundColor Yellow
         
            # Get the resource-id of the public ip
            $PUBLICIPID=$(az network public-ip list --query "[?ipAddress!=null]|[?contains(ipAddress, '$IP')].[id]" --output tsv)
            Write-Host "PublicIPID = $PUBLICIPID" -ForegroundColor Yellow
            $step++

            #update the azure network with the public IP ID
            Write-Host "-- Step $step - Update Azure Network with Public IP ID" -ForegroundColor Green
            if($SubscriptionNameOrId.Length -ne 0)
            {
              az network public-ip update --ids $PUBLICIPID --dns-name $dnsName --subscription $SubscriptionNameOrId
            }
            else
            {
              az network public-ip update --ids $PUBLICIPID --dns-name $dnsName
            }
            $step++

            #update and apply certificate with DNS and location
            Write-Host "-- Step $step - Update and apply the certificate" -ForegroundColor Green
            Copy-Item -Path "./certificate.yaml" -Destination "./certificate-copy.yaml"
            UpdateYaml -newValue $dnsName -matchString "INGRESSDNS" -filename "./certificate-copy.yaml"
            UpdateYaml -newValue $Location -matchString "LOCATION" -filename "./certificate-copy.yaml"
            ApplyYaml "./certificate-copy.yaml"
            Remove-Item -Path "./certificate-copy.yaml"
            $step++

            #add orleans VM to a new node pool
            Write-Host "-- Step $step - Adding Orleans node to node pool" -ForegroundColor Green
            az aks nodepool add --resource-group $ResourceGroupName --cluster-name $ClusterName --name "nodepool2" --node-count $NodeCount --node-vm-size $OrleansVMSize
            Write-Host "Waiting 90 seconds for node to initialize"
            Start-Sleep -Seconds 90
            $step++

            #label the new node pool
            Write-Host "-- Step $step - Label orleans node in cluster pool=nodepool2" -ForegroundColor Green
            SetNodeLabel "nodepool2" "pool" "nodepool2"
            $step++

            #apply the piraeus silo helm chart
            Write-Host "-- Step $step - Deploying helm chart for piraeus-silo" -ForegroundColor Green
            helm install ./piraeus-silo --name piraeus-silo --namespace kube-system --set dataConnectionString=$dataConnectionString
            $step++

            
            Write-Host "-- Step $step - Deploying helm chart for piraeus management api" -ForegroundColor Green
            helm install ./piraeus-mgmt-api --namespace kube-system --set dataConnectionString="$dataConnectionString"  --set managementApiIssuer="$ApiIssuer" --set managementApiAudience="$ApiAudience" --set managmentApiSymmetricKey="$ApiSymmetricKey" --set managementApiSecurityCodes="$ApiSecurityCodes"
			$step++
			
			
			Write-Host "-- Step $step - Deploying helm chart for piraeus front end" -ForegroundColor Green
            helm install ./piraeus-websocket --namespace kube-system --set dataConnectionString="$dataConnectionString" --set auditConnectionString="$auditConnectionString" --set clientIdentityNameClaimType="$IdentityClaimType" --set clientIssuer="$Issuer" --set clientAudience="$Audience" --set clientTokenType="$TokenType" --set clientSymmetricKey="$SymmetricKey" --set coapAuthority="$CoapAuthority" 
			$step++
			
            #update the ingress controller with the routing data and dns
            Write-Host "-- Step $step - Apply update to NGINX ingress controller" -ForegroundColor Green
            Copy-Item -Path "./ingress.yaml" -Destination "./ingress-copy.yaml"
            UpdateYaml -newValue $dnsName -matchString "INGRESSDNS" -filename "./ingress-copy.yaml"
            UpdateYaml -newValue $Location -matchString "LOCATION" -filename "./ingress-copy.yaml"
            ApplyYaml "./ingress-copy.yaml"
            Remove-Item -Path "./ingress-copy.yaml"

            Write-Host "---- OUTPUTS -----" -ForegroundColor Cyan
            Write-Host "Hostname - $DnsName.$Location.cloudapp.azure.com" -ForegroundColor Magenta
            Write-Host "Application ID - $appId" -ForegroundColor Magenta
            Write-Host "Password - $pwd" -ForegroundColor Magenta
            Write-Host "-------------------" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "--- Done :-) Dare Mighty Things ---" -ForegroundColor Cyan

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
            #$lineParams = $lineValue.Replace("  "," ")
            #$lineParams = $lineParams.Replace("  "," ")
            #$lineItems = $lineParams.Split(" ")
            #$externalIP = $lineItems[4]      
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
    param([string]$file)

    $looper = $true
    while($looper)
    {
        kubectl apply -f $file
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