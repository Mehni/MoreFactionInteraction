using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;
using MoreFactionInteraction.General;

namespace MoreFactionInteraction.More_Flavour
{
    public class AnnualExpoDialogue
    {
        public AnnualExpoDialogue()
        {
        }

        public DiaNode AnnualExpoDialogueNode(Pawn pawn, Caravan caravan, EventDef eventDef, Faction host)
        {
            //Tale tale = null;
            //TaleReference taleRef = new TaleReference(tale);
            string flavourText = "fat accountants sing songs.";// taleRef.GenerateText(TextGenerationPurpose.ArtDescription, RulePackDefOf.ArtDescriptionRoot_Taleless);

            DiaNode dialogueGreeting = new DiaNode(text: "MFI_AnnualExpoDialogueIntroduction".Translate(("MFI_" + eventDef.theme).Translate(), flavourText));

            foreach (DiaOption option in DialogueOptions(pawn: pawn, caravan, eventDef, host))
            {
                dialogueGreeting.options.Add(item: option);
            }
            if (Prefs.DevMode)
            {
                foreach (Buff item in Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().ActiveBuffsList)
                {
                    dialogueGreeting.options.Add(item: new DiaOption(text: $"(Dev: Get {item} buff)")
                    {
                        action = () => Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().ApplyRandomBuff(x => x == item),
                        linkLateBind = () => DialogueResolver(textResult: $"It is done.\n\n{item.Description()}")
                    });
                }
            }
            return dialogueGreeting;
        }

        private IEnumerable<DiaOption> DialogueOptions(Pawn pawn, Caravan caravan, EventDef eventDef, Faction host)
        {
            string annualExpoDialogueOutcome = "Something went wrong with More Faction Interaction. Please contact mod author.";

            yield return new DiaOption(text: "MFI_AnnualExpoFirstOption".Translate())
            {
                action = () => DetermineOutcome(pawn: pawn, caravan: caravan, eventDef: eventDef, annualExpoDialogueOutcome: out annualExpoDialogueOutcome, host),
                linkLateBind = () => DialogueResolver(textResult: annualExpoDialogueOutcome),
            };
        }

        private void DetermineOutcome(Pawn pawn, Caravan caravan, EventDef eventDef, out string annualExpoDialogueOutcome, Faction host)
        {
            string rewards = "Something went wrong with More Faction Interaction. Contact the mod author with this year's theme.";
            SkillDef thisYearsRelevantSkill = eventDef.learnedSkills.RandomElement();

            const float BaseWeight_FirstPlace = 0.2f;
            const float BaseWeight_FirstLoser = 0.5f;
            const float BaseWeight_FirstOther = 0.3f;

            List<KeyValuePair<float, int>> outComeAndChances = new List<KeyValuePair<float, int>>
            {
                new KeyValuePair<float, int>(BaseWeight_FirstPlace * 1 / GetOutcomeWeightFactor(pawn.GetStatValue(eventDef.relevantStat)), 1),
                new KeyValuePair<float, int>(BaseWeight_FirstLoser * 1 / GetOutcomeWeightFactor(pawn.GetStatValue(eventDef.relevantStat)), 2),
                new KeyValuePair<float, int>(BaseWeight_FirstOther * 1 / GetOutcomeWeightFactor(pawn.GetStatValue(eventDef.relevantStat)), 3),
            };

            int placement = outComeAndChances.RandomElementByWeight(x => x.Key).Value;

            switch (placement)
            {
                case 1:
                    rewards = eventDef.Worker.GenerateRewards(pawn, caravan, eventDef.Worker.ValidatorFirstPlace, eventDef.rewardFirstPlace);
                    pawn.skills.Learn(sDef: thisYearsRelevantSkill, xp: eventDef.xPGainFirstPlace, direct: true);
                    TryAppendExpGainInfo(ref rewards, pawn, thisYearsRelevantSkill, eventDef.xPGainFirstPlace);
                    annualExpoDialogueOutcome = eventDef.outComeFirstPlace.Translate(rewards).AdjustedFor(pawn);
                    break;

                case 2:
                    rewards = eventDef.Worker.GenerateRewards(pawn, caravan, eventDef.Worker.ValidatorFirstLoser, eventDef.rewardFirstLoser);
                    pawn.skills.Learn(sDef: thisYearsRelevantSkill, xp: eventDef.xPGainFirstLoser, direct: true);
                    TryAppendExpGainInfo(ref rewards, pawn, thisYearsRelevantSkill, eventDef.xPGainFirstLoser);
                    annualExpoDialogueOutcome = eventDef.outcomeFirstLoser.Translate(rewards).AdjustedFor(pawn);
                    break;

                case 3:
                    rewards = eventDef.Worker.GenerateRewards(pawn, caravan, eventDef.Worker.ValidatorFirstOther, eventDef.rewardFirstOther);
                    pawn.skills.Learn(sDef: thisYearsRelevantSkill, xp: eventDef.xPGainFirstOther, direct: true);
                    TryAppendExpGainInfo(ref rewards, pawn, thisYearsRelevantSkill, eventDef.xPGainFirstOther);
                    annualExpoDialogueOutcome = eventDef.outComeFirstOther.Translate(rewards).AdjustedFor(pawn);
                    break;

                default:
                    annualExpoDialogueOutcome = "Something went wrong with More Faction Interaction. Contact the mod author with this year's theme. If you bring a log (press CTRL+F12 now), you get a cookie.";
                    break;
            }
        }

