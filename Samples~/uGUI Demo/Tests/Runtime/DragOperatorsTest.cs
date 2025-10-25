// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.GameObjectMatchers;
using TestHelper.UI.Operators;
using UnityEngine.UI;

namespace TestHelper.UI.Samples.UguiDemo
{
    [TestFixture]
    public class DragOperatorsTest
    {
        private const string ScenePath = "../../Scenes/uGUIDemo.unity";
        private readonly GameObjectFinder _finder = new GameObjectFinder();

        [SetUp]
        public async Task SetUp()
        {
            var matcher = new ComponentMatcher(componentType: typeof(Dropdown), name: "TabSwitcher");
            var dropdown = await _finder.FindByMatcherAsync(matcher);
            dropdown.GameObject.GetComponent<Dropdown>().value = 2; // DragDemo
        }

        [Test]
        [LoadScene(ScenePath)]
        public async Task ClickDragButton()
        {
            var button = await _finder.FindByNameAsync("DragButton");
            var clickOperator = new UguiClickOperator();
            Assume.That(clickOperator.CanOperate(button.GameObject), Is.True);

            await clickOperator.OperateAsync(button.GameObject);
            await Task.Delay(400 + 1000); // wait for drag and show popup
        }

        [Test]
        [LoadScene(ScenePath)]
        public async Task ClickFlickButton()
        {
            var button = await _finder.FindByNameAsync("FlickButton");
            var clickOperator = new UguiClickOperator();
            Assume.That(clickOperator.CanOperate(button.GameObject), Is.True);

            await clickOperator.OperateAsync(button.GameObject);
            await Task.Delay(400 + 1000); // wait for drag and show popup
        }

        [Test]
        [LoadScene(ScenePath)]
        public async Task ClickSwipeButton()
        {
            var button = await _finder.FindByNameAsync("SwipeButton");
            var clickOperator = new UguiClickOperator();
            Assume.That(clickOperator.CanOperate(button.GameObject), Is.True);

            await clickOperator.OperateAsync(button.GameObject);
            await Task.Delay(400 + 1000); // wait for swipe and show popup
        }
    }
}
