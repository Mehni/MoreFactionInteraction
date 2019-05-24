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
        private enum Placement
        {
            First,
            Second,
            Third
        }

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

            foreach (DiaOption option in DialogueOptions(participant))
                dialogueGreeting.options.Add(item: option);

            return dialogueGreeting;
        }

        private IEnumerable<DiaOption> DialogueOptions(Pawn participatingPawn)
        {
            string annualExpoDialogueOutcome = $"Something went wrong with More Faction Interaction. Contact the mod author with this year's theme. If you bring a log (press CTRL + F12 now), you get a cookie. P: {participatingPawn} C: {caravan} E: {activity} H: {host}";

            bool broughtArt = activity == MFI_DefOf.MFI_CulturalSwap & MFI_Utilities.TryGetBestArt(caravan, out Thing art, out Pawn owner);

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
            DiaOption devModeTest = new DiaOption("DevMode: Test chances and outcomes")
            { action = DebugLogChances };

            if (Prefs.DevMode)
            {
                yield return devModeTest;
                yield return new DiaOption("restart")
                {
                    action = GenCommandLine.Restart
                };
            }
#endif
        }

#if DEBUG
        internal void DebugLogChances()
        {
            StringBuilder sb = new StringBuilder();
            foreach (EventDef defEvent in DefDatabase<EventDef>.AllDefsListForReading)
            {
                int outComeOne = 0;
                int outComeTwo = 0;
                int outComeThree = 0;
                const float pawnsToTest = 1000;
                float skill = 0;

                double mean = 0;
                double variance = 0;
                double stdDev = 0;
                double min = 0;
                double max = 0;

                sb.AppendLine(defEvent.LabelCap);

                for (int i = 0; i < pawnsToTest; i++)
                {
                    Pawn bestpawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);

                    while (defEvent.relevantStat.Worker.IsDisabledFor(bestpawn))
                    {
                        bestpawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
                    }

                    skill += bestpawn.GetStatValue(defEvent.relevantStat);

                    Placement placement = DeterminePlacementFor(bestpawn, defEvent, out mean, out variance, out stdDev, out max, out min);
                    switch (placement)
                    {
                        case Placement.First:
                            outComeOne++;
                            break;
                        case Placement.Second:
                            outComeTwo++;
                            break;
                        case Placement.Third:
                            outComeThree++;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                sb.AppendLine($"Chances for {pawnsToTest} pawns with stat {defEvent.relevantStat} @ {skill / pawnsToTest}:" +
                              $" first: {(outComeOne / pawnsToTest).ToStringPercent()}, " +
                              $" second: {(outComeTwo / pawnsToTest).ToStringPercent()}, " +
                              $" third: {(outComeThree / pawnsToTest).ToStringPercent()} " +
                              $" mean: {mean}, variance: {variance}, stdDev: {stdDev}, min: {min}, max: {max}");
            }
            Log.Error(sb.ToString(), true);
        }
#endif

        private void DetermineOutcome(out string annualExpoDialogueOutcome)
        {
            string rewards = "Something went wrong with More Faction Interaction. Contact the mod author with this year's theme. If you bring a log(press CTRL + F12 now), you get a cookie.";
            SkillDef thisYearsRelevantSkill = activity.learnedSkills.RandomElement();

            if (participant.skills.GetSkill(thisYearsRelevantSkill).TotallyDisabled)
            {
                thisYearsRelevantSkill = participant.skills.skills.Where(x => !x.TotallyDisabled).RandomElementByWeight(x => (int)x.passion).def;
            }

            Placement placement = DeterminePlacementFor(participant, activity, out double mean, out double variance, out double stdDev, out double max, out double min);

            switch (placement)
            {
                case Placement.First:
                    rewards = activity.Worker.GenerateRewards(participant, caravan, activity.Worker.ValidatorFirstPlace, activity.rewardFirstPlace);
                    participant.skills.Learn(sDef: thisYearsRelevantSkill, xp: activity.xPGainFirstPlace, direct: true);
                    TryAppendExpGainInfo(ref rewards, participant, thisYearsRelevantSkill, activity.xPGainFirstPlace);
                    annualExpoDialogueOutcome = activity.outComeFirstPlace.Formatted(rewards).AdjustedFor(participant);
                    break;

                case Placement.Second:
                    rewards = activity.Worker.GenerateRewards(participant, caravan, activity.Worker.ValidatorFirstLoser, activity.rewardFirstLoser);
                    participant.skills.Learn(sDef: thisYearsRelevantSkill, xp: activity.xPGainFirstLoser, direct: true);
                    TryAppendExpGainInfo(ref rewards, participant, thisYearsRelevantSkill, activity.xPGainFirstLoser);
                    annualExpoDialogueOutcome = activity.outcomeFirstLoser.Formatted(rewards).AdjustedFor(participant);
                    break;

                case Placement.Third:
                    rewards = activity.Worker.GenerateRewards(participant, caravan, activity.Worker.ValidatorFirstOther, activity.rewardFirstOther);
                    participant.skills.Learn(sDef: thisYearsRelevantSkill, xp: activity.xPGainFirstOther, direct: true);
                    TryAppendExpGainInfo(ref rewards, participant, thisYearsRelevantSkill, activity.xPGainFirstOther);
                    annualExpoDialogueOutcome = activity.outComeFirstOther.Formatted(rewards).AdjustedFor(participant);
                    break;

                default:
                    Log.Error($"P: {participant}, C: {caravan}, E: {activity}");
                    throw new Exception($"Something went wrong with More Faction Interaction. Contact the mod author with this year's theme. " +
                                        $"If you bring a log (press CTRL+F12 now), you get a cookie. P: {participant} C: {caravan} E: {activity} H: {host}. C: default.");
            }
        }

        private Placement DeterminePlacementFor(Pawn rep, EventDef eventDef, out double mean, out double variance, out double stdDev, out double max, out double min)
        {
            float difficultyModifier = 1.05f + 0.01f * Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().timesHeld;

            difficultyModifier = Mathf.Clamp(difficultyModifier, 1.05f, 1.1f);

            var leaders = Find.FactionManager.AllFactionsVisible
                              .Select(faction => faction.leader)
                              .Where(leader => leader != null && !eventDef.relevantStat.Worker.IsDisabledFor(leader))
                              .Concat(new[] { rep })
                              .Concat(Find.WorldPawns.AllPawnsAlive.Where(x => x.Faction == host && !eventDef.relevantStat.Worker.IsDisabledFor(x)).Take(25))
                              .Select(pawn => new { pawn, score = pawn.Faction.leader == pawn ? pawn.GetStatValue(eventDef.relevantStat) * difficultyModifier : pawn.GetStatValue(eventDef.relevantStat) })
                              .OrderBy(x => x.score)
                              .ToArray();

            float repSkill = rep.GetStatValue(eventDef.relevantStat);

            max = leaders.Max(x => x.score);
            min = leaders.Min(x => x.score);
            mean = leaders.Average(x => x.score);
            variance = (((max - min + 1) * (max - min + 1)) - 1.0) / 12;
            stdDev = Math.Sqrt(variance);

            FloatRange averageSkillRange = new FloatRange((float)(mean - stdDev * 0.3), (float)(mean + stdDev * 0.3));

            if (leaders.First().pawn == rep)
                return Placement.First;

            if (averageSkillRange.Includes(repSkill))
                return Placement.Second;

            return repSkill > mean ? Placement.First : Placement.Third;
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

        private static string FirstCharacterToLower(string str)
        {
            if (str.NullOrEmpty() || char.IsLower(str[0]))
                return str;

            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
    }
}
