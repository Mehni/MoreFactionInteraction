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
    public static class AnnualExpoDialogue
    {
        //private static readonly List<Pair<Pair<Action, float>, string>> tmpPossibleOutcomes = new List<Pair<Pair<Action, float>, string>>();
        private static readonly Dictionary<string, List<Pair<Tuple<Action, float>, string>>> eventsChancesAndOutcomes = new Dictionary<string, List<Pair<Tuple<Action, float>, string>>>();

        //string = Events
        //Action = outcome
        //float = chance
        //string = flavourtext

        public static DiaNode AnnualExpoDialogueNode(Pawn pawn, Caravan caravan, string eventTheme)
        {
            Tale tale = null;
            TaleReference taleRef = new TaleReference(tale);
            string flavourText = taleRef.GenerateText(TextGenerationPurpose.ArtDescription, RulePackDefOf.ArtDescriptionRoot_Taleless);

            DiaNode dialogueGreeting = new DiaNode(text: "MFI_AnnualExpoDialogueIntroduction".Translate(eventTheme.Translate(), flavourText));

            foreach (DiaOption option in DialogueOptions(pawn: pawn, caravan, eventTheme))
            {
                dialogueGreeting.options.Add(item: option);
            }
            if (Prefs.DevMode)
            {
                dialogueGreeting.options.Add(item: new DiaOption(text: "(Dev: Get random buff)")
                {
                    action = () => Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().ApplyRandomBuff(x => !x.Active && x.MinTechLevel() >= TechLevel.Undefined),
                    linkLateBind = () => DialogueResolver(textResult: "It is done. Sorry about the lack of fancy flavour text for this dev mode only option.")
                });
            }
            return dialogueGreeting;
        }

        private static IEnumerable<DiaOption> DialogueOptions(Pawn pawn, Caravan caravan, string eventTheme)
        {
            string annualExpoDialogueOutcome = "Something went wrong with More Faction Interaction. Please contact mod author.";

            yield return new DiaOption(text: "MFI_AnnualExpoFirstOption".Translate())
            {
                action = () => DetermineOutcome(pawn: pawn, caravan: caravan, eventTheme: eventTheme, annualExpoDialogueOutcome: out annualExpoDialogueOutcome),
                linkLateBind = () => DialogueResolver(textResult: annualExpoDialogueOutcome),
            };
        }

        private static void DetermineOutcome(Pawn pawn, Caravan caravan, string eventTheme, out string annualExpoDialogueOutcome)
        {
            PopulateDictionary(pawn, caravan);
            Pair<Tuple<Action, float>, string> outcome = eventsChancesAndOutcomes[eventTheme].RandomElementByWeight(x => x.First.Item2);
            outcome.First.Item1();
            annualExpoDialogueOutcome = outcome.Second;

            //pawn.skills.Learn(sDef: SkillDefOf.Social, xp: 6000f, direct: true);
        }

        private static void PopulateDictionary(Pawn pawn, Caravan caravan)
        {
            eventsChancesAndOutcomes.Clear();

            float outcomeWeightFactor = 1 / GetOutcomeWeightFactor(pawn.GetStatValue(stat: StatDefOf.NegotiationAbility));

            foreach (string Event in Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().Events.Keys)
            {
                eventsChancesAndOutcomes.Add(Event, new List<Pair<Tuple<Action, float>, string>>());
            }

            //attempts to avoid hardcoding this stuff fell flat because I am a stickler for cause and effect in flavour
            foreach (KeyValuePair<string, List<Pair<Tuple<Action, float>, string>>> kvp in eventsChancesAndOutcomes)
            {
                string rewards = string.Empty;
                if (kvp.Key == "gameOfUrComp")
                {
                    kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                                                                                    () => GenerateRewards(pawn, caravan, out rewards, 
                                                                                      thingSetMakerDef: ThingSetMakerDefOf.MapGen_AncientTempleContents),                      
                                                                                    outcomeWeightFactor),
                                                                                second: $"MFI_{kvp.Key}OutcomeFirstPlace".Translate(rewards).AdjustedFor(pawn)));

                    kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                                                                                    () => GenerateRewards(pawn, caravan, out rewards,
                                                                                    thingSetMakerDef: ThingSetMakerDefOf.MapGen_AncientPodContents),
                                                                                    outcomeWeightFactor),
                                                                                second: $"MFI_{kvp.Key}CompOutcomeFirstLoser".Translate(rewards).AdjustedFor(pawn)));

                    kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                                                                                    () => GenerateRewards(pawn, caravan, out rewards),
                                                                                    outcomeWeightFactor),
                                                                                second: $"MFI_{kvp.Key}CompOutcomeFirstOther".Translate(rewards).AdjustedFor(pawn)));
                }
                else if (kvp.Key == "shootingComp")
                {
                    kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                                                                                    () => GenerateRewards(pawn, caravan, out rewards,
                                                                                    //nice pew-pews
                                                                                        (ThingDef x) => x.equipmentType == EquipmentType.Primary, ThingSetMakerDefOf.Reward_ItemStashQuestContents),
                                                                                    outcomeWeightFactor),
                                                                                second: $"MFI_{kvp.Key}OutcomeFirstPlace".Translate(rewards).AdjustedFor(pawn)));

                    kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                                                                                    () => GenerateRewards(pawn, caravan, out rewards,
                                                                                    //nice bionics
                                                                                        (ThingDef x) => x.isTechHediff && x.techLevel >= TechLevel.Industrial, ThingSetMakerDefOf.Reward_ItemStashQuestContents),
                                                                                    outcomeWeightFactor),
                                                                                second: $"MFI_{kvp.Key}CompOutcomeFirstLoser".Translate(rewards).AdjustedFor(pawn)));

                    kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                                                                                    () => GenerateRewards(pawn, caravan, out rewards, 
                                                                                    //GLORIOUS POTAT!
                                                                                        (ThingDef x) => x == ThingDefOf.RawPotatoes),
                                                                                    outcomeWeightFactor),
                                                                                second: $"MFI_{kvp.Key}CompOutcomeFirstOther".Translate(rewards).AdjustedFor(pawn)));
                }
                else if (kvp.Key == "culturalSwap")
                {
                    if (MFI_Utilities.TryGetBestArt(caravan, out Thing art, out Pawn owner))
                    {
                        //kvp.Value.Add(new Pair<Tuple<Action, float>, string>(new Tuple<Action, float>(
                        //                                                        () => {
                                                                                        Dialog_NodeTree tree = (Dialog_NodeTree)Find.WindowStack.Windows.First(x => x is Dialog_NodeTree);
                                                                                        tree.GotoNode(DialogueResolverArtOffer("MFI_culturalSwapOutcomeWhoaYouActuallyBroughtArt", art, caravan));
                         //                                                             },
                         //                                                       100f), "hey"));
                        return;
                    }

                    kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                                                                                    () => GenerateRewards(pawn, caravan, out rewards,
                                                                                        (ThingDef x) => x.IsApparel && x.BaseMarketValue > 100f, ThingSetMakerDefOf.Reward_TradeRequest),
                                                                                    outcomeWeightFactor),
                                                                                second: $"MFI_{kvp.Key}OutcomeFirstPlace".Translate(rewards).AdjustedFor(pawn)));

                    kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                                                                                    () => GenerateRewards(pawn, caravan, out rewards, (ThingDef x) => x == ThingDefOf.Silver, ThingSetMakerDefOf.Reward_TradeRequest),
                                                                                    outcomeWeightFactor),
                                                                                second: $"MFI_{kvp.Key}CompOutcomeFirstLoser".Translate(rewards).AdjustedFor(pawn)));

                    kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                                                                                    () => GenerateRewards(pawn, caravan, out rewards, (ThingDef x) => x == ThingDef.Named("Meat_Megaspider"), ThingSetMakerDefOf.MapGen_PrisonCellStockpile),
                                                                                    outcomeWeightFactor),
                                                                                second: $"MFI_{kvp.Key}CompOutcomeFirstOther".Translate(rewards).AdjustedFor(pawn)));
                }
                else if (kvp.Key == "scienceFaire")
                {
                    kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                        // may have to write a 3rd method for giving XP and handing out Buffs.
                                                                                    () => GenerateRewards(pawn, caravan, out rewards),
                                                                                    outcomeWeightFactor),
                                                                                second: $"MFI_{kvp.Key}OutcomeFirstPlace".Translate(rewards).AdjustedFor(pawn)));

                    kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                                                                                    () => GenerateRewards(pawn, caravan, out rewards),
                                                                                    outcomeWeightFactor),
                                                                                second: $"MFI_{kvp.Key}CompOutcomeFirstLoser".Translate(rewards).AdjustedFor(pawn)));

                    kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                                                                                    () => GenerateRewards(pawn, caravan, out rewards, (ThingDef x) => x == ThingDefOf.TechprofSubpersonaCore, ThingSetMakerDefOf.MapGen_AncientTempleContents),
                                                                                    outcomeWeightFactor),
                                                                                second: $"MFI_{kvp.Key}CompOutcomeFirstOther".Translate(rewards).AdjustedFor(pawn)));
                }
                else if (kvp.Key == "acousticShow")
                {
                    kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                                                                                    () => GenerateRewards(pawn, caravan, out rewards),
                                                                                    outcomeWeightFactor),
                                                                                second: $"MFI_{kvp.Key}OutcomeFirstPlace".Translate(rewards).AdjustedFor(pawn)));

                    kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                                                                                    () => GenerateRewards(pawn, caravan, out rewards),
                                                                                    outcomeWeightFactor),
                                                                                second: $"MFI_{kvp.Key}CompOutcomeFirstLoser".Translate(rewards).AdjustedFor(pawn)));

                    kvp.Value.Add(item: new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                                                                                    () => GenerateRewards(pawn, caravan, out rewards),
                                                                                    outcomeWeightFactor),
                                                                                second: $"MFI_{kvp.Key}CompOutcomeFirstOther".Translate(rewards).AdjustedFor(pawn)));
                }
                else
                {
                    kvp.Value.Add(new Pair<Tuple<Action, float>, string>(first: new Tuple<Action, float>(
                                                                                        () => GenerateRewards(pawn, caravan, out rewards),
                                                                                        outcomeWeightFactor),
                                                                                    second: "Something went wrong with More Faction Interaction. Contact the mod author with this year's theme"));
                }
            }
        }

        private static void GenerateRewards(Pawn pawn, Caravan caravan, out string rewardsToCommaList, Predicate<ThingDef> globalValidator = null, ThingSetMakerDef thingSetMakerDef = null)
        {
            List<Thing> rewards = new List<Thing>();
            if (thingSetMakerDef != null)
            {
                ThingSetMakerParams parms = default;
                parms.validator = globalValidator;
                parms.qualityGenerator = QualityGenerator.Reward;
                rewards = thingSetMakerDef.root.Generate(parms);
            }
            else if (globalValidator(ThingDefOf.RawPotatoes))
                rewards.Add(ThingMaker.MakeThing(ThingDefOf.RawPotatoes));

            rewardsToCommaList = GenThing.ThingsToCommaList(rewards);
            GenThing.TryAppendSingleRewardInfo(ref rewardsToCommaList, rewards);
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

        private static DiaNode DialogueResolverArtOffer(string textResult, Thing broughtSculpture, Caravan caravan)
        {
            float marketValue = broughtSculpture.GetStatValue(StatDefOf.MarketValue);
            DiaNode resolver = new DiaNode(text: textResult.Translate(broughtSculpture, marketValue * 6, marketValue));
            DiaOption accept = new DiaOption(text: "RansomDemand_Accept".Translate())
            {
                resolveTree = true,
                action = () => 
                {
                    broughtSculpture.Destroy();
                    Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                    silver.stackCount = (int) (marketValue * 6);
                    CaravanInventoryUtility.GiveThing(caravan, silver);
                }
            };
            DiaOption reject = new DiaOption("RansomDemand_Reject".Translate())
            {
                resolveTree = true
            };
            resolver.options.Add(accept);
            resolver.options.Add(reject);
            return resolver;
        }

        private static float GetOutcomeWeightFactor(float statPower) =>
            MoreFactionWar.FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks.BadOutcomeFactorAtStatPower.Evaluate(x: statPower);

    }
}
