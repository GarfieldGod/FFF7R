using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

public static class Property
{
    // 👉热更新下载之后的本地缓存路径，如果文件存在优先读这个（热更后的json）
    private static string HotfixChessJsonCachePath;
    // 新：存放多CardJson的目录相对路径
    private const string CardPropertiesFolderName = @"Data\CardProperties";

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
        ChessPropertiesDict_.Clear();
        try
        {
            string jsonText = null;
            // 1. 如果热更缓存文件存在，优先读取热更版本（热更新单个文件逻辑不变）
            if (!string.IsNullOrEmpty(HotfixChessJsonCachePath) && File.Exists(HotfixChessJsonCachePath))
            {
                jsonText = File.ReadAllText(HotfixChessJsonCachePath);
                DeserializeAndAddList(jsonText);
            }
            else
            {
                // 热更不存在 → 扫描 Data\CardProperties 目录下全部 *.json
                string cardPropFolder;
#if UNITY_ENGINE
                string streamingAssetsBase = UnityEngine.Application.streamingAssetsPath;
                cardPropFolder = Path.Combine(streamingAssetsBase, CardPropertiesFolderName);
#else
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                cardPropFolder = Path.Combine(baseDirectory, CardPropertiesFolderName);
#endif

                if (Directory.Exists(cardPropFolder))
                {
                    // 获取目录下全部json文件
                    var jsonFiles = Directory.GetFiles(cardPropFolder, "*.json", SearchOption.TopDirectoryOnly);
                    foreach (var filePath in jsonFiles)
                    {
                        try
                        {
                            Log.TestLine($"Loading file:{filePath}");
                            string fileContent = File.ReadAllText(filePath);
                            DeserializeAndAddList(fileContent);
                        }
                        catch (Exception fileEx)
                        {
                            Log.TestLine($"Skip file:{filePath}, error:{fileEx.Message}");
                        }
                    }
                }
                else
                {
                    Log.TestLine($"CardProperties folder not exist: {cardPropFolder}");
                }
            }
        }
        catch (Exception ex)
        {
            Log.TestLine($"LoadChessProperties top‑level failed:{ex.Message}");
        }
    }

    /// <summary>
    /// 反序列化List<ChessProperty>，加入字典；CardCode重复直接跳过，先到先得
    /// </summary>
    private static void DeserializeAndAddList(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            Log.TestLine($"Empty Json File.");
            return;
        }
        var list = JsonConvert.DeserializeObject<List<ChessProperty>>(json);
        if (list == null)
        {
            Log.TestLine($"Invalid Json File.");
            return;
        }

        Log.TestLine($"Parsed list count = {list.Count}");

        foreach (var chess in list)
        {
            if (string.IsNullOrWhiteSpace(chess.CardCode))
            {
                Log.TestLine($"CardCode is empty, skip: {chess.CardCode}");
                continue;
            }
            // 重复CardCode直接跳过，不覆盖原有数据
            if (ChessPropertiesDict_.ContainsKey(chess.CardCode))
            {
                Log.TestLine($"Duplicate CardCode skip: {chess.CardCode}");
                continue;
            }
            chess.Init();
            ChessPropertiesDict_.Add(chess.CardCode, chess);
            Log.TestLine($"Load chess property: {chess.CardCode}");
        }
    }
}
