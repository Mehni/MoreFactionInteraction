using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;


namespace MoreFactionInteraction
{
    public class MoreFactionInteraction_Settings : ModSettings
    {
        public static int ticksToUpgrade = 3 * GenDate.DaysPerQuadrum * GenDate.TicksPerDay;
        public static float timeModifierBetweenFactionInteraction = 1f;
        public static float traderWealthOffsetFromTimesTraded = 1f;

        public void DoWindowContents(Rect rect)
        {
            Listing_Standard options = new Listing_Standard();
            options.Begin(rect);
            options.Gap();
            options.SliderLabeled("MFI.ticksToUpgrade".Translate(), ref ticksToUpgrade, ticksToUpgrade.ToStringTicksToPeriodVague(false, true), 0, GenDate.TicksPerYear);
            options.GapLine();
            options.SliderLabeled("MFI_timeModifierBetweenFactionInteraction".Translate(), ref timeModifierBetweenFactionInteraction, timeModifierBetweenFactionInteraction.ToStringByStyle(ToStringStyle.FloatOne), 0.5f, 3f);
            options.Gap();
            options.SliderLabeled("MFI_traderWealthOffsetFromTimesTraded".Translate(), ref traderWealthOffsetFromTimesTraded, traderWealthOffsetFromTimesTraded.ToStringByStyle(ToStringStyle.FloatOne), 0.5f, 3f);
            options.End();
            Mod.GetSettings<MoreFactionInteraction_Settings>().Write();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref ticksToUpgrade, "ticksToUpgrade", 2700000);
            Scribe_Values.Look(ref timeModifierBetweenFactionInteraction, "timeModifierBetweenFactionInteraction", 1f);
            Scribe_Values.Look(ref traderWealthOffsetFromTimesTraded, "traderWealthOffsetFromTimesTraded", 1f);
        }
    }


    public class MoreFactionInteractionMod : Mod
    {
        public MoreFactionInteractionMod(ModContentPack content) : base(content)
        {
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            GetSettings<MoreFactionInteraction_Settings>().DoWindowContents(inRect);
        }

        public override string SettingsCategory() => "More Faction Interaction";

        public override void WriteSettings()
        {
            base.WriteSettings();
        }
    }
}
