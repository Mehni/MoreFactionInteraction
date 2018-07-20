using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction
{

    //thanks to AlexTD for the below
    internal static class SettingsHelper
    {
        //private static float gap = 12f;

        public static void SliderLabeled(this Listing_Standard ls, string label, ref int val, string format, float min = 0f, float max = 100f, string tooltip = null)
        {
            float fVal = val;
            ls.SliderLabeled(label: label, val: ref fVal, format: format, min: min, max: max);
            val = (int)fVal;
        }
        public static void SliderLabeled(this Listing_Standard ls, string label, ref float val, string format, float min = 0f, float max = 1f, string tooltip = null)
        {
            Rect rect = ls.GetRect(height: Text.LineHeight);
            Rect rect2 = rect.LeftPart(pct: .70f).Rounded();
            Rect rect3 = rect.RightPart(pct: .30f).Rounded().LeftPart(pct: .67f).Rounded();
            Rect rect4 = rect.RightPart(pct: .10f).Rounded();

            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect: rect2, label: label);

            float result = Widgets.HorizontalSlider(rect: rect3, value: val, leftValue: min, rightValue: max, middleAlignment: true);
            val = result;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(rect: rect4, label: String.Format(format: format, arg0: val));
            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect: rect, tip: tooltip);
            }

            Text.Anchor = anchor;
            ls.Gap(gapHeight: ls.verticalSpacing);
        }

        public static void FloatRange(this Listing_Standard ls, string label, ref FloatRange range, float min = 0f, float max = 1f, string tooltip = null, ToStringStyle valueStyle = ToStringStyle.FloatTwo)
        {
            Rect rect = ls.GetRect(height: Text.LineHeight);
            Rect rect2 = rect.LeftPart(pct: .70f).Rounded();
            Rect rect3 = rect.RightPart(pct: .30f).Rounded().LeftPart(pct: .9f).Rounded();
            rect3.yMin -= 5f;
            //Rect rect4 = rect.RightPart(.10f).Rounded();

            TextAnchor anchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect: rect2, label: label);

            Text.Anchor = TextAnchor.MiddleRight;
            int id = ls.CurHeight.GetHashCode();
            Widgets.FloatRange(rect: rect3, id: id, range: ref range, min: min, max: max, labelKey: null, valueStyle: valueStyle);
            if (!tooltip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect: rect, tip: tooltip);
            }
            Text.Anchor = anchor;
            ls.Gap(gapHeight: ls.verticalSpacing);
        }


        public static Rect GetRect(this Listing_Standard listing_Standard, float? height = null)
        {
            return listing_Standard.GetRect(height: height ?? Text.LineHeight);
        }

        //thanks to Why_is_that for the below
        public static void AddLabeledRadioList(this Listing_Standard listing_Standard, string header, string[] labels, ref string val, float? headerHeight = null)
        {
            //listing_Standard.Gap();
            if (header != string.Empty) { Widgets.Label(rect: listing_Standard.GetRect(height: headerHeight), label: header); }
            listing_Standard.AddRadioList<string>(GenerateLabeledRadioValues(labels: labels), ref val);
        }

        //public static void AddLabeledRadioList<T>(this Listing_Standard listing_Standard, string header, Dictionary<string, T> dict, ref T val, float? headerHeight = null)
        //{
        //    listing_Standard.Gap();
        //    if (header != string.Empty) { Widgets.Label(listing_Standard.GetRect(headerHeight), header); }
        //    listing_Standard.AddRadioList<T>(GenerateLabeledRadioValues<T>(dict), ref val);
        //}

        private static void AddRadioList<T>(this Listing_Standard listing_Standard, List<LabeledRadioValue<T>> items, ref T val, float? height = null)
        {
            foreach (LabeledRadioValue<T> item in items)
            {
                //listing_Standard.Gap();
                Rect lineRect = listing_Standard.GetRect(height: height);
                if (Widgets.RadioButtonLabeled(rect: lineRect, labelText: item.Label, chosen: EqualityComparer<T>.Default.Equals(x: item.Value, y: val)))
                    val = item.Value;
            }
        }

        private static List<LabeledRadioValue<string>> GenerateLabeledRadioValues(string[] labels)
        {
            List<LabeledRadioValue<string>> list = new List<LabeledRadioValue<string>>();
            foreach (string label in labels)
            {
                list.Add(item: new LabeledRadioValue<string>(label: label, val: label));
            }
            return list;
        }

        //// (label, value) => (key, value)
        //private static List<LabeledRadioValue<T>> GenerateLabeledRadioValues<T>(Dictionary<string, T> dict)
        //{
        //    List<LabeledRadioValue<T>> list = new List<LabeledRadioValue<T>>();
        //    foreach (KeyValuePair<string, T> entry in dict)
        //    {
        //        list.Add(new LabeledRadioValue<T>(entry.Key, entry.Value));
        //    }
        //    return list;
        //}

        public class LabeledRadioValue<T>
        {
            private string label;
            private T val;

            public LabeledRadioValue(string label, T val)
            {
                this.Label = label;
                this.Value = val;
            }

            public string Label
            {
                get => this.label;
                set => this.label = value;
            }

            public T Value
            {
                get => this.val;
                set => this.val = value;
            }

        }
    }
}
