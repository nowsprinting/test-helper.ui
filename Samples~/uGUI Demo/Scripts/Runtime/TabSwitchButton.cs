// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.UI.Samples.UguiDemo
{
    /// <summary>
    /// Show target tab and hide other tabs when clicked.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class TabSwitchButton : MonoBehaviour
    {
        [field: SerializeField]
        public TabContent TargetContent { get; set; }

        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                if (TargetContent == null)
                {
                    Debug.LogError("Target Content is not assigned.", this);
                    return;
                }

                TargetContent.Select();
            });
        }
    }
}
