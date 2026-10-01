using System;
using System.IO;
using UnityEngine;

namespace Overmind
{
    [Serializable]
    public sealed class Settings
    {
        public bool naturalTraits = true;
        public int afflictionManaCost = 30;
        public int negligenceChancePercent = 25;
        public float negligenceExtraDuration = 0.5f;
        public int envyOpinionPenalty = 10;
        public int paranoidOpinionPenalty = 5;
        public float paranoidNegativeOpinionMultiplier = 1.25f;
        public bool diagnosticLogging = false;
        internal static Settings Current = new Settings();

        internal static void Load(string directory)
        {
            string path = Path.Combine(directory, "config.json");
            try
            {
                if (File.Exists(path))
                    Current = JsonUtility.FromJson<Settings>(File.ReadAllText(path)) ?? new Settings();
                Current.afflictionManaCost = Math.Max(0, Current.afflictionManaCost);
                Current.negligenceChancePercent = Math.Max(0, Math.Min(100, Current.negligenceChancePercent));
                if (float.IsNaN(Current.negligenceExtraDuration) || float.IsInfinity(Current.negligenceExtraDuration))
                    Current.negligenceExtraDuration = 0.5f;
                Current.negligenceExtraDuration = Math.Max(0f, Math.Min(2f, Current.negligenceExtraDuration));
                Current.envyOpinionPenalty = Math.Max(0, Math.Min(30, Current.envyOpinionPenalty));
                Current.paranoidOpinionPenalty = Math.Max(0, Math.Min(20, Current.paranoidOpinionPenalty));
                if (float.IsNaN(Current.paranoidNegativeOpinionMultiplier) || float.IsInfinity(Current.paranoidNegativeOpinionMultiplier))
                    Current.paranoidNegativeOpinionMultiplier = 1.25f;
                Current.paranoidNegativeOpinionMultiplier = Math.Max(1f, Math.Min(2f, Current.paranoidNegativeOpinionMultiplier));
                if (!File.Exists(path)) File.WriteAllText(path, JsonUtility.ToJson(Current, true));
            }
            catch (Exception e)
            {
                Current = new Settings();
                OvermindMod.Log.Info("Config could not be loaded; using defaults: " + e.Message);
            }
        }
    }
}
