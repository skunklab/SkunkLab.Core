
#Piraeus Management API Key Generator
#generate a random 8 character string

Add-Type -AssemblyName System

[Reflection.Assembly]::LoadWithPartialName("System")
$alpha = "abcdefghijklmnopqrstuvwxyzABCDEFHIJKLMNOPQRSTUVWXYZ0123456789"
$array = $alpha.ToCharArray()
$maxLength = 8
$random = new-Object System.Random
$randonString = ""

For ($i=0; $i -lt $maxLength; $i++)  
{
   $index = $random.Next($alpha.Length)
   $randomString += $array[$index]   
}

Write-Host($randomString)
$randomString = ""