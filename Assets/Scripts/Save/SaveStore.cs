using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveStore
{
    const string FileName = "save.json";

    public static string Dir  => Application.persistentDataPath;
    public static string Path => System.IO.Path.Combine(Dir, FileName);

    // ---------------- 读 ----------------

    public static SaveData Load()
    {
        try
        {
            if (!File.Exists(Path)) return new SaveData();

            string json = File.ReadAllText(Path);
            if (string.IsNullOrWhiteSpace(json)) return new SaveData();

            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) return new SaveData();

            Migrate(data);
            Normalize(data);
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Save] 读取失败，改用默认存档。{e.GetType().Name}: {e.Message}");
            Quarantine();
            return new SaveData();
        }
    }

    // ---------------- 写 ----------------

    public static bool Save(SaveData data)
    {
        if (data == null) return false;

        try
        {
            Directory.CreateDirectory(Dir);

            string tmp = Path + ".tmp";
            File.WriteAllText(tmp, JsonUtility.ToJson(data, true));

            AtomicMove(tmp, Path);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Save] 写入失败。{e.GetType().Name}: {e.Message}");
            return false;
        }
    }

    /// <summary>先写临时文件再替换，避免写到一半崩溃把存档写坏。</summary>
    static void AtomicMove(string tmp, string target)
    {
        if (File.Exists(target))
        {
            try
            {
                File.Replace(tmp, target, target + ".bak");   // 一步完成"备份旧的 + 换新的"
                return;
            }
            catch (Exception)
            {
                // 部分平台（Android 某些文件系统）不支持 Replace，降级处理
            }

            TryBackup(target);
            File.Delete(target);
        }

        File.Move(tmp, target);
    }

    static void TryBackup(string target)
    {
        try { File.Copy(target, target + ".bak", true); }
        catch { /* 备份失败不影响主流程 */ }
    }

    // ---------------- 兜底 ----------------

    /// <summary>坏档改名留证，避免下次启动又读到它。</summary>
    static void Quarantine()
    {
        try
        {
            if (!File.Exists(Path)) return;

            string bad = Path + ".corrupt";
            if (File.Exists(bad)) File.Delete(bad);
            File.Move(Path, bad);

            Debug.LogWarning($"[Save] 坏档已隔离到 {bad}");
        }
        catch { /* 兜底，别再抛 */ }
    }

    /// <summary>版本迁移，以后加字段在这里按版本号补。</summary>
    static void Migrate(SaveData d)
    {
        if (d.version < 1) d.version = 1;

        // 例：
        // if (d.version == 1) { d.someNewField = 默认值; d.version = 2; }
    }

    /// <summary>
    /// 修复反序列化后可能为 null 的集合。
    /// JsonUtility 遇到 "ownedCards": null 会老老实实给 null，不是空 List。
    /// </summary>
    static void Normalize(SaveData d)
    {
        d.clearedLevels ??= new List<string>();
        d.cardData      ??= new PlayerCardData();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("FFF7/打开存档目录")]
    static void RevealInFinder()
    {
        Directory.CreateDirectory(Dir);
        UnityEditor.EditorUtility.RevealInFinder(Path);
    }
#endif
}
