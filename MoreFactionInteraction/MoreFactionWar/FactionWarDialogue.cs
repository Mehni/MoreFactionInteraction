using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using RimWorld.Planet;
using Verse.AI.Group;

namespace MoreFactionInteraction.MoreFactionWar
{
    public enum DesiredOutcome
    {
        CURRY_FAVOUR_FACTION_ONE = 1,
        CURRY_FAVOUR_FACTION_TWO = 2,
        SABOTAGE = 3,
        BROKER_PEACE = 4
    }

    public class FactionWarDialogue
    {
        private List<(Action Outcome_Talks, float weight, string dialogueResolverText)> outComes;

        private const float BaseWeight_Disaster = 0.05f;
        private const float BaseWeight_Backfire = 0.1f;
        private const float BaseWeight_TalksFlounder = 0.2f;
        private const float BaseWeight_Success = 0.55f;
        private const float BaseWeight_Triumph = 0.1f;

        private Pawn _pawn;
        private Faction _favouredFaction;
        private Faction _burdenedFaction;
        private IIncidentTarget _incidentTarget;

        public FactionWarDialogue(Pawn pawn, Faction factionOne, Faction factionInstigator, IIncidentTarget incidentTarget)
        {
            _pawn = pawn;
            _favouredFaction = factionOne;
            _burdenedFaction = factionInstigator;
            _incidentTarget = incidentTarget;
        }

        public DiaNode FactionWarPeaceTalks()
        {
            string factionInstigatorLeaderName = _burdenedFaction.leader != null
                ? _burdenedFaction.leader.Name.ToStringFull
                : _burdenedFaction.Name;

            string factionOneLeaderName =
                _favouredFaction.leader != null ? _favouredFaction.leader.Name.ToStringFull : _favouredFaction.Name;

            DiaNode dialogueGreeting = new DiaNode(text: "MFI_FactionWarPeaceTalksIntroduction".Translate( factionOneLeaderName, factionInstigatorLeaderName, _pawn.Label ));

            foreach (DiaOption option in DialogueOptions())
            {
                dialogueGreeting.options.Add(item: option);
            }
            if (Prefs.DevMode)
            {
                dialogueGreeting.options.Add(item: new DiaOption(text: "(Dev: start war)")
                {
                    action =() => Find.World.GetComponent<WorldComponent_MFI_FactionWar>().StartWar(factionOne: _favouredFaction, factionInstigator: _burdenedFaction, selfResolved: false),
                    linkLateBind = () => DialogueResolver(textResult: "Alrighty. War started. Sorry about the lack of fancy flavour text for this dev mode only option."),
                });
            }
            return dialogueGreeting;
        }

        private IEnumerable<DiaOption> DialogueOptions()
        {
            string factionWarNegotiationsOutcome = "Something went wrong with More Faction Interaction. Please contact mod author.";

            yield return new DiaOption(text: "MFI_FactionWarPeaceTalksCurryFavour".Translate( _favouredFaction.Name ))
            {
                action = () => DetermineOutcome(DesiredOutcome.CURRY_FAVOUR_FACTION_ONE, out factionWarNegotiationsOutcome),
                linkLateBind = () => DialogueResolver(factionWarNegotiationsOutcome),
            };
            yield return new DiaOption(text: "MFI_FactionWarPeaceTalksCurryFavour".Translate( _burdenedFaction.Name ))
            {
                action = () =>
                {
                    SwapFavouredFaction();
                    DetermineOutcome(DesiredOutcome.CURRY_FAVOUR_FACTION_TWO, out factionWarNegotiationsOutcome);
                },
                linkLateBind = () => DialogueResolver(textResult: factionWarNegotiationsOutcome),
            };
            yield return new DiaOption(text: "MFI_FactionWarPeaceTalksSabotage".Translate())
            {
                action = () => DetermineOutcome(DesiredOutcome.SABOTAGE, out factionWarNegotiationsOutcome),
                linkLateBind = () => DialogueResolver(factionWarNegotiationsOutcome),
            };
            yield return new DiaOption(text: "MFI_FactionWarPeaceTalksBrokerPeace".Translate())
            {
                action = () => DetermineOutcome(DesiredOutcome.BROKER_PEACE, out factionWarNegotiationsOutcome),
                linkLateBind = () => DialogueResolver(factionWarNegotiationsOutcome),
            };
        }

        public void DetermineOutcome(DesiredOutcome desiredOutcome, out string factionWarNegotiationsOutcome)
        {
            var badOutcomeWeightFactor = BaseWeight_Disaster * GetBadOutcomeWeightFactor(_pawn);
            float goodOutcomeWeightFactor = 1f / badOutcomeWeightFactor;
            factionWarNegotiationsOutcome = "Something went wrong with More Faction Interaction. Please contact mod author.";

            if (desiredOutcome == DesiredOutcome.CURRY_FAVOUR_FACTION_ONE ||
                desiredOutcome == DesiredOutcome.CURRY_FAVOUR_FACTION_TWO)
            {
                factionWarNegotiationsOutcome = CurryFavour(badOutcomeWeightFactor, goodOutcomeWeightFactor);
            }
            else if (desiredOutcome == DesiredOutcome.SABOTAGE)
            {
                factionWarNegotiationsOutcome = Sabotage(badOutcomeWeightFactor, goodOutcomeWeightFactor);
            }
            else if (desiredOutcome == DesiredOutcome.BROKER_PEACE)
            {
                factionWarNegotiationsOutcome = BrokerPeace(badOutcomeWeightFactor, goodOutcomeWeightFactor);
            }
        }

