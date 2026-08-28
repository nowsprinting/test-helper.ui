// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

// ReSharper disable once CheckNamespace -- namespace mirrors the package's own scheme, not the Assets/Samples/<name>/<version> import path Unity generates locally
namespace TestHelper.UI.Samples.UguiDemo
{
    public class TabContent : MonoBehaviour
    {
        private TabContent[] _tabContents;

        private void Start()
        {
#if UNITY_6000_4_OR_NEWER
            _tabContents = FindObjectsByType<TabContent>(FindObjectsInactive.Exclude);
#elif UNITY_2022_3_OR_NEWER
            _tabContents = FindObjectsByType<TabContent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
            _tabContents = FindObjectsOfType<TabContent>();
#endif
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
