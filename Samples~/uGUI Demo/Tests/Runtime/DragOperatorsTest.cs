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
        public async Task DragAndDrop()
        {
            var image = await _finder.FindByNameAsync("DraggableImage");
            var dragOperator = new UguiDragAndDropOperator();
            Assume.That(dragOperator.CanOperate(image.GameObject), Is.True);

            await dragOperator.OperateAsync(image.GameObject);
        }

        [Test]
        [LoadScene(ScenePath)]
        public async Task Swipe()
        {
            var image = await _finder.FindByNameAsync("DraggableImage");
            var dragOperator = new UguiSwipeOperator();
            Assume.That(dragOperator.CanOperate(image.GameObject), Is.True);

            await dragOperator.OperateAsync(image.GameObject);
        }
    }
}
