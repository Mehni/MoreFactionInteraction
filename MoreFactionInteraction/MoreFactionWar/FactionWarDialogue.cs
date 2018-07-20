using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using RimWorld.Planet;
using Verse.AI.Group;

namespace MoreFactionInteraction.MoreFactionWar
{

    public static class FactionWarDialogue
    {
        private static readonly List<Pair<Pair<Action, float>, string>> tmpPossibleOutcomes = new List<Pair<Pair<Action, float>, string>>();
        private const float BaseWeight_Disaster = 0.05f;
        private const float BaseWeight_Backfire = 0.1f;
        private const float BaseWeight_TalksFlounder = 0.2f;
        private const float BaseWeight_Success = 0.55f;
        private const float BaseWeight_Triumph = 0.1f;


        public static DiaNode FactionWarPeaceTalks(Pawn pawn, Faction factionOne, Faction factionInstigator, IIncidentTarget incidentTarget = null)
        {

            string factionInstigatorLeaderName = factionInstigator.leader != null
                ? factionInstigator.leader.Name.ToStringFull
                : factionInstigator.Name;

            string factionOneLeaderName =
                factionOne.leader != null ? factionOne.leader.Name.ToStringFull : factionOne.Name;

            DiaNode dialogueGreeting = new DiaNode(text: "MFI_FactionWarPeaceTalksIntroduction".Translate(args: new object[] { factionOneLeaderName, factionInstigatorLeaderName, pawn.Label }));

            foreach (DiaOption option in DialogueOptions(pawn: pawn, factionOne: factionOne, factionInstigator: factionInstigator, incidentTarget))
            {
                dialogueGreeting.options.Add(item: option);
            }
            if (Prefs.DevMode)
            {
                dialogueGreeting.options.Add(item: new DiaOption(text: "(Dev: start war)")
                                                { action =() => 
                                                { Find.World.GetComponent<WorldComponent_MFI_FactionWar>().StartWar(factionOne, factionInstigator, true);
                                                }, linkLateBind = () => DialogueResolver("Alrighty. War started.")
                                                });
            }

            return dialogueGreeting;
        }

        private static IEnumerable<DiaOption> DialogueOptions(Pawn pawn, Faction factionOne, Faction factionInstigator, IIncidentTarget incidentTarget)
        {
            string factionWarNegotiationsOutcome = "Something went wrong with More Faction Interaction. Please contact mod author.";

            yield return new DiaOption(text: "MFI_FactionWarPeaceTalksCurryFavour".Translate(args: new object[] { factionOne.Name }))
            {
                action = () =>
                {
                    DetermineOutcome(favouredFaction: factionOne, burdenedFaction: factionInstigator, pawn: pawn, desiredOutcome: 1, factionWarNegotiationsOutcome: out factionWarNegotiationsOutcome);
                },
                linkLateBind = () => DialogueResolver(textResult: factionWarNegotiationsOutcome),
            };
            yield return new DiaOption(text: "MFI_FactionWarPeaceTalksCurryFavour".Translate(args: new object[] { factionInstigator.Name }))
            {
                action = () =>
                {
                    DetermineOutcome(favouredFaction: factionInstigator, burdenedFaction: factionOne, pawn: pawn, desiredOutcome: 2, factionWarNegotiationsOutcome: out factionWarNegotiationsOutcome);
                },
                linkLateBind = () => DialogueResolver(textResult: factionWarNegotiationsOutcome),
            };
            yield return new DiaOption(text: "MFI_FactionWarPeaceTalksSabotage".Translate())
            {
                action = () =>
                {
                    DetermineOutcome(favouredFaction: factionOne, burdenedFaction: factionInstigator, pawn: pawn, desiredOutcome: 3, factionWarNegotiationsOutcome: out factionWarNegotiationsOutcome, incidentTarget: incidentTarget);
                },
                linkLateBind = () => DialogueResolver(textResult: factionWarNegotiationsOutcome),
            };
            yield return new DiaOption(text: "MFI_FactionWarPeaceTalksBrokerPeace".Translate())
            {
                action = () =>
                {
                    DetermineOutcome(favouredFaction: factionOne, burdenedFaction: factionInstigator, pawn: pawn, desiredOutcome: 4, factionWarNegotiationsOutcome: out factionWarNegotiationsOutcome);
                },
                linkLateBind = () => DialogueResolver(textResult: factionWarNegotiationsOutcome),
            };
        }

