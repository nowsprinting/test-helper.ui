// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.UI.TestComponents
{
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("/")] // Hide from "Add Component" picker
    public class RelayButton : MonoBehaviour
    {
        [field: SerializeField]
        public Button NextButton { get; private set; }

        [field: SerializeField]
        public bool EnabledOnLoad { get; set; }

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.interactable = EnabledOnLoad;
            _button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            _button.interactable = false;
            NextButton.interactable = true;
        }
    }
}
