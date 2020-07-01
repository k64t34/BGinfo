using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Security.Principal;
using System.ComponentModel;

namespace LockscreenBGInfo
{
    class Program
    {
        //[STAThread]
        static String ErrorTxt;
        static String ScriptName;
        static String hostName;
        static int ScreenHeight, ScreenWidth;
       
        static void ShowMessage() { ShowMessage(ErrorTxt);}
        static void ShowMessage(string Text) { MessageBox.Show(Text, ScriptName, MessageBoxButtons.OK,MessageBoxIcon.Error);}
        static void LogError() { }
        /// <summary>
        /// 
        /// </summary>
        static void Main()
        {
            if (SystemInformation.BootMode != 0/*Normal boot mode must equil zerro*/) return;            
            String ScriptFullPathName = Application.ExecutablePath;
            ScriptName = Path.GetFileNameWithoutExtension(ScriptFullPathName);            
            Process[] Proc = Process.GetProcessesByName(ScriptName);
            if (Proc.Length > 1) return; // if current exist running the same instance of program, then exiting
            String ScriptFolder = Path.GetDirectoryName(ScriptFullPathName);
            RegistryKey reg;            
            String FolderOOBEBGImage = Environment.GetEnvironmentVariable("windir") + @"\System32\oobe\info\backgrounds\";
            string LockScreenImage = "LockScreenImage.jpg";
            const String strCSPKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP";
            String FileWallpaper;
            //SystemInformation.UserInteractive Значение true, если текущий процесс выполняется в интерактивном режиме. В противном случае — значение false.
            //SystemInformation.TerminalServerSession Значение true, если вызывающий процесс сопоставлен с клиентским сеансом служб терминалов. В противном случае — значение false.
            //SystemInformation.ComputerName Имя этого компьютера.
            //SystemInformation.UserDomainName Возвращает имя домена, которому принадлежит пользователь.
            //System.DirectoryServices.ActiveDirectory Получает объект Domain для действующих учетных данных текущего пользователя для контекста безопасности, в котором выполняется приложение.
            //Domain.GetComputerDomain Есть нюансы https://docs.microsoft.com/ru-ru/dotnet/api/system.directoryservices.activedirectory.domain.getcomputerdomain?view=dotnet-plat-ext-3.1
            // **************************************************
            // Detect Mode
            // **************************************************
            int ProgramMode; // 0 - Install, 1 - Boot, 2 - Desktop
            Proc = Process.GetProcessesByName("explorer");
            if (Proc.Length == 0) 
                ProgramMode = 1;// boot
            else if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)) 
                ProgramMode = 0;// install
            else
                ProgramMode = 2;// desktop
            RegistryKey regHKLM = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

