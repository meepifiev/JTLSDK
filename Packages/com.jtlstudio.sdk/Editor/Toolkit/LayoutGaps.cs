using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace JTLStudio.SDK.Editor.Toolkit
{
    public class LayoutGaps
    {
        private const string VerticalPrefix = "jtl-vstack-";
        private const string HorizontalPrefix = "jtl-hstack-";

        public void Apply(VisualElement root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            List<VisualElement> elements = root.Query<VisualElement>().ToList();

            foreach (VisualElement element in elements)
            {
                ApplyTo(element);
            }
        }

        private void ApplyTo(VisualElement element)
        {
            foreach (string className in element.GetClasses())
            {
                if (className.StartsWith(VerticalPrefix))
                {
                    ApplyGap(element, ParseGap(className, VerticalPrefix), true);
                }
                else if (className.StartsWith(HorizontalPrefix))
                {
                    ApplyGap(element, ParseGap(className, HorizontalPrefix), false);
                }
            }
        }

        private int ParseGap(string className, string prefix)
        {
            return int.TryParse(className.Substring(prefix.Length), out int gap) ? gap : 0;
        }

        private void ApplyGap(VisualElement element, int gap, bool vertical)
        {
            int count = element.hierarchy.childCount;

            for (int index = 0; index < count; index++)
            {
                VisualElement child = element.hierarchy[index];
                bool last = index == count - 1;
                float value = last ? 0f : gap;

                if (vertical)
                {
                    child.style.marginBottom = value;
                }
                else
                {
                    child.style.marginRight = value;
                }
            }
        }
    }
}
