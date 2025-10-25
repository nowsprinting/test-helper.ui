// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.UI.Samples.UguiDemo
{
    public abstract class AbstractSpyEventHandler : MonoBehaviour,
        IUpdateSelectedHandler,
        ISelectHandler,
        IDeselectHandler,
        IMoveHandler,
        ISubmitHandler,
        ICancelHandler
    {
        private GameObject _popupPrefab;
        private TabContent _content;

        protected Toggle _togglePointerEnter;
        protected Toggle _togglePointerDown;
        protected Toggle _togglePointerClick;
        protected Toggle _toggleDrag;
        protected Toggle _toggleScroll;
        protected Toggle _toggleInputField;
        private Toggle _toggleKeyboard;
        private Toggle _toggleSelect;
        private Toggle _toggleUpdateSelected;

        private void Awake()
        {
            _popupPrefab = Resources.Load<GameObject>("TestHelper.UI.Samples.UguiDemo/EventPopup");
            _content = GetComponentInParent<TabContent>();

            foreach (var toggle in FindObjectsOfType<Toggle>())
            {
                switch (toggle.name)
                {
                    case "TogglePointerEnter":
                        _togglePointerEnter = toggle;
                        break;
                    case "TogglePointerDown":
                        _togglePointerDown = toggle;
                        break;
                    case "TogglePointerClick":
                        _togglePointerClick = toggle;
                        break;
                    case "ToggleDrag":
                        _toggleDrag = toggle;
                        break;
                    case "ToggleScroll":
                        _toggleScroll = toggle;
                        break;
                    case "ToggleInputField":
                        _toggleInputField = toggle;
                        break;
                    case "ToggleKeyboard":
                        _toggleKeyboard = toggle;
                        break;
                    case "ToggleSelect":
                        _toggleSelect = toggle;
                        break;
                    case "ToggleUpdateSelected":
                        _toggleUpdateSelected = toggle;
                        break;
                }
            }
        }

        protected void Popup(Vector2 position, string eventName)
        {
            var popup = Instantiate(_popupPrefab, _content.transform);
            popup.name = eventName;
            popup.transform.position = position;

            if (TryGetComponent<Image>(out var image))
            {
                var eventPopup = popup.GetComponent<EventPopup>();
                eventPopup.ParentImage = image;
            }
        }

        public void OnUpdateSelected(BaseEventData eventData)
        {
            if (_toggleUpdateSelected.isOn)
            {
                Popup(eventData.selectedObject.transform.position, "update selected");
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (_toggleSelect.isOn && eventData.selectedObject)
            {
                Popup(eventData.selectedObject.transform.position, "select");
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (_toggleSelect.isOn)
            {
                Popup(eventData.selectedObject.transform.position, "deselect");
            }
        }

        public void OnMove(AxisEventData eventData)
        {
            if (_toggleKeyboard.isOn)
            {
                Popup(eventData.selectedObject.transform.position, "move");
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (_toggleKeyboard.isOn)
            {
                Popup(eventData.selectedObject.transform.position, "submit");
            }
        }

        public void OnCancel(BaseEventData eventData)
        {
            if (_toggleKeyboard.isOn)
            {
                Popup(eventData.selectedObject.transform.position, "cancel");
            }
        }
    }
}
