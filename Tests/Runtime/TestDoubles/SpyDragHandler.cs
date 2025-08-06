// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.UI.TestDoubles
{
    [RequireComponent(typeof(Image))]
    public class SpyDragHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public bool WasPointerDown { get; private set; }
        public bool WasPointerUp { get; private set; }
        public bool WasInitializePotentialDrag { get; private set; }
        public bool WasBeginDrag { get; private set; }
        public bool WasEndDrag { get; private set; }
        public Vector2 LastDragPosition { get; private set; }

        private Image _image;

        private void Start()
        {
            _image = GetComponent<Image>();
            _image.color = UnityEngine.Random.ColorHSV();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log($"OnPointerDown: {eventData.position}");
            WasPointerDown = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log($"OnPointerUp: {eventData.position}");
            WasPointerUp = true;
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
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log($"OnBeginDrag: {eventData.position}");
            WasBeginDrag = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log($"OnEndDrag: {eventData.position}");
            WasEndDrag = true;
        }
    }
}
