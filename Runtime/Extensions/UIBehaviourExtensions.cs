// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.UI.Extensions
{
    public static class UIBehaviourExtensions
    {
        /// <summary>
        /// Returns true if the UI element is horizontally scrollable.
        /// </summary>
        public static bool CanScrollHorizontally(this UIBehaviour uiBehaviour)
        {
            if (uiBehaviour is Scrollbar scrollbar)
            {
                return scrollbar.direction == Scrollbar.Direction.LeftToRight
                       || scrollbar.direction == Scrollbar.Direction.RightToLeft;
            }

            if (uiBehaviour is ScrollRect scrollRect)
            {
                return scrollRect.horizontal;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the UI element is vertically scrollable.
        /// </summary>
        public static bool CanScrollVertically(this UIBehaviour uiBehaviour)
        {
            if (uiBehaviour is Scrollbar scrollbar)
            {
                return scrollbar.direction == Scrollbar.Direction.BottomToTop
                       || scrollbar.direction == Scrollbar.Direction.TopToBottom;
            }

            if (uiBehaviour is ScrollRect scrollRect)
            {
                return scrollRect.vertical;
            }

            return false;
        }
    }
}
