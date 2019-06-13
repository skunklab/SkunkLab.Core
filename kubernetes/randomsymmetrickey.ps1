#Piraeus Security Key Generator
#generate a random 256-bit/32-byte base64 encoded string

Add-Type -AssemblyName System

[Reflection.Assembly]::LoadWithPartialName("System")
$random = new-Object System.Random
$buffer = [System.Byte[]]::new(32)
$random.NextBytes($buffer)
Write-Host([Convert]::ToBase64String($buffer))


