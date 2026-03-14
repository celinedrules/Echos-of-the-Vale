using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.DialogueGraph
{
    public class DialogueGraphPortConnectManipulator : PointerManipulator
    {
        private readonly VisualElement _portElement;
        private readonly int _rowId;
        private readonly int _rowIndex;
        private readonly DialogueGraphWindow _window;
        private bool _dragging;

        public DialogueGraphPortConnectManipulator(
            VisualElement portElement,
            int rowId,
            int rowIndex,
            DialogueGraphWindow window)
        {
            _portElement = portElement;
            _rowId = rowId;
            _rowIndex = rowIndex;
            _window = window;

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
            target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!CanStartManipulation(evt))
                return;

            _window.SelectRow(_rowId, _rowIndex);
            _window.BeginPortDragConnection(_rowId, _rowIndex);

            _dragging = true;
            target.CapturePointer(evt.pointerId);
            _window.UpdatePortDragPreview(evt.position);

            evt.StopImmediatePropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_dragging || !target.HasPointerCapture(evt.pointerId))
                return;

            _window.UpdatePortDragPreview(evt.position);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_dragging || !CanStopManipulation(evt))
                return;

            _dragging = false;

            if (target.HasPointerCapture(evt.pointerId))
                target.ReleasePointer(evt.pointerId);

            _window.CompletePortDragConnection(evt.position);
            evt.StopPropagation();
        }

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (!_dragging)
                return;

            _dragging = false;
            _window.CancelConnectMode();
        }
    }
}