// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine.EventSystems;

namespace TestHelper.UI.Samples.UguiDemo
{
    public class SpyDropEventHandler : AbstractSpyEventHandler, IDropHandler
    {
        public void OnDrop(PointerEventData eventData)
        {
            if (_toggleDrag.isOn)
            {
                Popup(eventData.position, "drop");
            }
        }
    }
}
