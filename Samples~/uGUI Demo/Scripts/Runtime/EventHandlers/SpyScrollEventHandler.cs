// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine.EventSystems;

namespace TestHelper.UI.Samples.UguiDemo
{
    public class SpyScrollEventHandler : AbstractSpyEventHandler, IScrollHandler
    {
        public void OnScroll(PointerEventData eventData)
        {
            if (_toggleScroll.isOn)
            {
                Popup(eventData.position, "scroll");
            }
        }
    }
}
