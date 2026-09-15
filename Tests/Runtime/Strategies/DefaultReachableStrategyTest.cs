// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.RuntimeInternals;
using TestHelper.UI.Annotations;
using TestHelper.UI.Extensions;
using TestHelper.UI.GameObjectMatchers;
using TestHelper.UI.TestDoubles;
using TestHelper.UI.TestUtils;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Constraints;
using UnityEngine.UI;
// UnityEngine.TestTools.Constraints is imported for the AllocatingGCMemory extension method, which brings a
// second `Is` into scope. Aliased to NUnit's so that the existing assertions in this file keep resolving to it.
using Is = NUnit.Framework.Is;

namespace TestHelper.UI.Strategies
{
    [TestFixture]
    public static class DefaultReachableStrategyTest
    {
        private static Transform CanvasTransform => GameObject.Find("Canvas").transform;

        private static GameObject CreateImage(string name, Transform parent, Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            gameObject.layer = LayerMask.NameToLayer("UI");
            gameObject.transform.SetParent(parent, false);
            var rectTransform = (RectTransform)gameObject.transform;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = sizeDelta;
            return gameObject;
        }

        // Mask parent 100x30 at (0,-150); the Image is required by Mask and kept non-raycastable so it never blocks.
        private static GameObject CreateMask(Type maskType)
        {
            var mask = CreateImage("Mask", CanvasTransform, new Vector2(0, -150), new Vector2(100, 30));
            mask.GetComponent<Image>().raycastTarget = false;
            mask.AddComponent(maskType);
            return mask;
        }

        private static async Task WaitForRaycasterReady()
        {
            Canvas.ForceUpdateCanvases();
            await UniTask.NextFrame();
        }

        [TestFixture(RenderMode.ScreenSpaceOverlay)]
        [TestFixture(RenderMode.ScreenSpaceCamera)]
        [TestFixture(RenderMode.WorldSpace)]
        public class UI
        {
            private const string TestScenePath = "../../Scenes/GameObjectFinderUI.unity";
            private readonly GameObjectFinder _finder = new GameObjectFinder(0.1d);
            private readonly RenderMode _canvasRenderMode;

            public UI(RenderMode canvasRenderMode)
            {
                _canvasRenderMode = canvasRenderMode;
            }

            [SetUp]
            public void SetUp()
            {
                var canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
                canvas.renderMode = _canvasRenderMode;
                if (_canvasRenderMode == RenderMode.WorldSpace)
                {
                    canvas.worldCamera = Camera.main;
                    canvas.transform.position = new Vector3(0, 0, 500);
                }
            }

            [TestCase("ActiveText")]
            [TestCase("Dialog")] // Child objects do not block raycast
            [LoadScene(TestScenePath)]
            public async Task IsReachable_TargetOnScreenAndUnblocked_Reachable(string target)
            {
                var result = await _finder.FindByNameAsync(target, reachable: false);
                Assert.That(new DefaultReachableStrategy().IsReachable(result.GameObject, out _), Is.True);
            }