        private void PopulateDictionary(Pawn pawn, Caravan caravan, EventDef eventTheme, Faction host)
        {
            foreach (KeyValuePair<string, List<Pair<Tuple<Action, float>, string>>> kvp in eventsChancesAndOutcomes)
            {
                string rewards = "reward list";
                if (kvp.Key == "gameOfUrComp")
                {
                    //kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                    //                                                                () =>
                    //                                                                {
                    //                                                                    GenerateRewards(pawn, caravan, ref rewards, thingSetMakerDef: ThingSetMakerDefOf.MapGen_AncientTempleContents);
                    //                                                                    SkillDef thisYearsRelevantSkill = Rand.Bool ? SkillDefOf.Intellectual : SkillDefOf.Social;
                    //                                                                    pawn.skills.Learn(sDef: thisYearsRelevantSkill, xp: xPGainFirstPlace, direct: true);
                    //                                                                    TryAppendExpGainInfo(ref rewards, pawn, thisYearsRelevantSkill, xPGainFirstPlace);
                    //                                                                },
                    //                                                                BaseWeight_FirstPlace * 1 / GetOutcomeWeightFactor(pawn.GetStatValue(Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().relevantXpForEvent[kvp.Key]))),
                    //                                                             second: $"MFI_{kvp.Key}OutcomeFirstPlace".Translate(rewards).AdjustedFor(pawn)));

                    //kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                    //                                                                () =>
                    //                                                                {
                    //                                                                    GenerateRewards(pawn, caravan, ref rewards, thingSetMakerDef: ThingSetMakerDefOf.MapGen_AncientPodContents);
                    //                                                                    SkillDef thisYearsRelevantSkill = Rand.Bool ? SkillDefOf.Intellectual : SkillDefOf.Social;
                    //                                                                    pawn.skills.Learn(sDef: thisYearsRelevantSkill, xp: xPGainFirstOther, direct: true);
                    //                                                                    TryAppendExpGainInfo(ref rewards, pawn, thisYearsRelevantSkill, xPGainFirstOther);
                    //                                                                },
                    //                                                                BaseWeight_FirstLoser * 1 / GetOutcomeWeightFactor(pawn.GetStatValue(Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().relevantXpForEvent[kvp.Key]))),
                    //                                                            second: $"MFI_{kvp.Key}OutcomeFirstLoser".Translate(rewards).AdjustedFor(pawn)));

                    //kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                    //                                                                () =>
                    //                                                                {
                    //                                                                    rewards = "MFI_AnnualExpoMedicalEmergency".Translate();
                    //                                                                    foreach (Pawn brawler in caravan.PlayerPawnsForStoryteller)
                    //                                                                    {
                    //                                                                        if (!brawler.story?.WorkTagIsDisabled(WorkTags.Violent) ?? false)
                    //                                                                        {
                    //                                                                            brawler.skills.Learn(SkillDefOf.Melee, xPGainFirstPlace, true);
                    //                                                                            TryAppendExpGainInfo(ref rewards, brawler, SkillDefOf.Melee, xPGainFirstPlace);
                    //                                                                        }
                    //                                                                    }
                    //                                                                },
                    //                                                                BaseWeight_FirstOther * 1 / GetOutcomeWeightFactor(pawn.GetStatValue(Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().relevantXpForEvent[kvp.Key]))),
                    //                                                            second: $"MFI_{kvp.Key}OutcomeFirstOther".Translate(rewards).AdjustedFor(pawn)));
                }
                else if (kvp.Key == "shootingComp")
                {
                //    kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>( //nice pew-pews
                //                                                                    () =>
                //                                                                    {
                //                                                                        GenerateRewards(pawn, caravan, ref rewards, (ThingDef x) => x.equipmentType == EquipmentType.Primary, ThingSetMakerDefOf.Reward_ItemStashQuestContents);
                //                                                                        pawn.skills.Learn(sDef: SkillDefOf.Shooting, xp: xPGainFirstPlace, direct: true);
                //                                                                        TryAppendExpGainInfo(ref rewards, pawn, SkillDefOf.Shooting, xPGainFirstPlace);
                //                                                                    },
                //                                                                    BaseWeight_FirstPlace * 1 / GetOutcomeWeightFactor(pawn.GetStatValue(Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().relevantXpForEvent[kvp.Key]))),
                //                                                                second: $"MFI_{kvp.Key}OutcomeFirstPlace".Translate(rewards).AdjustedFor(pawn)));

                //    kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>( //nice bionics
                //                                                                    () =>
                //                                                                    {
                //                                                                        GenerateRewards(pawn, caravan, ref rewards, (ThingDef x) => x.isTechHediff && x.techLevel >= TechLevel.Industrial, ThingSetMakerDefOf.Reward_ItemStashQuestContents);
                //                                                                        pawn.skills.Learn(sDef: SkillDefOf.Shooting, xp: xPGainFirstOther, direct: true);
                //                                                                        TryAppendExpGainInfo(ref rewards, pawn, SkillDefOf.Shooting, xPGainFirstOther);
                //                                                                    },
                //                                                                    BaseWeight_FirstLoser * 1 / GetOutcomeWeightFactor(pawn.GetStatValue(Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().relevantXpForEvent[kvp.Key]))),
                //                                                                second: $"MFI_{kvp.Key}OutcomeFirstLoser".Translate(rewards).AdjustedFor(pawn)));

                //    kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>( //GLORIOUS POTAT!
                //                                                                    () =>
                //                                                                    {
                //                                                                        GenerateRewards(pawn, caravan, ref rewards, (ThingDef x) => x == ThingDefOf.RawPotatoes);
                //                                                                        pawn.skills.Learn(sDef: SkillDefOf.Shooting, xp: xPGainFirstOther, direct: true);
                //                                                                        TryAppendExpGainInfo(ref rewards, pawn, SkillDefOf.Shooting, xPGainFirstOther);
                //                                                                    },
                //                                                                    BaseWeight_FirstOther * 1 / GetOutcomeWeightFactor(pawn.GetStatValue(Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().relevantXpForEvent[kvp.Key]))),
                //                                                                second: $"MFI_{kvp.Key}OutcomeFirstOther".Translate(rewards).AdjustedFor(pawn)));
                }
                else if (kvp.Key == "culturalSwap")
                {
                    //if (MFI_Utilities.TryGetBestArt(caravan, out Thing art, out Pawn owner))
                    //{
                    //    //kvp.Value.Add(new Pair<Tuple<Action, float>, string>(new Tuple<Action, float>(
                    //    //                                                        () => {
                    //    Dialog_NodeTree tree = (Dialog_NodeTree)Find.WindowStack.Windows.First(x => x is Dialog_NodeTree);
                    //    tree.GotoNode(DialogueResolverArtOffer("MFI_culturalSwapOutcomeWhoaYouActuallyBroughtArt", art, caravan));
                    //    //                                                             },
                    //    //                                                       100f), "hey"));
                    //    return;
                    //}

                    //kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                    //                                                                () =>
                    //                                                                {
                    //                                                                    GenerateRewards(pawn, caravan, ref rewards,
                    //                                                                    (ThingDef x) => x.IsApparel && x.BaseMarketValue > 100f, ThingSetMakerDefOf.Reward_TradeRequest);
                    //                                                                    pawn.skills.Learn(sDef: SkillDefOf.Social, xp: xPGainFirstPlace, direct: true);
                    //                                                                    TryAppendExpGainInfo(ref rewards, pawn, SkillDefOf.Social, xPGainFirstPlace);
                    //                                                                },
                    //                                                                BaseWeight_FirstPlace * 1 / GetOutcomeWeightFactor(pawn.GetStatValue(Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().relevantXpForEvent[kvp.Key]))),
                    //                                                            second: $"MFI_{kvp.Key}OutcomeFirstPlace".Translate(pawn.story.childhood.titleShort, rewards).AdjustedFor(pawn)));

                    //kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                    //                                                                () =>
                    //                                                                {
                    //                                                                    GenerateRewards(pawn, caravan, ref rewards, (ThingDef x) => x == ThingDefOf.Silver, ThingSetMakerDefOf.Reward_TradeRequest);
                    //                                                                    foreach (Pawn performer in caravan.PlayerPawnsForStoryteller)
                    //                                                                    {
                    //                                                                        if (!performer.story?.WorkTagIsDisabled(WorkTags.Artistic) ?? false)
                    //                                                                        {
                    //                                                                            pawn.skills.Learn(sDef: SkillDefOf.Artistic, xp: xPGainFirstLoser, direct: true);
                    //                                                                            TryAppendExpGainInfo(ref rewards, pawn, SkillDefOf.Artistic, xPGainFirstLoser);
                    //                                                                        }
                    //                                                                    }
                    //                                                                },
                    //                                                                BaseWeight_FirstLoser * 1 / GetOutcomeWeightFactor(pawn.GetStatValue(Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().relevantXpForEvent[kvp.Key]))),
                    //                                                            second: $"MFI_{kvp.Key}OutcomeFirstLoser".Translate(rewards).AdjustedFor(pawn)));

                    //kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                    //                                                                () =>
                    //                                                                {
                    //                                                                    GenerateRewards(pawn, caravan, ref rewards, /*(ThingDef x) => x == ThingDef.Named("Meat_Megaspider"),*/ thingSetMakerDef: ThingSetMakerDefOf.MapGen_PrisonCellStockpile);
                    //                                                                    rewards = string.Concat(rewards + "\n\n---\n\n" + (Rand.Bool ? string.Empty : Rand.Bool ? "MFI_AnnualExpoMedicalEmergency".Translate() : "MFI_AnnualExpoMedicalEmergencySerious".Translate()));
                    //                                                                    pawn.skills.Learn(sDef: SkillDefOf.Artistic, xp: xPGainFirstOther, direct: true);
                    //                                                                    TryAppendExpGainInfo(ref rewards, pawn, SkillDefOf.Artistic, xPGainFirstOther);
                    //                                                                },
                    //                                                                BaseWeight_FirstOther * 1 / GetOutcomeWeightFactor(pawn.GetStatValue(Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().relevantXpForEvent[kvp.Key]))),
                    //                                                            second: $"MFI_{kvp.Key}OutcomeFirstOther".Translate(host.GetCallLabel(), Find.WorldFeatures.features.RandomElement().name, rewards).AdjustedFor(pawn)));
                }
                else if (kvp.Key == "scienceFaire")
                {
                    //kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                    //                                                                () =>
                    //                                                                {
                    //                                                                    GenerateBuff(TechLevel.Industrial, ref rewards, pawn, caravan);
                    //                                                                    pawn.skills.Learn(sDef: SkillDefOf.Intellectual, xp: xPGainFirstPlace, direct: true);
                    //                                                                    TryAppendExpGainInfo(ref rewards, pawn, SkillDefOf.Intellectual, xPGainFirstPlace);
                    //                                                                },
                    //                                                                BaseWeight_FirstPlace * 1 / GetOutcomeWeightFactor(pawn.GetStatValue(Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().relevantXpForEvent[kvp.Key]))),
                    //                                                            second: $"MFI_{kvp.Key}OutcomeFirstPlace".Translate(rewards).AdjustedFor(pawn)));

                    //kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                    //                                                                () =>
                    //                                                                {
                    //                                                                    GenerateBuff(TechLevel.Undefined, ref rewards, pawn, caravan);
                    //                                                                    pawn.skills.Learn(SkillDefOf.Intellectual, xPGainFirstLoser, true);
                    //                                                                    TryAppendExpGainInfo(ref rewards, pawn, SkillDefOf.Intellectual, xPGainFirstLoser);
                    //                                                                },
                    //                                                                BaseWeight_FirstLoser * 1 / GetOutcomeWeightFactor(pawn.GetStatValue(Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().relevantXpForEvent[kvp.Key]))),
                    //                                                            second: $"MFI_{kvp.Key}OutcomeFirstLoser".Translate(rewards).AdjustedFor(pawn)));

                    //kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                    //                                                                () =>
                    //                                                                {
                    //                                                                    GenerateRewards(pawn, caravan, ref rewards, (ThingDef x) => x == ThingDefOf.TechprofSubpersonaCore, ThingSetMakerDefOf.MapGen_AncientTempleContents);
                    //                                                                    pawn.skills.Learn(SkillDefOf.Intellectual, xPGainFirstOther, true);
                    //                                                                    TryAppendExpGainInfo(ref rewards, pawn, SkillDefOf.Intellectual, xPGainFirstOther);
                    //                                                                },
                    //                                                                BaseWeight_FirstOther * 1 / GetOutcomeWeightFactor(pawn.GetStatValue(Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().relevantXpForEvent[kvp.Key]))),
                    //                                                            second: $"MFI_{kvp.Key}OutcomeFirstOther".Translate(rewards).AdjustedFor(pawn)));
                }
                else if (kvp.Key == "acousticShow")
                {
                    //kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                    //                                                                () =>
                    //                                                                {
                    //                                                                    GenerateRewards(pawn, caravan, ref rewards, null, ThingSetMakerDefOf.MapGen_AncientTempleContents);
                    //                                                                    GiveHappyThoughtsToCaravan(caravan, 20);
                    //                                                                },
                    //                                                                BaseWeight_FirstPlace * 1 / GetOutcomeWeightFactor(pawn.GetStatValue(Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().relevantXpForEvent[kvp.Key]))),
                    //                                                            second: $"MFI_{kvp.Key}OutcomeFirstPlace".Translate(rewards).AdjustedFor(pawn)));

                    //kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                    //                                                                () =>
                    //                                                                {
                    //                                                                    GenerateRewards(pawn, caravan, ref rewards, (ThingDef x) => x.equipmentType == EquipmentType.None, ThingSetMakerDefOf.MapGen_DefaultStockpile);
                    //                                                                    GiveHappyThoughtsToCaravan(caravan, 15);
                    //                                                                },
                    //                                                                BaseWeight_FirstLoser * 1 / GetOutcomeWeightFactor(pawn.GetStatValue(Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().relevantXpForEvent[kvp.Key]))),
                    //                                                            second: $"MFI_{kvp.Key}OutcomeFirstLoser".Translate(rewards).AdjustedFor(pawn)));

                    //kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                    //                                                                () =>
                    //                                                                {
                    //                                                                    GenerateRewards(pawn, caravan, ref rewards, null, ThingSetMakerDefOf.MapGen_PrisonCellStockpile);
                    //                                                                    GiveHappyThoughtsToCaravan(caravan, 10);
                    //                                                                },
                    //                                                                BaseWeight_FirstOther * 1 / GetOutcomeWeightFactor(pawn.GetStatValue(Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().relevantXpForEvent[kvp.Key]))),
                    //                                                            second: $"MFI_{kvp.Key}OutcomeFirstOther".Translate(rewards).AdjustedFor(pawn)));
                }
                else
                {
                    //kvp.Value.Add(new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                    //                                                                    () => GenerateRewards(pawn, caravan, null, ThingSetMakerDefOf.Reward_PeaceTalksGift),
                    //                                                                    1f),
                    //                                                                second: $"Something went wrong with More Faction Interaction. Contact the mod author with this year's theme. Rewards: {rewards}"));
                }
            }
        }


        private static void TryAppendExpGainInfo(ref string rewardsOutcome, Pawn pawn, SkillDef skill, float amount)
        {
            if (amount > 0)
                rewardsOutcome = rewardsOutcome + "\n\n" + "MFI_AnnualExpoXPGain"
                    .Translate(pawn.LabelShort, amount.ToString("F0"), skill.label);
        }

        private static DiaNode DialogueResolver(string textResult)
        {
            DiaNode resolver = new DiaNode(text: textResult);
            DiaOption diaOption = new DiaOption(text: "OK".Translate())
            {
                resolveTree = true
            };
            resolver.options.Add(item: diaOption);
            return resolver;
        }

        private static float GetOutcomeWeightFactor(float statPower) =>
            MoreFactionWar.FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks.BadOutcomeFactorAtStatPower.Evaluate(x: statPower);
    }
}
