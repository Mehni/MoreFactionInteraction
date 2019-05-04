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
        private readonly Pawn participant;
        private readonly Caravan caravan;
        private readonly EventDef activity;
        private readonly Faction host;

        public AnnualExpoDialogue(Pawn participant, Caravan caravan, EventDef activity, Faction host)
        {
            this.participant = participant;
            this.caravan = caravan;
            this.activity = activity;
            this.host = host;
        }

        public DiaNode AnnualExpoDialogueNode()
        {
            GrammarRequest request = default;
            request.Includes.Add(RulePackDefOf.ArtDescriptionUtility_Global);

            string flavourText = GrammarResolver.Resolve("artextra_clause", request);

            DiaNode dialogueGreeting = new DiaNode(text: "MFI_AnnualExpoDialogueIntroduction".Translate(activity.theme, FirstCharacterToLower(flavourText)));

            foreach (DiaOption option in DialogueOptions(pawn: participant, caravan, activity, host))
                dialogueGreeting.options.Add(item: option);

            return dialogueGreeting;
        }

        private IEnumerable<DiaOption> DialogueOptions(Pawn pawn, Caravan caravan, EventDef eventDef, Faction host)
        {
            string annualExpoDialogueOutcome = $"Something went wrong with More Faction Interaction. Contact the mod author with this year's theme. If you bring a log (press CTRL + F12 now), you get a cookie. P: {pawn} C: {caravan} E: {eventDef} H: {host}";

            bool broughtArt = eventDef == MFI_DefOf.MFI_CulturalSwap & MFI_Utilities.TryGetBestArt(caravan, out Thing art, out Pawn owner);

            yield return new DiaOption(text: "MFI_AnnualExpoFirstOption".Translate())
            {
                action = () => DetermineOutcome(out annualExpoDialogueOutcome),
                linkLateBind = () =>
                               {
                                   DiaNode endpoint = DialogueResolver(annualExpoDialogueOutcome, broughtArt);

                                   if (broughtArt)
                                       endpoint.options.First().linkLateBind = () => EventRewardWorker_CulturalSwap.DialogueResolverArtOffer("MFI_culturalSwapOutcomeWhoaYouActuallyBroughtArt", art, caravan);

                                   return endpoint;
                               }
            };

#if DEBUG
            var devModeTest = new DiaOption("DevMode: Test chances and outcomes")
            {
                action = () =>
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var defEvent in DefDatabase<EventDef>.AllDefsListForReading)
                    {
                        sb.AppendLine(defEvent.LabelCap);

                        Pawn bestpawn = null;
                        do
                        {
                            bestpawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
                        }
                        while (!defEvent.relevantStat.Worker.IsDisabledFor(bestpawn));
                        //Pawn bestpawn = BestCaravanPawnUtility.FindPawnWithBestStat(caravan, eventDef.relevantStat);
                        if (bestpawn == null)
                            sb.AppendLine($"No pawn for {defEvent.LabelCap} found in caravan");
                        else
                        {
                            int outComeOne = 0;
                            int outComeTwo = 0;
                            int outComeThree = 0;
                            int other = 0;
                            float skill = 0;
                            for (int i = 0; i < 1000; i++)
                            {
                                int placement = DetermineOutComeFor(bestpawn, defEvent);
                                switch (placement)
                                {
                                    case 1:
                                        outComeOne++;
                                        break;
                                    case 2:
                                        outComeTwo++;
                                        break;
                                    case 3:
                                        outComeThree++;
                                        break;
                                    default:
                                        other++;
                                        break;
                                }
                                skill += bestpawn.GetStatValue(eventDef.relevantStat);
                            }
                            sb.AppendLine($"Chances for 1000 pawns with stat {defEvent.relevantStat} @ {(skill / 1000f)}:" +
                                          $" first: {(outComeOne / 1000f).ToStringPercent()}, " +
                                          $" second: {(outComeTwo / 1000f).ToStringPercent()}, " +
                                          $" third: {(outComeThree / 1000f).ToStringPercent()} " +
                                          $" other: {(other / 1000f).ToStringPercent()}" +
                                          $" avg skill {(skill / 1000f)}");
                        }
                    }
                    Log.Error(sb.ToString(), true);
                }
            };
            if (Prefs.DevMode)
                yield return devModeTest;
