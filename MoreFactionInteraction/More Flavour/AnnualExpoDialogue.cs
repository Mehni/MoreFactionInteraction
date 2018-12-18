using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;
using MoreFactionInteraction.General;
using Verse.Grammar;
using UnityEngine;

namespace MoreFactionInteraction.More_Flavour
{
    public class AnnualExpoDialogue
    {
        public AnnualExpoDialogue()
        {
        }

        public DiaNode AnnualExpoDialogueNode(Pawn pawn, Caravan caravan, EventDef eventDef, Faction host)
        {
            GrammarRequest request = default;
            request.Includes.Add(RulePackDefOf.ArtDescriptionUtility_Global);

            string flavourText = GrammarResolver.Resolve("artextra_clause", request);

            DiaNode dialogueGreeting = new DiaNode(text: "MFI_AnnualExpoDialogueIntroduction".Translate(eventDef.theme, FirstCharacterToLower(flavourText)));

            foreach (DiaOption option in DialogueOptions(pawn: pawn, caravan, eventDef, host))
            {
                dialogueGreeting.options.Add(item: option);
            }
            return dialogueGreeting;
        }

        private IEnumerable<DiaOption> DialogueOptions(Pawn pawn, Caravan caravan, EventDef eventDef, Faction host)
        {
            string annualExpoDialogueOutcome = $"Something went wrong with More Faction Interaction. Contact the mod author with this year's theme. If you bring a log (press CTRL + F12 now), you get a cookie. P: {pawn} C: {caravan} E: {eventDef} H: {host}";

            bool broughtArt = (eventDef == MFI_DefOf.MFI_CulturalSwap & MFI_Utilities.TryGetBestArt(caravan, out Thing art, out Pawn owner));

            yield return new DiaOption(text: "MFI_AnnualExpoFirstOption".Translate())
            {
                action = () => DetermineOutcome(pawn: pawn, caravan: caravan, eventDef: eventDef, annualExpoDialogueOutcome: out annualExpoDialogueOutcome, host),
                linkLateBind = () =>
                               {
                                   DiaNode endpoint = DialogueResolver(annualExpoDialogueOutcome, broughtArt);

                                   if (broughtArt)
                                       endpoint.options.First().linkLateBind = () => EventRewardWorker_CulturalSwap.DialogueResolverArtOffer("MFI_culturalSwapOutcomeWhoaYouActuallyBroughtArt", art, caravan);

                                   return endpoint;
                               }
            };
        }

        private void DetermineOutcome(Pawn pawn, Caravan caravan, EventDef eventDef, out string annualExpoDialogueOutcome, Faction host)
        {
            string rewards = "Something went wrong with More Faction Interaction. Contact the mod author with this year's theme. If you bring a log(press CTRL + F12 now), you get a cookie.";
            SkillDef thisYearsRelevantSkill = eventDef.learnedSkills.RandomElement();

            if (pawn.skills.GetSkill(thisYearsRelevantSkill).TotallyDisabled) //fallback
                thisYearsRelevantSkill = pawn.skills.skills.Where(x => !x.TotallyDisabled).RandomElementByWeight(x => (int)x.passion).def;

            const float BaseWeight_FirstPlace = 0.2f;
            const float BaseWeight_FirstLoser = 0.5f;
            const float BaseWeight_FirstOther = 0.3f;

            List<KeyValuePair<float, int>> outComeAndChances = new List<KeyValuePair<float, int>>
            {
                new KeyValuePair<float, int>(BaseWeight_FirstPlace * (1 / GetOutcomeWeightFactor(pawn.GetStatValue(eventDef.relevantStat))), 1),
                new KeyValuePair<float, int>(BaseWeight_FirstLoser * (1 / GetOutcomeWeightFactor(pawn.GetStatValue(eventDef.relevantStat))), 2),
                new KeyValuePair<float, int>(BaseWeight_FirstOther * (1 / GetOutcomeWeightFactor(pawn.GetStatValue(eventDef.relevantStat))), 3),
            };

            int placement = outComeAndChances.RandomElementByWeight(x => x.Key).Value;

            switch (placement)
            {
                case 1:
                    rewards = eventDef.Worker.GenerateRewards(pawn, caravan, eventDef.Worker.ValidatorFirstPlace, eventDef.rewardFirstPlace);
                    pawn.skills.Learn(sDef: thisYearsRelevantSkill, xp: eventDef.xPGainFirstPlace, direct: true);
                    TryAppendExpGainInfo(ref rewards, pawn, thisYearsRelevantSkill, eventDef.xPGainFirstPlace);
                    annualExpoDialogueOutcome = eventDef.outComeFirstPlace.Formatted(rewards).AdjustedFor(pawn);
                    break;

                case 2:
                    rewards = eventDef.Worker.GenerateRewards(pawn, caravan, eventDef.Worker.ValidatorFirstLoser, eventDef.rewardFirstLoser);
                    pawn.skills.Learn(sDef: thisYearsRelevantSkill, xp: eventDef.xPGainFirstLoser, direct: true);
                    TryAppendExpGainInfo(ref rewards, pawn, thisYearsRelevantSkill, eventDef.xPGainFirstLoser);
                    annualExpoDialogueOutcome = eventDef.outcomeFirstLoser.Formatted(rewards).AdjustedFor(pawn);
                    break;

                case 3:
                    rewards = eventDef.Worker.GenerateRewards(pawn, caravan, eventDef.Worker.ValidatorFirstOther, eventDef.rewardFirstOther);
                    pawn.skills.Learn(sDef: thisYearsRelevantSkill, xp: eventDef.xPGainFirstOther, direct: true);
                    TryAppendExpGainInfo(ref rewards, pawn, thisYearsRelevantSkill, eventDef.xPGainFirstOther);
                    annualExpoDialogueOutcome = eventDef.outComeFirstOther.Formatted(rewards).AdjustedFor(pawn);
                    break;

                default:
                    Log.Error($"P: {pawn}, C: {caravan}, E: {eventDef}");
                    throw new Exception($"Something went wrong with More Faction Interaction. Contact the mod author with this year's theme. If you bring a log (press CTRL+F12 now), you get a cookie. P: {pawn} C: {caravan} E: {eventDef} H: {host}. C: default.");
            }
        }

        private static void TryAppendExpGainInfo(ref string rewardsOutcome, Pawn pawn, SkillDef skill, float amount)
        {
            if (amount > 0)
                rewardsOutcome = rewardsOutcome + "\n\n" + "MFI_AnnualExpoXPGain"
                    .Translate(pawn.LabelShort, amount.ToString("F0"), skill.label);
        }

        private static DiaNode DialogueResolver(string textResult, bool broughtArt)
        {
            DiaNode resolver = new DiaNode(text: textResult);
            DiaOption diaOption = new DiaOption(text: "OK".Translate())
            {
                resolveTree = !broughtArt
            };
            resolver.options.Add(item: diaOption);
            return resolver;
        }

        private static float GetOutcomeWeightFactor(float statPower) => MoreFactionWar.FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks.BadOutcomeFactorAtStatPower.Evaluate(x: statPower);

        private static string FirstCharacterToLower(string str)
        {
            if (str.NullOrEmpty() || char.IsLower(str[0]))
                return str;

            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
    }
}
