// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.UI.Visualizers
{
    [RequireComponent(typeof(RectTransform))]
    internal class SpreadBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Scale amount per second.
        /// </summary>
        public float ScalePerSecond { private get; set; }

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            var scaleIncrease = ScalePerSecond * Time.deltaTime;
            _rectTransform.localScale += new Vector3(scaleIncrease, scaleIncrease, 0f);
        }
    }
}
