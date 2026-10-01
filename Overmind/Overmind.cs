using System;
using HarmonyLib;
using Ruinarch.Modding;

namespace Overmind
{
    public sealed class OvermindMod : IRuinarchMod
    {
        internal static ModLogger Log;
        public void OnLoad(ModContext context)
        {
            Log = context.Logger;
            Settings.Load(context.ModDirectory);
            TraitState.Register();
            new Harmony(context.Info.id).PatchAll(typeof(OvermindMod).Assembly);
            Log.Info("Overmind traits loaded. New flaws are available in the existing Afflict submenu.");
        }
    }
}
