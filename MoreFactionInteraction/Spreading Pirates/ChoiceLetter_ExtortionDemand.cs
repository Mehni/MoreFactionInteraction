using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                if (base.ArchivedOnly)
                {
                    yield return base.Option_Close;
                }
                else
                {
                    DiaOption accept = new DiaOption("RansomDemand_Accept".Translate())
                    {
                        action = () =>
                        {
                            TradeUtility.LaunchSilver(this.map, this.fee);
                            Find.LetterStack.RemoveLetter(this);
                        },
                        resolveTree = true
                    };
                    if (!TradeUtility.ColonyHasEnoughSilver(this.map, this.fee))
                    {
                        accept.Disable("NeedSilverLaunchable".Translate(new object[]
                        {
                            this.fee.ToString()
                        }));
                    }
                    yield return accept;

                    DiaOption reject = new DiaOption("RansomDemand_Reject".Translate())
                    {
                        action = () =>
                        {
                            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
                            incidentParms.forced = true;
                            incidentParms.faction = this.faction;
                            incidentParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                            incidentParms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
                            incidentParms.target = this.map;
                            if (outpost) incidentParms.points *= 0.7f;
                            IncidentDefOf.RaidEnemy.Worker.TryExecute(incidentParms);
                            Find.LetterStack.RemoveLetter(this);
                        },
                        resolveTree = true
                    };
                    yield return reject;
                    yield return base.Option_Postpone;
                }
            }
        }

        public override bool CanShowInLetterStack => base.CanShowInLetterStack && Find.Maps.Contains(this.map);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Map>(ref this.map, "map", false);
            Scribe_References.Look<Faction>(ref this.faction, "faction", false);
            Scribe_Values.Look<int>(ref this.fee, "fee", 0, false);
        }
    }
}
