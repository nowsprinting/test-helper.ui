// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.UI.Samples.UguiDemo
{
    public class SpyInputFieldEventReceiver : AbstractSpyEventHandler
    {
        private string _eventSourceName; // add if an event with the same name exists

        private void OnEnable()
        {
            if (TryGetComponent<InputField>(out var inputField))
            {
                _eventSourceName = inputField.GetType().Name;

                inputField.onEndEdit.AddListener(OnEndEdit);
                inputField.onSubmit.AddListener(OnSubmit);
                inputField.onValidateInput += OnValidateInput;
                inputField.onValueChanged.AddListener(OnValueChanged);
            }

            if (TryGetComponent<TMP_InputField>(out var tmpInputField))
            {
                _eventSourceName = tmpInputField.GetType().Name;

                // same as InputField
                tmpInputField.onEndEdit.AddListener(OnEndEdit);
                tmpInputField.onSubmit.AddListener(OnSubmit);
                tmpInputField.onValidateInput += OnValidateInput;
                tmpInputField.onValueChanged.AddListener(OnValueChanged);

                // additional events for TMP_InputField
                tmpInputField.onSelect.AddListener(OnSelect);
                tmpInputField.onDeselect.AddListener(OnDeselect);
                tmpInputField.onTouchScreenKeyboardStatusChanged.AddListener(OnTouchScreenKeyboardStatusChanged);
            }
        }

        private void OnDisable()
        {
            if (TryGetComponent<InputField>(out var inputField))
            {
                _eventSourceName = inputField.GetType().Name;

                inputField.onEndEdit.RemoveListener(OnEndEdit);
                inputField.onSubmit.RemoveListener(OnSubmit);
                inputField.onValidateInput -= OnValidateInput;
                inputField.onValueChanged.RemoveListener(OnValueChanged);
            }

            if (TryGetComponent<TMP_InputField>(out var tmpInputField))
            {
                _eventSourceName = tmpInputField.GetType().Name;

                // same as InputField
                tmpInputField.onEndEdit.RemoveListener(OnEndEdit);
                tmpInputField.onSubmit.RemoveListener(OnSubmit);
                tmpInputField.onValidateInput -= OnValidateInput;
                tmpInputField.onValueChanged.RemoveListener(OnValueChanged);

                // additional events for TMP_InputField
                tmpInputField.onSelect.RemoveListener(OnSelect);
                tmpInputField.onDeselect.RemoveListener(OnDeselect);
                tmpInputField.onTouchScreenKeyboardStatusChanged.RemoveListener(OnTouchScreenKeyboardStatusChanged);
            }
        }

        private void OnEndEdit(string text)
        {
            if (_toggleInputField.isOn)
            {
                Popup(gameObject.transform.position, "end edit");
            }
        }

        private void OnSubmit(string text)
        {
            if (_toggleInputField.isOn)
            {
                Popup(gameObject.transform.position, $"submit ({_eventSourceName})");
            }
        }

        private char OnValidateInput(string text, int charIndex, char addedChar)
        {
            if (_toggleInputField.isOn)
            {
                Popup(gameObject.transform.position, "validate input");
            }

            return addedChar;
        }

        private void OnValueChanged(string text)
        {
            if (_toggleInputField.isOn)
            {
                Popup(gameObject.transform.position, "value changed");
            }
        }

        private void OnSelect(string text)
        {
            if (_toggleInputField.isOn)
            {
                Popup(gameObject.transform.position, $"select ({_eventSourceName})");
            }
        }

        private void OnDeselect(string text)
        {
            if (_toggleInputField.isOn)
            {
                Popup(gameObject.transform.position, $"deselect ({_eventSourceName})");
            }
        }

        private void OnTouchScreenKeyboardStatusChanged(TouchScreenKeyboard.Status status)
        {
            if (_toggleInputField.isOn)
            {
                Popup(gameObject.transform.position, $"touch screen keyboard status changed: {status.ToString()}");
            }
        }
    }
}
