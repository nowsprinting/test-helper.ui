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
        public async Task TextInput()
        {
            var inputField = await _finder.FindByNameAsync("InputField (Legacy)");
            var inputOperator = new UguiTextInputOperator();
            Assume.That(inputOperator.CanOperate(inputField.GameObject), Is.True);

            await inputOperator.OperateAsync(inputField.GameObject);
        }
    }
}
