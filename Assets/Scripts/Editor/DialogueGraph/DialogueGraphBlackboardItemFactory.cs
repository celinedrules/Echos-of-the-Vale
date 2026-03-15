using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.DialogueGraph
{
    public static class DialogueGraphBlackboardItemFactory
    {
        private static readonly Color EntryBackgroundColor = new(0.20f, 0.20f, 0.20f);
        private static readonly Color EntryBorderColor = new(0.10f, 0.10f, 0.10f);
        private static readonly Color EntryTextColor = new(0.88f, 0.88f, 0.88f);

        public const float ItemWidth = 120f;
        public const float ItemHeight = 32f;
        private const float AccentWidth = 4f;

        public static VisualElement CreateLabelItem(string text, Color accentColor, bool clipContents = true)
        {
            VisualElement entry = new VisualElement();
            entry.style.position = Position.Relative;
            entry.style.flexDirection = FlexDirection.Row;
            entry.style.alignItems = Align.Center;
            entry.style.width = ItemWidth;
            entry.style.minWidth = ItemWidth;
            entry.style.maxWidth = ItemWidth;
            entry.style.height = ItemHeight;
            entry.style.minHeight = ItemHeight;
            entry.style.maxHeight = ItemHeight;
            entry.style.marginBottom = 6f;
            entry.style.backgroundColor = EntryBackgroundColor;
            entry.style.borderTopWidth = 1f;
            entry.style.borderBottomWidth = 1f;
            entry.style.borderLeftWidth = 1f;
            entry.style.borderRightWidth = 1f;
            entry.style.borderTopColor = EntryBorderColor;
            entry.style.borderBottomColor = EntryBorderColor;
            entry.style.borderLeftColor = EntryBorderColor;
            entry.style.borderRightColor = EntryBorderColor;
            entry.style.borderTopLeftRadius = 4f;
            entry.style.borderTopRightRadius = 4f;
            entry.style.borderBottomLeftRadius = 4f;
            entry.style.borderBottomRightRadius = 4f;
            entry.style.overflow = clipContents ? Overflow.Hidden : Overflow.Visible;

            VisualElement accent = new VisualElement();
            accent.style.width = AccentWidth;
            accent.style.alignSelf = Align.Stretch;
            accent.style.backgroundColor = accentColor;
            accent.style.flexShrink = 0;
            accent.style.borderTopLeftRadius = 4f;
            accent.style.borderBottomLeftRadius = 4f;

            Label nameLabel = new Label(text);
            nameLabel.style.flexGrow = 1;
            nameLabel.style.color = EntryTextColor;
            nameLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            nameLabel.style.paddingLeft = 8f;
            nameLabel.style.paddingRight = 8f;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.fontSize = 12f;

            entry.Add(accent);
            entry.Add(nameLabel);

            return entry;
        }
    }
}