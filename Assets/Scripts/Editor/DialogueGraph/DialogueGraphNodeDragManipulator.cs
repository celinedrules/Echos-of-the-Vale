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

        private bool _dragging;
        private bool _bypassSnapForCurrentDrag;
        private Vector2 _dragStartNodePosition;
        private Vector2 _dragStartPointerViewportPosition;
        private Vector2 _lastAppliedPosition;

        public DialogueGraphNodeDragManipulator(
            VisualElement targetNode,
            int rowId,
            int rowIndex,
            DialogueGraphWindow window)
        {
            _targetNode = targetNode;
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

            if (_window.IsConnectModeActive)
                return;

            if ((evt.modifiers & EventModifiers.Control) != 0 ||
                (evt.modifiers & EventModifiers.Command) != 0)
                return;

            if (evt.target is VisualElement clickedElement)
            {
                if (DialogueGraphNodeViewFactory.IsPortElement(clickedElement))
                    return;

                if (DialogueGraphNodeViewFactory.IsTextInputElement(clickedElement))
                    return;
            }

            _window.SelectRow(_rowId, _rowIndex);

            _dragging = true;
            _bypassSnapForCurrentDrag = (evt.modifiers & EventModifiers.Alt) != 0;
            _dragStartNodePosition = new Vector2(_targetNode.resolvedStyle.left, _targetNode.resolvedStyle.top);
            _dragStartPointerViewportPosition = new Vector2(evt.position.x, evt.position.y);
            _lastAppliedPosition = _dragStartNodePosition;

            target.CapturePointer(evt.pointerId);
            evt.StopImmediatePropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_dragging || !target.HasPointerCapture(evt.pointerId))
                return;

            Vector2 currentPointerViewportPosition = new Vector2(evt.position.x, evt.position.y);
            Vector2 pointerDeltaInViewport = currentPointerViewportPosition - _dragStartPointerViewportPosition;

            float zoom = _window.CurrentZoom;
            if (zoom <= 0.0001f)
                zoom = 1f;

            Vector2 pointerDeltaInGraph = pointerDeltaInViewport / zoom;
            Vector2 rawPosition = _dragStartNodePosition + pointerDeltaInGraph;

            bool isAltHeld = (evt.modifiers & EventModifiers.Alt) != 0;
            bool shouldSnap = _window.IsGridSnapEnabled && !isAltHeld;

            _bypassSnapForCurrentDrag = _bypassSnapForCurrentDrag || isAltHeld;

            Vector2 appliedPosition = shouldSnap
                ? _window.SnapToGrid(rawPosition)
                : rawPosition;

            if (appliedPosition == _lastAppliedPosition)
            {
                evt.StopPropagation();
                return;
            }

            _targetNode.style.left = appliedPosition.x;
            _targetNode.style.top = appliedPosition.y;
            _lastAppliedPosition = appliedPosition;

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
            _bypassSnapForCurrentDrag = false;
        }

        private void EndDrag(int pointerId)
        {
            _dragging = false;

            if (target.HasPointerCapture(pointerId))
                target.ReleasePointer(pointerId);

            _window.SaveNodePosition(_rowId, _targetNode, snapToGrid: !_bypassSnapForCurrentDrag);
            _bypassSnapForCurrentDrag = false;
        }
    }
}