// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.UI.Samples.UguiDemo
{
    [RequireComponent(typeof(Button))]
    public class GameObjectFinderButton : MonoBehaviour
    {
        [field: SerializeField]
        public List<GameObject> OperationTargets { get; set; }

        private Button _button;

        private readonly GameObjectFinder _finder = new GameObjectFinder(0.2f);
        private GameObject _popupPrefab;

        private void Awake()
        {
            _popupPrefab = Resources.Load<GameObject>("TestHelper.UI.Samples.UguiDemo/EventPopup");

            _button = GetComponent<Button>();
            _button.onClick.AddListener(() =>
            {
                DoFind().Forget();
            });
        }

        private async UniTask DoFind()
        {
            if (OperationTargets.Count == 0)
            {
                Debug.LogError("FindTargets is not assigned");
                return;
            }

            try
            {
                _button.interactable = false;

                foreach (var target in OperationTargets)
                {
                    await FindByPath(target);
                }
            }
            finally
            {
                _button.interactable = true;
            }
        }

        private async UniTask FindByPath(GameObject target)
        {
            try
            {
                var path = target.transform.GetPath();
                await _finder.FindByPathAsync(path, reachable: true, interactable: true);
                Popup(target, target.transform.position, "found");
            }
            catch (TimeoutException e)
            {
                Debug.Log(e);
                Popup(target, target.transform.position, GetReason(e.Message));
            }
        }

        private void Popup(GameObject target, Vector2 position, string eventName)
        {
            var popup = Instantiate(_popupPrefab, target.transform.parent);
            popup.name = eventName;
            popup.transform.position = position;
        }

        private static string GetReason(string message)
        {
            var regex = new Regex(".*is found, but (.+)\\.$");
            return regex.Match(message).Groups[1].Value;
        }
    }
}
