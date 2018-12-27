using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace MoreFactionInteraction
{
    public class EventDef : Def
    {
        public ThingSetMakerDef rewardFirstPlace;
        public ThingSetMakerDef rewardFirstLoser;
        public ThingSetMakerDef rewardFirstOther;

        public string theme;
        public string themeDesc;

        public StatDef relevantStat;

        public string outComeFirstPlace;
        public string outcomeFirstLoser;
        public string outComeFirstOther;

        public float xPGainFirstPlace = 4000f;
        public float xPGainFirstLoser = 2000f;
        public float xPGainFirstOther = 1000f;

        public List<SkillDef> learnedSkills;

        [Unsaved]
        private EventRewardWorker workerInt;

        public Type workerClass = typeof(EventRewardWorker);

        //dunno why vanilla caches it but there's prolly good reason for it.
        public EventRewardWorker Worker
        {
            get
            {
                if (this.workerInt == null)
                {
                    this.workerInt = (EventRewardWorker)Activator.CreateInstance(this.workerClass);
                }
                return this.workerInt;
            }
        }
    }
}
