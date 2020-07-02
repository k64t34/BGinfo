using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Security.Principal;
using BGInfo;


namespace LockscreenBGInfo
{
    class Program
    {
        //[STAThread]
        static String ErrorTxt;
        static String ScriptName;
        const String ProjectName="BGInfo";
        const String reg_ScreenWidth = "ScreenWidth";
        const String reg_ScreenHright = "ScreenHeight";       
       
        static void ShowMessage() { ShowMessage(ErrorTxt);}
        static void ShowMessage(string Text) { MessageBox.Show(Text, ProjectName, MessageBoxButtons.OK,MessageBoxIcon.Error);}
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
            const String regHKLM__CSPKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP\";
            const String regHKLM__Run = @"Software\Microsoft\Windows\CurrentVersion\Run\";
            String regHKLM__Project = @"Software\"+ ProjectName;            
            const string DesktopScriptName = "DeskTopBGInfo.exe";
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
            int ProgramMode; // 0 - Install, 1 - Boot
            Proc = Process.GetProcessesByName("explorer");
            if (Proc.Length == 0)
                ProgramMode = 1;// boot
            else if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                ProgramMode = 0;// install
            else
                return;
           
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
                BGInfo.Info.hostName = Dns.GetHostName().ToUpper();
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
                //Copy program file to %ProgramFiles%\%ProjectName%
                String ProgramFiles = Environment.GetEnvironmentVariable("ProgramFiles") + "\\" + ProjectName;
                if (string.Compare(ProgramFiles, ScriptFolder, true) != 0)
                {
                    if (!Directory.Exists(ProgramFiles))
                    {
                        try
                        {
                            Directory.CreateDirectory(ProgramFiles);
                        }
                        catch (Exception e)
                        {
                            ErrorTxt = e.ToString(); ShowMessage(); return;
                        }
                        if (!Directory.Exists(ProgramFiles)) { ErrorTxt = "Не удалось создать папку\n" + ProgramFiles; ShowMessage(); return; }
                    }
                    //TODO:Try
                    File.Copy(ScriptFullPathName, Path.Combine(ProgramFiles, ScriptName + ".exe"), true);
                }
                if (!File.Exists(Path.Combine(ProgramFiles, DesktopScriptName)))
                    if (File.Exists(Path.Combine(ScriptFolder, DesktopScriptName)))
                    {
                        try
                        {
                            File.Copy(Path.Combine(ScriptFolder, DesktopScriptName), Path.Combine(ProgramFiles, DesktopScriptName), true);
                        }
                        catch (Exception e)
                        {
                            ErrorTxt = e.ToString(); ShowMessage(); return;
                        }
                        try
                        {
                            reg = regHKLM.CreateSubKey(regHKLM__Run, true);
                            reg.SetValue(ProjectName, Path.Combine(ProgramFiles, DesktopScriptName), RegistryValueKind.String);
                            if (reg.GetValue(ProjectName) == null) { ErrorTxt = "Запись в реестр не удалась\n" + reg.Name; ShowMessage(); return; }
                        }
                        catch (Exception e)
                        {
                            ErrorTxt = e.ToString(); ShowMessage(); return;
                        }
                    }
                if (File.Exists(Path.Combine(ScriptFolder, LockScreenImage))) 
                { 
                    Directory.CreateDirectory(FolderOOBEBGImage);
                    File.Copy(Path.Combine(ScriptFolder, LockScreenImage), Path.Combine(FolderOOBEBGImage, LockScreenImage), true);
                }
                ScriptFullPathName = Path.Combine(ProgramFiles, ScriptName + ".exe");
                if (!File.Exists(ScriptFullPathName))
                { ErrorTxt = "Не удалось скопировать файл \n" + ScriptFullPathName; ShowMessage(); return; }
                // Get Screen Resolution
                // The Problem is get incorrect screen resolution if run from scheduler before logon
                //ScreenHeight = Screen.PrimaryScreen.Bounds.Height;
                //ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
                //int ScreenHeight = SystemInformation.PrimaryMonitorMaximizedWindowSize.Height;
                //int ScreenWidth = SystemInformation.PrimaryMonitorMaximizedWindowSize.Width;
                BGInfo.Info.ScreenHeight = SystemInformation.PrimaryMonitorSize.Height;
                BGInfo.Info.ScreenWidth = SystemInformation.PrimaryMonitorSize.Width;
                try
                {
                    reg = regHKLM.CreateSubKey(regHKLM__Project, true);                    
                    reg.SetValue("ScreenWidth", BGInfo.Info.ScreenWidth, RegistryValueKind.String);
                    if (reg.GetValue("ScreenWidth") == null) { ErrorTxt = "Запись в реестр не удалась\n" + reg.Name; ShowMessage(); return; }
                    reg.SetValue("ScreenHeight", BGInfo.Info.ScreenHeight, RegistryValueKind.String);
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
                startInfo.Arguments = "/create /SC ONSTART /TN "+ ProjectName + " /TR  \"" +ScriptFullPathName +"\" /F /NP /RL HIGHEST";                
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
                    reg = regHKLM.CreateSubKey(regHKLM__Project, true);
                    BGInfo.Info.ScreenWidth = Int32.Parse((string)reg.GetValue("ScreenWidth", "1920"));
                    BGInfo.Info.ScreenHeight = Int32.Parse((string)reg.GetValue("ScreenHeight", "1080"));
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
                FileWallpaper = FolderOOBEBGImage + BGInfo.Info.hostName + "-" + BGInfo.Info.ScreenWidth + "x" + BGInfo.Info.ScreenHeight + ".jpg";                
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
                        resultBGImage = BGInfo.Image.Copy(LockScreenImage, FileWallpaper);                        
                    }
                    else
                    {
                        //if wallpaper  пусто, то взять какой цвет заливки HKEY_CURRENT_USER\Control Panel\Colors\Background  reg_sz 216 81 113
                        resultBGImage = BGInfo.Image.Create(FileWallpaper, ColorTranslator.FromHtml("#FF004080"));
                    }
                    if (!resultBGImage) { ErrorTxt = "Ну удалось создать новый файл изображения\n" + FileWallpaper + "\n" + ErrorTxt; if (ProgramMode == 0) ShowMessage(); else LogError(); return; }
                    
                    try
                    {
                        reg = regHKLM.CreateSubKey(regHKLM__CSPKey, true);
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
            if (ProgramMode == 0)MessageBox.Show("Установка "+ ScriptName + " прошла успешно", ProjectName, MessageBoxButtons.OK, MessageBoxIcon.Information);            
        }
        
        

    }
    

}

