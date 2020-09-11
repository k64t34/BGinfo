using System;
using System.IO;
using System.Reflection;
//using System.Net;
//using System.Net.Sockets;
using Microsoft.Win32;
using System.Windows.Forms;
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
        static String ScriptVersion;
        const String ProjectName = "BGInfo";
        const String reg_ScreenWidth = "ScreenWidth";
        const String reg_ScreenHright = "ScreenHeight";
        const String reg_HostName = "Hostname";
        const String reg_HostDescription = "Description";
        const String reg_BGInfoversion = "version";
        static void ShowMessage() { ShowMessage(ErrorTxt); }
        static void ShowMessage(string Text) { MessageBox.Show(Text, ProjectName, MessageBoxButtons.OK, MessageBoxIcon.Error); }
        static void LogError() { LogError(ErrorTxt); }
        static void LogError(string Text)
        {
            const string LogName = "Application";
            try
            {
                if (!EventLog.SourceExists(ScriptName))
                    EventLog.CreateEventSource(ScriptName, LogName);
                using (EventLog eventLog = new EventLog(LogName))
                {
                    eventLog.Source = ScriptName;
                    eventLog.WriteEntry(Text, EventLogEntryType.Error);
                }
            }
            catch { }
        }
        /// <summary>
        /// 
        /// </summary>
        static void Main()
        {
            if (SystemInformation.BootMode != 0/*Normal boot mode must equil zerro*/) return;
            String ScriptFullPathName = Application.ExecutablePath;
            ScriptName = Path.GetFileNameWithoutExtension(ScriptFullPathName);
            Process[] SelfProc = Process.GetProcessesByName(ScriptName);
            if (SelfProc.Length > 1) return; // if current exist running the same instance of program, then exiting			
            String ScriptFolder = Path.GetDirectoryName(ScriptFullPathName);
            RegistryKey reg;
            String FolderOOBEBGImage = Environment.GetEnvironmentVariable("windir") + @"\System32\oobe\info\backgrounds\";
            string LockScreenImage = "LockScreenImage.jpg";
            const string DesktopScriptName = "DeskTopBGInfo.exe";
            const String regHKLM__CSPKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP\";
            const String regHKLM__Run = @"Software\Microsoft\Windows\CurrentVersion\Run\";
            String regHKLM__Project = @"Software\" + ProjectName;
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
            SelfProc = Process.GetProcessesByName("explorer");
            if (SelfProc.Length == 0)
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
                //BGInfo.Info.hostName = Dns.GetHostName().ToUpper();
                BGInfo.Info.hostName = Environment.GetEnvironmentVariable("COMPUTERNAME");
            }
            /*catch (SocketException e)
            {
                ErrorTxt = e.Source + " " + e.Message; if (ProgramMode == 0) ShowMessage(); else LogError(); return;
            }*/
            catch (Exception e){ErrorTxt = e.Source + " " + e.Message; if (ProgramMode == 0) ShowMessage(); else LogError(); return;}
            try {//Get Host description   HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters   srvcomment             
                reg = regHKLM.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters", true);
                BGInfo.Info.hostDescription = ((string)reg.GetValue("srvcomment", ""));
            }         
            catch (Exception e){ErrorTxt = e.Source + " " + e.Message; if (ProgramMode == 0) ShowMessage(); else LogError(); return;}
            //get version
            //ScriptVersion
            Version v=  System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            ScriptVersion = v.Major.ToString();
            ScriptVersion = v.Major.ToString();
            // **************************************************
            // Installation
            // **************************************************
            if (ProgramMode == 0) //Install
            {
                ProcessStartInfo startInfo;
                #region LockscreenBGinfofile
                //Copy program LockscreenBGinfofile to %ProgramFiles%\%ProjectName%
                String ProgramFiles = Environment.GetEnvironmentVariable("ProgramFiles") + "\\" + ProjectName;
                if (string.Compare(ProgramFiles, ScriptFolder, true) != 0)
                {
                    if (!Directory.Exists(ProgramFiles))
                    {
                        try { Directory.CreateDirectory(ProgramFiles); }
                        catch (Exception e) { ErrorTxt = e.ToString(); ShowMessage(); return; }
                        if (!Directory.Exists(ProgramFiles)) { ErrorTxt = "Не удалось создать папку\n" + ProgramFiles; ShowMessage(); return; }
                    }
                    try { File.Copy(ScriptFullPathName, Path.Combine(ProgramFiles, ScriptName + ".exe"), true); }
                    catch (Exception e) { ErrorTxt = e.ToString(); ShowMessage(); return; }
                    if (!File.Exists(Path.Combine(ProgramFiles, ScriptName + ".exe"))) { ShowMessage("Не удалось скопировать файл \n" + ScriptFullPathName + "\nв папку\n" + ProgramFiles); return; }
                    ScriptFullPathName = Path.Combine(ProgramFiles, ScriptName + ".exe");
                    #region Sheduler
                    // Add task to Sheduler SCHTASKS / create / SC ONSTART / TN BGInfo / TR  "C:\Program Files\LockScreenWallpaper\LockScreenWallpaper.exe" / F / NP / RL HIGHEST 

                    startInfo = new ProcessStartInfo();
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
                    startInfo.Arguments = "/create /SC ONSTART /TN " + ProjectName + " /TR  \"" + ScriptFullPathName + "\" /F /NP /RL HIGHEST";
                    try
                    {
                        using (Process exeProcess = Process.Start(startInfo))
                        {
                            exeProcess.WaitForExit();
                            if (exeProcess.ExitCode != 0) { ErrorTxt = "Ошибка при созаднии задания " + ScriptName + " в планировщике \n" + startInfo.FileName + "\n" + startInfo.Arguments; ShowMessage(); return; }
                        }
                    }
                    catch (Exception e)
                    {
                        ErrorTxt = e.ToString(); ShowMessage(); return;
                    }

                    //TODO:Check that task is realy exist in scheduler
                    //SCHTASKS.exe /query /tn bginfo | echo % ERRORLEVEL %
                    #endregion
                }
                #endregion
                #region DesktopBGinfo.exe
                //Copy program DesktopBGinfo.exe file to %ProgramFiles%\%ProjectName%
                if (!File.Exists(Path.Combine(ProgramFiles, DesktopScriptName)))
                    if (File.Exists(Path.Combine(ScriptFolder, DesktopScriptName)))
                    {
                        try { File.Copy(Path.Combine(ScriptFolder, DesktopScriptName), Path.Combine(ProgramFiles, DesktopScriptName), true); }
                        catch (Exception e) { ErrorTxt = e.ToString(); ShowMessage(); return; }
                        try
                        {
                            reg = regHKLM.CreateSubKey(regHKLM__Run, true);
                            reg.SetValue(ProjectName, Path.Combine(ProgramFiles, DesktopScriptName), RegistryValueKind.String);
                            if (reg.GetValue(ProjectName) == null) { ErrorTxt = "Запись в реестр не удалась\n" + reg.Name; ShowMessage(); return; }
                        }
                        catch (Exception e) { ErrorTxt = e.ToString(); ShowMessage(); return; }
                       
                        #region Run DesktopBGinfo.exe
                        //Run DesktopBGinfo.exe                                                
                        startInfo = new ProcessStartInfo();
                        startInfo.CreateNoWindow = true;
                        startInfo.UseShellExecute = false;
                        startInfo.FileName = Path.Combine(ProgramFiles, DesktopScriptName);                            
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        try
                        {
                            using (Process exeProcess = Process.Start(startInfo))
                            {
                                exeProcess.WaitForExit();
                                if (exeProcess.ExitCode != 0) { ErrorTxt = "Ошибка запуска " + DesktopScriptName + "\n" + exeProcess.ExitCode.ToString();ShowMessage();  }
                            }
                        }
                        catch (Exception e)
                        { { ErrorTxt = "Ошибка запуска " + DesktopScriptName + "\n" + e.Message; ShowMessage(); } }

                        #endregion
                    }
                #endregion
                #region LockScreenImage
                //Copy image LockScreenImage file to \windows\system32\OOBE\info\backgroud
                if (File.Exists(Path.Combine(ScriptFolder, LockScreenImage)))
                {
                    if (!Directory.Exists(FolderOOBEBGImage))
                        {
                        try {Directory.CreateDirectory(FolderOOBEBGImage);}
                        catch (Exception e)
                            {
                            /*ErrorTxt = e.Message;*/
                            ShowMessage(e.Message);
                            return ; }
                        }                    
                    if (!Directory.Exists(FolderOOBEBGImage)) { ErrorTxt = "Не удалось создать папку\n" + FolderOOBEBGImage; ShowMessage(); return; }
                    try { File.Copy(Path.Combine(ScriptFolder, LockScreenImage), Path.Combine(FolderOOBEBGImage, LockScreenImage), true); }
                    catch (Exception e) { ErrorTxt = e.ToString(); ShowMessage(); return; }
                    if (!File.Exists(Path.Combine(FolderOOBEBGImage, LockScreenImage))) { ShowMessage("Не удалось скопировать файл \n" + LockScreenImage + "\nв папку\n" + FolderOOBEBGImage); return; }
                }
                #endregion                
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
                    reg.SetValue(reg_ScreenWidth, BGInfo.Info.ScreenWidth, RegistryValueKind.String);
                    if (reg.GetValue(reg_ScreenWidth) == null) { ErrorTxt = "Запись в реестр не удалась\n" + reg.Name; ShowMessage(); return; }
                    reg.SetValue(reg_ScreenHright, BGInfo.Info.ScreenHeight, RegistryValueKind.String);
                    if (reg.GetValue(reg_ScreenHright) == null) { ErrorTxt = "Запись в реестр не удалась\n" + reg.Name; ShowMessage(); return; }
                    reg.SetValue(reg_HostName, BGInfo.Info.hostName, RegistryValueKind.String);
                    if (reg.GetValue(reg_HostName) == null) { ErrorTxt = "Запись в реестр не удалась\n" + reg.Name; ShowMessage(); return; }
                    reg.SetValue(reg_HostDescription, BGInfo.Info.hostDescription, RegistryValueKind.String);
                    if (reg.GetValue(reg_HostDescription) == null) { ErrorTxt = "Запись в реестр не удалась\n" + reg.Name; ShowMessage(); return; }
                }
                catch (Exception e)
                {
                    ErrorTxt = e.ToString(); ShowMessage(); return;
                }
   
                //Delete previos file if exist
                //FileWallpaper = FolderOOBEBGImage + BGInfo.Info.hostName + "-" + BGInfo.Info.ScreenWidth + "x" + BGInfo.Info.ScreenHeight + ".jpg";
               
            }
            // **************************************************
            // On Boot
            // **************************************************
            if (ProgramMode <= 1) //Boot
            {
                try //read screen resolution and other parameters from registry
                {
                    reg = regHKLM.CreateSubKey(regHKLM__Project, true);
                    BGInfo.Info.ScreenWidth = Int32.Parse((string)reg.GetValue(reg_ScreenWidth, "1920"));
                    BGInfo.Info.ScreenHeight = Int32.Parse((string)reg.GetValue(reg_ScreenHright, "1080"));
                    BGInfo.Info.hostName = (string)reg.GetValue(reg_HostName,null);
                    BGInfo.Info.hostDescription = (string)reg.GetValue(reg_HostDescription, null);
                }
                catch (Exception e)
                {
                    ErrorTxt = e.ToString(); if (ProgramMode == 0) ShowMessage(); else LogError(); return;
                }
                // Check wallpaper folder
                if (!Directory.Exists(FolderOOBEBGImage))
                {
                    try { Directory.CreateDirectory(FolderOOBEBGImage); }
                    catch (Exception e) { ErrorTxt = e.ToString(); if (ProgramMode == 0) ShowMessage(); else LogError(); return; }
                    if (!Directory.Exists(FolderOOBEBGImage)) return;
                }
                // Generate name for wallpaper file
                FileWallpaper = FolderOOBEBGImage + BGInfo.Info.hostName + "-" + BGInfo.Info.ScreenWidth + "x" + BGInfo.Info.ScreenHeight + ".jpg";
                // Generate name for wallpaper file
                //
#if DEBUG
                if (true)
#else
                if (!File.Exists(FileWallpaper) || ProgramMode==0)
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
                        resultBGImage = BGInfo.Image.Create(FileWallpaper);
                    }
                    if (!resultBGImage) { ErrorTxt = "Ну удалось создать новый файл изображения\n" + FileWallpaper + "\n" + ErrorTxt; if (ProgramMode == 0) ShowMessage(); else LogError(); return; }

                    try
                    {
                        const string reg_LockScreenImagePath = "LockScreenImagePath";
                        reg = regHKLM.CreateSubKey(regHKLM__CSPKey, true);
                        reg.SetValue(reg_LockScreenImagePath, FileWallpaper, RegistryValueKind.String);
                        if (reg.GetValue(reg_LockScreenImagePath) == null) { ErrorTxt = "Запись в реестр не удалась\n" + reg.Name; ShowMessage(); return; }
                        const string reg_LockScreenImageUrl = "LockScreenImageUrl";
                        reg.SetValue(reg_LockScreenImageUrl, FileWallpaper, RegistryValueKind.String);
                        if (reg.GetValue(reg_LockScreenImageUrl) == null) { ErrorTxt = "Запись в реестр не удалась\n" + reg.Name; ShowMessage(); return; }
                        const string reg_LockScreenImageStatus = "LockScreenImageStatus";
                        reg.SetValue(reg_LockScreenImageStatus, 1, RegistryValueKind.DWord);
                    }
                    catch (Exception e)
                    {
                        ErrorTxt = e.ToString(); if (ProgramMode == 0) ShowMessage(); else LogError(); return;
                    }
                }
            }
            if (ProgramMode == 0) MessageBox.Show("Установка " + ScriptName + " прошла успешно", ProjectName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

