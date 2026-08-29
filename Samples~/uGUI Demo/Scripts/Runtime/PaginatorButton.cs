// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Extensions;
using TestHelper.UI.Paginators;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace -- namespace mirrors the package's own scheme, not the Assets/Samples/<name>/<version> import path Unity generates locally
namespace TestHelper.UI.Samples.UguiDemo
{
    [RequireComponent(typeof(Button))]
    public class PaginatorButton : MonoBehaviour
    {
        [field: SerializeField]
        public GameObject ScrollContentIncludesOperationTargets { get; set; }

        private readonly List<GameObject> _targets = new List<GameObject>();
        private int _nextIndex;

        private TabContent _content;
        private Button _button;

        private readonly GameObjectFinder _finder = new GameObjectFinder(0.2f);
        private readonly List<Button> _buttonsBuffer = new List<Button>();
        private UguiScrollRectPaginator _paginator;
        private GameObject _popupPrefab;

        private void Awake()
        {
            var scrollRect = ScrollContentIncludesOperationTargets.GetComponentInParent<ScrollRect>();
            _paginator = new UguiScrollRectPaginator(scrollRect);

            _popupPrefab = Resources.Load<GameObject>("TestHelper.UI.Samples.UguiDemo/EventPopup");

            _content = GetComponentInParent<TabContent>();

            _button = GetComponent<Button>();
            _button.onClick.AddListener(() =>
            {
                DoFindAsync().Forget();
            });

            if (ScrollContentIncludesOperationTargets)
            {
                _targets.Clear();
                _buttonsBuffer.Clear();
                ScrollContentIncludesOperationTargets.GetComponentsInChildren(_buttonsBuffer);
                foreach (var current in _buttonsBuffer)
                {
                    _targets.Add(current.gameObject);
                }
            }

            LotteryNextTarget();
        }

        private void LotteryNextTarget()
        {
            _nextIndex = UnityEngine.Random.Range(0, _targets.Count);
            var nextName = _targets[_nextIndex].name;
            _button.GetComponentInChildren<Text>().text = $"Find \"{nextName}\"";
        }

        private async UniTask DoFindAsync()
        {
            if (_targets.Count == 0)
            {
                Debug.LogError("Targets is not assigned");
                return;
            }

            var target = _targets[_nextIndex];

            try
            {
                _button.interactable = false;

                var path = target.transform.GetPath();
                await _finder.FindByPathAsync(path, paginator: _paginator);
                Popup(target.transform.position, "found");
            }
            catch (TimeoutException e)
            {
                Debug.Log(e);
                Popup(target.transform.position, GetReason(e.Message));
            }
            finally
            {
                LotteryNextTarget();
                _button.interactable = true;
            }
        }

        private void Popup(Vector2 position, string eventName)
        {
            var popup = Instantiate(_popupPrefab, _content.transform);
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
