using Microsoft.Win32;
using System;
using System.Diagnostics;
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
        public const String reg_ScreenHright = "ScreenHeight";
        public static String BGInfoVersion;
        public const String reg_BGInfoVersion = "version";
        public static Exception LastError;
        public const String ProjectName = "BGInfo";
        const String regHKLM__CSPKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP\";
        const String regHKLM__Run = @"Software\Microsoft\Windows\CurrentVersion\Run\";        
        const String regHKLM__Project = @"Software\" + ProjectName;
        RegistryKey reg;

        static public bool GetInfo()
        {
            try { hostName = Environment.GetEnvironmentVariable("COMPUTERNAME"); } catch (Exception e) { LastError = e;return false;}
            return true;
        }
        public bool CompareInfo() //true - reg=pc, false = info changed
        {
            bool CompareInfo = true;
            RegistryKey regHKLM = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            reg = regHKLM.CreateSubKey(regHKLM__Project, true);
            String regValue_hostName;
            try
            {
                regValue_hostName = (string)reg.GetValue(reg_HostName, null);
                if (String.Compare(regValue_hostName, hostName) != 0)
                {
                    CompareInfo = false;
                    reg.SetValue(reg_HostName, hostName, RegistryValueKind.String);
                    if (reg.GetValue(reg_HostName) == null) { Log.ErrorTxt = "Запись в реестр не удалась\n" + reg.Name; ShowMessage(); return; }
                }
            }
            catch (Exception e) { LastError = e; return false};

            reg.SetValue(BGInfo.Info.reg_HostName, BGInfo.Info.hostName, RegistryValueKind.String);
            if (reg.GetValue(BGInfo.Info.reg_HostName) == null) { Log.ErrorTxt = "Запись в реестр не удалась\n" + reg.Name; ShowMessage(); return; }

            return CompareInfo;
        }
    }
}