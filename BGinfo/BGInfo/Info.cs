using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows.Forms;
namespace BGInfo

{
    public class Log
    {
        public static  String ErrorTxt;
        public static String ScriptName;
        public static void LogError() { LogError(ErrorTxt); }
        public static void LogError(string Text)
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
    }
    public class Info
    {
        
        public static String hostName;
        public const String reg_HostName = "Hostname";
        public static String hostDescription;
        public const String reg_HostDescription = "Description";
        public static int ScreenHeight, ScreenWidth;
        public const String reg_ScreenWidth = "ScreenWidth";
        public const String reg_ScreenHeight = "ScreenHeight";
        public static String BGInfoVersion;
        public const String reg_BGInfoVersion = "version";
        public static Exception LastError;
        public const String ProjectName = "BGInfo";
        public static  String FolderOOBEBGImage = Environment.GetEnvironmentVariable("windir") + @"\System32\oobe\info\backgrounds\";
        
        const String regHKLM__Run = @"Software\Microsoft\Windows\CurrentVersion\Run\";        
        const String regHKLM__Project = @"Software\" + ProjectName;
        static RegistryKey reg;
        public  const string __ERR1_fail_write_registry = "Не удалось записать в реестр требуемые данные\n";

        static public bool ReadScreenResolutionFromRegistry() 
        {
            bool Result = true;
            try 
            {
                RegistryKey regHKLM = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
               reg = regHKLM.CreateSubKey(regHKLM__Project, true);
                BGInfo.Info.ScreenWidth = Int32.Parse((string)reg.GetValue(BGInfo.Info.reg_ScreenWidth, "1920"));
                BGInfo.Info.ScreenHeight = Int32.Parse((string)reg.GetValue(BGInfo.Info.reg_ScreenHeight, "1080"));
            }
            catch (Exception e)
            {
                Result = false;
                Log.LogError(e.ToString());
            }
            return Result;
        }
        static public bool WriteInfoToRegistry()
        {
            bool result = true;
            try
            {
                RegistryKey regHKLM = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                reg = regHKLM.CreateSubKey(regHKLM__Project, true);
                reg.SetValue(BGInfo.Info.reg_ScreenWidth, BGInfo.Info.ScreenWidth, RegistryValueKind.String);
                if (reg.GetValue(BGInfo.Info.reg_ScreenWidth) == null) throw new Exception(__ERR1_fail_write_registry + reg.Name);                
                reg.SetValue(BGInfo.Info.reg_ScreenHeight, BGInfo.Info.ScreenHeight, RegistryValueKind.String);
                if (reg.GetValue(BGInfo.Info.reg_ScreenHeight) == null) throw new Exception(__ERR1_fail_write_registry + reg.Name);
                reg.SetValue(BGInfo.Info.reg_HostName, BGInfo.Info.hostName, RegistryValueKind.String);
                if (reg.GetValue(BGInfo.Info.reg_HostName) == null) throw new Exception(__ERR1_fail_write_registry + reg.Name);
                reg.SetValue(BGInfo.Info.reg_HostDescription, BGInfo.Info.hostDescription, RegistryValueKind.String);
                if (reg.GetValue(BGInfo.Info.reg_HostDescription) == null) throw new Exception(__ERR1_fail_write_registry + reg.Name);
                reg.SetValue(BGInfo.Info.reg_BGInfoVersion, BGInfo.Info.BGInfoVersion, RegistryValueKind.String);
                if (reg.GetValue(BGInfo.Info.reg_BGInfoVersion) == null) throw new Exception(__ERR1_fail_write_registry  + reg.Name);
            }
            catch (Exception e)
            {
                BGInfo.Info.LastError = e;
                result = false;
            }
            return result;
        }

        static public bool GetInfo()
        {
            try
            { 
                hostName = Environment.GetEnvironmentVariable("COMPUTERNAME");
                RegistryKey regHKLM = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                reg = regHKLM.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters", true);
                hostDescription = ((string)reg.GetValue("srvcomment", ""));
                Version v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                BGInfoVersion = v.Major.ToString() + "." + v.Major.ToString() + "." + v.Build.ToString();
            } catch (Exception e) { LastError = e;return false;}

            return true;
        }
        static public bool CompareInfo() //true - last pc info = current pc info, false = info changed, on error throw exception
        {
            bool result = true;
            RegistryKey regHKLM = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            String regValue_hostName; 
            String regValue_HostDescription;
            int regValue_ScreenWidth;
            int regValue_ScreenHeight;
            String regValue_Ver;
            try
            {
                reg = regHKLM.CreateSubKey(regHKLM__Project, true);
                regValue_hostName = (string)reg.GetValue(reg_HostName, null);
                if (String.Compare(regValue_hostName, hostName) != 0)
                {
                    result = false;
                    reg.SetValue(reg_HostName, hostName, RegistryValueKind.String);
                    //TODO: if (reg.GetValue(reg_HostName) == null) { Log.ErrorTxt = "Запись в реестр не удалась\n" + reg.Name; ShowMessage(); return; }
                }
                regValue_HostDescription= (string)reg.GetValue(reg_HostDescription, null);
                if (String.Compare(regValue_HostDescription, hostDescription) != 0)
                {
                    result = false;
                    reg.SetValue(reg_HostDescription, hostDescription, RegistryValueKind.String);
                }
                regValue_Ver = (string)reg.GetValue(reg_BGInfoVersion, null);
                if (String.Compare(regValue_Ver, BGInfoVersion) != 0)
                {
                    result = false;
                    reg.SetValue(reg_BGInfoVersion, BGInfoVersion, RegistryValueKind.String);
                }                
                regValue_ScreenWidth = Int32.Parse((string)reg.GetValue(reg_ScreenWidth, "1080"));
                if (regValue_ScreenWidth!=ScreenWidth)
                {
                    result = false;
                    reg.SetValue(reg_ScreenWidth, ScreenWidth, RegistryValueKind.String);
                }
                regValue_ScreenHeight = Int32.Parse((string)reg.GetValue(reg_ScreenHeight, "1920"));
                if (regValue_ScreenHeight != ScreenHeight)
                {
                    result = false;
                    reg.SetValue(reg_ScreenHeight, ScreenHeight, RegistryValueKind.String);
                }
            }
            catch (Exception e) { Log.LogError(e.ToString()); throw ;}
            return result;
        }
        static public bool WriteToRegistryRun(string File)
        {
            bool result = true;
            try
            {
                RegistryKey regHKLM = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                reg = regHKLM.CreateSubKey(regHKLM__Run, true);
                reg.SetValue(ProjectName, File, RegistryValueKind.String);
                if (reg.GetValue(ProjectName) == null)
                    throw new Exception(__ERR1_fail_write_registry + reg.Name);                
            }
            catch (Exception e) { LastError = e; result=false; }
            return result;        
        }
        static public void GetCurrentScreenResolution()
        {
            ScreenHeight = SystemInformation.PrimaryMonitorSize.Height;
            ScreenWidth = SystemInformation.PrimaryMonitorSize.Width;
        }
    }
}