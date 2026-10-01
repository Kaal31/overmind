using System;
using HarmonyLib;
using Traits;

namespace Overmind
{
    [HarmonyPatch(typeof(TraitManager), "Initialize")]
    internal static class RegisterTraits
    {
        private static void Postfix(TraitManager __instance)
        {
            foreach (Trait trait in new Trait[] { new Negligent(), new Envious(), new Paranoid() })
            {
                if (__instance.allTraits.ContainsKey(trait.name))
                    throw new InvalidOperationException("Trait name already registered: " + trait.name);
                __instance.allTraits.Add(trait.name, trait);
                if (Settings.Current.naturalTraits && !__instance.flawTraitPool.Contains(trait.name))
                    __instance.flawTraitPool.Add(trait.name);
            }
        }
    }

    // Do not write custom names into stock saves: without this mod, the stock loader
    // indexes allTraits[name] and throws. ModSave restores the traits after world load.
    [HarmonyPatch(typeof(SaveDataTraitContainer), "Save")]
    internal static class KeepStockSaveReadable
    {
        private static void Postfix(SaveDataTraitContainer __instance)
        {
            __instance.nonInstancedTraits.RemoveAll(TraitState.IsCustom);
            foreach (string name in TraitState.Names)
            {
                __instance.stacks.Remove(name);
                __instance.scheduleTickets.Remove(name);
            }
        }
    }

    [HarmonyPatch(typeof(ActualGoapNode), "SetActionDuration")]
    internal static class NegligentWork
    {
        private static readonly System.Reflection.MethodInfo DurationSetter = AccessTools.PropertySetter(typeof(ActualGoapNode), "expectedActionStateDuration");
        private static void Postfix(ActualGoapNode __instance)
        {
            var actor = __instance.actor;
            if (actor == null || actor.isDead || !actor.limiterComponent.canPerform || !actor.traitContainer.HasTrait("Negligent")) return;
            if (!IsWork(__instance.goapType) || __instance.expectedActionStateDuration <= 0) return;
            if (UnityEngine.Random.Range(0, 100) >= Settings.Current.negligenceChancePercent) return;
            int original = __instance.expectedActionStateDuration;
            int extra = (int)Math.Ceiling(original * (double)Settings.Current.negligenceExtraDuration);
            if (extra <= 0) return;
            DurationSetter.Invoke(__instance, new object[] { (int)Math.Min(int.MaxValue, (long)original + extra) });
            TraitState.WriteLog(actor, actor.name + " made a careless mistake and needs extra time to finish work.");
        }

        internal static bool IsWork(INTERACTION_TYPE action)
        {
            switch (action)
            {
                case INTERACTION_TYPE.CHOP_WOOD:
                case INTERACTION_TYPE.MINE_STONE:
                case INTERACTION_TYPE.MINE_ORE:
                case INTERACTION_TYPE.HARVEST_PLANT:
                case INTERACTION_TYPE.HARVEST_CROPS:
                case INTERACTION_TYPE.TILL_TILE:
                case INTERACTION_TYPE.FISH:
                case INTERACTION_TYPE.BUTCHER:
                case INTERACTION_TYPE.COOK:
                case INTERACTION_TYPE.BUILD_BLUEPRINT:
                case INTERACTION_TYPE.REPAIR:
                case INTERACTION_TYPE.REPAIR_STRUCTURE:
                case INTERACTION_TYPE.CRAFT_TILE_OBJECT:
                case INTERACTION_TYPE.CRAFT_EQUIPMENT:
                case INTERACTION_TYPE.CRAFT_FURNITURE_WOOD:
                case INTERACTION_TYPE.CRAFT_FURNITURE_STONE:
                    return true;
                default: return false;
            }
        }
    }

    [HarmonyPatch(typeof(BaseRelationshipContainer), "AdjustOpinion", new Type[] { typeof(Character), typeof(Character), typeof(string), typeof(int), typeof(string), typeof(bool) })]
    internal static class ParanoidNegativeEvents
    {
        private static void Prefix(Character owner, string opinionText, ref int opinionValue)
        {
            if (owner == null || owner.isDead || opinionValue >= 0 || !owner.traitContainer.HasTrait("Paranoid")) return;
            if (opinionText != null && opinionText.StartsWith("Overmind:", StringComparison.Ordinal)) return;
            // Bounded penalties added with SetOpinion are deliberately not amplified.
            double scaled = Math.Floor(opinionValue * (double)Settings.Current.paranoidNegativeOpinionMultiplier);
            opinionValue = (int)Math.Max(int.MinValue, scaled);
        }
    }

    [HarmonyPatch(typeof(LocalizationManager), "GetLocalizedValue", new Type[] { typeof(string), typeof(string) })]
    internal static class TraitText
    {
        private static bool Prefix(string table, string key, ref string __result)
        {
            if (table == "Overmind") { __result = key; return false; }
            if (table != "Traits_Table" || key == null) return true;
            string name = key.EndsWith("_Description", StringComparison.Ordinal) ? key.Substring(0, key.Length - 12) : key;
            if (!TraitState.IsCustom(name)) return true;
            Trait trait = TraitManager.Instance?.GetTrait(name);
            __result = key == name ? name : trait?.description ?? name;
            return false;
        }
    }

}
