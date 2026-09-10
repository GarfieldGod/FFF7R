using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

public static class Property
{
    private const string chessPropertiesJsonAssetPath = "Json/ChessProperties.json";
    // 👉热更新下载之后的本地缓存路径，如果文件存在优先读这个（热更后的json）
    private static string HotfixChessJsonCachePath;

    private static Dictionary<string, ChessProperty> ChessPropertiesDict_ = new Dictionary<string, ChessProperty>();

    public static void SetHotfixCachePath(string hotfixJsonFullPath)
    {
        HotfixChessJsonCachePath = hotfixJsonFullPath;
    }

    public static ChessProperty GetChessProperty(string cardCode)
    {
        if (string.IsNullOrEmpty(cardCode))
            return null;
        ChessPropertiesDict_.TryGetValue(cardCode, out var prop);
        return prop;
    }

    public static bool TryGetChessProperty(string cardCode, out ChessProperty property)
    {
        property = null;
        if (string.IsNullOrEmpty(cardCode))
            return false;
        return ChessPropertiesDict_.TryGetValue(cardCode, out property);
    }

    public static void LoadChessProperties()
    {
        string jsonText = null;
        try
        {
            // 1. 如果热更缓存文件存在，优先读取热更版本（热更新核心逻辑）
            if (!string.IsNullOrEmpty(HotfixChessJsonCachePath) && File.Exists(HotfixChessJsonCachePath))
            {
                jsonText = File.ReadAllText(HotfixChessJsonCachePath);
            }
            else
            {
#if UNITY_ENGINE
                string assetPath = Path.Combine(UnityEngine.Application.streamingAssetsPath, chessPropertiesJsonAssetPath);
                jsonText = File.ReadAllText(assetPath);
#else
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string assetPath = Path.Combine(baseDirectory, chessPropertiesJsonAssetPath);
                jsonText = File.ReadAllText(assetPath);
#endif
            }

            ChessPropertiesDict_.Clear();

            var list = JsonConvert.DeserializeObject<List<ChessProperty>>(jsonText);
            if (list == null)
                return;

            foreach (var chess in list)
            {
                if (string.IsNullOrWhiteSpace(chess.CardCode))
                {
                    continue;
                }
                chess.Init();
                ChessPropertiesDict_.Add(chess.CardCode, chess);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadChessProperties failed:{ex.Message}");
        }
    }
}
