// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine.EventSystems;

// ReSharper disable once CheckNamespace -- namespace mirrors the package's own scheme, not the Assets/Samples/<name>/<version> import path Unity generates locally
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
