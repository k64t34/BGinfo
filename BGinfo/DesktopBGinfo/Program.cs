using System;
using System.IO;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using BGInfo;

namespace DesktopBGinfo
{
    class Program
    {
        //[STAThread]
        static String ErrorTxt;
        static String ScriptName;
        static String hostName;
        static int ScreenHeight, ScreenWidth;
        const String ProjectName = "BGInfo";
        const String reg_ScreenWidth = "ScreenWidth";
        const String reg_ScreenHright = "ScreenHeight";
        static void LogError(string Text)
        {
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = ScriptName;
                eventLog.WriteEntry(Text, EventLogEntryType.Error);
            }
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
            String FileWallpaper;
            string regHKLM__Project = @"Software\" + ProjectName;
            //SystemInformation.UserInteractive Значение true, если текущий процесс выполняется в интерактивном режиме. В противном случае — значение false.
            //SystemInformation.TerminalServerSession Значение true, если вызывающий процесс сопоставлен с клиентским сеансом служб терминалов. В противном случае — значение false.
            //SystemInformation.ComputerName Имя этого компьютера.
            //SystemInformation.UserDomainName Возвращает имя домена, которому принадлежит пользователь.
            //System.DirectoryServices.ActiveDirectory Получает объект Domain для действующих учетных данных текущего пользователя для контекста безопасности, в котором выполняется приложение.
            //Domain.GetComputerDomain Есть нюансы https://docs.microsoft.com/ru-ru/dotnet/api/system.directoryservices.activedirectory.domain.getcomputerdomain?view=dotnet-plat-ext-3.1
            /*// **************************************************
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
                    /* Bitmap Img;
                    Graphics graphics;
                    if (File.Exists(LockScreenImage))
                    {
                        Img = new Bitmap(LockScreenImage);
                        graphics = Graphics.FromImage(Img);
                        //TODO: Resize origin image to real resolution
                    }
                    else
                    {
                        //TODO:Try
                        Img = new Bitmap(ScreenWidth, ScreenHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        //for (int w=0;w!=ScreenWidth;w++) for (int h = 0; h != ScreenHeight; h++) Img.SetPixel(w,h,Color.DarkBlue);
                        graphics = Graphics.FromImage(Img);
                        graphics.Clear(ColorTranslator.FromHtml("#FF004080"));
                    }
                    //https://docs.microsoft.com/en-us/dotnet/framework/winforms/advanced/how-to-align-drawn-text               
                    int xPosText = 10;
                    int yPosText = 10;
                    Font font1 = new Font("Arial", 72, FontStyle.Bold, GraphicsUnit.Point);
                    Rectangle rect1 = new Rectangle(ScreenWidth / 2, yPosText, (ScreenWidth - xPosText) / 2, (ScreenHeight - yPosText) / 4);
                    TextFormatFlags flags = TextFormatFlags.Right;
                    //https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.textrenderer.drawtext?view=netframework-4.8#System_Windows_Forms_TextRenderer_DrawText_System_Drawing_IDeviceContext_System_String_System_Drawing_Font_System_Drawing_Rectangle_System_Drawing_Color_System_Windows_Forms_TextFormatFlags_
                    System.Windows.Forms.TextRenderer.DrawText(graphics, hostName, font1, rect1,
                            System.Drawing.Color.White, flags);
                    // Draw the text and the surrounding rectangle.
                    //graphics.DrawString(hostName, font1, Brushes.White, rect1, stringFormat);
                    //graphics.DrawRectangle(Pens.Black, rect1);

                    //graphics.DrawString(hostName, drawFont, Brushes.Black, xPosText-1, yPosText-1);
                    //                StringFormat stringFormat = new StringFormat();
                    //stringFormat.Alignment = StringAlignment.Center;
                    //stringFormat.LineAlignment = StringAlignment.Center;  
                    //graphics.DrawString(hostName, drawFont, Brushes.Black, xPosText+1, yPosText+1);
                    //graphics.DrawString(hostName, drawFont, Brushes.Black, xPosText-1, yPosText);
                    //graphics.DrawString(hostName, drawFont, Brushes.Black, xPosText+1, yPosText);
                    //graphics.DrawString(hostName, drawFont, Brushes.White, xPosText, yPosText);

                    Img.Save(FileWallpaper, System.Drawing.Imaging.ImageFormat.Jpeg);
                    Img.Dispose();
                    graphics.Dispose();
                    if (!File.Exists(FileWallpaper))
                    {
                        ErrorTxt = "Требуемый файл не создан\n"+ FileWallpaper;
                        if (ProgramMode == 0) ShowMessage(); else LogError(); 
                        return;
                    }*/


