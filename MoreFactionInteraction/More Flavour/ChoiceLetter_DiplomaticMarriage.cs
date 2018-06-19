using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace MoreFactionInteraction
{
    public class ChoiceLetter_DiplomaticMarriage : ChoiceLetter
    {
        public Pawn betrothed;
        public Map map;
        public Faction faction;

        public override bool CanShowInLetterStack
        {
            get
            {
                Log.Message("can show");
                return base.CanShowInLetterStack && PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Contains(this.betrothed);
            }
        }

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (base.ArchivedOnly)
                {
                    yield return base.OK;
                }
                else
                {
                    DiaOption accept = new DiaOption("RansomDemand_Accept".Translate())
                    {
                        action = () =>
                        {
                            betrothed.Kill(null, null);
                            faction.TryAffectGoodwillWith(Faction.OfPlayer, 100, true, false, "They're getting it on now!");
                        }
                    };

                    DiaNode dialogueNodeAccept = new DiaNode("MFI_TraderSent".Translate().CapitalizeFirst());
                            dialogueNodeAccept.options.Add(base.OK);
                            accept.link = dialogueNodeAccept;

                    DiaOption reject = new DiaOption("RansomDemand_Reject".Translate())
                    {
                        action = () =>
                        {
                            Log.Message("rejected, boohoo");
                            IncidentParms parms = new IncidentParms
                            {
                                target = map,
                                faction = faction,
                                points = 10000f
                            };
                            IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
                        }
                    };
                    DiaNode dialogueNodeReject = new DiaNode("MFI_TraderSent".Translate().CapitalizeFirst());
                            dialogueNodeReject.options.Add(base.OK);
                            reject.link = dialogueNodeReject;

                    yield return accept;
                    yield return reject;
                    yield return base.Postpone;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Map>(ref this.map, "map");
            Scribe_References.Look<Pawn>(ref this.betrothed, "betrothed");
        }
    }
}