            //https://stackoverflow.com/questions/2819934/detect-windows-version-in-net
            //OperatingSystem os = Environment.OSVersion;
            //if (os.Version.Major != 10)
            //{
            //if (ProgramMode == 0) ShowMessage("Эта программа предназначена для ОС Windows 10\nОбнаружена ОС Windows "+os.Version.Major.ToString());
            //    #if !DEBUG
            //        return;
            //    #endif
            //}
            try // Get hostname
            {
                hostName = Dns.GetHostName().ToUpper();
            }
            catch (SocketException e)
            {                
                ErrorTxt = e.Source + " " + e.Message; if (ProgramMode == 0) ShowMessage(); else LogError(); return;
            }
            catch (Exception e)
            {
                ErrorTxt = e.Source + " " + e.Message; if (ProgramMode == 0) ShowMessage(); else LogError(); return;
            }
            // **************************************************
            // Installation
            // **************************************************
            if (ProgramMode == 0)
            {
                //Copy program file to %ProgramFiles%\%ScriptName%
                String ProgramFiles = Environment.GetEnvironmentVariable("ProgramFiles") + "\\" + ScriptName;
                if (ProgramFiles != ScriptFolder)
                {
                    Directory.CreateDirectory(ProgramFiles);
                    File.Copy(ScriptFullPathName, Path.Combine(ProgramFiles, ScriptName + ".exe"), true);                    
                    if (File.Exists(Path.Combine(ScriptFolder, LockScreenImage))) 
                    { 
                        Directory.CreateDirectory(FolderOOBEBGImage);
                        File.Copy(Path.Combine(ScriptFolder, LockScreenImage), Path.Combine(FolderOOBEBGImage, LockScreenImage), true);
                    }
                    ScriptFullPathName = Path.Combine(ProgramFiles, ScriptName + ".exe");
                    if (!File.Exists(ScriptFullPathName))
                    { ErrorTxt = "Не удалось скопировать файл \n" + ScriptFullPathName; ShowMessage(); return; }
                }
                // Get Screen Resolution
                // The Problem is get incorrect screen resolution if run from scheduler before logon
                //ScreenHeight = Screen.PrimaryScreen.Bounds.Height;
                //ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
                //int ScreenHeight = SystemInformation.PrimaryMonitorMaximizedWindowSize.Height;
                //int ScreenWidth = SystemInformation.PrimaryMonitorMaximizedWindowSize.Width;
                ScreenHeight = SystemInformation.PrimaryMonitorSize.Height;
                ScreenWidth = SystemInformation.PrimaryMonitorSize.Width;
                try
                {                    
                    reg = regHKLM.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                    reg.SetValue(ScriptName, ScriptFullPathName, RegistryValueKind.String);
                    if (reg.GetValue(ScriptName) == null) { ErrorTxt = "Запись в реестр не удалась\n"+ reg.Name; ShowMessage(); return; }
                        //Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run", ScriptName, ScriptFullPathName, RegistryValueKind.String);
                    reg = regHKLM.CreateSubKey(@"Software\" + ScriptName, true); //Компьютер\HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\BGInfo
                    reg.SetValue("ScreenWidth", ScreenWidth, RegistryValueKind.String);
                    if (reg.GetValue("ScreenWidth") == null) { ErrorTxt = "Запись в реестр не удалась\n" + reg.Name; ShowMessage(); return; }
                    reg.SetValue("ScreenHeight", ScreenHeight, RegistryValueKind.String);
                    if (reg.GetValue("ScreenHeight") == null) { ErrorTxt = "Запись в реестр не удалась\n" + reg.Name; ShowMessage(); return; }
                }
                catch (Exception e)
                {
                    ErrorTxt = e.ToString(); ShowMessage(); return;
                }
                // Add task to Sheduler SCHTASKS / create / SC ONSTART / TN BGInfo / TR  "C:\Program Files\LockScreenWallpaper\LockScreenWallpaper.exe" / F / NP / RL HIGHEST 
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                //RedirectStandardError
                //RedirectStandardInput
                //RedirectStandardOutput
                //StandardErrorEncoding
                //StandardInputEncoding
                //StandardOutputEncoding
                startInfo.FileName = Environment.GetEnvironmentVariable("windir") + @"\System32\SCHTASKS.exe";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.Arguments = "/create /SC ONSTART /TN "+ ScriptName + " /TR  \"" +ScriptFullPathName +"\" /F /NP /RL HIGHEST";                
                try 
                {
                    using (Process exeProcess = Process.Start(startInfo))
                    {
                        exeProcess.WaitForExit();
                        if (exeProcess.ExitCode!=0) { ErrorTxt = "Ошибка при созаднии задания "+ ScriptName + " в планировщике \n" + startInfo.FileName + "\n"+startInfo.Arguments ; ShowMessage(); return; }
                    }                    
                }
                catch (Exception e)
                {
                    ErrorTxt = e.ToString(); ShowMessage(); return;
                }
                //TODO:Check that task is realy exist in scheduler
                //SCHTASKS.exe /query /tn bginfo | echo % ERRORLEVEL %
            }
            // **************************************************
            // On Boot
            // **************************************************
            if (ProgramMode <= 1)
            {
                try //read screen resolution from registry
                {
                    reg = regHKLM.CreateSubKey(@"Software\" + ScriptName, true);
                    ScreenWidth = Int32.Parse((string)reg.GetValue("ScreenWidth", "1920"));
                    ScreenHeight = Int32.Parse((string)reg.GetValue("ScreenHeight", "1080"));
                }
                catch (Exception e)
                {
                    ErrorTxt = e.ToString(); if (ProgramMode == 0) ShowMessage(); else LogError(); return;
                }
                // Check wallpaper folder
                if (!Directory.Exists(FolderOOBEBGImage))
                {
                    try
                    {
                        Directory.CreateDirectory(FolderOOBEBGImage);
                    }
                    catch (Exception e)
                    {
                        ErrorTxt = e.ToString(); if (ProgramMode == 0) ShowMessage(); else LogError(); return;
                    }
                    if (!Directory.Exists(FolderOOBEBGImage)) return;
                }
                // Generate name for wallpaper file
                FileWallpaper = hostName + "-" + ScreenWidth + "x" + ScreenHeight + ".jpg";
                FileWallpaper = FolderOOBEBGImage + FileWallpaper;
                // Generate name for wallpaper file
                //
#if DEBUG
                if (true)
#else
                if (!File.Exists(FileWallpaper))
#endif
                { // Create new file from origin or blank
                    LockScreenImage = Path.Combine(FolderOOBEBGImage, LockScreenImage);
                    bool resultBGImage = false;
                    if (File.Exists(LockScreenImage))
                    {
                        resultBGImage = CopyBGImage(LockScreenImage, FileWallpaper);                        
                    }
                    else
                    {
                        //if wallpaper  пусто, то взять какой цвет заливки HKEY_CURRENT_USER\Control Panel\Colors\Background  reg_sz 216 81 113
                        resultBGImage = CreateBGImage(LockScreenImage, ColorTranslator.FromHtml("#FF004080"));
                    }
                    if (!resultBGImage) { ErrorTxt = "Ну удалось создать новый файл изображения\n" + LockScreenImage + "\n" + ErrorTxt; if (ProgramMode == 0) ShowMessage(); else LogError(); return; }
                    
                    try
                    {
                        reg = regHKLM.CreateSubKey(strCSPKey, true);
                        reg.SetValue("LockScreenImagePath", FileWallpaper, RegistryValueKind.String);
                        reg.SetValue("LockScreenImageUrl", FileWallpaper, RegistryValueKind.String);
                        reg.SetValue("LockScreenImageStatus", 1, RegistryValueKind.DWord);
                    }
                    catch (Exception e)
                    {
                        ErrorTxt = e.ToString(); if (ProgramMode == 0) ShowMessage(); else LogError(); return;
                    }
                }
            }            
            if (ProgramMode == 0)MessageBox.Show("Установка прошла успешно", ScriptName, MessageBoxButtons.OK, MessageBoxIcon.Information);            
        }
        
        

    }
    

}
