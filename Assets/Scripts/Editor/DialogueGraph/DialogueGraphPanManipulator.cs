using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.DialogueGraph
{
    public class DialogueGraphPanManipulator : Manipulator
    {
        private readonly DialogueGraphWindow _window;
        private bool _panning;
        private int _panPointerId = -1;
        private Vector2 _startPointerPosition;
        private Vector2 _startPanPosition;

        public DialogueGraphPanManipulator(DialogueGraphWindow window)
        {
            _window = window;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != (int)MouseButton.LeftMouse)
                return;

            if ((evt.modifiers & EventModifiers.Control) == 0 &&
                (evt.modifiers & EventModifiers.Command) == 0)
                return;

            _panning = true;
            _panPointerId = evt.pointerId;
            _startPointerPosition = new Vector2(evt.position.x, evt.position.y);
            _startPanPosition = _window.GraphPanPosition;

            target.CapturePointer(evt.pointerId);
            evt.StopImmediatePropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_panning || !target.HasPointerCapture(evt.pointerId))
                return;

            Vector2 currentPointerPosition = new Vector2(evt.position.x, evt.position.y);
            Vector2 delta = currentPointerPosition - _startPointerPosition;

            _window.SetGraphPan(_startPanPosition + delta);

            evt.StopImmediatePropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_panning || evt.button != (int)MouseButton.LeftMouse)
                return;

            EndPan(evt.pointerId);
            evt.StopImmediatePropagation();
        }

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            _panning = false;
            _panPointerId = -1;
        }

        private void EndPan(int pointerId)
        {
            _panning = false;
            _panPointerId = -1;

            if (target.HasPointerCapture(pointerId))
                target.ReleasePointer(pointerId);
        }
    }
}