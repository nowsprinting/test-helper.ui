// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.UI.Samples.UguiDemo
{
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("/")] // Hide from "Add Component" picker
    public class EventPopup : MonoBehaviour
    {
        [field: SerializeField]
        private float Lifetime { get; set; } = 1.0f;

        [field: SerializeField]
        private float Speed { get; set; } = 40.0f;

        internal Image ParentImage { private get; set; }
        private Image _background;
        private float _scaleFactor;

        private static EventPopup s_lastPopup;

        private void Start()
        {
            Destroy(gameObject, Lifetime);

            var canvas = GetComponentInParent<Canvas>();
            _scaleFactor = canvas ? canvas.scaleFactor : 1.0f;

            _background = GetComponent<Image>();

            if (ParentImage && ParentImage.color != Color.white)
            {
                Color.RGBToHSV(ParentImage.color, out var h, out var s, out var v);
                _background.color = Color.HSVToRGB(h, 1.0f, 0.3f);
            }

            var text = GetComponentInChildren<Text>();
            text.text = name;

            var width = Mathf.Max(text.preferredWidth + 8.0f, 60.0f);
            var height = text.preferredHeight + 4.0f;
            var rectTransform = transform as RectTransform;
            rectTransform!.sizeDelta = new Vector2(width, height);

            transform.localPosition -= Vector3.down * (height * 0.5f);

            if (s_lastPopup)
            {
                s_lastPopup.transform.SetParent(transform, true); // Make chaining for bulk moves

                var lastPos = s_lastPopup.transform.localPosition;
                if (lastPos.y < height)
                {
                    // force up to avoid overlap
                    s_lastPopup.transform.localPosition = new Vector3(lastPos.x, height + 1.0f, lastPos.z);
                }
            }

            s_lastPopup = this;
        }

        private void Update()
        {
            if (this == s_lastPopup)
            {
                transform.Translate(Vector3.up * (Time.deltaTime * Speed * _scaleFactor));
            }

            var color = _background.color;
            color.a -= Time.deltaTime / Lifetime;
            _background.color = color;
        }
    }
}
