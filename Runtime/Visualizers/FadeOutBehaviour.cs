// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.UI.Visualizers
{
    /// <summary>
    /// Fade-out behavior for indicators.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class FadeOutBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Indicator lifetime in seconds.
        /// </summary>
        public float Lifetime { private get; set; } = 1.0f;

        /// <summary>
        /// Exponent for acceleration.
        /// 1 = linear,
        /// &gt;1 = accelerating (slow -> fast),
        /// &lt;1 = decelerating.
        /// </summary>
        public float Acceleration { private get; set; } = 1.0f;

        private Image _image;
        private float _elapsed;

        private void Start()
        {
            _image = GetComponent<Image>();
            Destroy(gameObject, Lifetime);
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(_elapsed / Lifetime);   // 0..1
            var accelerated = Mathf.Pow(t, Acceleration); // 0..1 with acceleration
            var color = _image.color;
            color.a = 1f - accelerated;
            _image.color = color;
        }
    }
}
