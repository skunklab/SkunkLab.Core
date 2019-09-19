
function New-RandomKey([int]$Length = 32)
{
	$random = new-Object System.Random
	$buffer = [System.Byte[]]::new($Length)
	$random.NextBytes($buffer)
	return [Convert]::ToBase64String($buffer)
}