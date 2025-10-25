// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.GameObjectMatchers;
using TestHelper.UI.Operators;
using TestHelper.UI.ScreenshotFilenameStrategies;
using TestHelper.UI.Visualizers;
using UnityEngine.UI;

namespace TestHelper.UI.Samples.UguiDemo
{
    [TestFixture]
    public class MonkeyTest
    {
        private const string ScenePath = "../../Scenes/uGUIDemo.unity";
        private readonly GameObjectFinder _finder = new GameObjectFinder();

        [SetUp]
        public async Task SetUp()
        {
            var matcher = new ComponentMatcher(componentType: typeof(Dropdown), name: "TabSwitcher");
            var dropdown = await _finder.FindByMatcherAsync(matcher);
            dropdown.GameObject.GetComponent<Dropdown>().value = 0; // FinderDemo
        }

        [Test]
        [LoadScene(ScenePath)]
        public async Task MonkeyTesting()
        {
            var config = new MonkeyConfig()
            {
                Lifetime = TimeSpan.FromSeconds(10),
                Visualizer = new DefaultDebugVisualizer(),
                Screenshots = new ScreenshotOptions()
                {
                    FilenameStrategy = new CounterBasedStrategy("UguiDemoTest"),
                },
                Operators = new IOperator[]
                {
                    new UguiClickAndHoldOperator(),
                    new UguiClickOperator(),
                    new UguiDoubleClickOperator(),
                    new UguiDragAndDropOperator(),
                    new UguiScrollWheelOperator(),
                    new UguiSwipeOperator(),
                    new UguiTextInputOperator(),
                }
            };

            await Monkey.Run(config);
        }
    }
}
