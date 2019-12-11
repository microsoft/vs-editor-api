using System;
using AppKit;

namespace System.Windows
{
    public class UIElement
    {
#pragma warning disable CS3003 // Type is not CLS-compliant
        public NSView NSView { get; }
#pragma warning restore CS3003 // Type is not CLS-compliant

        public UIElement(NSView view)
        {
            NSView = view;
        }

        public static implicit operator UIElement(NSView view)
        {
            return new UIElement(view);
        }

        public static implicit operator NSView(UIElement uiElement)
        {
            return uiElement.NSView;
        }
    }
}
