# Set-ExecutionPolicy Unrestricted -Force
# Get-ExecutionPolicy
#
# The Script sets custom background Images for the Desktop and the Lock Screen by leveraging the new feature of PersonalizationCSP that is only available in the Windows 10 v1703 aka Creators Update and later build versions #
# Note: The Image File names can be anything you wish, however the Image resolution that I have been using for my environment was 3840X2160 (not sure if that matters though) #
# Applicable only for Windows 10 v1703 and later build versions #
# Script also assumes that you have alreadt copied over the Desktop and LockScreen Images to the C:\OEMFiles\ folder and are named as Desktop.jpg and LockScreen.jpg respectively #

$RegKeyPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP"

$DesktopPath = "DesktopImagePath"
$DesktopStatus = "DesktopImageStatus"
$DesktopUrl = "DesktopImageUrl"
$LockScreenPath = "LockScreenImagePath"
$LockScreenStatus = "LockScreenImageStatus"
$LockScreenUrl = "LockScreenImageUrl"

$StatusValue = "1"
$DesktopImageValue = "c:\windows\system32\oobe\info\backgrounds\DESKTOP-SCI2A3P-1920x1080.jpg"  #Change as per your needs
$LockScreenImageValue = "c:\windows\system32\oobe\info\backgrounds\DESKTOP-SCI2A3P-1920x1080.jpg"  #Change as per your needs

IF(!(Test-Path $RegKeyPath))

  {

    New-Item -Path $RegKeyPath -Force | Out-Null

    New-ItemProperty -Path $RegKeyPath -Name $DesktopStatus -Value $StatusValue -PropertyType DWORD -Force | Out-Null
    New-ItemProperty -Path $RegKeyPath -Name $LockScreenStatus -Value $StatusValue -PropertyType DWORD -Force | Out-Null
    New-ItemProperty -Path $RegKeyPath -Name $DesktopPath -Value $DesktopImageValue -PropertyType STRING -Force | Out-Null
    New-ItemProperty -Path $RegKeyPath -Name $DesktopUrl -Value $DesktopImageValue -PropertyType STRING -Force | Out-Null
    New-ItemProperty -Path $RegKeyPath -Name $LockScreenPath -Value $LockScreenImageValue -PropertyType STRING -Force | Out-Null
    New-ItemProperty -Path $RegKeyPath -Name $LockScreenUrl -Value $LockScreenImageValue -PropertyType STRING -Force | Out-Null
    
    }

 ELSE {

        New-ItemProperty -Path $RegKeyPath -Name $DesktopStatus -Value $Statusvalue -PropertyType DWORD -Force | Out-Null
        New-ItemProperty -Path $RegKeyPath -Name $LockScreenStatus -Value $value -PropertyType DWORD -Force | Out-Null
        New-ItemProperty -Path $RegKeyPath -Name $DesktopPath -Value $DesktopImageValue -PropertyType STRING -Force | Out-Null
        New-ItemProperty -Path $RegKeyPath -Name $DesktopUrl -Value $DesktopImageValue -PropertyType STRING -Force | Out-Null
        New-ItemProperty -Path $RegKeyPath -Name $LockScreenPath -Value $LockScreenImageValue -PropertyType STRING -Force | Out-Null
        New-ItemProperty -Path $RegKeyPath -Name $LockScreenUrl -Value $LockScreenImageValue -PropertyType STRING -Force | Out-Null
    }