            ScreenHeight = SystemInformation.PrimaryMonitorSize.Height;
            ScreenWidth = SystemInformation.PrimaryMonitorSize.Width;
            RegistryKey regHKLM = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            try //read screen resolution from registry
            {
                reg = regHKLM.CreateSubKey(regHKLM__Project, true);
                BGInfo.Info.ScreenWidth = Int32.Parse((string)reg.GetValue(reg_ScreenWidth, "1920"));
                BGInfo.Info.ScreenHeight = Int32.Parse((string)reg.GetValue(reg_ScreenHright, "1080"));
            }
            catch (Exception e)
            {
                LogError(e.ToString()); return;
            }
            reg = regHKLM.CreateSubKey(@"Software\" + ScriptName, true);
            int cScreenWidth = Int32.Parse((string)reg.GetValue("ScreenWidth", "1920"));
            int cScreenHeight = Int32.Parse((string)reg.GetValue("ScreenHeight", "1080"));
            if (cScreenWidth != ScreenWidth) reg.SetValue("ScreenWidth", ScreenWidth, RegistryValueKind.String);
            if (cScreenHeight != ScreenHeight) reg.SetValue("ScreenHeight", ScreenHeight, RegistryValueKind.String);
            //Windows 10 -C:\Users\Andrew\AppData\Roaming\Microsoft\Windows\Themes\CachedFiles\CachedImage_1920_1080_POS4.jpg 
            //Windows7  - C:\Users\Andrew\AppData\Roaming\Microsoft\Windows\Themes\TranscodedWallpaper.jpg
            //APPDATA=C:\Users\Andrew\AppData\Roaming
            //COMPUTERNAME=M16
            //USERDOMAIN = M16
            //USERNAME = Andrew
            //USERPROFILE = C:\Users\Andrew
            reg = Registry.CurrentUser.CreateSubKey(@"Control Panel\Desktop", true);
            //В windows7 Как ни парадоксально в параметре wallpaper путь к C: \Users\Andrew\AppData\Roaming\Microsoft\Windows\Themes\TranscodedWallpaper.jpg

            FileWallpaper = (string)reg.GetValue("WallPaper", "");
            string newFileWallpaper = Environment.GetEnvironmentVariable("APPDATA") + @"\Microsoft\Windows\Themes\TranscodedWallpaper";
            bool resultBGImage = false;
            if (String.IsNullOrEmpty(FileWallpaper))
            {
                reg = Registry.CurrentUser.OpenSubKey(@"Control Panel\Colors", true);
                String strBGcolor = (string)reg.GetValue("Background", "0 0 0");
                char[] spaceSeparator = new char[] { ' ' };
                string[] tmpt = strBGcolor.Split(spaceSeparator, StringSplitOptions.RemoveEmptyEntries);
                Int32[] tmptINTS = Array.ConvertAll(tmpt, new Converter<string, int>(int.Parse));
                Color colorBG = Color.FromArgb(tmptINTS[0], tmptINTS[1], tmptINTS[2]);
                resultBGImage = BGInfo.Image.Create(newFileWallpaper, colorBG);
            }
            else
            {
                resultBGImage = BGInfo.Image.Copy(FileWallpaper, newFileWallpaper);
            }
            if (!resultBGImage) { LogError("Не удалось создать новый файл изображения\n" + newFileWallpaper + "\n"); return; }
            //FileWallpaper = Environment.GetEnvironmentVariable("APPDATA") + @"\Microsoft\Windows\Themes\TranscodedWallpaper.jpg";                                }
            //Environment.GetEnvironmentVariable("APPDATA") + @"\Microsoft\Windows\Themes\CachedFiles\CachedImage_1920_1080_POS4.jpg";


        }



    }


}
