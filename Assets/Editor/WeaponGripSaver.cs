using UnityEngine;
using UnityEditor;

// 編輯器工具：Play Mode 中把「手上武器目前的位置/旋轉」一鍵寫回 EquipmentData 資產。
// 用法：Play Mode 穿上武器 → Scene 視窗把武器調到握正 → Tools → 記錄手上武器的握持校準
// （資產的修改在退出 Play Mode 後依然保留，這是 ScriptableObject 的特性）
public static class WeaponGripSaver
{
    [MenuItem("Tools/記錄手上武器的握持校準 %#g")] // 快捷鍵 Ctrl+Shift+G
    static void SaveGrip()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("握持校準", "請在 Play Mode 中使用：先穿上武器並調整好位置", "OK");
            return;
        }

        // 找到玩家與武器插槽
        PlayerActions hero = Object.FindAnyObjectByType<PlayerActions>();
        if (hero == null || hero.weaponSocket == null)
        {
            Debug.LogError("找不到 PlayerActions 或 weaponSocket 未設定");
            return;
        }
        if (hero.weaponSocket.childCount == 0)
        {
            Debug.LogError("插槽底下沒有武器模型 —— 請先穿上一把武器");
            return;
        }

        // 目前裝備中的武器資產
        EquipmentData equip = EquipmentSystem.Instance.GetEquipped(EquipSlotType.Weapon);
        if (equip == null)
        {
            Debug.LogError("裝備系統顯示目前沒有佩帶武器");
            return;
        }

        // 把武器實例目前的本地位置/旋轉抄進資產
        Transform weaponInstance = hero.weaponSocket.GetChild(0);
        Undo.RecordObject(equip, "記錄握持校準");        // 支援 Ctrl+Z 反悔
        equip.gripPosition = weaponInstance.localPosition;
        equip.gripRotation = weaponInstance.localEulerAngles;
        EditorUtility.SetDirty(equip);                    // 標記資產已修改
        AssetDatabase.SaveAssets();                       // 立刻存檔，退出 Play 也不會丟

        Debug.Log($"<color=green>已記錄 {equip.itemName} 的握持校準：</color>" +
                  $"Pos {equip.gripPosition}, Rot {equip.gripRotation}");
    }
}
