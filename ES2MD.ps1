function ReadScript
{
	param([System.String]$scriptSource)
	
	Write-Host $scriptSource
	
	if (Test-Path $scriptSource) {
		Write-Host "It exists"
		$scriptData = Get-Content $scriptSource
	} else {
		Write-Host "noh"
		$scriptData = $scriptSource
	}
	
	$scriptLines = $scriptData -split "\r?\n"
	
	$scriptLines = $scriptLines.Trim() -replace "def [0-9\ ]*{",""
	
	$scriptJoined = $scriptLines -join ""
	
	while ($scriptJoined -like "*` ` *") {
		$scriptJoined = $scriptJoined -replace "` ` ","` "
	}
	
	$scriptJoined = $scriptJoined -replace "`"`"`"","`""
	$scriptJoined = $scriptJoined -replace "`"` ","`""
	
	$commands = $scriptJoined -split ";"
	
	$thisActor = ""
	$thisEffect = ""
	$thisFace = ""
	$TextInfo = (Get-Culture).TextInfo
	
	foreach ($command in $commands) {
		if ($command -like "*message_Talk*") {
			$command -match "`"(.*)`""
			if ($Matches.Length > 1) {
				$message = $Matches[1] -replace "\[CS:X\]"," **" -replace "\[CR\]","** " -replace "\[K\]"," "
				while ($message -like "*` ` *") {
					$message = $message -replace "` ` ","` "
				}
				
			}
		} elseif ($command -like "*SetEffect*") {
			
		} elseif ($command -like "*message_SetFace*") {
			$trimmed = $command -replace "message_SetFace\(","" -replace "\)",""
			$arguments = ($trimmed -split ",").Trim()
			$thisActor = $arguments[0]
			$id = $arguments[1] -replace "FACE_",""
			$thisFace = $id.substring(0,1).toupper()+$id.substring(1).tolower()
			if ($thisFace -eq "Tearyeyed") {
				$thisFace = "Teary-Eyed"
			}
		}
	}
}

