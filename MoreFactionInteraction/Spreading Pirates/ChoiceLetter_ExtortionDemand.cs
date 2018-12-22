using System.Collections.Generic;
using Verse;
using RimWorld;

namespace MoreFactionInteraction
{
    public class ChoiceLetter_ExtortionDemand : ChoiceLetter
    {
        public Map map;
        public Faction faction;
        public bool outpost = false;
        public int fee;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (this.ArchivedOnly)
                {
                    yield return this.Option_Close;
                }
                else
                {
                    DiaOption accept = new DiaOption(text: "RansomDemand_Accept".Translate())
                    {
                        action = () =>
                        {
                            TradeUtility.LaunchSilver(map: this.map, fee: this.fee);
                            Find.LetterStack.RemoveLetter(@let: this);
                        },
                        resolveTree = true
                    };
                    if (!TradeUtility.ColonyHasEnoughSilver(map: this.map, fee: this.fee))
                    {
                        accept.Disable(newDisabledReason: "NeedSilverLaunchable".Translate(this.fee.ToString()));
                    }
                    yield return accept;

                    DiaOption reject = new DiaOption(text: "RansomDemand_Reject".Translate())
                    {
                        action = () =>
                        {
                            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(incCat: IncidentCategoryDefOf.ThreatBig, target: this.map);
                            incidentParms.forced = true;
                            incidentParms.faction = this.faction;
                            incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                            incidentParms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
                            incidentParms.target = this.map;
                            if (this.outpost)
                                incidentParms.points *= 0.7f;
                            IncidentDefOf.RaidEnemy.Worker.TryExecute(parms: incidentParms);
                            Find.LetterStack.RemoveLetter(@let: this);
                        },
                        resolveTree = true
                    };
                    yield return reject;
                    yield return this.Option_Postpone;
                }
            }
        }

        public override bool CanShowInLetterStack => Find.Maps.Contains(item: this.map);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Map>(refee: ref this.map, label: "map");
            Scribe_References.Look<Faction>(refee: ref this.faction, label: "faction");
            Scribe_Values.Look<int>(value: ref this.fee, label: "fee");
        }
    }
}
