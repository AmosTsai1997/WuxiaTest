using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.IO;

// 編輯器工具：把選取的 Prefab 拍成透明背景的 512x512 Icon PNG，
// 存到 Assets/Art/Picture/Icons/，並自動設定成 Sprite (2D and UI)。
// 用法：Project 視窗選取一個或多個武器 Prefab → Tools → 生成選取Prefab的Icon
public static class WeaponIconGenerator
{
    const int IconSize = 512;
    const string OutputFolder = "Assets/Art/Picture/Icons";

    [MenuItem("Tools/生成選取Prefab的Icon")]
    static void GenerateIcons()
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length == 0)
        {
            EditorUtility.DisplayDialog("生成Icon", "請先在 Project 視窗選取至少一個 Prefab", "OK");
            return;
        }

        Directory.CreateDirectory(OutputFolder);

        foreach (GameObject prefab in selected)
        {
            string path = RenderPrefabToPng(prefab);
            if (path != null) Debug.Log($"<color=green>Icon 已生成：</color>{path}");
        }
        AssetDatabase.Refresh();
    }

    static string RenderPrefabToPng(GameObject prefab)
    {
        // 在遠離場景的位置臨時擺拍，拍完全部清掉
        Vector3 studioPos = new Vector3(0f, -1000f, 0f);

        GameObject instance = null, camObj = null, lightObj = null;
        RenderTexture rt = null;

        try
        {
            instance = Object.Instantiate(prefab, studioPos, Quaternion.identity);

            // 收集所有 Renderer 算出整體包圍盒
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                Debug.LogWarning($"{prefab.name} 沒有任何 Renderer，跳過");
                return null;
            }
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);

            // 相機視線 = 最薄的軸（看到最大的側面剪影）；畫面的「上」= 最長的軸
            Vector3 ext = bounds.extents;
            int thin = 0, longest = 0;
            for (int i = 1; i < 3; i++)
            {
                if (ext[i] < ext[thin]) thin = i;
                if (ext[i] > ext[longest]) longest = i;
            }
            if (thin == longest) { thin = 2; longest = 1; } // 立方體之類的退化情況

            Vector3 forward = Vector3.zero; forward[thin] = 1f;
            Vector3 up = Vector3.zero; up[longest] = 1f;

            // 補光燈：由斜上方打光
            lightObj = new GameObject("_IconLight");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.3f;
            lightObj.transform.rotation = Quaternion.LookRotation(forward + Vector3.down * 0.6f);

            // 相機：正交投影 + 透明背景，滾轉 45 度讓武器呈對角線（經典 RPG icon 擺法）
            camObj = new GameObject("_IconCamera");
            Camera cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0f, 0f); // alpha 0 = 透明
            cam.nearClipPlane = 0.01f;

            float radius = ext.magnitude;                 // 包圍球半徑，滾轉後也不會裁到
            cam.orthographicSize = radius * 1.05f;
            cam.farClipPlane = radius * 4f + 10f;
            camObj.transform.position = bounds.center - forward * (radius * 2f + 1f);
            camObj.transform.rotation =
                Quaternion.AngleAxis(-45f, forward) * Quaternion.LookRotation(forward, up);

            // URP 不能用 Camera.Render()，要透過 RenderRequest 提交
            rt = new RenderTexture(IconSize, IconSize, 24, RenderTextureFormat.ARGB32);
            var request = new RenderPipeline.StandardRequest();
            if (!RenderPipeline.SupportsRenderRequest(cam, request))
            {
                Debug.LogError("目前的渲染管線不支援 RenderRequest，無法拍照");
                return null;
            }
            request.destination = rt;
            RenderPipeline.SubmitRenderRequest(cam, request);

            // 把 GPU 上的圖讀回 CPU 存成 PNG
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(IconSize, IconSize, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, IconSize, IconSize), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;

            string outPath = $"{OutputFolder}/Icon_{prefab.name}.png";
            File.WriteAllBytes(outPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            // 匯入並直接設成 Sprite，省去手動改 Texture Type 的步驟
            AssetDatabase.ImportAsset(outPath);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(outPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();

            return outPath;
        }
        finally
        {
            // 無論成功失敗都要拆掉攝影棚
            if (rt != null) { rt.Release(); Object.DestroyImmediate(rt); }
            if (instance != null) Object.DestroyImmediate(instance);
            if (camObj != null) Object.DestroyImmediate(camObj);
            if (lightObj != null) Object.DestroyImmediate(lightObj);
        }
    }
}