            [TestCase("OutOfSight")]
            [TestCase("BehindTheWall")]
            [LoadScene(TestScenePath)]
            public async Task IsReachable_TargetOffScreenOrBlocked_NotReachable(string target)
            {
                var result = await _finder.FindByNameAsync(target, reachable: false);
                Assert.That(new DefaultReachableStrategy().IsReachable(result.GameObject, out _), Is.False);
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task IsReachable_BehindBlockerWithNonBlockingAnnotation_Reachable()
            {
                GameObject.Find("Wall").AddComponent<NonBlockingAnnotation>();

                var result = await _finder.FindByNameAsync("BehindTheWall", reachable: false);
                Assert.That(new DefaultReachableStrategy().IsReachable(result.GameObject, out _), Is.True);
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task IsReachable_BehindBlockerWithDisabledNonBlockingAnnotation_NotReachable()
            {
                var nonBlockingAnnotation = GameObject.Find("Wall").AddComponent<NonBlockingAnnotation>();
                nonBlockingAnnotation.enabled = false;

                var result = await _finder.FindByNameAsync("BehindTheWall", reachable: false);
                Assert.That(new DefaultReachableStrategy().IsReachable(result.GameObject, out _), Is.False);
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task IsReachable_BehindBlockerWithNonBlockingMatchers_Reachable()
            {
                var matchers = new List<IGameObjectMatcher>() { new NameMatcher("Wall") };
                var sut = new DefaultReachableStrategy(nonBlockingMatchers: matchers);

                var result = await _finder.FindByNameAsync("BehindTheWall", reachable: false);
                Assert.That(sut.IsReachable(result.GameObject, out _), Is.True);
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task IsReachable_WithNonBlockingAnnotation_Reachable()
            {
                const string Blocker = "Wall";
                GameObject.Find(Blocker).AddComponent<NonBlockingAnnotation>();

                var result = await _finder.FindByNameAsync(Blocker, reachable: false);
                Assert.That(new DefaultReachableStrategy().IsReachable(result.GameObject, out _), Is.True);
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task IsReachable_WithNonBlockingMatcher_Reachable()
            {
                const string Blocker = "Wall";
                var matchers = new List<IGameObjectMatcher>() { new NameMatcher(Blocker) };
                var sut = new DefaultReachableStrategy(nonBlockingMatchers: matchers);

                var result = await _finder.FindByNameAsync(Blocker, reachable: false);
                Assert.That(sut.IsReachable(result.GameObject, out _), Is.True);
            }

            [Test]
            [Category("Acceptance")]
            [LoadScene(TestScenePath)]
            public async Task IsReachable_PivotBlockedAndOtherAreaUnblocked_Reachable()
            {
                var target = (await _finder.FindByNameAsync("PartiallyBehindTheWall", reachable: false)).GameObject;
                var blocker = GameObject.Find("SmallWall");

                var actual = new DefaultReachableStrategy().IsReachable(target, out var raycastResult);

                Assert.That(actual, Is.True, "reachable");
                Assert.That(raycastResult.gameObject.transform.IsChildOf(target.transform), Is.True, "hit target");
                Assert.That(ScreenRectTestHelper.GetScreenRect(target).Contains(raycastResult.screenPosition),
                    Is.True, "inside target");
                Assert.That(ScreenRectTestHelper.GetScreenRect(blocker).Contains(raycastResult.screenPosition),
                    Is.False, "outside blocker");
            }

            [Test]
            [Category("Acceptance")]
            [LoadScene(TestScenePath)]
            public async Task IsReachable_PivotUnblockedAndCenterBlocked_ReachableAtPivot()
            {
                var target = (await _finder.FindByNameAsync("ActiveText", reachable: false)).GameObject;
                target.AddComponent<ScreenOffsetAnnotation>().offset = new Vector2(-50, 0); // first point off-center
                CreateImage("CenterBlocker", CanvasTransform, new Vector2(0, 100), new Vector2(40, 40));
                await WaitForRaycasterReady();

                var actual = new DefaultReachableStrategy().IsReachable(target, out var raycastResult);

                Assert.That(actual, Is.True, "reachable");
                Assert.That(raycastResult.screenPosition, Is.EqualTo(DefaultScreenPointStrategy.GetScreenPoint(target)),
                    "operated at first point");
            }

            [Test]
            [Category("Acceptance")]
            [LoadScene(TestScenePath)]
            public async Task IsReachable_FullyBlocked_RaycastResultIsFirstMiss()
            {
                var target = (await _finder.FindByNameAsync("BehindTheWall", reachable: false)).GameObject;

                var actual = new DefaultReachableStrategy().IsReachable(target, out var raycastResult);

                Assert.That(actual, Is.False, "not reachable");
                Assert.That(raycastResult.gameObject.name, Is.EqualTo("ChildOfWall"), "topmost blocker at pivot");
                Assert.That(raycastResult.screenPosition, Is.EqualTo(DefaultScreenPointStrategy.GetScreenPoint(target)),
                    "pivot position");
            }

            // Mask covers x -50..50 (canvas units around (0,-150)); target x -50..110; blocker x 0..40.
            // The unclipped remainder is the right strip (40..110, outside the mask); clipping leaves the left strip.
            [TestCase(typeof(RectMask2D))]
            [TestCase(typeof(Mask))]
            [Category("Acceptance")]
            [LoadScene(TestScenePath)]
            public async Task IsReachable_PivotBlockedAndLargestUnblockedAreaOutsideMask_ReachableInsideMask(
                Type maskType)
            {
                var mask = CreateMask(maskType);
                var target = CreateImage("Target", mask.transform, new Vector2(30, 0), new Vector2(160, 30));
                CreateImage("Blocker", CanvasTransform, new Vector2(20, -150), new Vector2(40, 40));
                await WaitForRaycasterReady();

                var actual = new DefaultReachableStrategy().IsReachable(target, out var raycastResult);

                Assert.That(actual, Is.True, "reachable");
                Assert.That(ScreenRectTestHelper.GetScreenRect(mask).Contains(raycastResult.screenPosition),
                    Is.True, "inside mask");
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task IsReachable_PivotBlockedAndLargestUnblockedAreaOutsideDisabledMask_ReachableOutsideMask()
            {
                var mask = CreateMask(typeof(RectMask2D));
                mask.GetComponent<RectMask2D>().enabled = false;
                var target = CreateImage("Target", mask.transform, new Vector2(30, 0), new Vector2(160, 30));
                CreateImage("Blocker", CanvasTransform, new Vector2(20, -150), new Vector2(40, 40));
                await WaitForRaycasterReady();

                var actual = new DefaultReachableStrategy().IsReachable(target, out var raycastResult);

                Assert.That(actual, Is.True, "reachable");
                Assert.That(ScreenRectTestHelper.GetScreenRect(mask).Contains(raycastResult.screenPosition),
                    Is.False, "outside disabled mask");
            }

            [Test]
            [Category("Acceptance")]
            [LoadScene(TestScenePath)]
            public async Task IsReachable_TargetFullyClippedByMask_NotReachable()
            {
                var mask = CreateMask(typeof(RectMask2D));
                var target = CreateImage("Target", mask.transform, new Vector2(200, 0), new Vector2(160, 30));
                await WaitForRaycasterReady();

                var actual = new DefaultReachableStrategy().IsReachable(target, out _);

                Assert.That(actual, Is.False);
            }

            // Target x 0..160 with pivot at 80, outside the mask (x -50..50); visible part is x 0..50.
            [TestCase(typeof(RectMask2D))]
            [TestCase(typeof(Mask))]
            [Category("Acceptance")]
            [LoadScene(TestScenePath)]
            public async Task IsReachable_PivotOutsideMaskAndVisibleAreaUnblocked_Reachable(Type maskType)
            {
                var mask = CreateMask(maskType);
                var target = CreateImage("Target", mask.transform, new Vector2(80, 0), new Vector2(160, 30));
                await WaitForRaycasterReady();

                var actual = new DefaultReachableStrategy().IsReachable(target, out var raycastResult);

                Assert.That(actual, Is.True, "reachable");
                Assert.That(ScreenRectTestHelper.GetScreenRect(target).Contains(raycastResult.screenPosition),
                    Is.True, "inside target");
                Assert.That(ScreenRectTestHelper.GetScreenRect(mask).Contains(raycastResult.screenPosition),
                    Is.True, "inside mask");
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task IsReachable_PivotOffScreenAndVisibleAreaUnblocked_Reachable()
            {
                var canvasRect = (RectTransform)CanvasTransform;
                var target = CreateImage("Target", canvasRect, Vector2.zero, new Vector2(160, 30));
                // Place the pivot 40 canvas units beyond the left screen edge; the edge is derived from the
                // camera because the visible canvas width differs between render modes.
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect,
                    new Vector2(0, Screen.height / 2f), target.GetAssociatedCamera(), out var leftEdge);
                ((RectTransform)target.transform).anchoredPosition =
                    new Vector2(leftEdge.x - 40f - canvasRect.rect.center.x, -200f);
                await WaitForRaycasterReady();

                var actual = new DefaultReachableStrategy().IsReachable(target, out var raycastResult);

                Assert.That(actual, Is.True, "reachable");
                Assert.That(new Rect(0, 0, Screen.width, Screen.height).Contains(raycastResult.screenPosition),
                    Is.True, "inside screen");
                Assert.That(ScreenRectTestHelper.GetScreenRect(target).Contains(raycastResult.screenPosition),
                    Is.True, "inside target");
            }
        }

        [TestFixture("2D")]
        [TestFixture("3D")]
        [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
        public class Object
        {
            private readonly GameObjectFinder _finder = new GameObjectFinder(0.1d);
            private readonly string _testScenePath;

            public Object(string dimension)
            {
                _testScenePath = $"../../Scenes/GameObjectFinder{dimension}.unity";
            }

            [SetUp]
            public async Task SetUp()
            {
                await SceneManagerHelper.LoadSceneAsync(_testScenePath);
            }

            [TestCase("NotInteractable")]
            public async Task IsReachable_TargetOnScreenAndUnblocked_Reachable(string target)
            {
                var result = await _finder.FindByNameAsync(target, reachable: false);
                Assert.That(new DefaultReachableStrategy().IsReachable(result.GameObject, out _), Is.True);
            }

            [TestCase("OutOfSight")]
            [TestCase("BehindTheWall")]
            public async Task IsReachable_TargetOffScreenOrBlocked_NotReachable(string target)
            {
                var result = await _finder.FindByNameAsync(target, reachable: false);
                Assert.That(new DefaultReachableStrategy().IsReachable(result.GameObject, out _), Is.False);
            }
        }

        [TestFixture]
        public class NoEventSystem
        {
            private readonly GameObjectFinder _finder = new GameObjectFinder(0.1d);

            [Test]
            [CreateScene(camera: true, unloadOthers: true)]
            public async Task IsReachable_NoEventSystem_ReturnsFalseAndLogWarning()
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = "Cube";
                cube.transform.position = new Vector3(0, 0, 0);

                var result = await _finder.FindByNameAsync("Cube", reachable: false);
                Assert.That(new DefaultReachableStrategy().IsReachable(result.GameObject, out _), Is.False);
                LogAssert.Expect(LogType.Error, "EventSystem is not found.");
            }
        }

        [TestFixture]
        public class Verbose
        {
            private const string TestScenePath = "../../Scenes/GameObjectFinderUI.unity";
            private readonly GameObjectFinder _finder = new GameObjectFinder(0.1d);

            [TestCase("ActiveText")]
            [TestCase("Dialog")] // Child objects do not block raycast
            [LoadScene(TestScenePath)]
            public async Task IsReachableWithVerbose_Reachable_NotOutputLog(string target)
            {
                var result = await _finder.FindByNameAsync(target, reachable: false);
                var spyLogger = new SpyLogger();
                var sut = new DefaultReachableStrategy(verboseLogger: spyLogger);
                var actual = sut.IsReachable(result.GameObject, out _);
                Assume.That(actual, Is.True);

                Assert.That(spyLogger.Messages, Is.Empty);
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task IsReachableWithVerbose_OutOfSight_LogVerboseNotHit()
            {
                var result = await _finder.FindByNameAsync("OutOfSight", reachable: false);
                var spyLogger = new SpyLogger();
                var sut = new DefaultReachableStrategy(verboseLogger: spyLogger);
                var actual = sut.IsReachable(result.GameObject, out _);
                Assume.That(actual, Is.False);

                Assert.That(spyLogger.Messages, Has.Count.EqualTo(1));
                Assert.That(spyLogger.Messages[0], Does.Match(
                    @"Not reachable to OutOfSight\([^)]+\), position=\(\d+,\d+\)\. Raycast is not hit\."));
                // Note: No camera when ScreenSpaceOverlay canvas.
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task IsReachableWithVerbose_BehindOtherObject_LogHitOtherObject()
            {
                var result = await _finder.FindByNameAsync("BehindTheWall", reachable: false);
                var spyLogger = new SpyLogger();
                var sut = new DefaultReachableStrategy(verboseLogger: spyLogger);
                var actual = sut.IsReachable(result.GameObject, out _);
                Assume.That(actual, Is.False);

                Assert.That(spyLogger.Messages, Has.Count.EqualTo(1));
                Assert.That(spyLogger.Messages[0], Does.Match(
                    @"Not reachable to BehindTheWall\([^)]+\), position=\(\d+,\d+\)\. Raycast hit other objects: \[ChildOfWall\([^)]+\), Wall\([^)]+\), BehindTheWall\([^)]+\)\]"));
                // Note: No camera when ScreenSpaceOverlay canvas.
            }

            [Test]
            [LoadScene(TestScenePath)]
            public async Task IsReachableWithVerbose_WorldSpace_LogVerboseWithCamera()
            {
                var canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = Camera.main;

                var result = await _finder.FindByNameAsync("OutOfSight", reachable: false);
                var spyLogger = new SpyLogger();
                var sut = new DefaultReachableStrategy(verboseLogger: spyLogger);
                var actual = sut.IsReachable(result.GameObject, out _);
                Assume.That(actual, Is.False);

                Assert.That(spyLogger.Messages, Has.Count.EqualTo(1));
                Assert.That(spyLogger.Messages[0], Does.Match(
                    @"Not reachable to OutOfSight\([^)]+\), position=\(\d+,\d+\), camera=Main Camera\([^)]+\)\. Raycast is not hit\."));
            }

            [Test]
            [Category("Acceptance")]
            [LoadScene(TestScenePath)]
            public async Task IsReachableWithVerbose_ReachableAfterFallback_LogHitOtherObject()
            {
                var result = await _finder.FindByNameAsync("PartiallyBehindTheWall", reachable: false);
                var spyLogger = new SpyLogger();
                var sut = new DefaultReachableStrategy(verboseLogger: spyLogger);

                var actual = sut.IsReachable(result.GameObject, out var raycastResult);

                Assert.That(actual, Is.True, "reachable");
                Assert.That(raycastResult.screenPosition, Is.EqualTo(new Vector2(70, 140)), "left strip center");
                Assert.That(spyLogger.Messages, Has.Count.EqualTo(1), "message count");
                Assert.That(spyLogger.Messages[0], Does.Match(
                        @"Not reachable to PartiallyBehindTheWall\([^)]+\), position=\(120,140\)\. Raycast hit other objects: \[SmallWall\([^)]+\), PartiallyBehindTheWall\([^)]+\)\]"),
                    "message");
            }

            // Target x 240..400 (screen). Blocker1 x 300..340 -> left strip 240..300, center 270.
            // Blocker2 x 260..300 covers 270 -> left strip 240..260, center 250 hits the target.
            [Test]
            [LoadScene(TestScenePath)]
            public async Task IsReachableWithVerbose_ReachableAfterTwoBlockers_LogTwoMessages()
            {
                var target = CreateImage("Target", CanvasTransform, new Vector2(0, -150), new Vector2(160, 30));
                CreateImage("Blocker1", CanvasTransform, new Vector2(0, -150), new Vector2(40, 40));
                CreateImage("Blocker2", CanvasTransform, new Vector2(-40, -150), new Vector2(40, 40));
                await WaitForRaycasterReady();
                var spyLogger = new SpyLogger();
                var sut = new DefaultReachableStrategy(verboseLogger: spyLogger);

                var actual = sut.IsReachable(target, out _);

                Assert.That(actual, Is.True, "reachable");
                Assert.That(spyLogger.Messages, Has.Count.EqualTo(2), "message count");
                Assert.That(spyLogger.Messages[1], Does.Contain("Raycast hit other objects: [Blocker2("),
                    "second message");
            }

            // Target x 160..480 (screen), pivot 320. Each blocker covers exactly one raycast point and none of the
            // others; strip areas never tie so the chain is deterministic:
            //   #1 320 -> Blocker1 305..325 -> right 325..480 (155 > 145), center 402.5
            //   #2 402.5 -> Blocker2 395..415 -> left 325..395 (70 > 65), center 360
            //   #3 360 -> Blocker3 352..372 -> left 325..352 (27 > 23), center 338.5
            //   #4 338.5 -> Blocker4 332..352 -> left 325..332 (7 > 0), center 328.5
            //   #5 328.5 -> Blocker5 326..331 -> limit reached
            [Test]
            [Category("Acceptance")]
            [LoadScene(TestScenePath)]
            public async Task IsReachableWithVerbose_BlockersExceedMaxRaycastCount_NotReachableAndLogFiveMessages()
            {
                var target = CreateImage("Target", CanvasTransform, new Vector2(0, -150), new Vector2(320, 30));
                CreateImage("Blocker1", CanvasTransform, new Vector2(-5, -150), new Vector2(20, 40));
                CreateImage("Blocker2", CanvasTransform, new Vector2(85, -150), new Vector2(20, 40));
                CreateImage("Blocker3", CanvasTransform, new Vector2(42, -150), new Vector2(20, 40));
                CreateImage("Blocker4", CanvasTransform, new Vector2(22, -150), new Vector2(20, 40));
                CreateImage("Blocker5", CanvasTransform, new Vector2(8.5f, -150), new Vector2(5, 40));
                await WaitForRaycasterReady();
                var spyLogger = new SpyLogger();
                var sut = new DefaultReachableStrategy(verboseLogger: spyLogger);

                var actual = sut.IsReachable(target, out _);

                Assert.That(actual, Is.False, "not reachable");
                Assert.That(spyLogger.Messages, Has.Count.EqualTo(5), "message count");
            }

            [Test]
            [Category("Acceptance")]
            [LoadScene(TestScenePath)]
            public async Task IsReachableWithVerbose_PivotBlockedByAncestor_NotReachableAndLogOnce()
            {
                var ancestor = CreateImage("Ancestor", CanvasTransform, new Vector2(0, -150), new Vector2(200, 60));
                var target = CreateImage("Target", ancestor.transform, Vector2.zero, new Vector2(160, 30));
                target.GetComponent<Image>().raycastTarget = false; // only the ancestor is hit at the pivot
                await WaitForRaycasterReady();
                var spyLogger = new SpyLogger();
                var sut = new DefaultReachableStrategy(verboseLogger: spyLogger);

                var actual = sut.IsReachable(target, out _);

                Assert.That(actual, Is.False, "not reachable");
                Assert.That(spyLogger.Messages, Has.Count.EqualTo(1), "message count");
                Assert.That(spyLogger.Messages[0], Does.Contain("Raycast hit other objects: [Ancestor("), "message");
            }
        }

        [TestFixture]
        public class Allocation
        {
            private const string TestScenePath = "../../Scenes/GameObjectFinderUI.unity";
            private readonly GameObjectFinder _finder = new GameObjectFinder(0.1d);

            [Test]
            [LoadScene(TestScenePath)]
            public async Task IsReachable_Reachable_DoesNotAllocateGCMemory()
            {
                var target = (await _finder.FindByNameAsync("ActiveText", reachable: false)).GameObject;
                var sut = new DefaultReachableStrategy();
                Assume.That(sut.IsReachable(target, out _), Is.True); // also warms up the measured path

                // Not a lambda with an expression body: a value-returning one binds to the ActualValueDelegate<T>
                // overload of Assert.That, and the constraint then rejects it as "not a TestDelegate".
                Assert.That(() => { _ = sut.IsReachable(target, out _); }, Is.Not.AllocatingGCMemory());
            }

            // Same blocker chain as Verbose.IsReachableWithVerbose_BlockersExceedMaxRaycastCount_..., so that every
            // raycast of the fallback loop runs, without a logger because message formatting allocates by design.
            [Test]
            [LoadScene(TestScenePath)]
            public async Task IsReachable_BlockersExceedMaxRaycastCount_DoesNotAllocateGCMemory()
            {
                var target = CreateImage("Target", CanvasTransform, new Vector2(0, -150), new Vector2(320, 30));
                CreateImage("Blocker1", CanvasTransform, new Vector2(-5, -150), new Vector2(20, 40));
                CreateImage("Blocker2", CanvasTransform, new Vector2(85, -150), new Vector2(20, 40));
                CreateImage("Blocker3", CanvasTransform, new Vector2(42, -150), new Vector2(20, 40));
                CreateImage("Blocker4", CanvasTransform, new Vector2(22, -150), new Vector2(20, 40));
                CreateImage("Blocker5", CanvasTransform, new Vector2(8.5f, -150), new Vector2(5, 40));
                await WaitForRaycasterReady();
                var sut = new DefaultReachableStrategy();
                Assume.That(sut.IsReachable(target, out _), Is.False); // also warms up the measured path

                Assert.That(() => { _ = sut.IsReachable(target, out _); }, Is.Not.AllocatingGCMemory());
            }
        }
    }
}
