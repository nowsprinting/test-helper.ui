// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine.EventSystems;

// ReSharper disable once CheckNamespace -- namespace mirrors the package's own scheme, not the Assets/Samples/<name>/<version> import path Unity generates locally
namespace TestHelper.UI.Samples.UguiDemo
{
    public class SpyPointerEventHandler : AbstractSpyEventHandler,
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
