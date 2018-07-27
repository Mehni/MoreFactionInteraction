using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RimWorld.Planet;
using MoreFactionInteraction.More_Flavour;

namespace MoreFactionInteraction
{
    public class ChoiceLetter_MysticalShaman : ChoiceLetter
    {
        public int tile;
        public Faction faction;
        public Map map;
        public int fee;
        private static readonly IntRange TimeoutDaysRange = new IntRange(min: 5, max: 15);

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
                            MysticalShaman mysticalShaman = (MysticalShaman)WorldObjectMaker.MakeWorldObject(def: MFI_DefOf.MFI_MysticalShaman);
                            mysticalShaman.Tile = tile;
                            mysticalShaman.SetFaction(newFaction: faction);
                            int randomInRange = TimeoutDaysRange.RandomInRange;
                            mysticalShaman.GetComponent<TimeoutComp>().StartTimeout(ticks: randomInRange * GenDate.TicksPerDay);
                            Find.WorldObjects.Add(o: mysticalShaman);

                            TradeUtility.LaunchSilver(map: this.map, fee: this.fee);
                            Find.LetterStack.RemoveLetter(let: this);
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
                            Find.LetterStack.RemoveLetter(let: this);
                        },
                        resolveTree = true
                    };
                    yield return reject;
                    yield return this.Option_Postpone;
                }
            }
        }

        public override bool CanShowInLetterStack => base.CanShowInLetterStack && Find.Maps.Contains(item: this.map);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Map>(refee: ref this.map, label: "MFI_Shaman_Map");
            Scribe_References.Look<Faction>(refee: ref this.faction, label: "MFI_Shaman_Faction");
            Scribe_Values.Look(ref this.tile, "MFI_ShamanTile");
            Scribe_Values.Look<int>(value: ref this.fee, label: "MFI_ShamanFee");
        }
    }
}
