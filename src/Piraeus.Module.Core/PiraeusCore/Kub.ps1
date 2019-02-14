$resoureGroup = "MyK8Test"
$location = "eastus"
$clusterName = "testletsencrypt"
#$helmNS = "kube-system"

az group create --name $resoureGroup --location $location

$json = az ad sp create-for-rbac --skip-assignment

$j = [string]$json

$appId = $j.Substring($j.LastIndexOf("appId") + 9, 36)
$pwd = $j.Substring($j.LastIndexOf("password") + 12, 36)

az aks create --resource-group $resoureGroup --name $clusterName --node-count 1 --service-principal $appId --client-secret $pwd --generate-ssh-keys

az aks get-credentials --resource-group $resoureGroup --name $clusterName

#kubectl get node

#kubectl create serviceaccount --namespace kube-system tiller
#kubectl create clusterrolebinding tiller-cluster-rule --clusterrole=cluster-admin --serviceaccount=kube-system:tiller
#kubectl patch deploy --namespace kube-system tiller-deploy -p '{"spec":{"template":{"spec":{"serviceAccount":"tiller"}}}}'


#helm install stable/nginx-ingress --namespace "kubesys" --set controller.replicaCount=2

#kubectl get service -l app=nginx-ingress --namespace "kubesys"