        public static void DetermineOutcome(Faction favouredFaction, Faction burdenedFaction, Pawn pawn, int desiredOutcome, out string factionWarNegotiationsOutcome, IIncidentTarget incidentTarget = null)
        {
            float badOutcomeWeightFactor = GetBadOutcomeWeightFactor(diplomacyPower: pawn.GetStatValue(stat: StatDefOf.NegotiationAbility));
            float goodOutcomeWeightFactor = 1f / badOutcomeWeightFactor;
            factionWarNegotiationsOutcome = "Something went wrong with More Faction Interaction. Please contact mod author.";

            if (desiredOutcome == 1 || desiredOutcome == 2)
            {
                tmpPossibleOutcomes.Clear();

                tmpPossibleOutcomes.Add(item: new Pair<Pair<Action, float>, string>(first: new Pair<Action, float>(
                                                                                        first: () => Outcome_TalksFactionFavourDisaster(favouredFaction: favouredFaction, burdenedFaction: burdenedFaction, pawn: pawn),
                                                                                        second: BaseWeight_Disaster * badOutcomeWeightFactor),
                                                                                    second: "MFI_FactionWarFavourFactionDisaster".Translate(favouredFaction.Name, burdenedFaction.Name)));

                tmpPossibleOutcomes.Add(item: new Pair<Pair<Action, float>, string>(first: new Pair<Action, float>(
                                                                                        first: () => Outcome_TalksFactionsFavourBackfire(favouredFaction: favouredFaction, burdenedFaction: burdenedFaction, pawn: pawn),
                                                                                        second: BaseWeight_Backfire * badOutcomeWeightFactor),
                                                                                    second: "MFI_FactionWarFavourFactionBackFire".Translate(favouredFaction.Name, burdenedFaction.Name)));

                tmpPossibleOutcomes.Add(item: new Pair<Pair<Action, float>, string>(first: new Pair<Action, float>(
                                                                                        first: () => Outcome_TalksFactionsFavourFlounder(favouredFaction: favouredFaction, burdenedFaction: burdenedFaction, pawn: pawn),
                                                                                        second: BaseWeight_TalksFlounder),
                                                                                    second: "MFI_FactionWarFavourFactionFlounder".Translate(favouredFaction.Name, burdenedFaction.Name)));

                tmpPossibleOutcomes.Add(item: new Pair<Pair<Action, float>, string>(first: new Pair<Action, float>(
                                                                                        first: () => Outcome_TalksFactionsFavourSuccess(favouredFaction: favouredFaction, burdenedFaction: burdenedFaction, pawn: pawn),
                                                                                        second: BaseWeight_Success * goodOutcomeWeightFactor),
                                                                                    second: "MFI_FactionWarFavourFactionSuccess".Translate(favouredFaction.Name, burdenedFaction.Name)));

                tmpPossibleOutcomes.Add(item: new Pair<Pair<Action, float>, string>(first: new Pair<Action, float>(
                                                                                        first: () => Outcome_TalksFactionsFavourTriumph(favouredFaction: favouredFaction, burdenedFaction: burdenedFaction, pawn: pawn),
                                                                                        second: BaseWeight_Triumph * goodOutcomeWeightFactor),
                                                                                    second: "MFI_FactionWarFavourFactionTriumph".Translate(favouredFaction.Name, burdenedFaction.Name)));

                Action first = tmpPossibleOutcomes.RandomElementByWeight(weightSelector: (Pair<Pair<Action, float>, string> x) => x.First.Second).First.First;
                factionWarNegotiationsOutcome = tmpPossibleOutcomes.RandomElementByWeight(weightSelector: (Pair<Pair<Action, float>, string> x) => x.First.Second).Second;
                first();

                pawn.skills.Learn(sDef: SkillDefOf.Social, xp: 6000f, direct: true);
            }
            else if (desiredOutcome == 3)
            {
                tmpPossibleOutcomes.Clear();

                tmpPossibleOutcomes.Add(item: new Pair<Pair<Action, float>, string>(first: new Pair<Action, float>(
                                                                                        first: () => Outcome_TalksSabotageDisaster(favouredFaction: favouredFaction, burdenedFaction: burdenedFaction, pawn: pawn, incidentTarget),
                                                                                        second: BaseWeight_Disaster * badOutcomeWeightFactor),
                                                                                    second: "MFI_FactionWarSabotageDisaster".Translate(favouredFaction.Name, burdenedFaction.Name)));

                tmpPossibleOutcomes.Add(item: new Pair<Pair<Action, float>, string>(first: new Pair<Action, float>(
                                                                                        first: () => Outcome_TalksSabotageBackfire(favouredFaction: favouredFaction, burdenedFaction: burdenedFaction, pawn: pawn),
                                                                                        second: BaseWeight_Backfire * badOutcomeWeightFactor),
                                                                                    second: "MFI_FactionWarSabotageBackFire".Translate(favouredFaction.Name, burdenedFaction.Name)));

                tmpPossibleOutcomes.Add(item: new Pair<Pair<Action, float>, string>(first: new Pair<Action, float>(
                                                                                        first: () => Outcome_TalksSabotageFlounder(favouredFaction: favouredFaction, burdenedFaction: burdenedFaction, pawn: pawn),
                                                                                        second: BaseWeight_TalksFlounder),
                                                                                    second: "MFI_FactionWarSabotageFlounder".Translate(favouredFaction.Name, burdenedFaction.Name)));

                tmpPossibleOutcomes.Add(item: new Pair<Pair<Action, float>, string>(first: new Pair<Action, float>(
                                                                                        first: () => Outcome_TalksSabotageSuccess(favouredFaction: favouredFaction, burdenedFaction: burdenedFaction, pawn: pawn),
                                                                                        second: BaseWeight_Success * goodOutcomeWeightFactor),
                                                                                    second: "MFI_FactionWarSabotageSuccess".Translate(favouredFaction.Name, burdenedFaction.Name)));

                tmpPossibleOutcomes.Add(item: new Pair<Pair<Action, float>, string>(first: new Pair<Action, float>(
                                                                                        first: () => Outcome_TalksSabotageTriumph(favouredFaction: favouredFaction, burdenedFaction: burdenedFaction, pawn: pawn),
                                                                                        second: BaseWeight_Triumph * goodOutcomeWeightFactor),
                                                                                    second: "MFI_FactionWarSabotageTriumph".Translate(favouredFaction.Name, burdenedFaction.Name)));

                Action first = tmpPossibleOutcomes.RandomElementByWeight(weightSelector: (Pair<Pair<Action, float>, string> x) => x.First.Second).First.First;
                factionWarNegotiationsOutcome = tmpPossibleOutcomes.RandomElementByWeight(weightSelector: (Pair<Pair<Action, float>, string> x) => x.First.Second).Second;
                first();

                pawn.skills.Learn(sDef: SkillDefOf.Social, xp: 6000f, direct: true);
            }
            else if (desiredOutcome == 4)
            {
                tmpPossibleOutcomes.Clear();

                tmpPossibleOutcomes.Add(item: new Pair<Pair<Action, float>, string>(first: new Pair<Action, float>(
                                                                                        first: () => Outcome_TalksBrokerPeaceDisaster(favouredFaction: favouredFaction, burdenedFaction: burdenedFaction, pawn: pawn),
                                                                                        second: BaseWeight_Disaster * badOutcomeWeightFactor),
                                                                                    second: "MFI_FactionWarBrokerPeaceDisaster".Translate(favouredFaction.Name, burdenedFaction.Name)));

                tmpPossibleOutcomes.Add(item: new Pair<Pair<Action, float>, string>(first: new Pair<Action, float>(
                                                                                        first: () => Outcome_TalksBrokerPeaceBackfire(favouredFaction: favouredFaction, burdenedFaction: burdenedFaction, pawn: pawn),
                                                                                        second: BaseWeight_Backfire * badOutcomeWeightFactor),
                                                                                    second: "MFI_FactionWarBrokerPeaceBackFire".Translate(favouredFaction.Name, burdenedFaction.Name)));

                tmpPossibleOutcomes.Add(item: new Pair<Pair<Action, float>, string>(first: new Pair<Action, float>(
                                                                                        first: () => Outcome_TalksBrokerPeaceFlounder(favouredFaction: favouredFaction, burdenedFaction: burdenedFaction, pawn: pawn),
                                                                                        second: BaseWeight_TalksFlounder),
                                                                                    second: "MFI_FactionWarBrokerPeaceFlounder".Translate(favouredFaction.Name, burdenedFaction.Name)));

                tmpPossibleOutcomes.Add(item: new Pair<Pair<Action, float>, string>(first: new Pair<Action, float>(
                                                                                        first: () => Outcome_TalksBrokerPeaceSuccess(favouredFaction: favouredFaction, burdenedFaction: burdenedFaction, pawn: pawn),
                                                                                        second: BaseWeight_Success * goodOutcomeWeightFactor),
                                                                                    second: "MFI_FactionWarBrokerPeaceSuccess".Translate(favouredFaction.Name, burdenedFaction.Name)));

                tmpPossibleOutcomes.Add(item: new Pair<Pair<Action, float>, string>(first: new Pair<Action, float>(
                                                                                        first: () => Outcome_TalksBrokerPeaceTriumph(favouredFaction: favouredFaction, burdenedFaction: burdenedFaction, pawn: pawn),
                                                                                        second: BaseWeight_Triumph * goodOutcomeWeightFactor),
                                                                                    second: "MFI_FactionWarBrokerPeaceTriumph".Translate(favouredFaction.Name, burdenedFaction.Name)));

                Action first = tmpPossibleOutcomes.RandomElementByWeight(weightSelector: (Pair<Pair<Action, float>, string> x) => x.First.Second).First.First;
                factionWarNegotiationsOutcome = tmpPossibleOutcomes.RandomElementByWeight(weightSelector: (Pair<Pair<Action, float>, string> x) => x.First.Second).Second;
                first();

                pawn.skills.Learn(sDef: SkillDefOf.Social, xp: 6000f, direct: true);
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

    #region TalksFavourFaction
        private static void Outcome_TalksFactionFavourDisaster(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            favouredFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: -FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                     .GoodWill_FactionWarPeaceTalks_ImpactMedium.RandomInRange);

            burdenedFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: -FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                     .GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange);
            burdenedFaction.TrySetRelationKind(pawn.Faction, FactionRelationKind.Hostile, false, null, null);

            Find.World.GetComponent<WorldComponent_MFI_FactionWar>().StartWar(favouredFaction, burdenedFaction, favouredFaction.leader == pawn);
        }

