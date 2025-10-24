// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.UI.TestDoubles
{
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("/")] // Hide from "Add Component" picker
    public class SpyOnDragHandler : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler,
        IDragHandler
    {
        public static readonly Vector2 LastDragPositionInitialValue = new Vector2(float.MinValue, float.MinValue);

        public bool WasInitializePotentialDrag { get; private set; }
        public bool WasBeginDrag { get; private set; }
        public bool WasEndDrag { get; private set; }
        public Vector2 LastDragPosition { get; private set; } = LastDragPositionInitialValue;

        private Image _image;
        private GameObject _draggingObject;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.color = UnityEngine.Random.ColorHSV();
            _image.raycastTarget = true;
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            Debug.Log($"OnInitializePotentialDrag: {eventData.position}");
            WasInitializePotentialDrag = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Debug.Log($"OnDrag: {eventData.position}");
            LastDragPosition = eventData.position;

            Dragging(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log($"OnBeginDrag: {eventData.position}");
            WasBeginDrag = true;

            BeginDrag();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log($"OnEndDrag: {eventData.position}");
            WasEndDrag = true;

            EndDrag();
        }

        private void BeginDrag()
        {
            _draggingObject = new GameObject($"Dragging {name}");
            _draggingObject.transform.SetParent(transform.parent);
            _draggingObject.transform.position = transform.position;

            var draggingObjectImage = _draggingObject.AddComponent<Image>();
            draggingObjectImage.color = new Color(_image.color.r, _image.color.g, _image.color.b, 0.5f);
            draggingObjectImage.raycastTarget = false;
            // Note: GameObject that are visible during dragging must not receive raycasts or implement IDropHandler.

            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 0.5f);
        }

        private void Dragging(PointerEventData eventData)
        {
            if (_draggingObject == null)
            {
                return;
            }

            _draggingObject.transform.position = eventData.position;
        }

        private void EndDrag()
        {
            if (_draggingObject == null)
            {
                return;
            }

            Destroy(_draggingObject);
            _draggingObject = null;

            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 1.0f);
        }
    }
}
