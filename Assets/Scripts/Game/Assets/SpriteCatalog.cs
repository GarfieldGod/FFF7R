using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public sealed class SpriteCatalog
{
    sealed class Entry
    {
        public string Key;
        public AsyncOperationHandle<Sprite> Handle;
        public Sprite Sprite;
        public List<Action<Sprite>> Waiters;
        public bool IsReady;
    }

    readonly Dictionary<string, Entry> _entries = new();

    public void Request(string key, Action<Sprite> onLoaded)
    {
        if (_entries.TryGetValue(key, out var e))
        {
            if (e.IsReady) { onLoaded(e.Sprite); return; }
            if (e.Waiters != null) { e.Waiters.Add(onLoaded); return; }
            _entries.Remove(key);
            Addressables.Release(e.Handle);
        }

        var entry = new Entry { Key = key };
        _entries[key] = entry;

        entry.Waiters = new List<Action<Sprite>> { onLoaded };
        entry.Handle = Addressables.LoadAssetAsync<Sprite>(key);
        entry.Handle.Completed += op => OnSpriteLoaded(entry, op);
    }

    void OnSpriteLoaded(Entry entry, AsyncOperationHandle<Sprite> op)
    {
        entry.IsReady = true;
        if (!_entries.TryGetValue(entry.Key, out var cur) || !ReferenceEquals(cur, entry)) 
            return;

        if (op.Status != AsyncOperationStatus.Succeeded) {
            _entries.Remove(entry.Key);
            Addressables.Release(entry.Handle);
            SafeInvoke(entry.Waiters, null);
            Debug.LogError($"Failed to load sprite: {entry.Key}");
            return;
        }

        var waiters = entry.Waiters;
        entry.Waiters = null;
        entry.Sprite  = op.Result;
        SafeInvoke(waiters, entry.Sprite);
    }

    static void SafeInvoke(List<Action<Sprite>> waiters, Sprite s)
    {
        if (waiters == null) return;
        foreach (var cb in waiters)
            try { cb(s); }
            catch (Exception e) { Debug.LogException(e); }
    }

    public void ReleaseAll()
    {
        foreach (var e in _entries.Values)
        {
            SafeInvoke(e.Waiters, null);
            Addressables.Release(e.Handle);
        }
        _entries.Clear();
    }

    public void Reset()
    {
        try
        {
            ReleaseAll();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}

