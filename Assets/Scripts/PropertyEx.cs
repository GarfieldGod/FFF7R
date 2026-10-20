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
        try
        {
            Addressables.LoadAssetAsync<Sprite>(cardCode).Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    onLoaded?.Invoke(op.Result);
                }
                else
                {
                    Debug.LogWarning($"Addressables加载卡面失败：{cardCode}");
                    onLoaded?.Invoke(null);
                }
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"Addressables加载卡面失败：{cardCode}\n{e}");
        }
    }
}
