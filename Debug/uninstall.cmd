@echo off
echo ****************************************************************
echo Uninstall
echo ****************************************************************

Set ScriptName=BGInfo
call :pDelFolder "%ProgramFiles%\%ScriptName%"
call :pDelFolder "%ProgramFiles(x86)%\%ScriptName%"
call :pDelFolder "%ProgramW6432%\%ScriptName%"
call :pDelFiles  "%windir%\System32\oobe\info\backgrounds\"
rem del /F /Q "%windir%\System32\oobe\info\backgrounds\*.*"

%windir%\system32\schtasks.exe /Delete /tn %ScriptName% /F  /HRESULT

REG DELETE HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP /f
REG DELETE HKLM\SOFTWARE\%ScriptName% /f
REG DELETE HKLM\SOFTWARE\WOW6432Node\%ScriptName% /f
REG DELETE HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run /v %ScriptName% /f



%~d0%~p0\CheckInstall.cmd 

pause
:pDelFile
set checkFile=%1
if Exist %checkFile%  echo Deleting file %checkFile% & del /F /Q %checkFile%
goto :EOF
:pDelFolder
set checkFolder=%1
if Exist %checkFolder%  echo Deleting folder %checkFolder% & RD /S /Q %checkFolder%
goto :EOF
:pDelFiles
set checkFiles=%1
Set Cnt=0
For %%I In (%checkFiles%\*) Do Set /A Cnt += 1
if %Cnt% NEQ 0 echo Deleting all files in folder %checkFiles% & del /F /Q %checkFiles%\*.*
goto :EOF
