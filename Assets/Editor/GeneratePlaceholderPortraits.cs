using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// Small editor helper: writes a few tiny placeholder PNGs into Assets/Resources/Portraits
// so the Editor has visible portraits even when art hasn't been added yet.
[InitializeOnLoad]
public static class GeneratePlaceholderPortraits
{
    static GeneratePlaceholderPortraits()
    {
        try
        {
            EnsurePlaceholders();
        }
        catch (Exception e)
        {
            Debug.LogWarning("GeneratePlaceholderPortraits: failed to create placeholders: " + e.Message);
        }
    }

    private static void EnsurePlaceholders()
    {
        // base path inside the repo
        string basePath = Path.Combine(Application.dataPath, "Resources/Portraits");

        // A tiny 8x8 muted PNG (base64). It's intentionally small to keep repository size tiny.
        // This single image will be used for Default_Silhouette and a few sample race portraits.
        string pngBase64 =
            "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAAKUlEQVQoU2NkYGD4z8DAwMDAwKCgYGBg+M/AwPDf4GJgYGBgYGAAAOcYB9f8n0cAAAAASUVORK5CYII=";

        // Files to create (relative to Resources)
        string[] targets = new[]
        {
            "Default_Silhouette.png",
            Path.Combine("EarthPony", "Female", "portrait.png"),
            Path.Combine("Unicorn", "Female", "portrait.png"),
            Path.Combine("Pegasus", "Female", "portrait.png"),
            Path.Combine("Human", "Female", "portrait.png")
        };

        foreach (var t in targets)
        {
            var full = Path.Combine(basePath, t);
            var dir = Path.GetDirectoryName(full);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            if (!File.Exists(full))
            {
                var bytes = Convert.FromBase64String(pngBase64);
                File.WriteAllBytes(full, bytes);
                // Register with AssetDatabase
                var assetPath = "Assets/Resources/Portraits/" + t.Replace(Path.DirectorySeparatorChar, '/');
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                Debug.Log($"GeneratePlaceholderPortraits: wrote placeholder '{assetPath}'");
            }
        }
    }
}
