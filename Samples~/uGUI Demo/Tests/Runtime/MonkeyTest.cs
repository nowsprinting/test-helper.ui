// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.GameObjectMatchers;
using TestHelper.UI.Operators;
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
        public async Task ClickRunMonkeyTestButton()
        {
            var button = await _finder.FindByNameAsync("RunMonkeyTest");
            var clickOperator = new UguiClickOperator();
            Assume.That(clickOperator.CanOperate(button.GameObject), Is.True);

            var monkeyTestButton = button.GameObject.GetComponent<MonkeyTestButton>();
            var lifetimeSeconds = monkeyTestButton.LifetimeSeconds;

            await clickOperator.OperateAsync(button.GameObject);
            await Task.Delay(TimeSpan.FromSeconds(lifetimeSeconds)); // wait for monkey test to finish
            await Task.Delay(1000);                                  // wait for show popup
        }
    }
}
