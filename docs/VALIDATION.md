# Required validation before a playable release

No compilation or gameplay run has been completed for 0.1.0-dev.
Use a disposable world and copies of saves. Record game and loader versions,
mod configuration, mods.log, and results. Test both with and without Ruinarch+.

1. Compile against installed references; run the loader's check-patches tool.
   Confirm all six patch classes resolve and no load errors appear.
2. Open a world, select a living villager and open the native Afflict submenu.
   Confirm the three new entries appear once alongside existing afflictions;
   apply each. Check names, descriptions, cost charged once and native cast logs.
   Insufficient mana, dead characters, Blessed/Temporal protection, protected
   ground and affliction immunity must prevent casting. Reapplying a trait must
   not spend mana. Remove them through native Remove Flaw, checking its native
   success/resistance, cost and immunity rules. Verify normal afflictions still work.
   Check Grounded/Nullchild interactions and whether shared AFFLICT metadata
   requires further integration for skill-locking behavior.
3. Set negligenceChancePercent=100. Compare otherwise equivalent work actions
   with and without Negligent: each affected state's duration increases by 50%,
   subject to whole-tick rounding. Check combat, eating, movement and first aid
   are unchanged. Set chance=0 and confirm no delay. Save during delayed work;
   reload and confirm the saved duration is not multiplied again solely by load.
4. Envious: observe an eligible stronger/equipped/Noble peer. Check the named
   opinion is -10 and remains -10 after repeated encounters over several days.
   Remove the advantage, wait a day and observe again: the penalty clears.
   No penalty for self, outsiders, dead villagers or unseen peers.
5. Paranoid: non-friend peer gets -5; friends do not. A subsequent natural
   negative opinion adjustment becomes 25% stronger (rounding away from zero).
   Positive events are unchanged. Player-touched object reaction behaves like
   Suspicious. With both traits, that reaction is not processed twice by Overmind.
6. Remove Envious/Paranoid and check their named opinion modifiers are removed.
   Ordinary opinions, relationships, crimes and other traits remain untouched.
7. Save/reload with all traits and active social modifiers: memberships and
   observation cooldowns survive; the penalties do not duplicate. Start another
   world in the same process and confirm prior-world state is cleared.
8. Load a copied save with Overmind disabled: there is no custom-name exception.
   Check and document residual opinions/log behavior. For clean removal, remove
   social flaws and save with Overmind enabled before disabling it.
9. naturalTraits=false: newly generated villagers never receive these traits
   naturally. true: verify the natural flaw selection path can choose each of them.
10. Large village at 4x speed: profile the vision callback and inspect log volume.
    Run multiple days to check observation throttling and opinion bounds.

Known preview limitations: no forced crime accusation, Trigger Flaw support,
material waste, item quality changes, translations, custom icons or affliction
upgrade progression. Paranoid amplifies negative AdjustOpinion calls only;
systems that directly use SetOpinion are not amplified.
