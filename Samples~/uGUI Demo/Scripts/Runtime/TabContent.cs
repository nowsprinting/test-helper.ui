// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TestHelper.UI.Samples.UguiDemo
{
    public class TabContent : MonoBehaviour
    {
        private List<TabContent> _tabContents = new List<TabContent>();

        private void Start()
        {
            _tabContents = FindObjectsOfType<TabContent>().ToList();
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
        }
    }
}
