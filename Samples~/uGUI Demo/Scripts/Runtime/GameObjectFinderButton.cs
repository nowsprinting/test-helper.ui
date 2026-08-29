// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Extensions;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace -- namespace mirrors the package's own scheme, not the Assets/Samples/<name>/<version> import path Unity generates locally
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
                DoFindAsync().Forget();
            });
        }

        private async UniTask DoFindAsync()
        {
            if (OperationTargets.Count == 0)
            {
                Debug.LogError("FindTargets is not assigned");
                return;
            }

            try
            {
                _button.interactable = false;
                await UniTask.WhenAll(OperationTargets.Select(FindByPathAsync));
            }
            finally
            {
                _button.interactable = true;
            }
        }

        private async UniTask FindByPathAsync(GameObject target)
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

        private static readonly Regex s_reasonRegex = new Regex(".*is found, but (.+)\\.$");

        private static string GetReason(string message)
        {
            return s_reasonRegex.Match(message).Groups[1].Value;
        }
    }
}
