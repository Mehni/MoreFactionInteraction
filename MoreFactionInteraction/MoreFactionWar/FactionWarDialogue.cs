using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using System.Collections;
using RimWorld.Planet;

namespace MoreFactionInteraction.MoreFactionWar
{
    public static class FactionWarDialogue
    {
        private static List<Pair<Pair<Action, float>, string>> tmpPossibleOutcomes = new List<Pair<Pair<Action, float>, string>>();
        private const float BaseWeight_Disaster = 0.05f;
        private const float BaseWeight_Backfire = 0.1f;
        private const float BaseWeight_TalksFlounder = 0.2f;
        private const float BaseWeight_Success = 0.55f;
        private const float BaseWeight_Triumph = 0.1f;


        public static DiaNode FactionWarPeaceTalks(Pawn pawn, Faction factionOne, Faction factionInstigator)
        {

            string factionInstigatorLeaderName = factionInstigator.leader != null
                ? factionInstigator.leader.Name.ToStringFull
                : factionInstigator.Name;

            string factionOneLeaderName =
                factionOne.leader != null ? factionOne.leader.Name.ToStringFull : factionOne.Name;

            DiaNode dialogueGreeting = new DiaNode("MFI_FactionWarPeaceTalksIntroduction".Translate(new object[] { factionOneLeaderName, factionInstigatorLeaderName, pawn.Label }));

            foreach (DiaOption option in FactionWarDialogue.DialogueOptions(pawn, factionOne, factionInstigator))
            {
                dialogueGreeting.options.Add(option);
            }

            return dialogueGreeting;
        }

        private static IEnumerable<DiaOption> DialogueOptions(Pawn pawn, Faction factionOne, Faction factionInstigator)
        {
            string factionWarNegotiationsOutcome = "Something went wrong with More Faction Interaction. Please contact mod author.";

            yield return new DiaOption("MFI_FactionWarPeaceTalksCurryFavour".Translate(new object[] { factionOne.Name }))
            {
                action = () =>
                {
                    factionOne.TryAffectGoodwillWith(pawn.Faction, 10);
                    factionInstigator.TryAffectGoodwillWith(pawn.Faction, -20);
                    DetermineOutcome(factionOne, factionInstigator, pawn, 1, out factionWarNegotiationsOutcome);
                },
                linkLateBind = (() => DialogueResolver(factionWarNegotiationsOutcome)),
            };
            yield return new DiaOption("MFI_FactionWarPeaceTalksCurryFavour".Translate(new object[] { factionInstigator.Name }))
            {
                action = () =>
                {
                    factionInstigator.TryAffectGoodwillWith(pawn.Faction, 10);
                    factionOne.TryAffectGoodwillWith(pawn.Faction, -20);
                    DetermineOutcome(factionInstigator, factionOne, pawn, 2, out factionWarNegotiationsOutcome);
                },
                linkLateBind = (() => DialogueResolver(factionWarNegotiationsOutcome)),
            };
            yield return new DiaOption("MFI_FactionWarPeaceTalksSabotage".Translate())
            {
                action = () =>
                {
                    factionOne.TryAffectGoodwillWith(pawn.Faction, 10);
                    factionInstigator.TryAffectGoodwillWith(pawn.Faction, 10);
                    factionInstigator.TryAffectGoodwillWith(factionOne, -100, true, true, "You fucked up.");

                    DetermineOutcome(factionOne, factionInstigator, pawn, 3, out factionWarNegotiationsOutcome);
                },
                linkLateBind = (() => DialogueResolver(factionWarNegotiationsOutcome)),
            };
            yield return new DiaOption("MFI_FactionWarPeaceTalksBrokerPeace".Translate())
            {
                action = () =>
                {
                    factionOne.TryAffectGoodwillWith(pawn.Faction, 10);
                    factionInstigator.TryAffectGoodwillWith(pawn.Faction, 10);
                    factionInstigator.TryAffectGoodwillWith(factionOne, 100, true, true, "You did well.");

                    DetermineOutcome(factionOne, factionInstigator, pawn, 4, out factionWarNegotiationsOutcome);
                },
                linkLateBind = (() => DialogueResolver(factionWarNegotiationsOutcome)),
            };
        }

