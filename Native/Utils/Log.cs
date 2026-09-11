namespace FFF7RCore {
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
        public static void TestLine(string info, TextColor color = TextColor.NONE, bool ifHightLight = false)
        {
            string textColor = GetColorText(color, ifHightLight);
    #if UNITY_ENGINE
            Debug.Log(textColor + info);
    #else
            Test(info, color, ifHightLight);
            Test("\n");
        #endif
        }

        public static void Test (string info, TextColor color = TextColor.NONE, bool ifHightLight = false) {
            string textColor = GetColorText(color, ifHightLight);
        #if UNITY_ENGINE
            Debug.Log(textColor + info);
        #else
            Console.Write(textColor + info);
        #endif
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