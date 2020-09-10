using System;
namespace BGInfo
{
    public class Info
    {
        public static String hostName;
        public static String hostDescription;
        public static int ScreenHeight, ScreenWidth;
        public static String BGInfoversion; //[Как получить версию программы во время выполнения]        String strVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}