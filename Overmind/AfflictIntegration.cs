using System.Collections.Generic;
using HarmonyLib;

namespace Overmind
{
    // These entries reuse AFFLICT's existing skill metadata; no virtual skill IDs
    // enter the player's saved skill list. Progression/upgrade trees come later.
    internal sealed class OvermindAffliction : AfflictData
    {
        private readonly string traitName;
        public override string afflictionTraitName { get { return traitName ?? ""; } }
        public override string name { get { return traitName ?? "Overmind"; } }
        public override string localizedName { get { return name; } }
        public override string localizedDescription { get { return TraitManager.Instance.GetTrait(name)?.description ?? ""; } }

        internal OvermindAffliction(string trait)
        {
            traitName = trait;
            SetMaxCharges(-1);
            SetManaCost(Settings.Current.afflictionManaCost);
            SetBaseSpiritEnergyCost(-1);
            SetCooldown(-1);
            SetIsInUse(true);
            SetIsUsable(true);
            ResetCachedTexts();
        }

        protected override List<IContextMenuItem> GetSubMenus(List<IContextMenuItem> items) { return null; }

        public override void ActivateAbility(IPointOfInterest target)
        {
            Character actor = target as Character;
            // Recheck at click time: selection, immunity and mana can change while
            // a context menu is open. Native validation covers Blessed, Temporal,
            // protected ground, affliction immunity, duplicate traits and death.
            if (actor == null || !IsValid(actor) || !CanPerformAbilityTowards(actor)) return;
            if (UIManager.Instance.contextMenuUIController.currentlyOpenedParentContextItem is PlayerAction parent
                && parent.type != PLAYER_SKILL_TYPE.AFFLICT) return;
            OnExecutePlayerSkill();
            if (!actor.traitContainer.HasTrait("Demon Cultist") && !RollSuccessChance(actor))
            {
                actor.reactionComponent.ResistRuinarchPower();
                return;
            }
            PlayerApplyAfflictionToTarget(actor);
        }
    }

    [HarmonyPatch(typeof(AfflictData), "GetSubMenus")]
    internal static class ExtendAfflictMenu
    {
        private static void Postfix(AfflictData __instance, ref List<IContextMenuItem> __result)
        {
            // The root menu only. Child entries have no submenu.
            if (__instance is OvermindAffliction || __instance.type != PLAYER_SKILL_TYPE.AFFLICT || __result == null) return;
            var target = PlayerManager.Instance.player.currentlySelectedPlayerActionTarget;
            if (!(target is Character)) return;
            foreach (string trait in TraitState.Names)
            {
                bool exists = __result.Exists(item => item is OvermindAffliction entry && entry.afflictionTraitName == trait);
                if (exists) continue;
                var action = new OvermindAffliction(trait);
                if (action.IsValid(target)) __result.Add(action);
            }
        }
    }
}