        private static void DetermineOutcome(Faction favouredFaction, Faction burdenedFaction, Pawn pawn, int desiredOutcome, out string factionWarNegotiationsOutcome)
        {
            float badOutcomeWeightFactor = FactionWarDialogue.GetBadOutcomeWeightFactor(pawn.GetStatValue(StatDefOf.DiplomacyPower, true));
            float goodOutcomeWeightFactor = 1f / badOutcomeWeightFactor;
            factionWarNegotiationsOutcome = "Something went wrong with More Faction Interaction. Please contact mod author.";

            if (desiredOutcome == 1 || desiredOutcome == 2)
            {
                tmpPossibleOutcomes.Clear();
                FactionWarDialogue.tmpPossibleOutcomes.Add(new Pair<Pair<Action, float>, string>(new Pair<Action, float>(() => Outcome_TalksFlounder(favouredFaction, burdenedFaction, pawn),       BaseWeight_Disaster * badOutcomeWeightFactor), "MFI_FactionWarFavourFactionDisaster".Translate()));
                FactionWarDialogue.tmpPossibleOutcomes.Add(new Pair<Pair<Action, float>, string>(new Pair<Action, float>(() => Outcome_TalksFlounder(favouredFaction, burdenedFaction, pawn),       BaseWeight_Backfire * badOutcomeWeightFactor), "MFI_FactionWarFavourFactionBackFire".Translate()));
                FactionWarDialogue.tmpPossibleOutcomes.Add(new Pair<Pair<Action, float>, string>(new Pair<Action, float>(() => Outcome_TalksFlounder(favouredFaction, burdenedFaction, pawn),       BaseWeight_TalksFlounder),                     "MFI_FactionWarFavourFactionFlounder".Translate()));
                FactionWarDialogue.tmpPossibleOutcomes.Add(new Pair<Pair<Action, float>, string>(new Pair<Action, float>(() => Outcome_TalksFlounder(favouredFaction, burdenedFaction, pawn),       BaseWeight_Success * goodOutcomeWeightFactor), "MFI_FactionWarFavourFactionSuccess".Translate()));
                FactionWarDialogue.tmpPossibleOutcomes.Add(new Pair<Pair<Action, float>, string>(new Pair<Action, float>(() => Outcome_TalksFlounder(favouredFaction, burdenedFaction, pawn),       BaseWeight_Triumph * goodOutcomeWeightFactor), "MFI_FactionWarFavourFactionTriumph".Translate()));                

                Action first = FactionWarDialogue.tmpPossibleOutcomes.RandomElementByWeight(((Pair<Pair<Action, float>, string> x) => x.First.Second)).First.First;
                factionWarNegotiationsOutcome = FactionWarDialogue.tmpPossibleOutcomes.RandomElementByWeight(((Pair<Pair<Action, float>, string> x) => x.First.Second)).Second;
                first();

                pawn.skills.Learn(SkillDefOf.Social, 6000f, true);

            }
            else if (desiredOutcome == 3)
            {

            }
            else if (desiredOutcome == 4)
            {

            }
            else throw new NotImplementedException();

        }

        private static DiaNode DialogueResolver(string textResult)
        {
            DiaNode resolver = new DiaNode(textResult);
            DiaOption diaOption = new DiaOption("Ok then.")
            {
                resolveTree = true
            };
            resolver.options.Add(diaOption);
            return resolver;
        }

        private static void Outcome_TalksDisaster(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            Find.LetterStack.ReceiveLetter("LetterLabelPeaceTalks_TalksFlounder".Translate(), "test", LetterDefOf.Death);
        }
        private static void Outcome_TalksBackfire(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            Find.LetterStack.ReceiveLetter("LetterLabelPeaceTalks_TalksFlounder".Translate(), "test", LetterDefOf.Death);
        }
        private static void Outcome_TalksFlounder(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            Find.LetterStack.ReceiveLetter("LetterLabelPeaceTalks_TalksFlounder".Translate(), "test", LetterDefOf.Death);
        }
        private static void Outcome_TalksSuccess(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            Find.LetterStack.ReceiveLetter("LetterLabelPeaceTalks_TalksFlounder".Translate(), "test", LetterDefOf.Death);
        }
        private static void Outcome_TalksTriump(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            Find.LetterStack.ReceiveLetter("LetterLabelPeaceTalks_TalksFlounder".Translate(), "test", LetterDefOf.Death);
        }



        private static float GetBadOutcomeWeightFactor(float diplomacyPower)
        {
            return FactionWarPeaceTalksDiplomacyTuningsBlatantlyCopiedFromPeaceTalks.BadOutcomeFactorAtDiplomacyPower.Evaluate(diplomacyPower);
        }



    }
}
