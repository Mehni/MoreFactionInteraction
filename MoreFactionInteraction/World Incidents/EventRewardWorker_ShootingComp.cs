using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction
{
    public class EventRewardWorker_ShootingComp : EventRewardWorker
    {
        public override Predicate<ThingDef> ValidatorFirstPlace => (ThingDef x) => base.ValidatorFirstPlace(x)
                                                    && x.techLevel >= TechLevel.Industrial 
                                                    && x.equipmentType == EquipmentType.Primary 
                                                    && x.GetStatValueAbstract(StatDefOf.MarketValue, GenStuff.DefaultStuffFor(x)) >= 100f;

        public override Predicate<ThingDef> ValidatorFirstLoser => (ThingDef x) => base.ValidatorFirstLoser(x)
                                                    && x.techLevel >= TechLevel.Spacer; //*bionics*, not wooden feet tyvm.

        public override Predicate<ThingDef> ValidatorFirstOther => (ThingDef x) => base.ValidatorFirstOther(x)
                                                    && x == ThingDefOf.RawPotatoes; //how nice, a representation of your shooting skills.
    }
}
