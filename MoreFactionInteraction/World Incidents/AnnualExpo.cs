using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace MoreFactionInteraction.More_Flavour
{
    public class AnnualExpo : WorldObject
    {
        public EventDef eventDef;
        public Faction host;

        public void Notify_CaravanArrived(Caravan caravan)
        {
            Pawn pawn = BestCaravanPawnUtility.FindPawnWithBestStat(caravan, eventDef.relevantStat);
            if (pawn == null)
            {
                Messages.Message(text: "MFI_AnnualExpoMessageNoRepresentative".Translate(), lookTargets: caravan, def: MessageTypeDefOf.NegativeEvent);
            }
            else
            {
                CameraJumper.TryJumpAndSelect(target: caravan);
                Find.WindowStack.Add(window: new Dialog_NodeTree(new AnnualExpoDialogue(pawn, caravan, eventDef, host).AnnualExpoDialogueNode()));
                Find.WorldObjects.Remove(this);
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            foreach (FloatMenuOption o in base.GetFloatMenuOptions(caravan: caravan))
                yield return o;
            foreach (FloatMenuOption f in CaravanArrivalAction_VisitAnnualExpo.GetFloatMenuOptions(caravan: caravan, annualExpo: this))
                yield return f;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref this.eventDef, "MFI_EventDef");
            Scribe_References.Look(ref this.host, "MFI_ExpoHost");
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.Append(eventDef.LabelCap);
            return stringBuilder.ToString();
        }
    }
}
