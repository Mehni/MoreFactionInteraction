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

        public void DoWindowContents(Rect rect)
        {
            Listing_Standard options = new Listing_Standard();
            options.Begin(rect);
            options.Gap();
            options.SliderLabeled("MFI.ticksToUpgrade".Translate(), ref ticksToUpgrade, ticksToUpgrade.ToStringTicksToPeriodVagueMax(), 0, GenDate.TicksPerYear);
            options.End();

            Mod.GetSettings<MoreFactionInteraction_Settings>().Write();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref ticksToUpgrade, "ticksToUpgrade", 2700000);
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
