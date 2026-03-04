// Done
using UnityEngine;

namespace UI.Common
{
    public interface IUiPanel
    {
        CanvasGroup CanvasGroup { get; }
        bool ShowMenuButtons { get; }
        bool ShowBackground { get; }
        bool DisablePlayerInput { get; }
        bool HasTooltips { get; }
        void OnOpened();
    }
}