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
    public class TextInputOperatorTest
    {
        private const string ScenePath = "../../Scenes/uGUIDemo.unity";
        private readonly GameObjectFinder _finder = new GameObjectFinder();

        [SetUp]
        public async Task SetUp()
        {
            var matcher = new ComponentMatcher(componentType: typeof(Dropdown), name: "TabSwitcher");
            var dropdown = await _finder.FindByMatcherAsync(matcher);
            dropdown.GameObject.GetComponent<Dropdown>().value = 4; // TextInputDemo
        }

        [Test]
        [LoadScene(ScenePath)]
        public async Task ClickTextInputToLegacyButton()
        {
            var button = await _finder.FindByNameAsync("TextInputToLegacyButton");
            var clickOperator = new UguiClickOperator();
            Assume.That(clickOperator.CanOperate(button.GameObject), Is.True);

            await clickOperator.OperateAsync(button.GameObject);
            await Task.Delay(200); // wait for input text
        }

        [Test]
        [LoadScene(ScenePath)]
        public async Task ClickTextInputToTmpButton()
        {
            var button = await _finder.FindByNameAsync("TextInputToTmpButton");
            var clickOperator = new UguiClickOperator();
            Assume.That(clickOperator.CanOperate(button.GameObject), Is.True);

            await clickOperator.OperateAsync(button.GameObject);
            await Task.Delay(200); // wait for input text
        }
    }
}