        private static void Outcome_TalksFactionsFavourBackfire(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            favouredFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: -FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                     .GoodWill_FactionWarPeaceTalks_ImpactSmall.RandomInRange);

            burdenedFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: -FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                     .GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange);
            burdenedFaction.TrySetRelationKind(pawn.Faction, FactionRelationKind.Hostile, false, null, null);

            Find.World.GetComponent<WorldComponent_MFI_FactionWar>().StartWar(favouredFaction, burdenedFaction, favouredFaction.leader == pawn);
        }

        private static void Outcome_TalksFactionsFavourFlounder(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            favouredFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                 .GoodWill_FactionWarPeaceTalks_ImpactMedium.RandomInRange);

            burdenedFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: -FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                  .GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange);
        }

        private static void Outcome_TalksFactionsFavourSuccess(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            favouredFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                 .GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange);

            burdenedFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: -FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                  .GoodWill_FactionWarPeaceTalks_ImpactMedium.RandomInRange);
        }

        private static void Outcome_TalksFactionsFavourTriumph(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            favouredFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                 .GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange);

            burdenedFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: -FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                  .GoodWill_FactionWarPeaceTalks_ImpactSmall.RandomInRange);
        }
    #endregion

    #region TalksSabotage
        private static void Outcome_TalksSabotageTriumph(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            favouredFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                 .GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange);

            burdenedFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                  .GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange);

            Find.World.GetComponent<WorldComponent_MFI_FactionWar>().StartWar(favouredFaction, burdenedFaction, favouredFaction.leader == pawn);
        }

        private static void Outcome_TalksSabotageSuccess(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            favouredFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                 .GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange);

            burdenedFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                  .GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange);

            Find.World.GetComponent<WorldComponent_MFI_FactionWar>().StartWar(favouredFaction, burdenedFaction, favouredFaction.leader == pawn);
        }

        private static void Outcome_TalksSabotageFlounder(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {

        }

        private static void Outcome_TalksSabotageBackfire(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            favouredFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: -FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                 .GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange);

            burdenedFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: -FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                  .GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange);

            Find.World.GetComponent<WorldComponent_MFI_FactionWar>().ResolveWar();
        }

        private static void Outcome_TalksSabotageDisaster(Faction favouredFaction, Faction burdenedFaction, Pawn pawn, IIncidentTarget incidentTarget)
        {
            favouredFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: -FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                  .GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange);

            favouredFaction.TrySetRelationKind(pawn.Faction, FactionRelationKind.Hostile, false, null, null);

            burdenedFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: -FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                  .GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange);
            burdenedFaction.TrySetRelationKind(pawn.Faction, FactionRelationKind.Hostile, false, null, null);

            Find.World.GetComponent<WorldComponent_MFI_FactionWar>().StartWar(favouredFaction, burdenedFaction, favouredFaction.leader == pawn);

            LongEventHandler.QueueLongEvent(delegate
            {

                IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, incidentTarget);
                incidentParms.faction = favouredFaction;
                PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms, true);
                defaultPawnGroupMakerParms.generateFightersOnly = true;
                List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList();

                IncidentParms burdenedFactionIncidentParms = incidentParms;
                burdenedFactionIncidentParms.faction = burdenedFaction;
                PawnGroupMakerParms burdenedFactionPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, incidentParms, true);
                burdenedFactionPawnGroupMakerParms.generateFightersOnly = true;
                List<Pawn> burdenedFactionWarriors = PawnGroupMakerUtility.GeneratePawns(burdenedFactionPawnGroupMakerParms).ToList();

                List<Pawn> combinedList = new List<Pawn>();
                combinedList.AddRange(list);
                combinedList.AddRange(burdenedFactionWarriors);

                Map map = CaravanIncidentUtility.SetupCaravanAttackMap(incidentTarget as Caravan, combinedList, true);

                if (list.Any())
                {
                    LordMaker.MakeNewLord(incidentParms.faction, new LordJob_AssaultColony(favouredFaction), map, list);
                }

                if (burdenedFactionWarriors.Any())
                {
                    LordMaker.MakeNewLord(burdenedFactionIncidentParms.faction, new LordJob_AssaultColony(burdenedFaction), map, burdenedFactionWarriors);
                }

                Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;

            }, "GeneratingMapForNewEncounter", false, null);
        }
    #endregion

    #region TalksBrokerPeace
        private static void Outcome_TalksBrokerPeaceTriumph(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            favouredFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                 .GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange);

            burdenedFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                  .GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange);
        }

        private static void Outcome_TalksBrokerPeaceSuccess(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            favouredFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                 .GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange);

            burdenedFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                  .GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange);
        }

        private static void Outcome_TalksBrokerPeaceFlounder(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            favouredFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                 .GoodWill_FactionWarPeaceTalks_ImpactMedium.RandomInRange);

            burdenedFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                  .GoodWill_FactionWarPeaceTalks_ImpactMedium.RandomInRange);
        }

        private static void Outcome_TalksBrokerPeaceBackfire(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            //"rescheduled for later"
        }

        private static void Outcome_TalksBrokerPeaceDisaster(Faction favouredFaction, Faction burdenedFaction, Pawn pawn)
        {
            favouredFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: -FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                 .GoodWill_FactionWarPeaceTalks_ImpactSmall.RandomInRange);

            burdenedFaction.TryAffectGoodwillWith(other: pawn.Faction,
                                                  goodwillChange: -FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
                                                                  .GoodWill_FactionWarPeaceTalks_ImpactSmall.RandomInRange);

            Find.World.GetComponent<WorldComponent_MFI_FactionWar>().StartWar(favouredFaction, burdenedFaction, favouredFaction.leader == pawn);
        }
    #endregion

        private static float GetBadOutcomeWeightFactor(float diplomacyPower) =>
            FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks.BadOutcomeFactorAtDiplomacyPower.Evaluate(x: diplomacyPower);
    }
}
