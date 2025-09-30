// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Operators;
using TestHelper.UI.ScreenshotFilenameStrategies;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.UI.Samples.UguiDemo
{
    [RequireComponent(typeof(Button))]
    public class MonkeyTestsButton : MonoBehaviour
    {
        [field: SerializeField]
        private int LifetimeSeconds { get; set; } = 10;

        [field: SerializeField]
        private int DelayMillis { get; set; } = 200;

        private Button _button;
        private Text _buttonText;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() => RunMonkeyTests().Forget());
            _buttonText = _button.GetComponentInChildren<Text>();
        }

        private async UniTask RunMonkeyTests()
        {
            var config = new MonkeyConfig()
            {
                Lifetime = TimeSpan.FromSeconds(LifetimeSeconds),
                DelayMillis = DelayMillis,
                BufferLengthForDetectLooping = 10,
                Screenshots = new ScreenshotOptions()
                {
                    FilenameStrategy = new CounterBasedStrategy("UguiDemo"),
                },
                Operators = new IOperator[]
                {
                    new UguiClickOperator(),
                    new UguiDoubleClickOperator(),
                    new UguiClickAndHoldOperator(),
                    new UguiDragAndDropOperator(),
                    new UguiSwipeOperator(),
                    new UguiScrollWheelOperator(),
                    new UguiTextInputOperator(),
                }
            };

            try
            {
                SetControlsInteractable(false);
                await Monkey.Run(config);
            }
            finally
            {
                SetControlsInteractable(true);
            }
        }

        private void SetControlsInteractable(bool interactable)
        {
            // myself
            _button.interactable = interactable;
            _buttonText.text = _button.interactable ? "Run Monkey Tests" : "Running...";

            // settings
            foreach (var toggle in GameObject.Find("SettingsPane").GetComponentsInChildren<Toggle>())
            {
                toggle.interactable = interactable;
            }

            // controls in content pane
            foreach (var content in FindObjectsOfType<TabContent>())
            {
                content.SetControlsInteractable(interactable);
            }
        }
    }
}
