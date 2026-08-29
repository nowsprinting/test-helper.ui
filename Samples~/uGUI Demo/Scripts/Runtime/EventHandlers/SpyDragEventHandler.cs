// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.UI.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace -- namespace mirrors the package's own scheme, not the Assets/Samples/<name>/<version> import path Unity generates locally
namespace TestHelper.UI.Samples.UguiDemo
{
    // THP3002 asks this to end with "SpyEventHandler". Not applied: this demo's established naming
    // convention is Spy<Verb>EventHandler (SpyDragEventHandler, SpyDropEventHandler, ...), which reads
    // better than appending the base class name a second time.
#pragma warning disable THP3002
    public class SpyDragEventHandler : SpyEventHandler,
#pragma warning restore THP3002
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
