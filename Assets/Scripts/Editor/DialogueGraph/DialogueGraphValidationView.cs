using System.Collections.Generic;
using Data.DialogueData;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.DialogueGraph
{
    public class DialogueGraphValidationView
    {
        private readonly System.Action<int> _selectRowById;

        private VisualElement _root;
        private Label _titleLabel;
        private ScrollView _messagesScrollView;

        public VisualElement Root => _root;

        public DialogueGraphValidationView(System.Action<int> selectRowById)
        {
            _selectRowById = selectRowById;
        }

        public VisualElement Build()
        {
            _root = new VisualElement();
            _root.style.height = 180f;
            _root.style.minHeight = 140f;
            _root.style.maxHeight = 240f;
            _root.style.flexShrink = 0;
            _root.style.backgroundColor = new Color(0.14f, 0.14f, 0.14f);
            _root.style.borderTopWidth = 1f;
            _root.style.borderTopColor = new Color(0.22f, 0.22f, 0.22f);
            _root.style.paddingLeft = 10f;
            _root.style.paddingRight = 10f;
            _root.style.paddingTop = 8f;
            _root.style.paddingBottom = 8f;

            _titleLabel = new Label("Validation");
            _titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            _titleLabel.style.fontSize = 14f;
            _titleLabel.style.color = Color.white;
            _titleLabel.style.marginBottom = 8f;

            _messagesScrollView = new ScrollView();
            _messagesScrollView.style.flexGrow = 1;

            _root.Add(_titleLabel);
            _root.Add(_messagesScrollView);

            return _root;
        }

        public void Refresh(DialogueTable table)
        {
            if (_messagesScrollView == null)
                return;

            _messagesScrollView.Clear();

            if (table == null)
            {
                _messagesScrollView.Add(BuildInfoLabel("Select a DialogueTable to view validation results."));
                return;
            }

            List<string> messages = table.GetValidationMessages();

            if (messages == null || messages.Count == 0)
            {
                _messagesScrollView.Add(BuildSuccessLabel("No validation issues found."));
                return;
            }

            for (int i = 0; i < messages.Count; i++)
            {
                string message = messages[i];
                int rowId = DialogueGraphValidationUtility.TryExtractRowId(message);

                Button messageButton = new Button(() =>
                {
                    if (rowId >= 0)
                        _selectRowById?.Invoke(rowId);
                });

                messageButton.text = $"• {message}";
                messageButton.style.unityTextAlign = TextAnchor.MiddleLeft;
                messageButton.style.whiteSpace = WhiteSpace.Normal;
                messageButton.style.marginBottom = 4f;
                messageButton.style.paddingTop = 6f;
                messageButton.style.paddingBottom = 6f;
                messageButton.style.paddingLeft = 8f;
                messageButton.style.paddingRight = 8f;
                messageButton.style.backgroundColor = new Color(0.23f, 0.18f, 0.18f);
                messageButton.style.color = new Color(1f, 0.82f, 0.82f);

                if (rowId < 0)
                    messageButton.SetEnabled(false);

                _messagesScrollView.Add(messageButton);
            }
        }

        private static Label BuildInfoLabel(string text)
        {
            Label label = new Label(text);
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.color = new Color(0.78f, 0.78f, 0.78f);
            return label;
        }

        private static Label BuildSuccessLabel(string text)
        {
            Label label = new Label(text);
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.color = new Color(0.68f, 0.95f, 0.68f);
            return label;
        }
    }
}