using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.Security.Principal;

namespace LockScreen
{    
    class Program
    {
        static String FileCurWallpaper, FileWallpaper;        
        static void Main(string[] args)         
        {
            if (SystemInformation.BootMode != 0/*Normal boot mode must equil zerro*/) return;
            String ScriptFullPathName = Application.ExecutablePath;
            String ScriptName = Path.GetFileNameWithoutExtension(ScriptFullPathName);
            Process[] Proc = Process.GetProcessesByName(ScriptName);
            if (Proc.Length > 1) return; // if current exist running the same instance of program, then exiting
            String ScriptFolder=Path.GetDirectoryName(ScriptFullPathName);
            RegistryKey reg;
            int ScreenHeight ;
            int ScreenWidth ;

            String hostName;
            //TODO: взять путь из set variable
            String FolderLockScreenImage = Environment.GetEnvironmentVariable("windir") + @"\System32\oobe\info\backgrounds\";
            const String strCSPKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP";

            Console.WriteLine("**************************************************************************");
            Console.WriteLine("Set text with hostname on LockScren                         by Skorik 2020");
            Console.WriteLine("**************************************************************************");

            //SystemInformation.UserInteractive Значение true, если текущий процесс выполняется в интерактивном режиме. В противном случае — значение false.
            //SystemInformation.TerminalServerSession Значение true, если вызывающий процесс сопоставлен с клиентским сеансом служб терминалов. В противном случае — значение false.
            //SystemInformation.ComputerName Имя этого компьютера.
            //SystemInformation.UserDomainName Возвращает имя домена, которому принадлежит пользователь.
            //System.DirectoryServices.ActiveDirectory Получает объект Domain для действующих учетных данных текущего пользователя для контекста безопасности, в котором выполняется приложение.
            //Domain.GetComputerDomain Есть нюансы https://docs.microsoft.com/ru-ru/dotnet/api/system.directoryservices.activedirectory.domain.getcomputerdomain?view=dotnet-plat-ext-3.1

            //
            // Detect Mode
            //
            int ProgramMode; // 0 - Install, 1 - Boot, 2 - Desktop
            Proc = Process.GetProcessesByName("explorer");
            if (Proc.Length == 0) ProgramMode = 1;// boot
            else
                if (new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                    ProgramMode = 0;// install
                else
                    ProgramMode = 2;// desktop
            //
            // Installation
            //
            if (ProgramMode == 0) 
            {
                //Copy program file to %ProgramFiles%\%ScriptName%
                String ProgramFiles = Environment.GetEnvironmentVariable("ProgramFiles") + "\\" + ScriptName;
                if (ProgramFiles != ScriptFolder)
                {
                    Directory.CreateDirectory(ProgramFiles);
                    File.Copy(ScriptFullPathName, Path.Combine(ProgramFiles, ScriptName + ".exe"), true);
                }
                //Get Screen Resolution
                // Problem is get incorrect screen resolution if run from scheduler
                ScreenHeight = Screen.PrimaryScreen.Bounds.Height;
                ScreenWidth = Screen.PrimaryScreen.Bounds.Width;
                //int ScreenHeight = SystemInformation.PrimaryMonitorMaximizedWindowSize.Height;
                //int ScreenWidth = SystemInformation.PrimaryMonitorMaximizedWindowSize.Width;
                //int ScreenHeight = SystemInformation.PrimaryMonitorSize.Height;
                //int ScreenWidth = SystemInformation.PrimaryMonitorSize.Width;

                //HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Run
                //HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\RunServices
                try
                {
                    reg = Registry.LocalMachine.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                    reg.SetValue(ScriptName, Path.Combine(ProgramFiles, ScriptName + ".exe"), RegistryValueKind.String);
                    reg = Registry.LocalMachine.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\RunServices", true);
                //C: \Users\skorik > SCHTASKS / create / SC ONSTART / TN BGInfo / TR  "C:\Program Files\LockScreenWallpaper\LockScreenWallpaper.exe" / F / NP / RL HIGHEST / HRESULT
                    reg.SetValue(ScriptName, Path.Combine(ProgramFiles, ScriptName + ".exe"), RegistryValueKind.String);
                    reg = Registry.LocalMachine.CreateSubKey(@"Software\"+ ScriptName, true);
                    reg.SetValue("ScreenWidth" , ScreenWidth , RegistryValueKind.String);
                    reg.SetValue("ScreenHeight", ScreenHeight, RegistryValueKind.String);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return;
                }
            }
            try // Get hostname
            {
                hostName = Dns.GetHostName();                
            }
            catch (SocketException e)
            {                
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
                return;
            }
            catch (Exception e)
            {                
                Console.WriteLine("Source : " + e.Source);
                Console.WriteLine("Message : " + e.Message);
                return;
            }
            //
            // On Boot
            //
            if (ProgramMode <= 1)
            {
                try //read screen resolution from registry
                {
                    reg = Registry.LocalMachine.CreateSubKey(@"Software\" + ScriptName, true);
                    ScreenWidth = Int32.Parse((string)reg.GetValue("ScreenWidth", "1920"));
                    ScreenHeight = Int32.Parse((string)reg.GetValue("ScreenHeight", "1080"));

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return;
                }
                // Check wallpaper folder
                if (!Directory.Exists(FolderLockScreenImage))
                {
                    try
                    {
                        Directory.CreateDirectory(FolderLockScreenImage);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        return;
                    }
                    if (!Directory.Exists(FolderLockScreenImage)) return;
                }
                // Generate name for wallpaper file
                FileWallpaper = hostName + "-" + ScreenWidth + "x" + ScreenHeight + ".jpg";
                FileWallpaper = FolderLockScreenImage + FileWallpaper;
                // Generate name for wallpaper file
                //
#if DEBUG
                if (true)
#else
                if (!File.Exists(FileWallpaper))
#endif
                { // Create new file from origin or blank
                    Console.WriteLine("Create new file from origin or blank");
                    String originFileWallpaper = FolderLockScreenImage + "backgroundOrigin.jpg";
                    Bitmap Img;
                    Graphics graphics;
                    if (File.Exists(originFileWallpaper))
                    {
                        Img = new Bitmap(originFileWallpaper);
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


                    /*graphics.DrawString(hostName, drawFont, Brushes.Black, xPosText-1, yPosText-1);
                                    StringFormat stringFormat = new StringFormat();
                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Center;  
                    graphics.DrawString(hostName, drawFont, Brushes.Black, xPosText+1, yPosText+1);
                    graphics.DrawString(hostName, drawFont, Brushes.Black, xPosText-1, yPosText);
                    graphics.DrawString(hostName, drawFont, Brushes.Black, xPosText+1, yPosText);
                    graphics.DrawString(hostName, drawFont, Brushes.White, xPosText, yPosText);*/

                    Img.Save(FileWallpaper, System.Drawing.Imaging.ImageFormat.Jpeg);
                    Img.Dispose();
                    graphics.Dispose();
                    if (!File.Exists(FileWallpaper)) return;
                    // set registry value
                    try
                    {
                        reg = Registry.LocalMachine.CreateSubKey(strCSPKey, true);
                        reg.SetValue("LockScreenImagePath", FileWallpaper, RegistryValueKind.String);
                        reg.SetValue("LockScreenImageUrl", FileWallpaper, RegistryValueKind.String);
                        reg.SetValue("LockScreenImageStatus", 1, RegistryValueKind.DWord);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString()); return;
                    }
                }
                Console.WriteLine("\nFinish");
               
            }
        }
    }
}
