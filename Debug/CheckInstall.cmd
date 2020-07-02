@echo off
echo ****************************************************************
echo Check Install
echo ****************************************************************
Set ProjectName=BGInfo
Set ScriptLockscreenName=LockscreenBGinfo
Set ScriptDesktopName=DesktopBGinfo

call :pCheckFile "%ProgramFiles%\%ProjectName%\%ScriptLockscreenName%.exe"
call :pCheckFile "%ProgramFiles(x86)%\%ProjectName%\%ScriptLockscreenName%.exe"
call :pCheckFile "%ProgramW6432%\%ProjectName%\%ScriptLockscreenName%.exe"
call :pCheckFile "%ProgramFiles%\%ProjectName%\%ScriptDesktopName%.exe"
call :pCheckFile "%ProgramFiles(x86)%\%ProjectName%\%ScriptDesktopName%.exe"
call :pCheckFile "%ProgramW6432%\%ProjectName%\%ScriptDesktopName%.exe"

call :pCheckFiles "%windir%\System32\oobe\info\backgrounds"


echo Is task %ProjectName%  in scheduler?
%windir%\system32\schtasks.exe /Query /tn %ProjectName%  >nul 2> nul
IF %ERRORLEVEL% EQU 0 (echo     Yes) else echo         No

call :pCheckReg HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP
call :pCheckReg HKLM\SOFTWARE\%ProjectName%
call :pCheckReg HKLM\SOFTWARE\WOW6432Node\%ProjectName%
call :pCheckRegParam  HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run  %ProjectName%

pause
goto :EOF

:pCheckFile
set checkFile=%1
echo Is file exist %checkFile%?
if Exist %checkFile%  (ECHO     Yes) ELSE echo         No
goto :EOF
:pCheckFiles
set checkFile=%1
echo Is any file exist in folder %checkFile%?
Set Cnt=0
For %%I In (%checkFile%\*) Do Set /A Cnt += 1
if %Cnt% NEQ 0 (echo     Yes - %Cnt% files &dir %checkFile% /b ) else echo         No
goto :EOF
:pCheckReg
set checkItem=%1
echo Is key registry exist  %checkItem%?
REG QUERY %checkItem%  >nul 2> nul
IF %ERRORLEVEL% NEQ 0  (echo         No) else echo     Yes&REG QUERY %checkItem% 
goto :EOF
:pCheckRegParam 
set checkItem=%1
set checkParam=%2
echo Is param %checkParam% exist in key registry %checkItem%?
REG QUERY %checkItem% /v %checkParam% >nul 2> nul
IF %ERRORLEVEL% NEQ 0  (echo         No) else echo     Yes
goto :EOF