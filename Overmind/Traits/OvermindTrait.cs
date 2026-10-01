using Traits;

namespace Overmind
{
    // Shared definitions carry NO owner or per-villager state. Save data lives in ModSave.
    public abstract class OvermindTrait : Trait
    {
        protected OvermindTrait(string traitName, string text)
        {
            name = traitName;
            description = text;
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = false;
        }
        public override string GetNameInUI(ITraitable owner) { return name; }
        protected override string GetDescriptionInUI() { return description; }
        public override string GetTestingData(ITraitable owner = null) { return ""; }
        public override void OnRemoveTrait(ITraitable owner, Character removedBy)
        {
            base.OnRemoveTrait(owner, removedBy);
            if (owner is Character character) TraitState.RemoveEffects(character, name);
        }
    }

    public sealed class Negligent : OvermindTrait
    {
        public Negligent() : base("Negligent", "Careless at work. Sometimes needs extra time to finish production, construction or repairs. Does not delay combat or emergency care.") { }
    }

    public sealed class Envious : OvermindTrait
    {
        public Envious() : base("Envious", "Resents peers seen with equipment they lack, superior martial skill, a noble title or supernatural strength. Envy is a bounded opinion penalty, not an automatic crime.")
        { AddTraitOverrideFunctionIdentifier("See_Poi_Trait"); }

        public override bool OnSeePOI(IPointOfInterest target, Character actor)
        {
            if (!TraitState.CanObserve(actor, target as Character)) return false;
            Character peer = (Character)target;
            bool privileged = peer.characterClass?.className == "Noble"
                || peer.traitContainer.HasTrait("Mighty", "Empowered")
                || (peer.equipmentComponent.HasEquips() && !actor.equipmentComponent.HasEquips())
                || peer.TryGetTalentLevel(CHARACTER_TALENT.Martial_Arts) >= actor.TryGetTalentLevel(CHARACTER_TALENT.Martial_Arts) + 3;
            TraitState.SetObservedOpinion(actor, peer, name, privileged ? -Settings.Current.envyOpinionPenalty : 0);
            return false;
        }
    }

    public sealed class Paranoid : OvermindTrait
    {
        private static readonly Suspicious Suspicion = new Suspicious();
        public Paranoid() : base("Paranoid", "Distrusts acquaintances and reacts more strongly to negative social events. Also treats player-manipulated objects as suspicious. Does not invent witnessed crimes.")
        { AddTraitOverrideFunctionIdentifier("See_Poi_Trait"); }

        public override bool OnSeePOI(IPointOfInterest target, Character actor)
        {
            if (actor == null || actor.isDead || actor.hasBeenCleanedUp || !actor.limiterComponent.canWitness) return false;
            // When both traits exist, the original Suspicious trait already handles objects.
            if (!actor.traitContainer.HasTrait("Suspicious")) Suspicion.OnSeePOI(target, actor);
            Character peer = target as Character;
            if (TraitState.CanObserve(actor, peer))
            {
                bool trusts = actor.relationshipContainer.IsFriendsWith(peer);
                TraitState.SetObservedOpinion(actor, peer, name, trusts ? 0 : -Settings.Current.paranoidOpinionPenalty);
            }
            return false;
        }
    }
}
