function clientCmdBottomPrint (%message, %time, %hideBar)
{
	if ($bottomPrintActive)
	{
		if ($bottomPrintDlg::removePrintEvent != 0)
		{
			cancel ($bottomPrintDlg::removePrintEvent);
			$bottomPrintDlg::removePrintEvent = 0;
		}
	}
	else 
	{
		bottomPrintDlg.setVisible (1);
		$bottomPrintActive = 1;
	}
	BottomPrintText.setText (detag(%message));
	if (%hideBar)
	{
		bottomPrintBar.setVisible (0);
	}
	else 
	{
		bottomPrintBar.setVisible (1);
	}
	if (%time > 0)
	{
		$bottomPrintDlg::removePrintEvent = schedule (%time * 1000, 0, "clientCmdClearbottomPrint");
	}
}

function clientCmdCenterPrint (%message, %time, %size)
{
	if ($centerPrintActive)
	{
		if ($CenterPrintDlg::removePrintEvent != 0)
		{
			cancel ($CenterPrintDlg::removePrintEvent);
			$CenterPrintDlg::removePrintEvent = 0;
		}
	}
	else 
	{
		CenterPrintDlg.visible = 1;
		$centerPrintActive = 1;
	}
	CenterPrintText.setText ("<just:center>" @ detag(%message) @ "\n");
	if (%time > 0)
	{
		$CenterPrintDlg::removePrintEvent = schedule (%time * 1000, 0, "clientCmdClearCenterPrint");
	}
}

function directSelectInv (%index)
{
	if ($BuildingDisabled)
	{
		setActiveTool(%index);
		commandToServer ('UseTool', %index);
		return;
	}

	if (%index < 0)
	{
		scrollBricks (1);
		return;
	}
	if ($InvData[%index] > 0)
	{
		if ($ScrollMode == $SCROLLMODE_BRICKS)
		{
			if ($CurrScrollBrickSlot == %index && HUD_BrickActive.visible == 1)
			{
				setActiveInv (-1);
				HUD_BrickName.setText ("");
				commandToServer ('unUseTool');
				setScrollMode ($SCROLLMODE_NONE);
			}
			else 
			{
				setActiveInv (%index);
				$CurrScrollBrickSlot = %index;
				commandToServer ('useInventory', %index);
				if ($RecordingBuildMacro && isObject ($BuildMacroSO))
				{
					$BuildMacroSO.pushEvent ("Server", 'useInventory', %index);
				}
			}
		}
		else 
		{
			setScrollMode ($SCROLLMODE_BRICKS);
			setActiveInv (%index);
			$CurrScrollBrickSlot = %index;
			commandToServer ('useInventory', %index);
			if ($RecordingBuildMacro && isObject ($BuildMacroSO))
			{
				$BuildMacroSO.pushEvent ("Server", 'useInventory', %index);
			}
			HUD_BrickName.setText ($InvData[$CurrScrollBrickSlot].uiName);
		}
	}
	else 
	{
		%direction = 1;
		$CurrScrollBrickSlot -= 1;
		%i = 0;
		while (%i < $BSD_NumInventorySlots - 1)
		{
			$CurrScrollBrickSlot += %direction;
			if ($CurrScrollBrickSlot < 0)
			{
				$CurrScrollBrickSlot = $BSD_NumInventorySlots - 1;
			}
			if ($CurrScrollBrickSlot >= $BSD_NumInventorySlots)
			{
				$CurrScrollBrickSlot = 0;
			}
			if ($InvData[$CurrScrollBrickSlot] > 0)
			{
				break;
			}
			%i += 1;
		}
		if ($InvData[$CurrScrollBrickSlot] > 0)
		{
			setActiveInv ($CurrScrollBrickSlot);
			commandToServer ('useInventory', $CurrScrollBrickSlot);
			if ($RecordingBuildMacro && isObject ($BuildMacroSO))
			{
				$BuildMacroSO.pushEvent ("Server", 'useInventory', $CurrScrollBrickSlot);
			}
		}
		else if (isObject ($LastInstantUseData))
		{
			commandToServer ('InstantUseBrick', $LastInstantUseData);
			$InstantUse = 1;
			setActiveInv (-1);
			setScrollMode ($SCROLLMODE_BRICKS);
			HUD_BrickName.setText ($LastInstantUseData.uiName);
			return 1;
		}
		else 
		{
			%device = getWord (moveMap.getBinding (openBSD), 0);
			if (%device $= "Keyboard")
			{
				%hintKey = strupr (getWord (moveMap.getBinding (openBSD), 1));
			}
			else if (%device $= "Mouse0")
			{
				%hintKey = "MOUSE " @ strupr (getWord (moveMap.getBinding (openBSD), 1));
			}
			else if (%device $= "Joystick0")
			{
				%hintKey = "JOYSTICK " @ strupr (getWord (moveMap.getBinding (openBSD), 1));
			}
			else 
			{
				%hintKey = moveMap.getBinding (openBSD);
			}
			if ($BuildingDisabled)
			{
				clientCmdCenterPrint ("\c5Building is currently disabled.", 2);
			}
			else 
			{
				clientCmdCenterPrint ("\c5You don\'t have any bricks!\nPress " @ %hintKey @ " to open the brick selector.", 3);
			}
			return 0;
		}
		if ($ScrollMode != $SCROLLMODE_BRICKS)
		{
			setScrollMode ($SCROLLMODE_BRICKS);
		}
	}
	return 1;
}

