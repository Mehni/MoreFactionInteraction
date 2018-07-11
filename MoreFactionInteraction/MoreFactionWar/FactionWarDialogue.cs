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

            DiaNode dialogueGreeting = new DiaNode(text: "MFI_FactionWarPeaceTalksIntroduction".Translate(args: new object[] { factionOneLeaderName, factionInstigatorLeaderName, pawn.Label }));

            foreach (DiaOption option in DialogueOptions(pawn: pawn, factionOne: factionOne, factionInstigator: factionInstigator))
            {
                dialogueGreeting.options.Add(item: option);
            }

            return dialogueGreeting;
        }

        private static IEnumerable<DiaOption> DialogueOptions(Pawn pawn, Faction factionOne, Faction factionInstigator)
        {
            string factionWarNegotiationsOutcome = "Something went wrong with More Faction Interaction. Please contact mod author.";

            yield return new DiaOption(text: "MFI_FactionWarPeaceTalksCurryFavour".Translate(args: new object[] { factionOne.Name }))
            {
                action = () =>
                {
                    factionOne.TryAffectGoodwillWith(other: pawn.Faction, goodwillChange: 10);
                    factionInstigator.TryAffectGoodwillWith(other: pawn.Faction, goodwillChange: -20);
                    DetermineOutcome(favouredFaction: factionOne, burdenedFaction: factionInstigator, pawn: pawn, desiredOutcome: 1, factionWarNegotiationsOutcome: out factionWarNegotiationsOutcome);
                },
                linkLateBind = (() => DialogueResolver(textResult: factionWarNegotiationsOutcome)),
            };
            yield return new DiaOption(text: "MFI_FactionWarPeaceTalksCurryFavour".Translate(args: new object[] { factionInstigator.Name }))
            {
                action = () =>
                {
                    factionInstigator.TryAffectGoodwillWith(other: pawn.Faction, goodwillChange: 10);
                    factionOne.TryAffectGoodwillWith(other: pawn.Faction, goodwillChange: -20);
                    DetermineOutcome(favouredFaction: factionInstigator, burdenedFaction: factionOne, pawn: pawn, desiredOutcome: 2, factionWarNegotiationsOutcome: out factionWarNegotiationsOutcome);
                },
                linkLateBind = (() => DialogueResolver(textResult: factionWarNegotiationsOutcome)),
            };
            yield return new DiaOption(text: "MFI_FactionWarPeaceTalksSabotage".Translate())
            {
                action = () =>
                {
                    factionOne.TryAffectGoodwillWith(other: pawn.Faction, goodwillChange: 10);
                    factionInstigator.TryAffectGoodwillWith(other: pawn.Faction, goodwillChange: 10);
                    factionInstigator.TryAffectGoodwillWith(other: factionOne, goodwillChange: -100, canSendMessage: true, canSendHostilityLetter: true, reason: "You fucked up.");

                    DetermineOutcome(favouredFaction: factionOne, burdenedFaction: factionInstigator, pawn: pawn, desiredOutcome: 3, factionWarNegotiationsOutcome: out factionWarNegotiationsOutcome);
                },
                linkLateBind = (() => DialogueResolver(textResult: factionWarNegotiationsOutcome)),
            };
            yield return new DiaOption(text: "MFI_FactionWarPeaceTalksBrokerPeace".Translate())
            {
                action = () =>
                {
                    factionOne.TryAffectGoodwillWith(other: pawn.Faction, goodwillChange: 10);
                    factionInstigator.TryAffectGoodwillWith(other: pawn.Faction, goodwillChange: 10);
                    factionInstigator.TryAffectGoodwillWith(other: factionOne, goodwillChange: 100, canSendMessage: true, canSendHostilityLetter: true, reason: "You did well.");

                    DetermineOutcome(favouredFaction: factionOne, burdenedFaction: factionInstigator, pawn: pawn, desiredOutcome: 4, factionWarNegotiationsOutcome: out factionWarNegotiationsOutcome);
                },
                linkLateBind = (() => DialogueResolver(textResult: factionWarNegotiationsOutcome)),
            };
        }

        private static void DetermineOutcome(Faction favouredFaction, Faction burdenedFaction, Pawn pawn, int desiredOutcome, out string factionWarNegotiationsOutcome)
        {
            float badOutcomeWeightFactor = GetBadOutcomeWeightFactor(diplomacyPower: pawn.GetStatValue(stat: StatDefOf.DiplomacyPower));
            float goodOutcomeWeightFactor = 1f / badOutcomeWeightFactor;
            factionWarNegotiationsOutcome = "Something went wrong with More Faction Interaction. Please contact mod author.";

            if (desiredOutcome == 1 || desiredOutcome == 2)
            {
                tmpPossibleOutcomes.Clear();
                tmpPossibleOutcomes.Add(item: new Pair<Pair<Action, float>, string>(first: new Pair<Action, float>(first: () => Outcome_TalksFlounder(favouredFaction: favouredFaction, burdenedFaction: burdenedFaction, pawn: pawn),       second: BaseWeight_Disaster * badOutcomeWeightFactor), second: "MFI_FactionWarFavourFactionDisaster".Translate()));
                tmpPossibleOutcomes.Add(item: new Pair<Pair<Action, float>, string>(first: new Pair<Action, float>(first: () => Outcome_TalksFlounder(favouredFaction: favouredFaction, burdenedFaction: burdenedFaction, pawn: pawn),       second: BaseWeight_Backfire * badOutcomeWeightFactor), second: "MFI_FactionWarFavourFactionBackFire".Translate()));
                tmpPossibleOutcomes.Add(item: new Pair<Pair<Action, float>, string>(first: new Pair<Action, float>(first: () => Outcome_TalksFlounder(favouredFaction: favouredFaction, burdenedFaction: burdenedFaction, pawn: pawn),       second: BaseWeight_TalksFlounder),                     second: "MFI_FactionWarFavourFactionFlounder".Translate()));
                tmpPossibleOutcomes.Add(item: new Pair<Pair<Action, float>, string>(first: new Pair<Action, float>(first: () => Outcome_TalksFlounder(favouredFaction: favouredFaction, burdenedFaction: burdenedFaction, pawn: pawn),       second: BaseWeight_Success * goodOutcomeWeightFactor), second: "MFI_FactionWarFavourFactionSuccess".Translate()));
                tmpPossibleOutcomes.Add(item: new Pair<Pair<Action, float>, string>(first: new Pair<Action, float>(first: () => Outcome_TalksFlounder(favouredFaction: favouredFaction, burdenedFaction: burdenedFaction, pawn: pawn),       second: BaseWeight_Triumph * goodOutcomeWeightFactor), second: "MFI_FactionWarFavourFactionTriumph".Translate()));                

                Action first = tmpPossibleOutcomes.RandomElementByWeight(((Pair<Pair<Action, float>, string> x) => x.First.Second)).First.First;
                factionWarNegotiationsOutcome = tmpPossibleOutcomes.RandomElementByWeight(((Pair<Pair<Action, float>, string> x) => x.First.Second)).Second;
                first();

                pawn.skills.Learn(sDef: SkillDefOf.Social, xp: 6000f, direct: true);

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
            DiaNode resolver = new DiaNode(text: textResult);
            DiaOption diaOption = new DiaOption(text: "Ok then.")
            {
                resolveTree = true
            };
            resolver.options.Add(item: diaOption);
            return resolver;
        }

        private static void Outcome_TalksDisaster(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            Find.LetterStack.ReceiveLetter(label: "LetterLabelPeaceTalks_TalksFlounder".Translate(), text: "test", textLetterDef: LetterDefOf.Death);
        }
        private static void Outcome_TalksBackfire(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            Find.LetterStack.ReceiveLetter(label: "LetterLabelPeaceTalks_TalksFlounder".Translate(), text: "test", textLetterDef: LetterDefOf.Death);
        }
        private static void Outcome_TalksFlounder(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            Find.LetterStack.ReceiveLetter(label: "LetterLabelPeaceTalks_TalksFlounder".Translate(), text: "test", textLetterDef: LetterDefOf.Death);
        }
        private static void Outcome_TalksSuccess(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            Find.LetterStack.ReceiveLetter(label: "LetterLabelPeaceTalks_TalksFlounder".Translate(), text: "test", textLetterDef: LetterDefOf.Death);
        }
        private static void Outcome_TalksTriump(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            Find.LetterStack.ReceiveLetter(label: "LetterLabelPeaceTalks_TalksFlounder".Translate(), text: "test", textLetterDef: LetterDefOf.Death);
        }



        private static float GetBadOutcomeWeightFactor(float diplomacyPower)
        {
            return FactionWarPeaceTalksDiplomacyTuningsBlatantlyCopiedFromPeaceTalks.BadOutcomeFactorAtDiplomacyPower.Evaluate(x: diplomacyPower);
        }



    }
}
