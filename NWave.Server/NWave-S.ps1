# NWave Server


function Play-NSound {
	param (
		$n
	)
	$x = Invoke-WebRequest -Method Post -Uri "https://208.110.232.218:60900/Play" -Body "$n"
	return $x
}

