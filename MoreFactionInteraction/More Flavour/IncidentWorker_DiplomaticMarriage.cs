namespace MoreFactionInteraction
{
    using System.Collections.Generic;
    using System.Linq;
    using General;
    using RimWorld;
    using RimWorld.Planet;
    using UnityEngine;
    using Verse;

    public class IncidentWorker_DiplomaticMarriage : IncidentWorker
    {
        private Pawn marriageSeeker;
        private Pawn betrothed;
        private Pawn marriageSeekerInternal;
        private Pawn betrothedInternal;
        private const int TimeoutTicks = GenDate.TicksPerDay;

        public override float AdjustedChance => base.AdjustedChance - StorytellerUtilityPopulation.PopulationIntent;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms: parms) && ((TryFindMarriageSeeker(marriageSeeker: out this.marriageSeeker) && this.TryFindBetrothed(betrothed: out this.betrothed))
                                                        || (TryFindMarriageSeekerForeverAlone(out this.marriageSeekerInternal) && TryFindBetrothedExternalBecauseNobodyInTheColonyWantsYou(out this.betrothedInternal)))
                                                    && !this.IsScenarioBlocked();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            //new: Determine gain or loss of pawn.
            //if gain: Find marriage seeker and bethroted preferably single or with outsider spouse
            //if gain: Send (different?) choice letter with dowry demand
            //is pure gain. Downsides? Unknown colonist, buying sight-unseen. Investment of X.
            //Market value of ~2k silver. Indicative of pawn value? Player says nooo on cheap pawns
            //give bionic? Interesting choice to organ harvest someone's wife; pawn value is skewed. Also inflated.
            //risk: too good of a pawn. Only implement if pawn value < 800 ? Keeps 'em guessing.
            //other: 10% chance of bionic.

            if (!TryFindMarriageSeeker(marriageSeeker: out this.marriageSeeker)
             || !TryFindMarriageSeekerForeverAlone(out this.marriageSeekerInternal))
            {
                if (Prefs.LogVerbose)
                    Log.Warning(text: "no marriageseeker");
                return false;
            }

            if ((!this.TryFindBetrothed(betrothed: out this.betrothed) && this.marriageSeeker == null) 
              || (!this.TryFindBetrothedExternalBecauseNobodyInTheColonyWantsYou(out this.betrothedInternal) && this.marriageSeekerInternal != null))
            {
                if (Prefs.LogVerbose)
                    Log.Warning(text: "no betrothed");
                return false;
            }

            if (this.marriageSeeker != null && Rand.Chance(0.01f))
            {
                ChoiceLetter_DiplomaticMarriage choiceLetterDiplomaticMarriage = (ChoiceLetter_DiplomaticMarriage)LetterMaker.MakeLetter(label: this.def.letterLabel, text: "MFI_DiplomaticMarriage".Translate(
                    this.marriageSeeker.LabelShort, this.betrothed.LabelShort, this.marriageSeeker.Faction.Name
                ).AdjustedFor(p: this.marriageSeeker), def: this.def.letterDef);

                choiceLetterDiplomaticMarriage.title = "MFI_DiplomaticMarriageLabel".Translate(this.betrothed.LabelShort).CapitalizeFirst();
                choiceLetterDiplomaticMarriage.radioMode = false;
                choiceLetterDiplomaticMarriage.marriageSeeker = this.marriageSeeker;
                choiceLetterDiplomaticMarriage.betrothed = this.betrothed;
                choiceLetterDiplomaticMarriage.StartTimeout(duration: TimeoutTicks);
                Find.LetterStack.ReceiveLetter(choiceLetterDiplomaticMarriage);
                Find.World.GetComponent<WorldComponent_OutpostGrower>().Registerletter(choiceLetterDiplomaticMarriage);

                this.ClearWeddingCandidates();
                return true;
            }

            Log.Message($"betrothed: {this.betrothedInternal}");

            if (!CommsConsoleUtility.PlayerHasPoweredCommsConsole())
                return false; //CommsConsole required because of launching dowry

            //skewing pawn value because I can.
            if (this.betrothedInternal.GetStatValue(StatDefOf.MarketValue) < 800 || Rand.Chance(0.1f))
                GenerateTechHediffsFor(this.betrothedInternal);

            List<ThingCount> dowry = this.GenerateSomeFairPriceFor(this.betrothedInternal);

            if (dowry.NullOrEmpty())
                return false;

            string text = "MFI_DiplomaticMarriageShotgunEdition".Translate(
                this.marriageSeekerInternal.LabelShort,
                this.betrothedInternal.Label,
                this.betrothedInternal.Faction.Name,
                this.betrothedInternal.KindLabel,
                GenLabel.ThingsLabel(dowry)
                ).AdjustedFor(p: this.marriageSeekerInternal);

            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, this.betrothedInternal);

            ChoiceLetter_DiplomaticMarriageShotgunEdition choiceLetterDiplomaticMarriageShotgun = (ChoiceLetter_DiplomaticMarriageShotgunEdition)LetterMaker.MakeLetter(label: this.def.letterLabel, text, def: DefDatabase<LetterDef>.GetNamed("DiplomaticMarriageShotgunEdition"));
            choiceLetterDiplomaticMarriageShotgun.title = "MFI_DiplomaticMarriageLabel".Translate(this.betrothed.LabelShort).CapitalizeFirst();
            choiceLetterDiplomaticMarriageShotgun.radioMode = false;
            choiceLetterDiplomaticMarriageShotgun.marriageSeekerInternal = this.marriageSeekerInternal;
            choiceLetterDiplomaticMarriageShotgun.betrothedInternal = this.betrothedInternal;
            choiceLetterDiplomaticMarriageShotgun.dowry = dowry;
            choiceLetterDiplomaticMarriageShotgun.StartTimeout(duration: TimeoutTicks / 2);
            Find.LetterStack.ReceiveLetter(choiceLetterDiplomaticMarriageShotgun);

            this.ClearWeddingCandidates();
            return true;
        }

        //eerily similar to IncidentWorker_CaravanDemand. I assure you that is totally coincidental.
        private List<ThingCount> GenerateSomeFairPriceFor(Pawn pawn)
        {
            //simplified version of TradeUtility.GetPricePlayerBuy
            float fairValue = pawn.MarketValue * 1.4f * (1f + Find.Storyteller.difficulty.tradePriceFactorLoss);

            Map map = TradeUtility.PlayerHomeMapWithMostLaunchableSilver();

            if (map == null)
                return null;

            List<ThingCount> generateItemsDemand = this.TryGenerateItemsAndAnimalsDemand(map, fairValue);
            if (!generateItemsDemand.NullOrEmpty())
            {
                return generateItemsDemand;
            }
            return null;
        }

        private List<ThingCount> TryGenerateItemsAndAnimalsDemand(Map map, float fairValue)
        {
            List<ThingCount> totallyReasonableDowry = new List<ThingCount>();
            List<Thing> listOfItems = new List<Thing>();
            listOfItems.AddRange(TradeUtility.AllLaunchableThingsForTrade(map));
            listOfItems.RemoveAll(x => x.MarketValue * x.stackCount < 50f);

            List<Pawn> listOfPawns = new List<Pawn>();
            listOfPawns.AddRange(map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer).Where(x => x.RaceProps.Animal && x.HostFaction != null && !x.InMentalState && !x.Downed && map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(x.def)));

            float num = listOfItems.Sum(x => x.MarketValue * x.stackCount);
            float requestedDowry = Mathf.Clamp(fairValue, 300f, 3500f);
            while (requestedDowry > 50f)
            {
                if (Rand.Chance(0.66f))
                {
                    if (listOfPawns.TryRandomElementByWeight(x => x.MarketValue, out Pawn pawn))
                    {
                        requestedDowry -= pawn.MarketValue * 1.2f; //moocows are worth BILLIONS
                        totallyReasonableDowry.Add(new ThingCount(pawn, 1)); //moocows don't stack
                        listOfPawns.Remove(pawn);
                    }
                }

                float dowry = requestedDowry;
                if (!(from x in listOfItems
                      where x.MarketValue * x.stackCount <= dowry * 2f
                      select x).TryRandomElementByWeight(x => Mathf.Pow(x.MarketValue / x.GetStatValue(StatDefOf.Mass), 2f), out Thing thing) && listOfPawns.Count == 0)
                {
                    return null;
                }
                int num2 = Mathf.Clamp((int)(requestedDowry / thing.MarketValue), 1, thing.stackCount);
                requestedDowry -= thing.MarketValue * num2;
                totallyReasonableDowry.Add(new ThingCount(thing, num2));
                listOfItems.Remove(thing);
            }
            return totallyReasonableDowry;
        }

        private void ClearWeddingCandidates()
        {
            this.marriageSeeker = null;
            this.marriageSeekerInternal = null;
            this.betrothed = null;
            this.betrothedInternal = null;
        }

        private bool TryFindBetrothed(out Pawn betrothed) => (from potentialPartners in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep
                                                              where !LovePartnerRelationUtility.HasAnyLovePartner(potentialPartners) || LovePartnerRelationUtility.ExistingMostLikedLovePartner(potentialPartners, false) == this.marriageSeeker
                                                              select potentialPartners).TryRandomElementByWeight(weightSelector: marriageSeeker2 => this.marriageSeeker.relations.SecondaryLovinChanceFactor(otherPawn: marriageSeeker2), result: out betrothed);

        private static bool TryFindMarriageSeeker(out Pawn marriageSeeker) => Find.WorldPawns.AllPawnsAlive.Where(IsSuitableCandidateForMarriage)
                                                                              .TryRandomElement(result: out marriageSeeker); //todo: make more likely to select hostile.

        private static bool IsSuitableCandidateForMarriage(Pawn x) => x.Faction != null && !x.Faction.def.hidden && !x.Faction.def.permanentEnemy && !x.Faction.IsPlayer
                                                                   && x.Faction.PlayerGoodwill <= 50 && !x.Faction.defeated && x.Faction.def.techLevel <= TechLevel.Medieval
                                                                   && x.Faction.leader != null && !x.Faction.leader.IsPrisoner && !x.Faction.leader.Spawned && x != x.Faction.leader
                                                                   && !x.IsPrisoner && !x.Spawned && x.relations != null && x.RaceProps.Humanlike
                                                                   && !SettlementUtility.IsPlayerAttackingAnySettlementOf(faction: x.Faction) && !PeaceTalksExist(faction: x.Faction)
                                                                   && (!LovePartnerRelationUtility.HasAnyLovePartner(pawn: x) || LovePartnerRelationUtility.ExistingMostLikedLovePartner(p: x, allowDead: false)?.Faction == Faction.OfPlayer);

        //find colonist in desperate need of a partner.
        private bool TryFindMarriageSeekerForeverAlone(out Pawn marriageSeekerInternal) =>
            PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep
                .Where(x => !LovePartnerRelationUtility.HasAnyLovePartner(pawn: x)
                          || LovePartnerRelationUtility.ExistingMostLikedLovePartner(p: x, allowDead: false)?.Faction != Faction.OfPlayer)
                    .TryRandomElementByWeight(x => x.records.GetValue(RecordDefOf.TimeAsColonistOrColonyAnimal), out marriageSeekerInternal);

        private bool TryFindBetrothedExternalBecauseNobodyInTheColonyWantsYou(out Pawn betrothedInternal)
        {
            if (this.marriageSeekerInternal?.GetSpouse() != null)
            {
                betrothedInternal = this.marriageSeekerInternal.GetSpouse();
                return true;
            }

            return Find.WorldPawns.AllPawnsAlive.Where(IsSuitableCandidateForMarriage).TryRandomElement(out betrothedInternal);
        }

        private static bool PeaceTalksExist(Faction faction)
        {
            List<PeaceTalks> peaceTalks = Find.WorldObjects.PeaceTalks;
            foreach (PeaceTalks peaceTalk in peaceTalks)
            {
                if (peaceTalk.Faction == faction)
                    return true;
            }
            return false;
        }

        private static void GenerateTechHediffsFor(Pawn pawn)
        {
            float partsMoney = new FloatRange(500, 2800).RandomInRange;

            IEnumerable<ThingDef> source = from x in DefDatabase<ThingDef>.AllDefs
                                           where x.isTechHediff && x.BaseMarketValue <= partsMoney
                                           select x;

            IEnumerable<ThingDef> thingDefs = source as ThingDef[] ?? source.ToArray();

            if (!thingDefs.Any())
                return;

            ThingDef partDef = thingDefs.RandomElementByWeight(w => w.BaseMarketValue);
            IEnumerable<RecipeDef> source2 = from x in DefDatabase<RecipeDef>.AllDefs
                                             where x.IsIngredient(partDef) && pawn.def.AllRecipes.Contains(x)
                                             select x;

            IEnumerable<RecipeDef> recipeDefs = source2 as RecipeDef[] ?? source2.ToArray();

            if (!recipeDefs.Any())
                return;

            RecipeDef recipeDef = recipeDefs.RandomElement();

            if (recipeDef.Worker.GetPartsToApplyOn(pawn, recipeDef).Any())
            {
                recipeDef.Worker.ApplyOnPawn(pawn, recipeDef.Worker.GetPartsToApplyOn(pawn, recipeDef).RandomElement(), null, new List<Thing>(), null);
            }
        }
    }
}