        private string CurryFavour(float badOutcomeWeightFactor, float goodOutcomeWeightFactor)
        {
            outComes = new List<(Action, float, string)>();

            string factionWarNegotiationsOutcome;
            outComes.Add((
                () => HandleOutcome(MFI_DiplomacyTunings.FavourDisaster),
                BaseWeight_Disaster * GetBadOutcomeWeightFactor(_pawn),
                "MFI_FactionWarFavourFactionDisaster".Translate(_favouredFaction.Name, _burdenedFaction.Name)));

            outComes.Add((
                () => HandleOutcome(MFI_DiplomacyTunings.FavourBackfire),
                BaseWeight_Backfire * badOutcomeWeightFactor,
                "MFI_FactionWarFavourFactionBackFire".Translate(_favouredFaction.Name, _burdenedFaction.Name)));

            outComes.Add((
                () => HandleOutcome(MFI_DiplomacyTunings.FavourFlounder),
                BaseWeight_TalksFlounder,
                "MFI_FactionWarFavourFactionFlounder".Translate(_favouredFaction.Name, _burdenedFaction.Name)));

            outComes.Add((
                () => HandleOutcome(MFI_DiplomacyTunings.FavourSuccess),
                BaseWeight_Success * goodOutcomeWeightFactor,
                "MFI_FactionWarFavourFactionSuccess".Translate(_favouredFaction.Name, _burdenedFaction.Name)));

            outComes.Add((
                () => HandleOutcome(MFI_DiplomacyTunings.FavourTriumph),
                BaseWeight_Triumph * goodOutcomeWeightFactor,
                "MFI_FactionWarFavourFactionTriumph".Translate(_favouredFaction.Name, _burdenedFaction.Name)));

            factionWarNegotiationsOutcome = TriggerOutcome();
            return factionWarNegotiationsOutcome;
        }

        private string Sabotage(float badOutcomeWeightFactor, float goodOutcomeWeightFactor)
        {
            outComes = new List<(Action, float, string)>();

            string factionWarNegotiationsOutcome;
            outComes.Add((
                () =>
                {
                    HandleOutcome(MFI_DiplomacyTunings.SabotageDisaster);
                    Outcome_TalksSabotageDisaster(_favouredFaction, _burdenedFaction, _pawn, _incidentTarget);
                },
                BaseWeight_Disaster * badOutcomeWeightFactor,
                "MFI_FactionWarSabotageDisaster".Translate(_favouredFaction.Name, _burdenedFaction.Name)));

            outComes.Add((
                () => HandleOutcome(MFI_DiplomacyTunings.SabotageBackfire),
                BaseWeight_Backfire * badOutcomeWeightFactor,
                "MFI_FactionWarSabotageBackFire".Translate(_favouredFaction.Name, _burdenedFaction.Name)));

            outComes.Add((
                () => HandleOutcome(MFI_DiplomacyTunings.SabotageFlounder),
                BaseWeight_TalksFlounder,
                "MFI_FactionWarSabotageFlounder".Translate(_favouredFaction.Name, _burdenedFaction.Name)));

            outComes.Add((
                () => HandleOutcome(MFI_DiplomacyTunings.SabotageSuccess),
                BaseWeight_Success * goodOutcomeWeightFactor,
                "MFI_FactionWarSabotageSuccess".Translate(_favouredFaction.Name, _burdenedFaction.Name)));

            outComes.Add((
                () => HandleOutcome(MFI_DiplomacyTunings.SabotageTriumph),
                BaseWeight_Triumph * goodOutcomeWeightFactor,
                "MFI_FactionWarSabotageTriumph".Translate(_favouredFaction.Name, _burdenedFaction.Name)));

            factionWarNegotiationsOutcome = TriggerOutcome();
            return factionWarNegotiationsOutcome;
        }

