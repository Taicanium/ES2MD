$globalEffects = @(
	@{Name1="EFFECT_EXCLAMATION_MARK";Name2="❗"},
	@{Name1="EFFECT_SHOCKED";Name2="〽"},
	@{Name1="EFFECT_SHOCKED_MIRRORED";Name2="〽"},
	@{Name1="EFFECT_SWEAT_DROP";Name2="💧"},
	@{Name1="EFFECT_SWEAT_DROP_SLOW";Name2="💧"},
	@{Name1="EFFECT_SWEAT_DROPS_FROM_BOTH_SIDES_MEDIUM";Name2="💦"},
	@{Name1="EFFECT_QUESTION_MARK";Name2="❓"},
	@{Name1="EFFECT_ANGRY";Name2="💢"},
	@{Name1="EFFECT_ANGRY_MIRRORED";Name2="💢"}
)

function ReadScript
{
	param([System.String]$scriptSource)
	
	if (Test-Path $scriptSource) {
		$scriptData = Get-Content $scriptSource
	} else {
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
	$effectActor = ""
	$thisFace = "Normal"
	$TextInfo = (Get-Culture).TextInfo
	
	$finalLines = New-Object System.Collections.ArrayList
	
	foreach ($command in $commands) {
		if ($command -like "*message_Talk*") {
			$mRes = $command -match "`"(.*)`""
			if ($Matches.Length > 1) {
				$message = $Matches[1] -replace "\[CS:X\]"," **" -replace "\[CR\]","** " -replace "\[K\]"," "
				while ($message -like "*` ` *") {
					$message = $message -replace "` ` ","` "
				}
				$thisLine = "``" + $thisFace + "``` " + $thisEffect + "`n" + $message
				if ($thisActor -eq "") {
					$thisLine = "💬: " + $thisLine
				} else {
					$thisLine = "``" + $thisActor + "``: " + $thisLine
				}
				$finalLines.Add($thisLine)
				$thisEffect = ""
			}
		} elseif ($command -like "message_ResetActor*") {
			$thisActor = ""
			$thisFace = ""
		} elseif ($command -like "*SetEffect*") {
			$mRes = $command -match "(EFFECT_.*)"
			if ($Matches.Length > 1) {
				foreach ($rEffect in $globalEffects) {
					if ($rEffect -eq $Matches[1]) {
						$thisEffect = $rEffect
					}
				}
			}
		} elseif ($command -like "*with*actor ACTOR_*") {
			$mRes = $command -match "(ACTOR_.*)"
			if ($Matches.Length > 1) {
				$effectActor = $Matches[1]
			}
		} elseif ($command -like "*message_SetFace*") {
			$trimmed = $command -replace "message_SetFace\(","" -replace "\)",""
			$arguments = ($trimmed -split ",").Trim()
			$thisActor = $arguments[0]
			$id = $arguments[1] -replace "FACE_",""
			$thisFace = $id.substring(0,1).toupper()+$id.substring(1).tolower()
			if ($thisFace -eq "Teary_eyed") {
				$thisFace = "Teary-Eyed"
			}
		}
	}
	
	return $finalLines
}

