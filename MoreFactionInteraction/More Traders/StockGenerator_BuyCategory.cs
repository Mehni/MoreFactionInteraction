using RimWorld;
using Verse;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MoreFactionInteraction
{

	public class StockGenerator_BuyCategory : StockGenerator
	{
		public ThingCategoryDef thingCategoryDef;
	    private const float maxValuePerUnit = 1000f;

	    public override IEnumerable<Thing> GenerateThings(int forTile)
		{
            yield break;
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
            //TODO: Look into maxTechLevelBuy. From what I can tell, nothing uses it.
            //TODO: Balance maxValuePerUnit. 1k is nonsense since traders generally don't have much more than that, but then again I also want some limit. Currently ignores stuff, so golden helmets ahoy.
            return this.thingCategoryDef.DescendantThingDefs.Contains(thingDef) 
                && thingDef.tradeability != Tradeability.None 
                && thingDef.BaseMarketValue / thingDef.VolumePerUnit < maxValuePerUnit;
		}
	}
}