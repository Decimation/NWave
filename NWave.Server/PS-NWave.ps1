$global:NWaveServer = "http://localhost:25565"

<# 
App.MapPost("/Play", Routes.PlayAsync);
App.MapPost("/Stop", Routes.StopAsync);
App.MapPost("/Pause", Routes.PauseAsync);
App.MapGet("/Status", Routes.StatusAsync);
App.MapGet("/List", Routes.ListAsync);
App.MapPost("/Add", Routes.AddAsync);
App.MapPost("/Remove", Routes.RemoveAsync);
App.MapPost("/Update", Routes.UpdateAsync);
App.MapPost("/AddYouTubeFile", Routes.AddYouTubeAudioFileAsync);
App.MapPost("/AddYouTubeUrl", Routes.AddYouTubeAudioUrlAsync);
#>


function Update-NWaveRoutes {
	param (
		$Server = $global:NWaveServer
	)

	# region 

	$script:Route_Play = @{Uri = "/Play"; Method = "Post" }
	$script:Route_Stop = @{Uri = "/Stop"; Method = "Post" }
	$script:Route_Pause = @{Uri = "/Pause"; Method = "Post" }
	$script:Route_Status = @{Uri = "/Status"; Method = "Get" }
	$script:Route_List = @{Uri = "/List"; Method = "Get" }
	$script:Route_Add = @{Uri = "/Add"; Method = "Post" }
	$script:Route_Remove = @{Uri = "/Remove"; Method = "Post" }
	$script:Route_Update = @{Uri = "/Update"; Method = "Post" }
	$script:Route_AddYouTubeFile = @{Uri = "/AddYouTubeFile"; Method = "Post" }
	$script:Route_AddYouTubeUrl = @{Uri = "/AddYouTubeUrl"; Method = "Post" }


	$script:Routes = Get-Variable -Name Route_* -ValueOnly -Scope Script

	$script:Routes | ForEach-Object {
		$_.Uri = "$Server$($_.Uri)"
	}

	# endregion
}

function Get-NWaveStatus {
	param (
		$Body,
		[ValidateSet('Regex', 'Simple')]
		$Mode
	)

	$response = Invoke-WebRequest @script:Route_Status -Body $Body `
		-Headers @{'Mode' = $Mode }

	return $response
	
}

function Stop-NWaveSound {
	param (
		$Body,
		[ValidateSet('Regex', 'Simple')]
		$Mode
	)
	$response = Invoke-WebRequest @script:Route_Stop -Body $Body `
		-Headers @{'Mode' = $Mode }

	return $response
}

function Add-NWaveYouTubeUrl {
	param (
		$Body,
		[ValidateSet('Regex', 'Simple')]
		$Mode
	)
	
	$response = Invoke-WebRequest @script:Route_AddYouTubeUrl -Body $Body `
		-Headers @{'Mode' = $Mode }
	
	return $response
}

function Get-NWaveSounds {
	$response = Invoke-WebRequest @script:Route_List
	$ret = $response | ConvertFrom-Json -ErrorAction SilentlyContinue
	return $ret
}

function Play-NWaveSound {
	param (
		$Body,
		[ValidateSet('Regex', 'Simple')]
		$Mode
	)

	$response = Invoke-WebRequest @script:Route_Play -Body $Body `
		-Headers @{'Mode' = $Mode }

	$ret = $response
	return $ret
}

Update-NWaveRoutes