function Fixes_ToggleMinigame(%val)
{
	if(%val)
	{
		$Fixes_AltActivate = !$Fixes_AltActivate;
		clientCmdSetPaintingDisabled($Fixes_AltActivate);
		clientCmdSetBuildingDisabled($Fixes_AltActivate);
	}
}

function toggleZoom (%val)
{
	if (!%val)
	{
		if($ZoomOn)
		{
			$ZoomOn = 0;
			setFov ($pref::Player::defaultFov);
		}
		else
		{
			$ZoomOn = 1;
			setFov ($Pref::player::CurrentFOV);
		}
		
	}
}

function toggleFreeLook (%val)
{
	if($lastToggleFreeLook <= (getSimTime() - 250) && !%val)
	{
		$mvFreeLook = 0;
		return;
	}

	$lastToggleFreeLook = getSimTime();
	if(%val)
	{
		$mvFreeLook = !$mvFreeLook;
	}
}

function emoteAlarm (%val)
{
	if (%val && !isEventPending($Fixes_AlarmSched))
	{
		$Fixes_AlarmSched = schedule(1100,0,"emoteAlarm",true);
		commandToServer('alarm');
	}
	else
	{
		cancel($Fixes_AlarmSched);
	}
}

function Fixes_bindConsole()
{
	GlobalActionMap.unbind(keyboard, "tilde");
	GlobalActionMap.bind(keyboard, "f2", toggleConsole);
}

if(!$Fixes_Executed)
{
	$RemapDivision[$RemapCount] = "Fixes";
	$RemapName[$RemapCount] = "Toggle Minigame";
	$RemapCmd[$RemapCount] = "Fixes_ToggleMinigame";
	$RemapCount += 1;
	
	schedule(1000,0,"Fixes_bindConsole");
	$Fixes_Executed = true;
}


