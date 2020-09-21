using System;
using System.IO;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using BGInfo;
using System.Security;
using System.Windows;

namespace DesktopBGinfo
{
    class Program
    {   
        /// <summary>
        /// D E S K  T O P    BG    I N F O 
        /// </summary>
        static void Main()
        {
            if (SystemInformation.BootMode != 0/*Normal boot mode must equil zerro*/) return;            
            String ScriptFullPathName = Application.ExecutablePath;
            Log.ScriptName = Path.GetFileNameWithoutExtension(ScriptFullPathName);
            #if DEBUG
            Log.LogError("Start debug" + ((DateTime)(DateTime.Now)).ToString());
            #endif
            Process[] SelfProc = Process.GetProcessesByName(Log.ScriptName);
            if (SelfProc.Length > 1) return; // if current exist running the same instance of program, then exiting            
            //TODO: Проверить запущенность explorer
            String ScriptFolder = Path.GetDirectoryName(ScriptFullPathName);            
            //Read current wallpaprer style
            const String regHKCU__DESKTOP = @"Control Panel\Desktop";
            const String regHKCU__COLORS =  @"Control Panel\Colors";
            String FileWallpaprer,Colors_Background;
            int TileWallpaper, WallpaperStyle;
            RegistryKey reg;
            try
            {
                RegistryKey regHKCU = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                reg = regHKCU.OpenSubKey/*CreateSubKey*/(regHKCU__DESKTOP, true);
                FileWallpaprer = ((string)reg.GetValue("WallPaper", ""));
                TileWallpaper = Int32.Parse((string)reg.GetValue("TileWallpaper", "0"));
                WallpaperStyle = Int32.Parse((string)reg.GetValue("WallpaperStyle", "0"));
                reg = regHKCU.CreateSubKey(regHKCU__COLORS, true);
                Colors_Background = ((string)reg.GetValue("Background", "0 0 0"));

            }
            catch (Exception e) { Log.LogError(e.ToString());return; }
            
            BGInfo.Info.GetCurrentScreenResolution();
            //Center
            if (WallpaperStyle == 0 && TileWallpaper == 0 && !String.IsNullOrEmpty(FileWallpaprer))if (File.Exists(FileWallpaprer))
            {
                //String FolderTranscode= Environment.GetEnvironmentVariable("APPDATA")+ @"\Microsoft\Windows\Themes\";
                String FileTranscodedWallpaper = Path.Combine(Environment.GetEnvironmentVariable("APPDATA") + @"\Microsoft\Windows\Themes\", "TranscodedWallpaper");
                //Создать файл;
                    BGInfo.Wallpaper.BGImageFile = FileWallpaprer;
                    BGInfo.Wallpaper.BGImageAlign_H = 0;
                    BGInfo.Wallpaper.BGImageAlign_V = 0;                    
                    int[] BGrgb=Array.ConvertAll(Colors_Background.Split(' '), int.Parse);
                    BGInfo.Wallpaper.BGColor= System.Drawing.Color.FromArgb(BGrgb[0], BGrgb[1], BGrgb[2]);
                    if (!BGInfo.Wallpaper.Create(FileTranscodedWallpaper)) { Log.LogError("Не удалось создать новый файл обоев"); return; }
                }



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
            Log.LogError("Finish debug" + ((DateTime)(DateTime.Now)).ToString());
            #endif
        }
    }
}
