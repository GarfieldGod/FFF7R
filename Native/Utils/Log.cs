namespace FFF7RCore {
    using System.Text;
    using System.IO;
    using System;

    public enum TextColor {
        NONE,
        RED,
        YELLOW,
        BLUE,
        GREEN,
        PURPLE,
        BLACK,
        WHITE
    }

    public class Log {
        public static bool PrintToConsole = true;
        private static bool saveLog = false;
        private static int maxSaveLength = 1024 * 1024;
        private static readonly StringBuilder logPlainSb = new StringBuilder();
        private static readonly StringBuilder logAnsiSb = new StringBuilder();
        private static readonly object logLock = new object();

        public static void SaveStart(int maxSaveLength = 1024 * 1024)
        {
            lock (logLock)
            {
                saveLog = true;
                Log.maxSaveLength = maxSaveLength;
                logPlainSb.Clear();
                logAnsiSb.Clear();
            }
        }

        public static void SaveEnd(string filePath)
        {
            lock (logLock)
            {
                saveLog = false;
                bool hasContent = logPlainSb.Length > 0;
                if (hasContent)
                {
                    string dir = Path.GetDirectoryName(filePath);
                    if(string.IsNullOrEmpty(dir))
                        dir = Environment.CurrentDirectory;
                    Directory.CreateDirectory(dir);
                    string nameNoExt = Path.GetFileNameWithoutExtension(filePath);
                    string ext = Path.GetExtension(filePath);
                    string ansiFilePath = Path.Combine(dir, $"{nameNoExt}_colored{ext}");

                    File.WriteAllText(filePath, logPlainSb.ToString());
                    File.WriteAllText(ansiFilePath, logAnsiSb.ToString());
                }
                logPlainSb.Clear();
                logAnsiSb.Clear();
            }
        }

        public static void TestLine(string info, TextColor color = TextColor.NONE, bool ifHightLight = false)
        {
            Output(info, color, ifHightLight);
#if !UNITY_ENGINE
            Output("\n");
#endif
        }

        public static void Test(string info, TextColor color = TextColor.NONE, bool ifHightLight = false) {
            Output(info, color, ifHightLight);
        }

        private static void Output(string info, TextColor color = TextColor.NONE, bool ifHightLight = false) {
            string textColor = GetColorText(color, ifHightLight);

            if (PrintToConsole) {
#if UNITY_ENGINE
                UnityEngine.Debug.Log(info);
#else
                Console.Write(textColor + info);
#endif
            }

            lock (logLock)
            {
                if (saveLog && logPlainSb.Length + info.Length <= maxSaveLength)
                {
                    logPlainSb.Append(info);
                    logAnsiSb.Append(textColor + info);
                }
            }
        }

        public static void Clear() {
        #if UNITY_ENGINE
        #else
            if (Environment.UserInteractive && Console.In != StreamReader.Null)
            {
                try
                {
                    Console.Clear();
                }
                catch (IOException)
                {

                }
            }
        #endif
        }

        static string GetColorText(TextColor color, bool ifHightLight = false) {
            string result = color switch
            {
                TextColor.NONE => "\x1b[0",
                TextColor.RED => "\x1b[31",
                TextColor.YELLOW => "\x1b[33",
                TextColor.BLUE => "\x1b[34",
                TextColor.GREEN => "\x1b[32",
                TextColor.PURPLE => "\x1b[35",
                TextColor.BLACK => "\x1b[30",
                TextColor.WHITE => "\x1b[37",
                _ => "",
            };

            string highLight = "";
            if (result != "") {
                if (ifHightLight)
                    highLight = ";1m";
                else
                    highLight = "m";
            }
        
            result += highLight;
            return result;
        }
    }
}