#endif
        }

        private void DetermineOutcome(out string annualExpoDialogueOutcome)
        {
            string rewards = "Something went wrong with More Faction Interaction. Contact the mod author with this year's theme. If you bring a log(press CTRL + F12 now), you get a cookie.";
            SkillDef thisYearsRelevantSkill = activity.learnedSkills.RandomElement();

            if (participant.skills.GetSkill(thisYearsRelevantSkill).TotallyDisabled) //fallback
                thisYearsRelevantSkill = participant.skills.skills.Where(x => !x.TotallyDisabled).RandomElementByWeight(x => (int)x.passion).def;

            int placement = DetermineOutComeFor(participant, activity);

            switch (placement)
            {
                case 1:
                    rewards = activity.Worker.GenerateRewards(participant, caravan, activity.Worker.ValidatorFirstPlace, activity.rewardFirstPlace);
                    participant.skills.Learn(sDef: thisYearsRelevantSkill, xp: activity.xPGainFirstPlace, direct: true);
                    TryAppendExpGainInfo(ref rewards, participant, thisYearsRelevantSkill, activity.xPGainFirstPlace);
                    annualExpoDialogueOutcome = activity.outComeFirstPlace.Formatted(rewards).AdjustedFor(participant);
                    break;

                case 2:
                    rewards = activity.Worker.GenerateRewards(participant, caravan, activity.Worker.ValidatorFirstLoser, activity.rewardFirstLoser);
                    participant.skills.Learn(sDef: thisYearsRelevantSkill, xp: activity.xPGainFirstLoser, direct: true);
                    TryAppendExpGainInfo(ref rewards, participant, thisYearsRelevantSkill, activity.xPGainFirstLoser);
                    annualExpoDialogueOutcome = activity.outcomeFirstLoser.Formatted(rewards).AdjustedFor(participant);
                    break;

                case 3:
                    rewards = activity.Worker.GenerateRewards(participant, caravan, activity.Worker.ValidatorFirstOther, activity.rewardFirstOther);
                    participant.skills.Learn(sDef: thisYearsRelevantSkill, xp: activity.xPGainFirstOther, direct: true);
                    TryAppendExpGainInfo(ref rewards, participant, thisYearsRelevantSkill, activity.xPGainFirstOther);
                    annualExpoDialogueOutcome = activity.outComeFirstOther.Formatted(rewards).AdjustedFor(participant);
                    break;

                default:
                    Log.Error($"P: {participant}, C: {caravan}, E: {activity}");
                    throw new Exception($"Something went wrong with More Faction Interaction. Contact the mod author with this year's theme. If you bring a log (press CTRL+F12 now), you get a cookie. P: {participant} C: {caravan} E: {activity} H: {host}. C: default.");
            }
        }

        private int DetermineOutComeFor(Pawn participant, EventDef eventDef)
        {
            var leaders = Find.FactionManager.AllFactionsVisible
                .Select(faction => faction.leader)
                .Where(leader => !eventDef.relevantStat.Worker.IsDisabledFor(leader))
                .Concat(new[] { participant })
                .Select((pawn)
                    => new { pawn, score = pawn.Faction == host ? pawn.GetStatValue(eventDef.relevantStat) * 1.1 : pawn.GetStatValue(eventDef.relevantStat) })
                .OrderBy(x => x.score)
                .ToArray();

            return leaders.FirstIndexOf(x => x.pawn == participant);
            //return Mathf.Min(3, leaders.FirstIndexOf(x => x.pawn == participant) );
        }

        private int DetermineOutComeFor(Pawn particpant, EventDef eventDef, bool useOutdatedAlgo = false)
        {
            const float BaseWeight_FirstPlace = 0.2f;
            const float BaseWeight_FirstLoser = 0.5f;
            const float BaseWeight_FirstOther = 0.3f;

            List<KeyValuePair<float, int>> outComeAndChances = new List<KeyValuePair<float, int>>
            {
                new KeyValuePair<float, int>(BaseWeight_FirstPlace * (1 / GetOutcomeWeightFactor(participant.GetStatValue(eventDef.relevantStat))), 1),
                new KeyValuePair<float, int>(BaseWeight_FirstLoser * (1 / GetOutcomeWeightFactor(participant.GetStatValue(eventDef.relevantStat))), 2),
                new KeyValuePair<float, int>(BaseWeight_FirstOther * (1 / GetOutcomeWeightFactor(participant.GetStatValue(eventDef.relevantStat))), 3),
            };

            return outComeAndChances.RandomElementByWeight(x => x.Key).Value;
        }

        private static void TryAppendExpGainInfo(ref string rewardsOutcome, Pawn pawn, SkillDef skill, float amount)
        {
            if (amount > 0)
            {
                rewardsOutcome = rewardsOutcome + "\n\n" + "MFI_AnnualExpoXPGain".Translate(pawn.LabelShort, amount.ToString("F0"), skill.label);
            }
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