        private string BrokerPeace(float badOutcomeWeightFactor, float goodOutcomeWeightFactor)
        {
            outComes = new List<(Action, float, string)>();

            string factionWarNegotiationsOutcome;
            outComes.Add((
                () => HandleOutcome(MFI_DiplomacyTunings.BrokerPeaceDisaster),
                BaseWeight_Disaster * badOutcomeWeightFactor,
                "MFI_FactionWarBrokerPeaceDisaster".Translate(_favouredFaction.Name, _burdenedFaction.Name)));

            outComes.Add((
                () => HandleOutcome(MFI_DiplomacyTunings.BrokerPeaceBackfire),
                BaseWeight_Backfire * badOutcomeWeightFactor,
                "MFI_FactionWarBrokerPeaceBackFire".Translate(_favouredFaction.Name, _burdenedFaction.Name)));

            outComes.Add((
                () => HandleOutcome(MFI_DiplomacyTunings.BrokerPeaceFlounder),
                BaseWeight_TalksFlounder,
                "MFI_FactionWarBrokerPeaceFlounder".Translate(_favouredFaction.Name, _burdenedFaction.Name)));

            outComes.Add((
                () => HandleOutcome(MFI_DiplomacyTunings.BrokerPeaceSuccess),
                BaseWeight_Success * goodOutcomeWeightFactor,
                "MFI_FactionWarBrokerPeaceSuccess".Translate(_favouredFaction.Name, _burdenedFaction.Name)));

            outComes.Add((
                () => HandleOutcome(MFI_DiplomacyTunings.BrokerPeaceTriumph),
                BaseWeight_Triumph * goodOutcomeWeightFactor,
                "MFI_FactionWarBrokerPeaceTriumph".Translate(_favouredFaction.Name, _burdenedFaction.Name)));

            factionWarNegotiationsOutcome = TriggerOutcome();
            return factionWarNegotiationsOutcome;
        }

        private string TriggerOutcome()
        {
            _pawn.skills.Learn(sDef: SkillDefOf.Social, xp: 6000f, direct: true);

            var (chosenOutcome, weight, flavor) = outComes.RandomElementByWeight(x => x.weight);
            chosenOutcome();
            return flavor;
        }

        private void SwapFavouredFaction()
        {
            var temp = _favouredFaction;
            _favouredFaction = _burdenedFaction;
            _burdenedFaction = temp;
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

        private void HandleOutcome(Outcome result)
        {
            _favouredFaction.TryAffectGoodwillWith(_pawn.Faction, result.goodwillChangeFavouredFaction);
            _burdenedFaction.TryAffectGoodwillWith(_pawn.Faction, result.goodwillChangeBurdenedFaction);

            if (result.setHostile)
            {
                _burdenedFaction.TrySetRelationKind(_pawn.Faction, FactionRelationKind.Hostile, canSendLetter: false, reason: null, lookTarget: null);
            }

            if (result.startWar)
            {
                Find.World.GetComponent<WorldComponent_MFI_FactionWar>().StartWar(factionOne: _favouredFaction, factionInstigator: _burdenedFaction, selfResolved: _favouredFaction.leader == _pawn);
            }
        }

        private static void Outcome_TalksSabotageDisaster(Faction favouredFaction, Faction burdenedFaction, Pawn pawn, IIncidentTarget incidentTarget)
        {
            favouredFaction.TrySetRelationKind(other: pawn.Faction, kind: FactionRelationKind.Hostile, canSendLetter: false, reason: null, lookTarget: null);
            LongEventHandler.QueueLongEvent(action: delegate
            {

                IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(incCat: IncidentCategoryDefOf.ThreatBig, target: incidentTarget);
                incidentParms.faction = favouredFaction;
                PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(groupKind: PawnGroupKindDefOf.Combat, parms: incidentParms, ensureCanGenerateAtLeastOnePawn: true);
                defaultPawnGroupMakerParms.generateFightersOnly = true;
                List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(parms: defaultPawnGroupMakerParms).ToList();

                IncidentParms burdenedFactionIncidentParms = incidentParms;
                burdenedFactionIncidentParms.faction = burdenedFaction;
                PawnGroupMakerParms burdenedFactionPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(groupKind: PawnGroupKindDefOf.Combat, parms: incidentParms, ensureCanGenerateAtLeastOnePawn: true);
                burdenedFactionPawnGroupMakerParms.generateFightersOnly = true;
                List<Pawn> burdenedFactionWarriors = PawnGroupMakerUtility.GeneratePawns(parms: burdenedFactionPawnGroupMakerParms).ToList();

                List<Pawn> combinedList = new List<Pawn>();
                combinedList.AddRange(collection: list);
                combinedList.AddRange(collection: burdenedFactionWarriors);

                Map map = CaravanIncidentUtility.SetupCaravanAttackMap(caravan: incidentTarget as Caravan, enemies: combinedList, sendLetterIfRelatedPawns: true);

                if (list.Any())
                {
                    LordMaker.MakeNewLord(faction: incidentParms.faction, lordJob: new LordJob_AssaultColony(assaulterFaction: favouredFaction), map: map, startingPawns: list);
                }

                if (burdenedFactionWarriors.Any())
                {
                    LordMaker.MakeNewLord(faction: burdenedFactionIncidentParms.faction, lordJob: new LordJob_AssaultColony(assaulterFaction: burdenedFaction), map: map, startingPawns: burdenedFactionWarriors);
                }

                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();

            }, textKey: "GeneratingMapForNewEncounter", doAsynchronously: false, exceptionHandler: null);
        }

        private static float GetBadOutcomeWeightFactor(Pawn _pawn) => 
            MFI_DiplomacyTunings.BadOutcomeFactorAtStatPower.Evaluate(_pawn.GetStatValue(stat: StatDefOf.NegotiationAbility));
    }

    public struct Outcome
    {
        public bool startWar;
        public bool setHostile;
        public int goodwillChangeFavouredFaction;
        public int goodwillChangeBurdenedFaction;
    }
}
