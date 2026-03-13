using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.DialogueGraph
{
    public class DialogueGraphNodeDragManipulator : PointerManipulator
    {
        private readonly VisualElement _targetNode;
        private readonly int _rowId;
        private readonly int _rowIndex;
        private readonly DialogueGraphWindow _window;
        private readonly Label _positionLabel;

        private bool _dragging;
        private Vector2 _pointerOffset;

        public DialogueGraphNodeDragManipulator(
            VisualElement targetNode,
            int rowId,
            int rowIndex,
            DialogueGraphWindow window,
            Label positionLabel)
        {
            _targetNode = targetNode;
            _rowId = rowId;
            _rowIndex = rowIndex;
            _window = window;
            _positionLabel = positionLabel;

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

            if (_window.IsConnectModeActive)
                return;

            _window.SelectRow(_rowId, _rowIndex);

            _dragging = true;
            _pointerOffset = evt.localPosition;
            target.CapturePointer(evt.pointerId);
            evt.StopImmediatePropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_dragging || !target.HasPointerCapture(evt.pointerId))
                return;

            Vector2 parentPosition = _targetNode.parent.WorldToLocal(evt.position);
            float newLeft = parentPosition.x - _pointerOffset.x;
            float newTop = parentPosition.y - _pointerOffset.y;

            _targetNode.style.left = Mathf.Max(0f, newLeft);
            _targetNode.style.top = Mathf.Max(0f, newTop);

            Vector2 updatedPosition = new Vector2(_targetNode.resolvedStyle.left, _targetNode.resolvedStyle.top);
            _positionLabel.text = $"({Mathf.RoundToInt(updatedPosition.x)}, {Mathf.RoundToInt(updatedPosition.y)})";
            _window.MarkGraphDirty();

            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_dragging || !CanStopManipulation(evt))
                return;

            EndDrag(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            _dragging = false;
        }

        private void EndDrag(int pointerId)
        {
            _dragging = false;

            if (target.HasPointerCapture(pointerId))
                target.ReleasePointer(pointerId);

            _window.SaveNodePosition(_rowId, _targetNode, _positionLabel);
        }
    }
}