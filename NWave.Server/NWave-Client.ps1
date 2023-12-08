using namespace System
using namespace System.Net.Http
using namespace System.Net.Http.Headers

param([switch]$Debug)

$HOST_LOCAL1 = "192.168.1.79"
$HOST_LOCAL2 = "localhost"
$HOST_REMOTE = "206.196.32.236"
$PORT = "60900"

$Server = $Debug ? $HOST_LOCAL1 : $HOST_REMOTE
$hc = [HttpClient]@{
	BaseAddress = "http://$Server`:$PORT"
}
	
$job = Start-ThreadJob -Name "NWave-Client" -ScriptBlock {
	
	param($cl)

	while ($true) {
		$req = [System.Net.Http.HttpRequestMessage] @{
			Method     = [System.Net.Http.HttpMethod]::Get
			RequestUri = [System.Uri]"Status"
		}
		$res = $cl.Send($req)
		$rstream = $res.Content.ReadAsStream()
		$sr = [System.IO.StreamReader]::new($rstream)
		$content = $sr.ReadToEnd()
		$sr.Dispose()
		Write-Output $content
		# Write-Host "$content"
		Start-Sleep -Seconds 3
		# Clear-Host
	}
} -ThrottleLimit 1 -ArgumentList $hc

return $job