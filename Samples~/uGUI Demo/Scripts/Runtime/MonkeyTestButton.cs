// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Operators;
using TestHelper.UI.ScreenshotFilenameStrategies;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.UI.Samples.UguiDemo
{
    [RequireComponent(typeof(Button))]
    public class MonkeyTestButton : MonoBehaviour
    {
        [field: SerializeField]
        private int LifetimeSeconds { get; set; } = 10;

        [field: SerializeField]
        private int DelayMillis { get; set; } = 200;

        [field: SerializeField]
        private int BufferLengthForDetectLooping { get; set; } = 10;

        [field: SerializeField]
        private bool VerboseLogger { get; set; }

        [field: SerializeField]
        private bool DebugVisualizer { get; set; }

        private Button _button;
        private Text _buttonText;
        private string _originalButtonText;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(() => RunMonkeyTests().Forget());
            _buttonText = _button.GetComponentInChildren<Text>();
            _originalButtonText = _buttonText.text;
        }

        private async UniTask RunMonkeyTests()
        {
            var config = new MonkeyConfig()
            {
                Lifetime = TimeSpan.FromSeconds(LifetimeSeconds),
                DelayMillis = DelayMillis,
                BufferLengthForDetectLooping = BufferLengthForDetectLooping,
                Verbose = VerboseLogger,
                Visualizer = DebugVisualizer ? new DefaultDebugVisualizer() : null,
                Screenshots = new ScreenshotOptions()
                {
                    FilenameStrategy = new CounterBasedStrategy("UguiDemo"),
                },
                Operators = new IOperator[]
                {
                    new UguiClickAndHoldOperator(),
                    new UguiClickOperator(),
                    new UguiDoubleClickOperator(),
                    new UguiDragAndDropOperator(),
                    new UguiScrollWheelOperator(),
                    new UguiSwipeOperator(),
                    new UguiTextInputOperator(),
                }
            };

            try
            {
                _button.interactable = false;
                _buttonText.text = "Running...";
                await Monkey.Run(config);
            }
            finally
            {
                _buttonText.text = _originalButtonText;
                _button.interactable = true;
            }
        }
    }
}
