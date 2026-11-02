using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class PropertyEx
{
    public static void LoadPropertiesEx()
    {
        FFF7RCore.Property.LoadChessProperties();
    }

    public static void GetCardSpriteAsync(string cardCode, Action<Sprite> onLoaded)
    {
        LoadSpriteAsync(cardCode, onLoaded);
    }

    public static void LoadSpriteAsync(string spriteName, Action<Sprite> onLoaded)
    {
        try
        {
            Addressables.LoadAssetAsync<Sprite>(spriteName).Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    onLoaded?.Invoke(op.Result);
                }
                else
                {
                    Debug.LogWarning($"Addressables加载失败：{spriteName}");
                    onLoaded?.Invoke(null);
                }
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"Addressables加载失败：{spriteName}\n{e}");
        }
    }
}
