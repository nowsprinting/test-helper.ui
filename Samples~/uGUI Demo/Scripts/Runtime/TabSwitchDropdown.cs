// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.UI.Samples.UguiDemo
{
    /// <summary>
    /// Show target tab and hide other tabs when value changed.
    /// </summary>
    [RequireComponent(typeof(Dropdown))]
    public class TabSwitchDropdown : MonoBehaviour
    {
        [field: SerializeField]
        public List<TabContent> TargetContents { get; set; }

        private void Awake()
        {
            var dropdown = GetComponent<Dropdown>();
            dropdown.options.Clear();
            foreach (var content in TargetContents)
            {
                dropdown.options.Add(new Dropdown.OptionData(content.gameObject.name));
            }

            dropdown.onValueChanged.AddListener(_ =>
            {
                TargetContents[dropdown.value].Select();
            });
        }
    }
}
