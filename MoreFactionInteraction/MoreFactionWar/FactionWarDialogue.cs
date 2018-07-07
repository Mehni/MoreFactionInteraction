using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace MoreFactionInteraction.MoreFactionWar
{
    public static class FactionWarDialogue
    {
        public static DiaNode FactionWarPeaceTalks(Pawn pawn, Faction factionOne, Faction factionInstigator)
        {
            Pawn factionOneLeader;
            Pawn factionInstigatorLeader;

            string factionOneLeaderName;
            string factionInstigatorLeaderName;

            if (factionOne.leader != null)
            {
                factionOneLeader = factionOne.leader;
                factionOneLeaderName = factionOne.leader.Name.ToStringFull;
            }
            else
            {
                Log.Error("Faction " + factionOne + " has no leader.", false);
                factionOneLeaderName = factionOne.Name;
            }

            if (factionInstigator.leader != null)
            {
                factionInstigatorLeader = factionInstigator.leader;
                factionInstigatorLeaderName = factionInstigator.leader.Name.ToStringFull;
            }
            else
            {
                Log.Error("Faction " + factionInstigator + " has no leader.", false);
                factionInstigatorLeaderName = factionInstigator.Name;
            }

            DiaNode dialogueGreeting = new DiaNode($"The two faction leaders {factionOneLeaderName} and {factionInstigatorLeaderName} sit in opposite corners of the camp," +
                $" each surrounded by their trusted whathaveyou's. {pawn.LabelCap} strides into camp, knowing what they say can have far reaching consequences.\n\nYour negotiator has the following options:");

            foreach(DiaOption option in FactionWarDialogue.DialogueOptions(pawn, factionOne, factionInstigator))
            {
                dialogueGreeting.options.Add(option);
            }

            return dialogueGreeting;
        }

        private static IEnumerable<DiaOption> DialogueOptions(Pawn pawn, Faction factionOne, Faction factionInstigator)
        {
            yield return new DiaOption($"Curry favour with {factionOne}")
            {
                action = () =>
                {
                    factionOne.TryAffectGoodwillWith(pawn.Faction, 10);
                    factionInstigator.TryAffectGoodwillWith(pawn.Faction, -20);
                },
                linkLateBind = (() => Resolver($"hey congrats you gained favour with {factionOne.Name}")),
            };
            yield return new DiaOption($"Curry favour with {factionInstigator}")
            {
                action = () =>
                {
                    factionInstigator.TryAffectGoodwillWith(pawn.Faction, 10);
                    factionOne.TryAffectGoodwillWith(pawn.Faction, -20);
                },
                linkLateBind = (() => Resolver($"hey congrats you gained favour with {factionInstigator.Name} but {factionOne.Name} hates you now."))
            };
            yield return new DiaOption($"Sabotage peacetalks to favour colony")
            {
                action = () =>
                {
                    factionOne.TryAffectGoodwillWith(pawn.Faction, 10);
                    factionInstigator.TryAffectGoodwillWith(pawn.Faction, 10);
                    factionInstigator.TryAffectGoodwillWith(factionOne, -100, true, true, "You fucked up.");
                },
                linkLateBind = (() => Resolver($"hey congrats you started a world war."))
            };
            yield return new DiaOption($"Broke peace between the two factions")
            {
                action = () =>
                {
                    factionOne.TryAffectGoodwillWith(pawn.Faction, 10);
                    factionInstigator.TryAffectGoodwillWith(pawn.Faction, 10);
                    factionInstigator.TryAffectGoodwillWith(factionOne, 100, true, true, "You did well.");
                },
                linkLateBind = (() => Resolver("hey congrats you did well"))
            };
        }

        private static DiaNode Resolver(string textResult)
        {
            DiaNode resolver = new DiaNode(textResult);
            DiaOption diaOption = new DiaOption("Ok then.")
            {
                resolveTree = true
            };
            resolver.options.Add(diaOption);
            return resolver;
        }
    }
}
