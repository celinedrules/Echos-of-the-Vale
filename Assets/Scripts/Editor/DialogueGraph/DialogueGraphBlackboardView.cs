using System;
using System.Collections.Generic;
using Data.DialogueData;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.DialogueGraph
{
    public class DialogueGraphBlackboardView
    {
        private readonly float _width;

        private static readonly Color PanelColor = new(0.15f, 0.15f, 0.15f);
        private static readonly Color BorderColor = new(0.22f, 0.22f, 0.22f);
        private static readonly Color ButtonColor = new(0.24f, 0.24f, 0.24f);
        private static readonly Color ButtonBorderColor = new(0.12f, 0.12f, 0.12f);
        private static readonly Color DividerColor = new(0.16f, 0.16f, 0.16f);
        private static readonly Color SpeakerAccentColor = new(0.86f, 0.60f, 0.16f);

        private DialogueTable _selectedTable;

        private VisualElement _root;
        private Label _titleLabel;
        private VisualElement _entriesContainer;

        public VisualElement Root => _root;

        public DialogueGraphBlackboardView(float width)
        {
            _width = width;
        }

        public VisualElement Build()
        {
            _root = new VisualElement();
            _root.style.width = _width;
            _root.style.minWidth = _width;
            _root.style.maxWidth = _width;
            _root.style.flexShrink = 0;
            _root.style.backgroundColor = PanelColor;
            _root.style.borderRightWidth = 1f;
            _root.style.borderRightColor = BorderColor;
            _root.style.paddingLeft = 10f;
            _root.style.paddingRight = 10f;
            _root.style.paddingTop = 10f;
            _root.style.paddingBottom = 10f;

            _titleLabel = new Label("Blackboard");
            _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _titleLabel.style.fontSize = 14f;
            _titleLabel.style.color = Color.white;
            _titleLabel.style.marginBottom = 8f;

            _entriesContainer = new VisualElement();
            _entriesContainer.style.flexDirection = FlexDirection.Column;
            _entriesContainer.style.marginTop = 4f;

            _root.Add(_titleLabel);
            _root.Add(BuildAddToolbar());
            _root.Add(_entriesContainer);

            RefreshEntries();

            return _root;
        }

        public void SetSelectedTable(DialogueTable table)
        {
            _selectedTable = table;
            RefreshEntries();
        }

        private VisualElement BuildAddToolbar()
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 10f;

            VisualElement compositeButton = new VisualElement();
            compositeButton.style.flexDirection = FlexDirection.Row;
            compositeButton.style.alignItems = Align.Stretch;
            compositeButton.style.height = 22f;
            compositeButton.style.backgroundColor = ButtonColor;
            compositeButton.style.borderTopWidth = 1f;
            compositeButton.style.borderBottomWidth = 1f;
            compositeButton.style.borderLeftWidth = 1f;
            compositeButton.style.borderRightWidth = 1f;
            compositeButton.style.borderTopColor = ButtonBorderColor;
            compositeButton.style.borderBottomColor = ButtonBorderColor;
            compositeButton.style.borderLeftColor = ButtonBorderColor;
            compositeButton.style.borderRightColor = ButtonBorderColor;
            compositeButton.style.borderTopLeftRadius = 2f;
            compositeButton.style.borderTopRightRadius = 2f;
            compositeButton.style.borderBottomLeftRadius = 2f;
            compositeButton.style.borderBottomRightRadius = 2f;

            Button addButton = new Button
            {
                text = "+"
            };
            addButton.style.width = 24f;
            addButton.style.height = 20f;
            addButton.style.marginLeft = 0f;
            addButton.style.marginRight = 0f;
            addButton.style.marginTop = 0f;
            addButton.style.marginBottom = 0f;
            addButton.style.paddingLeft = 0f;
            addButton.style.paddingRight = 0f;
            addButton.style.paddingTop = 0f;
            addButton.style.paddingBottom = 1f;
            addButton.style.borderTopWidth = 0f;
            addButton.style.borderBottomWidth = 0f;
            addButton.style.borderLeftWidth = 0f;
            addButton.style.borderRightWidth = 0f;
            addButton.style.backgroundColor = Color.clear;
            addButton.style.color = new Color(0.9f, 0.9f, 0.9f);
            addButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            addButton.style.fontSize = 13f;
            addButton.tooltip = "Add blackboard item";

            VisualElement divider = new VisualElement();
            divider.style.width = 1f;
            divider.style.height = 20f;
            divider.style.backgroundColor = DividerColor;
            divider.style.marginTop = 0f;
            divider.style.marginBottom = 0f;

            Button dropdownButton = new Button
            {
                text = "▼"
            };
            dropdownButton.style.width = 18f;
            dropdownButton.style.height = 20f;
            dropdownButton.style.marginLeft = 0f;
            dropdownButton.style.marginRight = 0f;
            dropdownButton.style.marginTop = 0f;
            dropdownButton.style.marginBottom = 0f;
            dropdownButton.style.paddingLeft = 0f;
            dropdownButton.style.paddingRight = 0f;
            dropdownButton.style.paddingTop = 0f;
            dropdownButton.style.paddingBottom = 0f;
            dropdownButton.style.borderTopWidth = 0f;
            dropdownButton.style.borderBottomWidth = 0f;
            dropdownButton.style.borderLeftWidth = 0f;
            dropdownButton.style.borderRightWidth = 0f;
            dropdownButton.style.backgroundColor = Color.clear;
            dropdownButton.style.color = new Color(0.82f, 0.82f, 0.82f);
            dropdownButton.style.fontSize = 8f;
            dropdownButton.tooltip = "Blackboard item types";

            addButton.clicked += () => ShowAddMenu(compositeButton);
            dropdownButton.clicked += () => ShowAddMenu(compositeButton);

            compositeButton.Add(addButton);
            compositeButton.Add(divider);
            compositeButton.Add(dropdownButton);

            row.Add(compositeButton);
            return row;
        }

        private void ShowAddMenu(VisualElement anchor)
        {
            GenericMenu menu = new GenericMenu();

            List<DialogueSpeakerData> speakerAssets = GetSpeakerAssets();

            if (speakerAssets.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("Speaker/No Speakers Found"));
            }
            else
            {
                for (int i = 0; i < speakerAssets.Count; i++)
                {
                    DialogueSpeakerData speaker = speakerAssets[i];
                    string speakerName = GetSpeakerDisplayName(speaker);

                    menu.AddItem(new GUIContent($"Speaker/{speakerName}"), false, () =>
                    {
                        AddSpeakerToBlackboard(speaker);
                    });
                }
            }

            Rect screenRect = GUIUtility.GUIToScreenRect(anchor.worldBound);
            menu.DropDown(screenRect);
        }

        private void AddSpeakerToBlackboard(DialogueSpeakerData speaker)
        {
            if (_selectedTable == null || speaker == null)
                return;

            Undo.RecordObject(_selectedTable, "Add Blackboard Speaker");

            if (!_selectedTable.AddBlackboardSpeaker(speaker))
                return;

            EditorUtility.SetDirty(_selectedTable);
            AssetDatabase.SaveAssets();
            RefreshEntries();
        }

        private void RemoveSpeakerFromBlackboard(DialogueSpeakerData speaker)
        {
            if (_selectedTable == null || speaker == null)
                return;

            Undo.RecordObject(_selectedTable, "Remove Blackboard Speaker");

            if (!_selectedTable.RemoveBlackboardSpeaker(speaker))
                return;

            EditorUtility.SetDirty(_selectedTable);
            AssetDatabase.SaveAssets();
            RefreshEntries();
        }

        private void RefreshEntries()
        {
            if (_entriesContainer == null)
                return;

            _entriesContainer.Clear();

            if (_selectedTable == null)
                return;

            IReadOnlyList<DialogueSpeakerData> speakers = _selectedTable.BlackboardSpeakers;
            if (speakers == null)
                return;

            for (int i = 0; i < speakers.Count; i++)
                _entriesContainer.Add(BuildSpeakerEntry(speakers[i]));
        }

        private VisualElement BuildSpeakerEntry(DialogueSpeakerData speaker)
        {
            VisualElement item = DialogueGraphBlackboardItemFactory.CreateLabelItem(
                GetSpeakerDisplayName(speaker),
                SpeakerAccentColor);

            item.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Remove", _ => RemoveSpeakerFromBlackboard(speaker));
            }));

            return item;
        }

        private static List<DialogueSpeakerData> GetSpeakerAssets()
        {
            List<DialogueSpeakerData> speakers = new List<DialogueSpeakerData>();
            string[] guids = AssetDatabase.FindAssets("t:DialogueSpeakerData");

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                DialogueSpeakerData speaker = AssetDatabase.LoadAssetAtPath<DialogueSpeakerData>(path);

                if (speaker != null)
                    speakers.Add(speaker);
            }

            speakers.Sort((a, b) =>
                string.Compare(
                    GetSpeakerDisplayName(a),
                    GetSpeakerDisplayName(b),
                    StringComparison.OrdinalIgnoreCase));

            return speakers;
        }

        private static string GetSpeakerDisplayName(DialogueSpeakerData speaker)
        {
            if (speaker == null)
                return "(Missing Speaker)";

            if (!string.IsNullOrWhiteSpace(speaker.SpeakerName))
                return speaker.SpeakerName.Trim();

            return speaker.name;
        }
    }
}