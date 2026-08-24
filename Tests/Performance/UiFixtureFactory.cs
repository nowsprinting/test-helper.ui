// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.UI.Performance
{
    // Fixtures are built in code instead of scene files so that object count and hierarchy depth are tunable
    // without maintaining large scene assets.
    internal static class UiFixtureFactory
    {
        public static void CreateNestedButtons(int count, int nestDepth)
        {
            var canvas = CreateCanvas();
            for (var i = 0; i < count; i++)
            {
                var parent = canvas.transform;
                for (var depth = 0; depth < nestDepth; depth++)
                {
                    var nest = new GameObject("Nest");
                    nest.transform.SetParent(parent, false);
                    parent = nest.transform;
                }

                CreateButton(parent, $"Button_{i}", withImage: false);
            }
        }

        public static void CreateGridButtons(int count)
        {
            const int Columns = 5;
            const float CellWidth = 110f;
            const float CellHeight = 40f;

            var canvas = CreateCanvas();
            var rows = (count + Columns - 1) / Columns;
            for (var i = 0; i < count; i++)
            {
                var column = i % Columns;
                var row = i / Columns;
                var button = CreateButton(canvas.transform, $"Button_{i}", withImage: true);
                var rectTransform = (RectTransform)button.transform;
                rectTransform.sizeDelta = new Vector2(100f, 30f);
                rectTransform.anchoredPosition = new Vector2(
                    (column - (Columns - 1) * 0.5f) * CellWidth,
                    (row - (rows - 1) * 0.5f) * CellHeight);
            }
        }

        private static Canvas CreateCanvas()
        {
            var canvas = new GameObject("Canvas", typeof(Canvas), typeof(GraphicRaycaster)).GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _ = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            return canvas;
        }

        private static GameObject CreateButton(Transform parent, string name, bool withImage)
        {
            // An Image is attached only when the button must be hit by GraphicRaycaster (reachability check);
            // matcher-scan fixtures omit it to keep setup light. No Font is assigned to the Text because
            // nothing needs to render; matchers only read the text string.
            var button = withImage
                ? new GameObject(name, typeof(Image), typeof(Button))
                : new GameObject(name, typeof(RectTransform), typeof(Button));
            button.transform.SetParent(parent, false);

            var text = new GameObject("Text", typeof(Text));
            text.transform.SetParent(button.transform, false);
            text.GetComponent<Text>().text = name;

            return button;
        }
    }
}
