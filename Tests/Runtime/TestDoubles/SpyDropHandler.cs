// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.UI.TestDoubles
{
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("/")] // Hide from "Add Component" picker
    public class SpyDropHandler : MonoBehaviour, IDropHandler
    {
        public bool WasDrop { get; private set; }

        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _image.color = UnityEngine.Random.ColorHSV();
            _image.raycastTarget = true;
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log($"OnDrop: {eventData.position}");
            WasDrop = true;
        }
    }
}
