using System;
using System.IO;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using BGInfo;
using System.Security;

namespace DesktopBGinfo
{
    class Program
    {
        //[STAThread]
        //static String ErrorTxt;
        static String ScriptName;                
        const String ProjectName = "BGInfo";
        
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
            #if DEBUG
            LogError("Start debug" + ((DateTime)(DateTime.Now)).ToString());
            #endif
            Process[] SelfProc = Process.GetProcessesByName(ScriptName);
            if (SelfProc.Length > 1) return; // if current exist running the same instance of program, then exiting            
            String ScriptFolder = Path.GetDirectoryName(ScriptFullPathName);
            RegistryKey reg;
            String FileWallpaper;
            string regHKLM__Project = @"Software\" + ProjectName;            
            const String reg_ScreenWidth = "ScreenWidth";
            const String reg_ScreenHright = "ScreenHeight";
            int currentScreenHeight, currentScreenWidth;
            currentScreenHeight = SystemInformation.PrimaryMonitorSize.Height;
            currentScreenWidth = SystemInformation.PrimaryMonitorSize.Width;
            //read screen resolution from registry
            RegistryKey regHKLM;            
                try
                {
                    regHKLM = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    reg = regHKLM.CreateSubKey(regHKLM__Project, true);
                    BGInfo.Info.ScreenWidth = Int32.Parse((string)reg.GetValue(reg_ScreenWidth, "1920"));
                    BGInfo.Info.ScreenHeight = Int32.Parse((string)reg.GetValue(reg_ScreenHright, "1080"));
                }
                catch (Exception e) { LogError(e.ToString()); return; }
                try
                {
                    if (currentScreenWidth != BGInfo.Info.ScreenWidth) reg.SetValue(reg_ScreenWidth, currentScreenWidth, RegistryValueKind.String);
                    if (currentScreenHeight != BGInfo.Info.ScreenHeight) reg.SetValue(reg_ScreenHright, currentScreenHeight, RegistryValueKind.String);
                }
                catch (Exception e) { LogError(e.ToString()); }
                finally { reg.Dispose(); }
            //Windows 10 -%APPDATA%\Roaming\Microsoft\Windows\Themes\CachedFiles\CachedImage_1920_1080_POS4.jpg 
            //Windows7  - C:\Users\<Username>\AppData\Roaming\Microsoft\Windows\Themes\TranscodedWallpaper.jpg
            //APPDATA=C:\Users\<Username>\AppData\Roaming
            //В windows7 Как ни парадоксально в параметре wallpaper путь к C: \Users\<UserName>\AppData\Roaming\Microsoft\Windows\Themes\TranscodedWallpaper.jpg
            //COMPUTERNAME=PCNAME
            //USERDOMAIN =PCNAME or DOMAINNAME 
            //USERNAME = Username
            //USERPROFILE = C:\Users\<Username>            
            /*try
            {
                reg = Registry.CurrentUser.CreateSubKey(@"Control Panel\Desktop", true);
                FileWallpaper = (string)reg.GetValue("WallPaper", "");
            }
            catch (Exception e) { LogError(e.ToString()); return; }
            finally { reg.Dispose(); }

            bool resultBGImage = false;
            string newFileWallpaper = Environment.GetEnvironmentVariable("APPDATA") + @"\Microsoft\Windows\Themes\TranscodedWallpaper";            
            if (String.IsNullOrEmpty(FileWallpaper))
            {
                reg = Registry.CurrentUser.OpenSubKey(@"Control Panel\Colors", true);
                String strBGcolor = (string)reg.GetValue("Background", "0 0 0");
                char[] spaceSeparator = new char[] { ' ' };
                string[] tmpt = strBGcolor.Split(spaceSeparator, StringSplitOptions.RemoveEmptyEntries);
                Int32[] tmptINTS = Array.ConvertAll(tmpt, new Converter<string, int>(int.Parse));
                Color colorBG = Color.FromArgb(tmptINTS[0], tmptINTS[1], tmptINTS[2]);
                //String FolderOOBEBGImage = Environment.GetEnvironmentVariable("windir") + @"\System32\oobe\info\backgrounds\";
                //Path.GetTempFileName();
                resultBGImage = BGInfo.Image.Create(newFileWallpaper, colorBG);
                try
                {

                    reg = Registry.CurrentUser.CreateSubKey(@"Control Panel\Desktop", true);
                    FileWallpaper = (string)reg.SetValue("WallPaper", "");
                }

            }
            else
            {
                resultBGImage = BGInfo.Image.Edit(newFileWallpaper);
            }
            if (!resultBGImage) { LogError("Не удалось создать новый файл изображения\n" + newFileWallpaper + "\n"); return; }*/
            //FileWallpaper = Environment.GetEnvironmentVariable("APPDATA") + @"\Microsoft\Windows\Themes\TranscodedWallpaper.jpg";                                }
            //Environment.GetEnvironmentVariable("APPDATA") + @"\Microsoft\Windows\Themes\CachedFiles\CachedImage_1920_1080_POS4.jpg";
            /*How to set wallpaper JPEG quality reduction in Windows 10 ( default 85)
HKEY_CURRENT_USER\Control Panel\Desktop
Create a new 32-bit DWORD value here called JPEGImportQuality*/
            #if DEBUG
            LogError("Finish debug" + ((DateTime)(DateTime.Now)).ToString());
            #endif
        }
    }
}