//NEVER FINISHED FUNCTION DUMPER!!! TODO!!!
$DumpMatch::TempFile = "config/functiondump.txt";
$DumpMatch::Logger = isObject($DumpMatch::Logger) ? $DumpMatch::Logger : new consoleLogger("",$DumpMatch::TempFile);
$DumpMatch::File = isObject($DumpMatch::File) ? $DumpMatch::File : new fileObject();
function dumpFunctions(%a,%b)
{
	%search = %a;
	if(%b !$= "")
	{
		%namespace = %a;
		%search = %b;
	}

	%pre = %search;
	%post = "";
	%wildPos = strPos(%search,"*");
	if(%wildPos >= 0)
	{
		%wild = true;
		%pre = getSubStr(%search, 0, %wildPos); 
		%post = getSubStr(%search, %wildPos + 1, 999999);
	}
	%pre = " " @ %pre;
	%post = %post @ "(";

	$DumpMatch::Logger.attach();
	if(%namespace !$= "")
	{
		dumpFunctionsMatch("*","*","*");
		//seperate parsing method for this
	}
	else
	{
		dumpConsoleFunctions();
	}
	$DumpMatch::Logger.detach();

	%s = "";
	%file = $DumpMatch::File;
	if(%file.openForRead($DumpMatch::TempFile))
	{
		while(!%file.isEOF())
		{
			%s = %s NL %file.readLine();
		}
		%file.close();
	}
	else
	{
		echo("failed to open");
	}

	%foundStart = 0;
	%foundLen = 0;
	%len = strLen(%s);
	%preLen = strLen(%pre);
	%safetyA = 0;
	while(%safetyA++ <= 50)
	{
		%foundStart = striPos(%s,%pre,%foundStart + 1);
		%foundLen = striPos(%s,%post,%foundStart + %preLen) - %foundStart;
		if(%foundStart < 0)
		{
			break;
		}
		
		%name = getSubStr(%s,%foundStart,%foundLen);
		if(!(%wild || %foundLen == %preLen) || %foundLen < 0 || striPos(%name, "\n") >= 0)
		{
			continue;
		}

		//go through comments
		echo("FOUND" SPC %entry);
	}
	echo("DONE");
}

//allows you to launch a gamemode even if you don't have required files lol
function GameModeGui::ClickGameMode (%this, %idx)
{
	%idx = mFloor (%idx);
	$GameModeGui::CurrGameModeIdx = %idx;
	$Pref::Gui::SelectedGameMode = $GameModeGui::GameMode[%idx];
	%filename = $GameModeGui::GameMode[%idx];
	%path = filePath (%filename);
	%descriptionFile = %path @ "/description.txt";
	%previewFile = %path @ "/preview.jpg";
	%thumbFile = %path @ "/thumb.jpg";
	%displayName = %path;
	%displayName = strreplace (%displayName, "Add-Ons/", "");
	%displayName = getSubStr (%displayName, strlen ("gamemode_"), 999);
	%displayName = strreplace (%displayName, "_", " ");
	%i = 0;
	while (%i < $GameModeGui::GameModeCount)
	{
		if (strlen ($GameModeGui::MissingAddOns[%i]) > 0)
		{
			%cmd = "GameModeGui_Swatch" @ %i @ ".setColor(\"255 0 0 110\");";
		}
		else 
		{
			%cmd = "GameModeGui_Swatch" @ %i @ ".setColor(\"0 0 0 110\");";
		}
		eval (%cmd);
		%i += 1;
	}
	%cmd = "GameModeGui_Swatch" @ %idx @ ".setColor(\"255 255 255 110\");";
	eval (%cmd);
	%text = "";
	%text = %text @ "<font:arial:10><br>";
	%text = %text @ "<lmargin:" @ GameModeGui.mainGutter @ "><bitmap:" @ %previewFile @ ">";
	%text = %text @ "<lmargin:" @ GameModeGui.mainGutter + 256 + GameModeGui.mainGutter + 8 @ "><font:impact:47><color:FFFFFF>" @ %displayName @ "<BR>";
	%text = %text @ "<lmargin:" @ GameModeGui.mainGutter + 256 + GameModeGui.mainGutter + GameModeGui.mainGutter + 16 @ ">" @ (GameModeGui_Description @ %idx).getText ();
	GameModeGui_LongDescription.setText (%text);
	if (strlen ($GameModeGui::MissingAddOns[%idx]) > 0)
	{
		// GameModeGui_SelectButton.setVisible (0);
		GameModeGui_LongDescriptionBG.setColor ("255 0 0 110");
	}
	else 
	{
		GameModeGui_LongDescriptionBG.setColor ("0 0 0 110");
	}
	GameModeGui_SelectButton.setVisible (1); // moved outside to allow both trees to use it
}