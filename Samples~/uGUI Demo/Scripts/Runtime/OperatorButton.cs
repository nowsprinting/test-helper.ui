// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using Cysharp.Threading.Tasks;
using TestHelper.UI.Operators;
using UnityEngine;
using UnityEngine.UI;

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
                    Debug.LogError($"Unsupported ClickType: {OperatorType}");
                    break;
            }

            _button = GetComponent<Button>();
            _button.onClick.AddListener(() =>
            {
                DoOperate().Forget();
            });
        }

        private async UniTask DoOperate()
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
