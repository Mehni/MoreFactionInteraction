using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace MoreFactionInteraction.More_Flavour
{
    public static class AnnualExpoDialogue
    {
        public static DiaNode AnnualExpoDialogueNode(Pawn pawn, Caravan caravan, string eventTheme)
        {
            Tale tale = null;
            TaleReference taleRef = new TaleReference(tale);
            string flavourText = taleRef.GenerateText(TextGenerationPurpose.ArtDescription, RulePackDefOf.ArtDescriptionRoot_Taleless);

            DiaNode dialogueGreeting = new DiaNode(text: "MFI_AnnualExpoDialogueIntroduction".Translate( eventTheme.Translate(), flavourText ));

            foreach (DiaOption option in DialogueOptions(pawn: pawn, caravan, eventTheme))
            {
                dialogueGreeting.options.Add(item: option);
            }
            if (Prefs.DevMode)
            {
                dialogueGreeting.options.Add(item: new DiaOption(text: "(Dev: Get random buff)")
                {
                    action = () => Find.World.GetComponent<WorldComponent_MFI_AnnualExpo>().ApplyRandomBuff(x => !x.Active && x.MinTechLevel() >= TechLevel.Undefined),
                    linkLateBind = () => DialogueResolver(textResult: "It is done. Sorry about the lack of fancy flavour text for this dev mode only option.")
                });
            }
            return dialogueGreeting;
        }

        private static IEnumerable<DiaOption> DialogueOptions(Pawn pawn, Caravan caravan, string eventTheme)
        {
            yield return new DiaOption(text: "MFI_AnnualExpoFirstOption".Translate())
            {
                action = () => Log.Message("hi"),
                linkLateBind = () => DialogueResolver(textResult: "Goodbye."),
            };
        }

        private static DiaNode DialogueResolver(string textResult)
        {
            DiaNode resolver = new DiaNode(text: textResult);
            DiaOption diaOption = new DiaOption(text: "OK".Translate())
            {
                resolveTree = true
            };
            resolver.options.Add(item: diaOption);
            return resolver;
        }
    }
}
