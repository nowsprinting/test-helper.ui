// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.Operators;
using TestHelper.UI.ScreenshotFilenameStrategies;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TestHelper.UI.Samples.UguiDemo
{
    [TestFixture]
    public class MonkeyTest
    {
        private const string ScenePath = "../../Scenes/uGUIDemo.unity";

        [SetUp]
        public void SetUp()
        {
            SetControlsInteractable(false);
        }

        [TearDown]
        public void TearDown()
        {
            SetControlsInteractable(true);
        }

        private static void SetControlsInteractable(bool interactable)
        {
            // run monkey button 
            var monkeyTestsButton = Object.FindAnyObjectByType<MonkeyTestsButton>();
            monkeyTestsButton.GetComponent<Button>().interactable = interactable;

            // settings
            foreach (var toggle in GameObject.Find("SettingsPane").GetComponentsInChildren<Toggle>())
            {
                toggle.interactable = interactable;
            }

            // controls in content pane
            foreach (var content in GameObject.FindObjectsByType<TabContent>(FindObjectsSortMode.None))
            {
                content.SetControlsInteractable(interactable);
            }
        }

        [Test]
        [LoadScene(ScenePath)]
        public async Task RunMonkeyTests()
        {
            var config = new MonkeyConfig()
            {
                Lifetime = TimeSpan.FromSeconds(10),
                Screenshots = new ScreenshotOptions()
                {
                    FilenameStrategy = new CounterBasedStrategy("UguiDemoTest"),
                },
                Operators = new IOperator[]
                {
                    new UguiClickOperator(),
                    new UguiDoubleClickOperator(),
                    new UguiClickAndHoldOperator(),
                    new UguiDragAndDropOperator(),
                    new UguiSwipeOperator(),
                    new UguiScrollWheelOperator(),
                    new UguiTextInputOperator(),
                }
            };

            await Monkey.Run(config);
        }
    }
}
