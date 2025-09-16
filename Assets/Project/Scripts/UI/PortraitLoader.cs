// Assets/Project/Scripts/UI/PortraitLoader.cs
// Centralized portrait loading logic with BatPony supported and *no Changeling*.
// Folder convention: Assets/Resources/Portraits/{Race}/{Gender}/portrait.png
// Race normalizations: supports "Earth Pony" -> "EarthPony", "Bat Pony" -> "BatPony", case-insensitive.

#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyGameNamespace.UI
{
    public static class PortraitLoader
    {
        // Allowed races (no Changeling)
        static readonly HashSet<string> AllowedRaces = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "EarthPony","Unicorn","Pegasus","Griffon","Dragon","Human","BatPony"
        };

        // Allowed genders
        static readonly HashSet<string> AllowedGenders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Male","Female"
        };

        /// <summary>
        /// Normalize race display names into folder-safe names.
        /// </summary>
        public static string NormalizeRace(string race)
        {
            if (string.IsNullOrWhiteSpace(race)) return "Human";
            var r = race.Trim();
            r = r.Replace(" ", ""); // "Earth Pony" -> "EarthPony", "Bat Pony" -> "BatPony"
            // Common aliases
            if (r.Equals("Earth", StringComparison.OrdinalIgnoreCase)) r = "EarthPony";
            if (r.Equals("Bat", StringComparison.OrdinalIgnoreCase) || r.Equals("Batpony", StringComparison.OrdinalIgnoreCase)) r = "BatPony";
            if (!AllowedRaces.Contains(r)) return "Human";
            return r;
        }

        /// <summary>
        /// Normalize gender to "Male" or "Female" with a safe default.
        /// </summary>
        public static string NormalizeGender(string gender)
        {
            if (string.IsNullOrWhiteSpace(gender)) return "Female";
            var g = gender.Trim();
            if (g.Equals("M", StringComparison.OrdinalIgnoreCase) || g.Equals("Man", StringComparison.OrdinalIgnoreCase)) g = "Male";
            if (g.Equals("F", StringComparison.OrdinalIgnoreCase) || g.Equals("Woman", StringComparison.OrdinalIgnoreCase)) g = "Female";
            if (!AllowedGenders.Contains(g)) return "Female";
            return g;
        }

        /// <summary>
        /// Build Resources path for a portrait file (without extension).
        /// e.g., Portraits/EarthPony/Female/portrait
        /// </summary>
        public static string GetPortraitResourcePath(string race, string gender)
        {
            var r = NormalizeRace(race);
            var g = NormalizeGender(gender);
            return $"Portraits/{r}/{g}/portrait";
        }

        /// <summary>
        /// Build Resources path for a HUD portrait file (without extension).
        /// e.g., Portraits/HUDPortraits/EarthPony/Female/portrait
        /// </summary>
        public static string GetHUDPortraitResourcePath(string race, string gender)
        {
            var r = NormalizeRace(race);
            var g = NormalizeGender(gender);
            return $"Portraits/HUDPortraits/{r}/{g}/portrait";
        }

        /// <summary>
        /// Attempt to load a Sprite HUD portrait from Resources.
        /// Returns true and assigns 'sprite' on success. Provides multiple graceful fallbacks.
        /// HUD portraits are torso-focused for better UI display.
        /// </summary>
        public static bool TryLoadHUDPortrait(string race, string gender, out Sprite? sprite)
        {
            sprite = null;

            // 1) Perfect match: Portraits/HUDPortraits/{Race}/{Gender}/portrait
            var path = GetHUDPortraitResourcePath(race, gender);
            sprite = Resources.Load<Sprite>(path);
            if (sprite != null) return true;

            // 2) Try folder default: Portraits/HUDPortraits/{Race}/{Gender}
            var pathFolder = $"Portraits/HUDPortraits/{NormalizeRace(race)}/{NormalizeGender(gender)}";
            sprite = Resources.Load<Sprite>(pathFolder);
            if (sprite != null) return true;

            // 3) Race-level default: Portraits/HUDPortraits/{NormalizeRace(race)}/portrait
            var pathRaceDefault = $"Portraits/HUDPortraits/{NormalizeRace(race)}/portrait";
            sprite = Resources.Load<Sprite>(pathRaceDefault);
            if (sprite != null) return true;

            // 4) Fallback to regular portrait system
            return TryLoadPortrait(race, gender, out sprite);
        }

        /// <summary>
        /// Attempt to load a Sprite portrait from Resources.
        /// Returns true and assigns 'sprite' on success. Provides multiple graceful fallbacks.
        /// </summary>
        public static bool TryLoadPortrait(string race, string gender, out Sprite? sprite)
        {
            sprite = null;

            // 1) Perfect match: Portraits/{Race}/{Gender}/portrait
            var path = GetPortraitResourcePath(race, gender);
            sprite = Resources.Load<Sprite>(path);
            if (sprite != null) return true;

            // 2) Try folder default: Portraits/{Race}/{Gender}
            var pathFolder = $"Portraits/{NormalizeRace(race)}/{NormalizeGender(gender)}";
            sprite = Resources.Load<Sprite>(pathFolder);
            if (sprite != null) return true;

            // 3) Race-level default: Portraits/{NormalizeRace(race)}/portrait
            var pathRaceDefault = $"Portraits/{NormalizeRace(race)}/portrait";
            sprite = Resources.Load<Sprite>(pathRaceDefault);
            if (sprite != null) return true;

            // 4) Global default (Sprite)
            sprite = Resources.Load<Sprite>("Portraits/Default_Silhouette");
            if (sprite != null) return true;

            // 5) Global default via Texture2D -> Sprite
            var texDef = Resources.Load<Texture2D>("Portraits/Default_Silhouette");
            if (texDef != null)
            {
                sprite = Sprite.Create(texDef, new Rect(0, 0, texDef.width, texDef.height), new Vector2(0.5f, 0.5f), 100f);
                return true;
            }

            // Last resort: generate a small solid-color sprite at runtime so the UI
            // never receives a null sprite. This prevents empty portraits when the
            // Resources folder hasn't been populated during tests or early imports.
            try
            {
                int size = 64;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                // Choose a color based on race/gender hash to provide visual variety
                System.Random rnd = new System.Random((race ?? "").GetHashCode() ^ (gender ?? "").GetHashCode());
                Color c = new Color((float)rnd.NextDouble() * 0.6f + 0.2f, (float)rnd.NextDouble() * 0.6f + 0.2f, (float)rnd.NextDouble() * 0.6f + 0.2f, 1f);
                var cols = new Color[size * size];
                for (int i = 0; i < cols.Length; i++) cols[i] = c;
                tex.SetPixels(cols);
                tex.Apply();
                sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
