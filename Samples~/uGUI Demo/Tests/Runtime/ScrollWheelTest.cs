// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.GameObjectMatchers;
using TestHelper.UI.Operators;

namespace TestHelper.UI.Samples.UguiDemo
{
    [TestFixture]
    public class ScrollWheelTest
    {
        private const string ScenePath = "../../Scenes/uGUIDemo.unity";

        private readonly GameObjectFinder _finder = new GameObjectFinder();

        [SetUp]
        public async Task SetUp()
        {
            var button = await _finder.FindByMatcherAsync(new ButtonMatcher(text: "ScrollWheel Operator"));
            var clickOperator = new UguiClickOperator();
            Assume.That(clickOperator.CanOperate(button.GameObject), Is.True);

            await clickOperator.OperateAsync(button.GameObject);
        }

        [Test]
        [LoadScene(ScenePath)]
        public async Task ScrollWheel()
        {
            var image = await _finder.FindByNameAsync("Scroll View");
            var scrollOperator = new UguiScrollWheelOperator();
            Assume.That(scrollOperator.CanOperate(image.GameObject), Is.True);

            await scrollOperator.OperateAsync(image.GameObject);
        }
    }
}
