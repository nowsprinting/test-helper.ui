// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine.EventSystems;

// ReSharper disable once CheckNamespace -- namespace mirrors the package's own scheme, not the Assets/Samples/<name>/<version> import path Unity generates locally
namespace TestHelper.UI.Samples.UguiDemo
{
    // THP3002 asks this to end with "SpyEventHandler". Not applied: this demo's established naming
    // convention is Spy<Verb>EventHandler (SpyDragEventHandler, SpyDropEventHandler, ...), which reads
    // better than appending the base class name a second time.
#pragma warning disable THP3002
    public class SpyPointerEventHandler : SpyEventHandler,
#pragma warning restore THP3002
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_togglePointerEnter.isOn)
            {
                Popup(eventData.position, "enter");
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_togglePointerEnter.isOn)
            {
                Popup(eventData.position, "exit");
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_togglePointerDown.isOn)
            {
                Popup(eventData.position, "down");
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_togglePointerDown.isOn)
            {
                Popup(eventData.position, "up");
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_togglePointerClick.isOn)
            {
                Popup(eventData.position, "click");
            }
        }
    }
}
