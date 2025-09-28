// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.UI.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.UI.Samples.UguiDemo
{
    public class SpyDragEventHandler : AbstractSpyEventHandler,
        IInitializePotentialDragHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [field: SerializeField]
        private bool ShowDraggingObject { get; set; }

        private GameObject _draggingObject;

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (_toggleDrag.isOn)
            {
                Popup(eventData.position, "initialize potential drag");
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (ShowDraggingObject)
            {
                _draggingObject = new GameObject("dragging");
                _draggingObject.transform.parent = transform.parent;
                _draggingObject.transform.localScale = transform.localScale;
                if (gameObject.TryGetEnabledComponent<Image>(out var image))
                {
                    var draggingImage = _draggingObject.AddComponent<Image>();
                    draggingImage.raycastTarget = false;
                    var color = image.color;
                    color.a = 0.5f;
                    draggingImage.color = color;
                }
            }

            if (_toggleDrag.isOn)
            {
                Popup(eventData.position, "begin drag");
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (ShowDraggingObject)
            {
                _draggingObject.transform.position = eventData.position;
            }

            if (_toggleDrag.isOn)
            {
                Popup(eventData.position, "drag");
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (ShowDraggingObject)
            {
                Destroy(_draggingObject);
                _draggingObject = null;
            }

            if (_toggleDrag.isOn)
            {
                Popup(eventData.position, "end drag");
            }
        }
    }
}
