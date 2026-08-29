// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Operators;
using TestHelper.UI.ScreenshotFilenameStrategies;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace -- namespace mirrors the package's own scheme, not the Assets/Samples/<name>/<version> import path Unity generates locally
namespace TestHelper.UI.Samples.UguiDemo
{
    [RequireComponent(typeof(Button))]
    public class MonkeyTestButton : MonoBehaviour
    {
        [field: SerializeField]
        public int LifetimeSeconds { get; set; } = 10;

        [field: SerializeField]
        private int DelayMillis { get; set; } = 200;

        [field: SerializeField]
        public int BufferLengthForDetectLooping { get; set; } = 10;

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
            _button.onClick.AddListener(() => RunMonkeyTestsAsync().Forget());
            _buttonText = _button.GetComponentInChildren<Text>();
            _originalButtonText = _buttonText.text;
        }

        private async UniTask RunMonkeyTestsAsync()
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
                OperatorPool = new OperatorPool()
                    .Register<UguiClickAndHoldOperator>()
                    .Register<UguiClickOperator>()
                    .Register<UguiDoubleClickOperator>()
                    .Register<UguiDragAndDropOperator>()
                    .Register<UguiScrollWheelOperator>()
                    .Register<UguiSwipeOperator>()
                    .Register<UguiTextInputOperator>()
            };

            try
            {
                _button.interactable = false;
                _buttonText.text = "Running...";
                await Monkey.Run(config);
            }
            finally
            {
                if (_buttonText)
                {
                    _buttonText.text = _originalButtonText;
                }

                if (_button)
                {
                    _button.interactable = true;
                }
            }
        }
    }
}
