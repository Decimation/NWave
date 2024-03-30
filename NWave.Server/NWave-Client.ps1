using namespace System
using namespace System.Net.Http
using namespace System.Net.Http.Headers

param([switch]$Debug)

#region

$HOST_LOCAL1 = "192.168.1.79"
$HOST_LOCAL2 = "localhost"
$HOST_REMOTE = "206.196.32.236"
$PORT = "60900"

$Server = $Debug ? $HOST_LOCAL1 : $HOST_REMOTE

$Client = [HttpClient]@{
	BaseAddress = "http://$Server`:$PORT"
}

#endregion

Write-Host "$($PSStyle.Bold)"

function Update-NWave {
	
	[CmdletBinding()]
	param(
		$Client2
	)

	
	begin {
		
	}
	
	process {
		while ($true) {
			$req = [System.Net.Http.HttpRequestMessage] @{
				Method     = [System.Net.Http.HttpMethod]::Get
				RequestUri = [System.Uri] "Status"
			}

			$res = $Client2.Send($req)
			$rstream = $res.Content.ReadAsStream()
			$sr = [System.IO.StreamReader]::new($rstream)
			$content = $sr.ReadToEnd()
			$sr.Dispose()

			Write-Output $content
			# Write-Host "$content"
			Start-Sleep -Seconds 3
			# Clear-Host
		}
		
	}
	
	end {
		
	}
}


$job = Start-ThreadJob -Name "NWave-Client" `
	-ScriptBlock ${function:Update-NWave} 	`
	-ThrottleLimit 1 						`
	-ArgumentList $Client

return $job