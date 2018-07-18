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
            options.Begin(rect: rect);
            options.Gap();
            options.SliderLabeled(label: "MFI_ticksToUpgrade".Translate(), val: ref ticksToUpgrade, format: ticksToUpgrade.ToStringTicksToPeriodVague(vagueMin: false), min: 0, max: GenDate.TicksPerYear);
            options.GapLine();
            options.SliderLabeled(label: "MFI_timeModifierBetweenFactionInteraction".Translate(), val: ref timeModifierBetweenFactionInteraction, format: timeModifierBetweenFactionInteraction.ToStringByStyle(style: ToStringStyle.FloatOne), min: 0.5f, max: 3f);
            options.Gap();
            options.SliderLabeled(label: "MFI_traderWealthOffsetFromTimesTraded".Translate(), val: ref traderWealthOffsetFromTimesTraded, format: traderWealthOffsetFromTimesTraded.ToStringByStyle(style: ToStringStyle.FloatOne), min: 0.5f, max: 3f);
            options.End();
            this.Mod.GetSettings<MoreFactionInteraction_Settings>().Write();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(value: ref ticksToUpgrade, label: "ticksToUpgrade", defaultValue: 2700000);
            Scribe_Values.Look(value: ref timeModifierBetweenFactionInteraction, label: "timeModifierBetweenFactionInteraction", defaultValue: 1f);
            Scribe_Values.Look(value: ref traderWealthOffsetFromTimesTraded, label: "traderWealthOffsetFromTimesTraded", defaultValue: 1f);
        }
    }


    public class MoreFactionInteractionMod : Mod
    {
        public MoreFactionInteractionMod(ModContentPack content) : base(content: content)
        {
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect: inRect);
            this.GetSettings<MoreFactionInteraction_Settings>().DoWindowContents(rect: inRect);
        }

        public override string SettingsCategory() => "More Faction Interaction";

        public override void WriteSettings()
        {
            base.WriteSettings();
        }
    }
}
