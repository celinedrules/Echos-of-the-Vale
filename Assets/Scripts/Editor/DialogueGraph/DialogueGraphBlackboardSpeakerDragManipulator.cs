using Data.DialogueData;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.DialogueGraph
{
    public class DialogueGraphBlackboardSpeakerDragManipulator : PointerManipulator
    {
        private const string DragDataKey = "DialogueGraphBlackboardSpeaker";

        private readonly DialogueSpeakerData _speaker;
        private Vector2 _pointerDownPosition;
        private bool _pressed;
        private bool _dragStarted;

        public static string SpeakerDragDataKey => DragDataKey;

        public DialogueGraphBlackboardSpeakerDragManipulator(DialogueSpeakerData speaker)
        {
            _speaker = speaker;

            activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.LeftMouse
            });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!CanStartManipulation(evt) || _speaker == null)
                return;

            _pressed = true;
            _dragStarted = false;
            _pointerDownPosition = evt.position;
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_pressed || _dragStarted || _speaker == null)
                return;

            if (Vector2.Distance(_pointerDownPosition, evt.position) < 4f)
                return;

            _dragStarted = true;

            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = new Object[] { _speaker };
            DragAndDrop.SetGenericData(DragDataKey, _speaker);
            DragAndDrop.StartDrag($"Speaker: {_speaker.SpeakerName}");
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            _pressed = false;
            _dragStarted = false;
        }
    }
}