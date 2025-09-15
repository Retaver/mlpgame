// Finds the existing GameManager or creates one if missing.
// Safe to use from any scene.

using UnityEngine;

namespace MyGameNamespace
{
    public static class GameManagerLocator
    {
        public static GameManager GetOrCreate()
        {
            // If the singleton is already set, use it.
            if (GameManager.Instance != default)
                return GameManager.Instance;

            // Try to find one already in the scene.
            var found = Object.FindAnyObjectByType<GameManager>();
            if (found != default)
                return found;

            // Optional: if you have a prefab in Resources named "GameManager"
            // you can uncomment this bit to instantiate the prefab instead.
            /*
            var prefab = Resources.Load<GameManager>("GameManager");
            if (prefab != default)
            {
                var goPrefab = Object.Instantiate(prefab.gameObject);
                var gmPrefab = goPrefab.GetComponent<GameManager>();
                if (gmPrefab != default)
                {
                    Object.DontDestroyOnLoad(goPrefab);
                    return gmPrefab;
                }
            }
            */

            // Last resort: create a new one on the fly.
            var go = new GameObject("GameManager (Auto)");
            var gm = go.AddComponent<GameManager>();
            Object.DontDestroyOnLoad(go);
            return gm;
        }
    }
}
