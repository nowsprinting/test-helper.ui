// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.GameObjectMatchers;
using TestHelper.UI.Operators;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace -- namespace mirrors the package's own scheme, not the Assets/Samples/<name>/<version> import path Unity generates locally
namespace TestHelper.UI.Samples.UguiDemo
{
    [TestFixture]
    public class ClickOperatorsTest
    {
        private const string ScenePath = "../../Scenes/uGUIDemo.unity";
        private readonly GameObjectFinder _finder = new GameObjectFinder();

        [SetUp]
        public async Task SetUp()
        {
            var matcher = new ComponentMatcher(componentType: typeof(Dropdown), name: "TabSwitcher");
            var dropdown = await _finder.FindByMatcherAsync(matcher);
            dropdown.GameObject.GetComponent<Dropdown>().value = 1; // ClickDemo
        }

        [Test]
        [LoadScene(ScenePath)]
        public async Task ClickClickButton()
        {
            var button = await _finder.FindByNameAsync("ClickButton");
            var clickOperator = new UguiClickOperator();
            Assume.That(clickOperator.CanOperate(button.GameObject), Is.True);

            await clickOperator.OperateAsync(button.GameObject);
            await Task.Delay(1000); // wait for show popup
        }

        [Test]
        [LoadScene(ScenePath)]
        public async Task ClickDoubleClickButton()
        {
            var button = await _finder.FindByNameAsync("DoubleClickButton");
            var clickOperator = new UguiDoubleClickOperator();
            Assume.That(clickOperator.CanOperate(button.GameObject), Is.True);

            await clickOperator.OperateAsync(button.GameObject);
            await Task.Delay(1000); // wait for show popup
        }

        [Test]
        [LoadScene(ScenePath)]
        public async Task ClickClickAndHoldButton()
        {
            var button = await _finder.FindByNameAsync("ClickAndHoldButton");
            var clickOperator = new UguiClickAndHoldOperator();
            Assume.That(clickOperator.CanOperate(button.GameObject), Is.True);

            await clickOperator.OperateAsync(button.GameObject);
            await Task.Delay(1000 + 1000); // wait for hold and show popup
        }
    }
}
