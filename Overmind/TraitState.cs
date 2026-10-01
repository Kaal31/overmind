using System;
using System.Collections.Generic;
using System.Linq;
using Ruinarch.ModContent;
using UnityEngine;

namespace Overmind
{
    [Serializable]
    public sealed class OpinionRecord
    {
        public string ownerId;
        public string targetId;
        public string trait;
        public int lastDay = -1;
    }

    [Serializable]
    public sealed class TraitHolder
    {
        public string characterId;
        public List<string> traits = new List<string>();
    }

    [Serializable]
    public sealed class TraitSave
    {
        public int version = 1;
        public List<TraitHolder> holders = new List<TraitHolder>();
        public List<OpinionRecord> opinions = new List<OpinionRecord>();
    }

    internal static class TraitState
    {
        internal const string SaveId = "overmind.traits";
        private static readonly Dictionary<string, OpinionRecord> Opinions = new Dictionary<string, OpinionRecord>();
        internal static readonly string[] Names = { "Negligent", "Envious", "Paranoid" };
        internal static bool IsCustom(string name) { return Array.IndexOf(Names, name) >= 0; }

        internal static void Register() { ModSave.Register(SaveId, Save, Load); }

        internal static bool CanObserve(Character actor, Character peer)
        {
            return actor != null && peer != null && actor != peer && !actor.isDead && !peer.isDead
                && !actor.hasBeenCleanedUp && !peer.hasBeenCleanedUp
                && actor.isNormalCharacter && peer.isNormalCharacter
                && actor.limiterComponent.canWitness && actor.limiterComponent.canPerform
                && actor.homeSettlement != null && actor.homeSettlement == peer.homeSettlement;
        }

        private static string Key(string owner, string target, string trait) { return owner + "/" + target + "/" + trait; }
        private static string Label(string trait) { return "Overmind: " + trait; }

        internal static void SetObservedOpinion(Character actor, Character peer, string trait, int penalty)
        {
            string key = Key(actor.persistentID, peer.persistentID, trait);
            int day = GameManager.Instance.Today().ConvertToContinuousDays();
            OpinionRecord record;
            if (Opinions.TryGetValue(key, out record) && record.lastDay == day) return;
            if (record == null)
            {
                if (penalty == 0) return;
                record = new OpinionRecord { ownerId = actor.persistentID, targetId = peer.persistentID, trait = trait };
                Opinions[key] = record;
            }
            record.lastDay = day;
            int old = actor.relationshipContainer.GetTotalOpinion(peer);
            if (penalty == 0) actor.relationshipContainer.RemoveOpinion(peer, Label(trait));
            else actor.relationshipContainer.SetOpinion(actor, peer, Label(trait), penalty, trait);
            if (actor.relationshipContainer.GetTotalOpinion(peer) != old)
                WriteLog(actor, actor.name + (penalty == 0 ? " no longer feels " + trait.ToLowerInvariant() + " toward " : " feels " + trait.ToLowerInvariant() + " toward ") + peer.name + ".");
        }

        internal static void RemoveEffects(Character actor, string trait)
        {
            foreach (var pair in Opinions.Where(p => p.Value.ownerId == actor.persistentID && p.Value.trait == trait).ToArray())
            {
                Character target = CharacterManager.Instance.GetCharacterByPersistentID(pair.Value.targetId);
                if (target != null) actor.relationshipContainer.RemoveOpinion(target, Label(trait));
                Opinions.Remove(pair.Key);
            }
        }

        internal static void WriteLog(Character actor, string text)
        {
            Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Trait", "Overmind", text, LOG_TAG.Social);
            actor.logComponent.RegisterLog(log);
            if (Settings.Current.diagnosticLogging) OvermindMod.Log.Info(text);
        }

        private static string Save()
        {
            var data = new TraitSave();
            foreach (Character actor in DatabaseManager.Instance.characterDatabase.allCharacters.Values)
            {
                if (actor == null || actor.hasBeenCleanedUp) continue;
                List<string> traits = Names.Where(n => actor.traitContainer.HasTrait(n)).ToList();
                if (traits.Count > 0) data.holders.Add(new TraitHolder { characterId = actor.persistentID, traits = traits });
            }
            data.opinions = Opinions.Values.Where(r => CharacterManager.Instance.GetCharacterByPersistentID(r.ownerId) != null
                && CharacterManager.Instance.GetCharacterByPersistentID(r.targetId) != null).ToList();
            return JsonUtility.ToJson(data);
        }

        private static void Load(string json)
        {
            Opinions.Clear();
            if (string.IsNullOrEmpty(json)) return;
            var data = JsonUtility.FromJson<TraitSave>(json);
            if (data == null || data.version != 1)
                throw new InvalidOperationException("Unsupported Overmind trait save version.");
            foreach (TraitHolder holder in data.holders ?? new List<TraitHolder>())
            {
                Character actor = CharacterManager.Instance.GetCharacterByPersistentID(holder.characterId);
                if (actor == null || actor.hasBeenCleanedUp) continue;
                foreach (string trait in holder.traits ?? new List<string>())
                    if (IsCustom(trait)) actor.traitContainer.AddTrait(actor, trait);
            }
            foreach (OpinionRecord record in data.opinions ?? new List<OpinionRecord>())
            {
                if (!IsCustom(record.trait)) continue;
                Character actor = CharacterManager.Instance.GetCharacterByPersistentID(record.ownerId);
                if (actor == null || !actor.traitContainer.HasTrait(record.trait)) continue;
                Opinions[Key(record.ownerId, record.targetId, record.trait)] = record;
            }
        }
    }
}
