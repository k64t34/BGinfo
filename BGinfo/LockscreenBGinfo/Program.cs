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
        static void ShowMessage() { ShowMessage(Log.ErrorTxt); }
        static void ShowMessage(string Text) { MessageBox.Show(Text, BGInfo.Info.ProjectName, MessageBoxButtons.OK, MessageBoxIcon.Error); }        
        /// <summary>
        /// 
        /// </summary>
        static void Main()
        {
            if (SystemInformation.BootMode != 0/*Normal boot mode must equil zerro*/) return;
            String ScriptFullPathName = Application.ExecutablePath;
            Log.ScriptName = Path.GetFileNameWithoutExtension(ScriptFullPathName);
            Process[] SelfProc = Process.GetProcessesByName(Log.ScriptName);
            if (SelfProc.Length > 1) return; // if current exist running the same instance of program, then exiting			
            String ScriptFolder = Path.GetDirectoryName(ScriptFullPathName);
            string LockScreenImage = "LockScreenImage.jpg";
            const string DesktopScriptName = "DeskTopBGInfo.exe";            
            String FileWallpaper;
            const String regHKLM__CSPKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP\";
            //SystemInformation.UserInteractive Значение true, если текущий процесс выполняется в интерактивном режиме. В противном случае — значение false.
            //SystemInformation.TerminalServerSession Значение true, если вызывающий процесс сопоставлен с клиентским сеансом служб терминалов. В противном случае — значение false.
            //SystemInformation.ComputerName Имя этого компьютера.
            //SystemInformation.UserDomainName Возвращает имя домена, которому принадлежит пользователь.
            //System.DirectoryServices.ActiveDirectory Получает объект Domain для действующих учетных данных текущего пользователя для контекста безопасности, в котором выполняется приложение.
            //Domain.GetComputerDomain Есть нюансы https://docs.microsoft.com/ru-ru/dotnet/api/system.directoryservices.activedirectory.domain.getcomputerdomain?view=dotnet-plat-ext-3.1

            #region Detect boot mode or elevate admin right
            int ProgramMode; // 0 - Install, 1 - Boot
            SelfProc = Process.GetProcessesByName("explorer");
            if (SelfProc.Length == 0)
                ProgramMode = 1;// boot
            else if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                ProgramMode = 0;// install
            else
                return;
            //https://stackoverflow.com/questions/2819934/detect-windows-version-in-net
            //OperatingSystem os = Environment.OSVersion;
            //if (os.Version.Major != 10)
            //{
            //if (ProgramMode == 0) ShowMessage("Эта программа предназначена для ОС Windows 10\nОбнаружена ОС Windows "+os.Version.Major.ToString());
            //    #if !DEBUG
            //        return;
            //    #endif
            //}
            #endregion
            if (!BGInfo.Info.GetInfo())
            {
                Log.ErrorTxt = BGInfo.Info.LastError.Source + " " + BGInfo.Info.LastError.Message; 
                if (ProgramMode == 0) ShowMessage(); else Log.LogError(); return;
            }
            // **************************************************
            // Installation
            // **************************************************
            if (ProgramMode == 0) //Install
            {
                ProcessStartInfo startInfo;
                #region LockscreenBGinfofile
                //Copy program LockscreenBGinfofile to %ProgramFiles%\%ProjectName%
                String ProgramFiles = Environment.GetEnvironmentVariable("ProgramFiles") + "\\" + BGInfo.Info.ProjectName;
                if (string.Compare(ProgramFiles, ScriptFolder, true) != 0)
                {
                    if (!Directory.Exists(ProgramFiles))
                    {
                        try { Directory.CreateDirectory(ProgramFiles); }
                        catch (Exception e) { ShowMessage(e.ToString()); return; }
                        if (!Directory.Exists(ProgramFiles)) { ShowMessage("Не удалось создать папку\n" + ProgramFiles); return; }
                    }
                    try { File.Copy(ScriptFullPathName, Path.Combine(ProgramFiles, Log.ScriptName + ".exe"), true); }
                    catch (Exception e) { ShowMessage(e.ToString()); return; }
                    if (!File.Exists(Path.Combine(ProgramFiles, Log.ScriptName + ".exe"))) { ShowMessage("Не удалось скопировать файл \n" + ScriptFullPathName + "\nв папку\n" + ProgramFiles); return; }
                    ScriptFullPathName = Path.Combine(ProgramFiles, Log.ScriptName + ".exe");
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
                    startInfo.Arguments = "/create /SC ONSTART /TN " + BGInfo.Info.ProjectName + " /TR  \"" + ScriptFullPathName + "\" /F /NP /RL HIGHEST";
                    try
                    {
                        using (Process exeProcess = Process.Start(startInfo))
                        {
                            exeProcess.WaitForExit();
                            if (exeProcess.ExitCode != 0) { ShowMessage("Ошибка при созаднии задания " + Log.ScriptName + " в планировщике \n" + startInfo.FileName + "\n" + startInfo.Arguments); return; }
                        }
                    }
                    catch (Exception e) { ShowMessage(e.ToString()); return; }

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
                        catch (Exception e) { ShowMessage(e.ToString()); return; }

                        if (!BGInfo.Info.WriteToRegistryRun(Path.Combine(ProgramFiles, DesktopScriptName))) { ShowMessage(BGInfo.Info.LastError.ToString()); return; }
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
                                if (exeProcess.ExitCode != 0) { ShowMessage("Ошибка запуска " + DesktopScriptName + "\n" + exeProcess.ExitCode.ToString()); }
                            }
                        }
                        catch (Exception e)
                        { ShowMessage("Ошибка запуска " + DesktopScriptName + "\n" + e.Message); }

                        #endregion
                    }
                #endregion
                #region LockScreenImage
                //Copy image LockScreenImage file to \windows\system32\OOBE\info\backgroud
                if (File.Exists(Path.Combine(ScriptFolder, LockScreenImage)))
                {
                    if (!Directory.Exists(BGInfo.Info.FolderOOBEBGImage))
                    {
                        try { Directory.CreateDirectory(BGInfo.Info.FolderOOBEBGImage); }
                        catch (Exception e)
                        {
                            /*ErrorTxt = e.Message;*/
                            ShowMessage(e.Message);
                            return; }
                    }
                    if (!Directory.Exists(BGInfo.Info.FolderOOBEBGImage)) { ShowMessage("Не удалось создать папку\n" + BGInfo.Info.FolderOOBEBGImage); return; }
                    try { File.Copy(Path.Combine(ScriptFolder, LockScreenImage), Path.Combine(BGInfo.Info.FolderOOBEBGImage, LockScreenImage), true); }
                    catch (Exception e) { ShowMessage(e.ToString()); return; }
                    if (!File.Exists(Path.Combine(BGInfo.Info.FolderOOBEBGImage, LockScreenImage))) { ShowMessage("Не удалось скопировать файл \n" + LockScreenImage + "\nв папку\n" + BGInfo.Info.FolderOOBEBGImage); return; }
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

                if (!BGInfo.Info.WriteInfoToRegistry()) {ShowMessage(BGInfo.Info.LastError.ToString()); return; }   
                //Delete previos file if exist FileWallpaper = FolderOOBEBGImage + BGInfo.Info.hostName + "-" + BGInfo.Info.ScreenWidth + "x" + BGInfo.Info.ScreenHeight + ".jpg";               
            }

            // **************************************************
            // On Boot
            // **************************************************
            if (ProgramMode == 1) if (!BGInfo.Info.ReadScreenResolutionFromRegistry()) return;
            if (ProgramMode <= 1) //Boot
            {// Check wallpaper folder
                if (!Directory.Exists(BGInfo.Info.FolderOOBEBGImage))
                {
                    try { Directory.CreateDirectory(BGInfo.Info.FolderOOBEBGImage); }
                    catch (Exception e) { Log.ErrorTxt = e.ToString(); if (ProgramMode == 0) ShowMessage(); else Log.LogError(); return; }
                    if (!Directory.Exists(BGInfo.Info.FolderOOBEBGImage)) return;
                }
                // Generate name for wallpaper file
                FileWallpaper = BGInfo.Info.FolderOOBEBGImage + BGInfo.Info.hostName + "-" + BGInfo.Info.ScreenWidth + "x" + BGInfo.Info.ScreenHeight + ".jpg";
                // Generate name for wallpaper file      
                
                bool NeedRecreateWallpaperFile = false;
                if (ProgramMode == 0) NeedRecreateWallpaperFile = true;
                else if (!File.Exists(FileWallpaper)) NeedRecreateWallpaperFile = true;
                else
                    try
                    {
                        if (!BGInfo.Info.CompareInfo()) NeedRecreateWallpaperFile = true;
                    }
                    catch
                    { return; }
#if DEBUG
                        if (true)
#else
                if (NeedRecreateWallpaperFile)
#endif
                { // Create new file from origin or blank
                    LockScreenImage = Path.Combine(BGInfo.Info.FolderOOBEBGImage, LockScreenImage);
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
                    if (!resultBGImage) { Log.ErrorTxt = "Ну удалось создать новый файл изображения\n" + FileWallpaper + "\n" + Log.ErrorTxt; if (ProgramMode == 0) ShowMessage(); else Log.LogError(); return; }

                    try {//Write OS Windows registry setting for lockscreen property
                        const string reg_LockScreenImagePath = "LockScreenImagePath";
                        RegistryKey reg;
                        RegistryKey regHKLM = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                        reg = regHKLM.CreateSubKey(regHKLM__CSPKey, true);
                        reg.SetValue(reg_LockScreenImagePath, FileWallpaper, RegistryValueKind.String);
                        if (reg.GetValue(reg_LockScreenImagePath) == null) { Log.ErrorTxt = BGInfo.Info.__ERR1_fail_write_registry + reg.Name; ShowMessage(); return; }
                        const string reg_LockScreenImageUrl = "LockScreenImageUrl";
                        reg.SetValue(reg_LockScreenImageUrl, FileWallpaper, RegistryValueKind.String);
                        if (reg.GetValue(reg_LockScreenImageUrl) == null) { Log.ErrorTxt = BGInfo.Info.__ERR1_fail_write_registry + reg.Name; ShowMessage(); return; }
                        const string reg_LockScreenImageStatus = "LockScreenImageStatus";
                        reg.SetValue(reg_LockScreenImageStatus, 1, RegistryValueKind.DWord);
                    }
                    catch (Exception e)
                    {
                        Log.ErrorTxt = e.ToString(); if (ProgramMode == 0) ShowMessage(); else Log.LogError(); return;
                    }
                }
            }
            if (ProgramMode == 0) MessageBox.Show("Установка " + Log.ScriptName + " прошла успешно", BGInfo.Info.ProjectName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

