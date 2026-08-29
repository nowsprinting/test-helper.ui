// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using Cysharp.Threading.Tasks;
using TestHelper.UI.Operators;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace -- namespace mirrors the package's own scheme, not the Assets/Samples/<name>/<version> import path Unity generates locally
namespace TestHelper.UI.Samples.UguiDemo
{
    public enum OperatorType
    {
        SingleClick,
        DoubleClick,
        ClickAndHold,
        RightClick,
        Hover,
        DragAndDrop,
        Flick,
        Swipe,
        ScrollWheel,
        Pinch,
        TextInput,
    }

    [RequireComponent(typeof(Button))]
    public class OperatorButton : MonoBehaviour
    {
        [field: SerializeField]
        public GameObject OperationTarget { get; set; }

        [field: SerializeField]
        public OperatorType OperatorType { get; set; } = OperatorType.SingleClick;

        private Button _button;
        private IOperator _operator;

        private void Awake()
        {
            // This switch triggers resharper_switch_statement_handles_some_known_enum_values_with_default_highlighting
            // (RightClick, Hover, Pinch fall through to default). Not suppressed: their IOperator
            // implementations are planned for the near future, so the warning should stay visible as a reminder.
            switch (OperatorType)
            {
                case OperatorType.SingleClick:
                    _operator = new UguiClickOperator();
                    break;
                case OperatorType.DoubleClick:
                    _operator = new UguiDoubleClickOperator();
                    break;
                case OperatorType.ClickAndHold:
                    _operator = new UguiClickAndHoldOperator();
                    break;
                case OperatorType.DragAndDrop:
                    _operator = new UguiDragAndDropOperator();
                    break;
                case OperatorType.Swipe:
                    _operator = new UguiSwipeOperator();
                    break;
                case OperatorType.Flick:
                    _operator = new UguiSwipeOperator(swipeSpeed: 2000, swipeDistance: 80f);
                    break;
                case OperatorType.ScrollWheel:
                    _operator = new UguiScrollWheelOperator();
                    break;
                case OperatorType.TextInput:
                    _operator = new UguiTextInputOperator();
                    break;
                default:
                    // Boxes OperatorType to build the log message; only reachable for an unimplemented
                    // operator type (RightClick, Hover, Pinch), not worth a hand-rolled enum-to-string switch.
#pragma warning disable RCS1198
                    Debug.LogError($"Unsupported OperatorType: {OperatorType}");
#pragma warning restore RCS1198
                    break;
            }

            _button = GetComponent<Button>();
            _button.onClick.AddListener(() =>
            {
                DoOperateAsync().Forget();
            });
        }

        private async UniTask DoOperateAsync()
        {
            if (OperationTarget == null)
            {
                Debug.LogError("OperationTarget is not assigned");
                return;
            }

            if (!_operator.CanOperate(OperationTarget))
            {
                Debug.LogError($"Cannot operate on the target: {OperationTarget.name} with {_operator.GetType().Name}");
                return;
            }

            try
            {
                _button.interactable = false;
                await _operator.OperateAsync(OperationTarget);
            }
            finally
            {
                _button.interactable = true;
            }
        }
    }
}
