// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.UI.Samples.UguiDemo
{
    public class TabContent : MonoBehaviour
    {
        [field: SerializeField]
        public GameObject ControlPanel { get; set; }

        private List<TabContent> _tabContents = new List<TabContent>();

        private void Start()
        {
            _tabContents = FindObjectsByType<TabContent>(FindObjectsSortMode.None).ToList();
        }

        /// <summary>
        /// Activate only myself in the contents.
        /// </summary>
        public void Select()
        {
            foreach (var tabContent in _tabContents)
            {
                tabContent.gameObject.SetActive(tabContent == this);
            }

            var monkeyTestsButton = FindAnyObjectByType<MonkeyTestsButton>();
            var interactable = monkeyTestsButton.GetComponent<Button>().interactable;
            SetControlsInteractable(interactable);
        }

        /// <summary>
        /// Set interactable state for all Selectable components in ControlPanel.
        /// </summary>
        /// <param name="interactable"></param>
        public void SetControlsInteractable(bool interactable)
        {
            if (ControlPanel == null)
            {
                return;
            }

            foreach (var selectable in ControlPanel.GetComponentsInChildren<Selectable>())
            {
                selectable.interactable = interactable;
            }
        }
    }
}
