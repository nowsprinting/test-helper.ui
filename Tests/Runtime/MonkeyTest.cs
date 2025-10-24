// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Random;
using TestHelper.RuntimeInternals;
using TestHelper.UI.Annotations;
using TestHelper.UI.Exceptions;
using TestHelper.UI.Operators;
using TestHelper.UI.Strategies;
using TestHelper.UI.TestDoubles;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Is = TestHelper.Constraints.Is;

namespace TestHelper.UI
{
    [TestFixture]
    [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
    public class MonkeyTest
    {
        private const string TestScene = "../Scenes/Operators.unity";

        private IEnumerable<IOperator> _operators;
        private InteractableComponentsFinder _interactableComponentsFinder;

        [SetUp]
        public void SetUp()
        {
            _operators = new IOperator[]
            {
                new UguiClickOperator(),         // click
                new UguiClickAndHoldOperator(1), // click and hold 1ms
                new UguiTextInputOperator()
            };
            _interactableComponentsFinder = new InteractableComponentsFinder(operators: _operators);
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task RunStep_finish()
        {
            var config = new MonkeyConfig();
            var (didAction, _) = await Monkey.RunStep(
                config.Random,
                config.Logger,
                _interactableComponentsFinder,
                config.IgnoreStrategy,
                config.ReachableStrategy);

            Assert.That(didAction, Is.EqualTo(true));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task RunStep_noInteractiveComponent_DoNoAction()
        {
            // Make to no interactable objects
            foreach (var component in _interactableComponentsFinder.FindInteractableComponents())
            {
                component.gameObject.SetActive(false);
            }

            var config = new MonkeyConfig();
            var (didAction, _) = await Monkey.RunStep(
                config.Random,
                config.Logger,
                _interactableComponentsFinder,
                config.IgnoreStrategy,
                config.ReachableStrategy);

            Assert.That(didAction, Is.EqualTo(false));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_oneStepMode_finish()
        {
            var spyLogger = new SpyLogger();
            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromMilliseconds(200), // 200ms
                DelayMillis = 1,                           // 1ms
                BufferLengthForDetectLooping = 0,          // disable loop detection
                Logger = spyLogger,
                Operators = _operators,
            };

            await Monkey.Run(config, oneStepMode: true);

            Assert.That(spyLogger.Messages, Has.Count.EqualTo(2));
            Assert.That(spyLogger.Messages[0], Does.StartWith("Using RandomWrapper"));
            Assert.That(spyLogger.Messages[1], Does.Contain("Operator operates to"));
            // Note: Only one operator logged
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_finish()
        {
            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromMilliseconds(200), // 200ms
                DelayMillis = 1,                           // 1ms
                BufferLengthForDetectLooping = 0,          // disable loop detection
                Operators = _operators,
            };
            var task = Monkey.Run(config);
            await UniTask.Delay(1000, DelayType.DeltaTime);

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_cancel()
        {
            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.MaxValue, // Test that it does not overflow
                Operators = Enumerable.Empty<IOperator>(),
            };
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var task = Monkey.Run(config, cancellationToken: cancellationTokenSource.Token);
                await UniTask.Delay(1000, DelayType.DeltaTime);

                cancellationTokenSource.Cancel();
                await UniTask.NextFrame();

                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));
            }
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_noInteractiveComponent_throwTimeoutException()
        {
            // Make to no interactable objects
            foreach (var component in _interactableComponentsFinder.FindInteractableComponents())
            {
                component.gameObject.SetActive(false);
            }

            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromSeconds(2),          // 2sec
                SecondsToErrorForNoInteractiveComponent = 1, // 1sec
            };

            try
            {
                await Monkey.Run(config);
                Assert.Fail("TimeoutException was not thrown");
            }
            catch (TimeoutException e)
            {
                Assert.That(e.Message, Does.Contain("Interactive component not found in 1 seconds"));
            }
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_noInteractiveComponentAndSecondsToErrorForNoInteractiveComponentIsZero_finish()
        {
            // Make to no interactable objects
            foreach (var component in _interactableComponentsFinder.FindInteractableComponents())
            {
                component.gameObject.SetActive(false);
            }

            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromSeconds(2),          // 2sec
                DelayMillis = 1,                             // 1ms
                SecondsToErrorForNoInteractiveComponent = 0, // not detect error
            };

            var task = Monkey.Run(config);
            await UniTask.Delay(2200, DelayType.DeltaTime);

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_usingConfigObjects()
        {
            var spyLogger = new SpyLogger();
            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromSeconds(1), // 1sec
                Random = new RandomWrapper(0),      // pin seed
                Logger = spyLogger,
                Operators = _operators,
            };

            await Monkey.Run(config);

            Assert.That(spyLogger.Messages, Does.Contain("Using RandomWrapper includes System.Random, seed=0"));
        }

        [Test]
        [LoadScene(TestScene)]
        [Description("Shown Gizmos, See for yourself! Be a witness!!")]
        public async Task Run_withGizmos_showGizmosAndReverted()
        {
            Assume.That(GameViewControlHelper.GetGizmos(), Is.False);

            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromMilliseconds(200), // 200ms
                DelayMillis = 1,                           // 1ms
                BufferLengthForDetectLooping = 0,          // disable loop detection
                Gizmos = true,                             // show Gizmos
                Operators = _operators,
            };
            var task = Monkey.Run(config);
            await UniTask.Delay(1000, DelayType.DeltaTime);

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            Assert.That(GameViewControlHelper.GetGizmos(), Is.False, "Reverted Gizmos");
        }

        [Test]
        [LoadScene(TestScene)]
        public void GetLotteryEntries_GotAllInteractableComponentAndOperators()
        {
            var lotteryEntries = Monkey.GetLotteryEntries(_interactableComponentsFinder);
            var actual = new List<string>();
            foreach (var (gameObject, @operator) in lotteryEntries)
            {
                actual.Add($"{gameObject.name}|{@operator.GetType().Name}");
            }

            var expected = new List<string>
            {
                "UsingOnPointerClickHandler|UguiClickOperator",
                "UsingPointerClickEventTrigger|UguiClickOperator",
                "UsingOnPointerDownUpHandler|UguiClickAndHoldOperator",
                "UsingOnPointerDownUpHandler|UguiClickAndHoldOperator", // down and up handler are owned by the same GameObject
                "UsingPointerDownUpEventTrigger|UguiClickAndHoldOperator",
                "UsingMultipleEventTriggers|UguiClickOperator",
                "UsingMultipleEventTriggers|UguiClickAndHoldOperator",
                "DestroyItselfIfPointerDown|UguiClickAndHoldOperator",
                "InputField|UguiClickOperator",
                "InputField|UguiClickAndHoldOperator",
                "InputField|UguiTextInputOperator",
            };

            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [Test]
        [LoadScene(TestScene)]
        public void GetLotteryEntries_NoOperators_ReturnsEmpty()
        {
            var notHasOperatorFinder = new InteractableComponentsFinder();
            var actual = Monkey.GetLotteryEntries(notHasOperatorFinder);

            Assert.That(actual, Is.Not.Null.And.Empty);
        }

        [Test]
        public void LotteryOperator_NothingOperators_ReturnNull()
        {
            var operators = Enumerable.Empty<(GameObject, IOperator)>();
            var random = new StubRandom(0);
            var actual = Monkey.LotteryOperator(operators, random,
                new DefaultIgnoreStrategy(), new DefaultReachableStrategy());

            Assert.That(actual.Item1, Is.Null, "InteractiveComponent is null");
            Assert.That(actual.Item2, Is.Null, "Operator is null");
        }

        [Test]
        [LoadScene("../Scenes/PhysicsRaycasterSandbox.unity")]
        public void LotteryOperator_IgnoredObjectOnly_ReturnNull()
        {
            var cube = GameObject.Find("Cube");
            cube.AddComponent<SpyOnPointerClickHandler>();
            cube.transform.position = new Vector3(0, 0, 0);
            cube.AddComponent<IgnoreAnnotation>(); // ignored

            var operators = new List<(GameObject, IOperator)> { (cube, new UguiClickOperator()), };
            var random = new RandomWrapper();
            var actual = Monkey.LotteryOperator(operators, random,
                new DefaultIgnoreStrategy(), new DefaultReachableStrategy());

            Assert.That(actual.Item1, Is.Null, "InteractiveComponent is null");
            Assert.That(actual.Item2, Is.Null, "Operator is null");
        }

        [Test]
        [LoadScene("../Scenes/PhysicsRaycasterSandbox.unity")]
        public void LotteryOperator_NotReachableObjectOnly_ReturnNull()
        {
            var cube = GameObject.Find("Cube");
            cube.AddComponent<SpyOnPointerClickHandler>();
            cube.transform.position = new Vector3(0, 0, -20); // out of sight

            var operators = new List<(GameObject, IOperator)> { (cube, new UguiClickOperator()), };
            var random = new RandomWrapper();
            var actual = Monkey.LotteryOperator(operators, random,
                new DefaultIgnoreStrategy(), new DefaultReachableStrategy());

            Assert.That(actual.Item1, Is.Null, "InteractiveComponent is null");
            Assert.That(actual.Item2, Is.Null, "Operator is null");
        }

        [Test]
        [LoadScene("../Scenes/PhysicsRaycasterSandbox.unity")]
        public void LotteryOperator_BingoReachableComponent_ReturnOperator()
        {
            var cube = GameObject.Find("Cube");
            cube.AddComponent<SpyOnPointerClickHandler>();
            var clickOperator = new UguiClickOperator();

            var operators = new List<(GameObject, IOperator)>()
            {
                (null, null), // dummy
                (cube, clickOperator),
                (null, null), // dummy
            };
            var random = new StubRandom(new[] { 1 });
            var actual = Monkey.LotteryOperator(operators, random,
                new DefaultIgnoreStrategy(), new DefaultReachableStrategy());

            Assert.That(actual.Item1, Is.EqualTo(cube));
            Assert.That(actual.Item2, Is.EqualTo(clickOperator));
        }

        [TestFixture]
        [GameViewResolution(GameViewResolution.VGA)]
        [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
        public class Screenshots
        {
            private const int FileSizeThreshold = 5441; // VGA size solid color file size
            private readonly string _defaultOutputDirectory = CommandLineArgs.GetScreenshotDirectory();
            private string _filename;
            private string _path;

            [SetUp]
            public void SetUp()
            {
                _filename = $"{TestContext.CurrentContext.Test.Name}_0001.png";
                _path = Path.Combine(_defaultOutputDirectory, _filename);

                if (File.Exists(_path))
                {
                    File.Delete(_path);
                }
            }

            private static MonkeyConfig CreateMonkeyConfig(ScreenshotOptions screenshotOptions)
            {
                var config = new MonkeyConfig
                {
                    Operators = new IOperator[] { new UguiClickOperator() },
                    Screenshots = screenshotOptions
                };
                foreach (var iOperator in config.Operators)
                {
                    iOperator.Logger = config.Logger;
                    iOperator.ScreenshotOptions = config.Screenshots;
                }

                return config;
            }

            private static InteractableComponentsFinder CreateInteractableComponentsFinder(MonkeyConfig config)
            {
                return new InteractableComponentsFinder(operators: config.Operators);
            }

            [Test]
            [LoadScene(TestScene)]
            public async Task RunStep_withScreenshots_takeScreenshotsAndSaveToDefaultPath()
            {
                var config = CreateMonkeyConfig(new ScreenshotOptions());
                var interactableComponentsFinder = CreateInteractableComponentsFinder(config);

                await Monkey.RunStep(
                    config.Random,
                    config.Logger,
                    interactableComponentsFinder,
                    config.IgnoreStrategy,
                    config.ReachableStrategy);

                Assert.That(_path, Does.Exist);
                Assert.That(new FileInfo(_path), Has.Length.GreaterThan(FileSizeThreshold));
            }

            [Test]
            [LoadScene(TestScene)]
            public async Task RunStep_withScreenshots_specifyPath_takeScreenshotsAndSaveToSpecifiedPath()
            {
                var relativeDirectory = Path.Combine(Application.temporaryCachePath,
                    TestContext.CurrentContext.Test.ClassName);
                if (Directory.Exists(relativeDirectory))
                {
                    Directory.Delete(relativeDirectory, true);
                }

                var filenamePrefix = TestContext.CurrentContext.Test.Name;
                var filename = $"{filenamePrefix}_0001.png";
                var path = Path.Combine(relativeDirectory, filename);

                var config = CreateMonkeyConfig(new ScreenshotOptions
                {
                    Directory = relativeDirectory,
                    FilenameStrategy = new StubScreenshotFilenameStrategy(filename),
                });
                var interactableComponentsFinder = CreateInteractableComponentsFinder(config);

                await Monkey.RunStep(
                    config.Random,
                    config.Logger,
                    interactableComponentsFinder,
                    config.IgnoreStrategy,
                    config.ReachableStrategy);

                Assert.That(path, Does.Exist);
                Assert.That(new FileInfo(path), Has.Length.GreaterThan(FileSizeThreshold));
            }

            [Test]
            [LoadScene(TestScene)]
            public async Task Run_withScreenshots_noInteractiveComponent_takeScreenshot()
            {
                var config = CreateMonkeyConfig(new ScreenshotOptions());
                config.Lifetime = TimeSpan.FromSeconds(2);          // 2sec
                config.SecondsToErrorForNoInteractiveComponent = 1; // 1sec

                // Make to no interactable objects
                var interactableComponentsFinder = CreateInteractableComponentsFinder(config);
                foreach (var component in interactableComponentsFinder.FindInteractableComponents())
                {
                    component.gameObject.SetActive(false);
                }

                try
                {
                    await Monkey.Run(config);
                    Assert.Fail("TimeoutException was not thrown");
                }
                catch (TimeoutException e)
                {
                    Assert.That(e.Message, Does.Contain(
                        $"Interactive component not found in 1 seconds, screenshot={_filename}"));
                }

                Assert.That(_path, Does.Exist);
            }

            [Test]
            [LoadScene("../Scenes/InfiniteLoop.unity")]
            public async Task Run_withScreenshots_InfiniteLoop_takeScreenshot()
            {
                var filename = $"{TestContext.CurrentContext.Test.Name}_0011.png"; // 10 steps + 1
                var path = Path.Combine(_defaultOutputDirectory, filename);

                var config = CreateMonkeyConfig(new ScreenshotOptions());
                config.Lifetime = TimeSpan.FromSeconds(2); // 2sec
                config.DelayMillis = 1;                    // 1ms
                config.BufferLengthForDetectLooping = 10;  // repeating 5-step sequences can be detected

                try
                {
                    await Monkey.Run(config);
                    Assert.Fail("InfiniteLoopException was not thrown");
                }
                catch (InfiniteLoopException e)
                {
                    Assert.That(e.Message, Does.Contain(filename));
                }

                Assert.That(path, Does.Exist);
            }
        }

        [TestFixture]
        public class Verbose
        {
            private static InteractableComponentsFinder CreateInteractableComponentsFinder()
            {
                var operators = new IOperator[]
                {
                    new UguiClickOperator(),         // click
                    new UguiClickAndHoldOperator(1), // click and hold 1ms
                    new UguiTextInputOperator()
                };
                return new InteractableComponentsFinder(operators: operators);
            }

            [Test]
            [LoadScene(TestScene)]
            public void GetLotteryEntries_NotOutputLog()
            {
                var lotteryEntries = Monkey.GetLotteryEntries(CreateInteractableComponentsFinder());
                Assume.That(lotteryEntries.Count, Is.GreaterThan(0));

                LogAssert.NoUnexpectedReceived();
            }

            [Test]
            [LoadScene(TestScene)]
            public void GetLotteryEntries_LogLotteryEntries()
            {
                GameObject.Find("UsingOnPointerClickHandler").AddComponent<IgnoreAnnotation>();

                var spyLogger = new SpyLogger();
                var lotteryEntries = Monkey.GetLotteryEntries(CreateInteractableComponentsFinder(),
                    verboseLogger: spyLogger);
                Assume.That(lotteryEntries.Count, Is.GreaterThan(0));

                Assert.That(spyLogger.Messages, Has.Count.EqualTo(1));
                // @formatter:off
                Assert.That(spyLogger.Messages[0], Does.StartWith("Lottery entries: "));
                Assert.That(spyLogger.Messages[0], Does.Match(@"UsingPointerClickEventTrigger\(\d+\):EventTrigger:UguiClickOperator"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"UsingOnPointerDownUpHandler\(\d+\):SpyOnPointerDownHandler:UguiClickAndHoldOperator"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"UsingOnPointerDownUpHandler\(\d+\):SpyOnPointerUpHandler:UguiClickAndHoldOperator"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"UsingPointerDownUpEventTrigger\(\d+\):EventTrigger:UguiClickAndHoldOperator"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"UsingOnPointerClickHandler\(\d+\):SpyOnPointerClickHandler:UguiClickOperator")); // includes ignored objects
                Assert.That(spyLogger.Messages[0], Does.Match(@"UsingMultipleEventTriggers\(\d+\):EventTrigger:UguiClickOperator"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"UsingMultipleEventTriggers\(\d+\):EventTrigger:UguiClickAndHoldOperator"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"DestroyItselfIfPointerDown\(\d+\):StubDestroyingItselfWhenPointerDown:UguiClickAndHoldOperator"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"InputField\(\d+\):InputField:UguiClickOperator"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"InputField\(\d+\):InputField:UguiClickAndHoldOperator"));
                Assert.That(spyLogger.Messages[0], Does.Match(@"InputField\(\d+\):InputField:UguiTextInputOperator"));
                // @formatter:on
            }

            [Test]
            [LoadScene("../Scenes/PhysicsRaycasterSandbox.unity")] // no interactable objects
            public void GetLotteryEntries_NoInteractableObject_LogNoLotteryEntries()
            {
                var spyLogger = new SpyLogger();
                var lotteryEntries = Monkey.GetLotteryEntries(CreateInteractableComponentsFinder(),
                    verboseLogger: spyLogger);
                Assume.That(lotteryEntries, Is.Empty);

                Assert.That(spyLogger.Messages, Has.Count.EqualTo(1));
                Assert.That(spyLogger.Messages[0], Is.EqualTo("No lottery entries."));
            }

            [Test]
            public void LotteryOperator_NothingOperators_LogNotLottery()
            {
                var operators = Enumerable.Empty<(GameObject, IOperator)>();
                var random = new StubRandom(0);
                var spyLogger = new SpyLogger();
                var ignoreStrategy = new DefaultIgnoreStrategy(verboseLogger: spyLogger);
                var reachableStrategy = new DefaultReachableStrategy(verboseLogger: spyLogger);
                Monkey.LotteryOperator(operators, random, ignoreStrategy, reachableStrategy, spyLogger);

                Assert.That(spyLogger.Messages, Has.Count.EqualTo(1));
                Assert.That(spyLogger.Messages[0], Is.EqualTo("Lottery entries are empty or all of not reachable."));
            }

            [Test]
            [LoadScene("../Scenes/PhysicsRaycasterSandbox.unity")]
            public void LotteryOperator_IgnoredObjectOnly_LogNotLottery()
            {
                var cube = GameObject.Find("Cube");
                cube.AddComponent<SpyOnPointerClickHandler>();
                cube.transform.position = new Vector3(0, 0, 0);
                cube.AddComponent<IgnoreAnnotation>(); // ignored

                var operators = new List<(GameObject, IOperator)> { (cube, new UguiClickOperator()), };
                var random = new RandomWrapper();
                var spyLogger = new SpyLogger();
                var ignoreStrategy = new DefaultIgnoreStrategy(verboseLogger: spyLogger);
                var reachableStrategy = new DefaultReachableStrategy(verboseLogger: spyLogger);
                Monkey.LotteryOperator(operators, random, ignoreStrategy, reachableStrategy, spyLogger);

                Assert.That(spyLogger.Messages, Has.Count.EqualTo(2));
                Assert.That(spyLogger.Messages[0], Does.Match(@"Ignored Cube\(\d+\)."));
                Assert.That(spyLogger.Messages[1], Is.EqualTo("Lottery entries are empty or all of not reachable."));
            }

            [Test]
            [LoadScene("../Scenes/PhysicsRaycasterSandbox.unity")]
            public void LotteryOperator_NotReachableObjectOnly_LogNotLottery()
            {
                var cube = GameObject.Find("Cube");
                cube.AddComponent<SpyOnPointerClickHandler>();
                cube.transform.position = new Vector3(0, 0, -20); // out of sight

                var operators = new List<(GameObject, IOperator)> { (cube, new UguiClickOperator()), };
                var random = new RandomWrapper();
                var spyLogger = new SpyLogger();
                var ignoreStrategy = new DefaultIgnoreStrategy(verboseLogger: spyLogger);
                var reachableStrategy = new DefaultReachableStrategy(verboseLogger: spyLogger);
                Monkey.LotteryOperator(operators, random, ignoreStrategy, reachableStrategy, spyLogger);

                Assert.That(spyLogger.Messages, Has.Count.EqualTo(2));
                Assert.That(spyLogger.Messages[0],
                    Does.Match(
                        @"Not reachable to Cube\(\d+\), position=\(\d+,\d+\), camera=Main Camera\(\d+\)\. Raycast is not hit\."));
                Assert.That(spyLogger.Messages[1], Is.EqualTo("Lottery entries are empty or all of not reachable."));
            }
        }

        [TestFixture]
        public class Visualizer
        {
            private const float IndicatorLifetime = 0.5f;
            private DefaultDebugVisualizer _visualizer;

            [OneTimeSetUp]
            public void OneTimeSetUp()
            {
                _visualizer = new DefaultDebugVisualizer() { IndicatorLifetime = IndicatorLifetime };
            }

            [OneTimeTearDown]
            public void OneTimeTearDown()
            {
                _visualizer.Dispose();
            }

            [Test]
            [LoadScene("../Scenes/PhysicsRaycasterSandbox.unity")]
            public async Task LotteryOperator_IgnoredObjectOnly_IgnoredIndicatorIsShown()
            {
                var cube = GameObject.Find("Cube");
                cube.transform.position = new Vector3(0, 0, 0);
                cube.AddComponent<IgnoreAnnotation>(); // ignored

                await UniTask.DelayFrame(5); // warm up for physics raycaster (maybe)

                var operators = new List<(GameObject, IOperator)> { (cube, new UguiClickOperator()), };
                var random = new RandomWrapper();
                var ignoreStrategy = new DefaultIgnoreStrategy();
                var reachableStrategy = new DefaultReachableStrategy();
                Monkey.LotteryOperator(operators, random, ignoreStrategy, reachableStrategy, visualizer: _visualizer);

                await UniTask.NextFrame();

                var indicator = GameObject.Find("Indicator"); // exist multiple, so only one
                Assert.That(indicator, Is.Not.Null);
                Assert.That(indicator.GetComponent<Image>().sprite.name, Is.EqualTo("lock"));
                Assert.That(indicator.GetComponent<Image>().raycastTarget, Is.False);

                await Task.Delay(TimeSpan.FromSeconds(IndicatorLifetime)); // wait for end of life
                Assert.That(indicator, Is.Destroyed);
            }

            [Test]
            [LoadScene("../Scenes/PhysicsRaycasterSandbox.unity")]
            public async Task LotteryOperator_NotReachableObjectOnly_NotReachableIndicatorIsShown()
            {
                var cube = GameObject.Find("Cube");
                cube.transform.position = new Vector3(0, 0, 0);

                var blocker = GameObject.CreatePrimitive(PrimitiveType.Quad);
                blocker.transform.position = new Vector3(0, 1, -7);
                blocker.GetComponent<MeshRenderer>().materials[0].color = Color.gray;

                await UniTask.DelayFrame(5); // warm up for physics raycaster (maybe)

                var operators = new List<(GameObject, IOperator)> { (cube, new UguiClickOperator()), };
                var random = new RandomWrapper();
                var ignoreStrategy = new DefaultIgnoreStrategy();
                var reachableStrategy = new DefaultReachableStrategy();
                Monkey.LotteryOperator(operators, random, ignoreStrategy, reachableStrategy, visualizer: _visualizer);

                await UniTask.NextFrame();

                var indicator = GameObject.Find("Indicator"); // exist multiple, so only one
                Assert.That(indicator, Is.Not.Null);
                Assert.That(indicator.GetComponent<Image>().sprite.name, Is.EqualTo("eye_slash"));
                Assert.That(indicator.GetComponent<Image>().raycastTarget, Is.False);

                await Task.Delay(TimeSpan.FromSeconds(IndicatorLifetime)); // wait for end of life
                Assert.That(indicator, Is.Destroyed);
            }
        }

        [TestFixture]
        [GameViewResolution(GameViewResolution.VGA)]
        public class DetectingInfiniteLoop
        {
            [Test]
            [LoadScene("../Scenes/InfiniteLoop.unity")]
            public async Task Run_InfiniteLoop_throwsInfiniteLoopException()
            {
                var config = new MonkeyConfig
                {
                    Lifetime = TimeSpan.FromSeconds(2), // 2sec
                    DelayMillis = 1,                    // 1ms
                    BufferLengthForDetectLooping = 10,  // repeating 5-step sequences can be detected
                    Operators = new IOperator[] { new UguiClickOperator() },
                };

                try
                {
                    await Monkey.Run(config);
                    Assert.Fail("InfiniteLoopException was not thrown");
                }
                catch (InfiniteLoopException)
                {
                    // pass
                }
            }

            [TestCase("1")]             // under pattern length
            [TestCase("1, 1")]          // under repeating length
            [TestCase("1, 1, 1")]       // under repeating length
            [TestCase("1, 2, 3, 1, 2")] // not looping yet
            [TestCase("1, 2, 2, 2, 2")] // not looping yet
            [TestCase("2, 2, 2, 2, 3")] // precondition is (1, 2, 2, 2, 2), add "3" and remove "1" (buffer overflow)
            public void DetectInfiniteLoop_NotRepeatingSequence_ReturnsFalse(string commaSeparatedSequence)
                // Note: If a parameter type is `int[]`, all test names will contain `System.Int32[]` will be indistinguishable, so pass it as a comma-separated string and parse it.
            {
                var sequence = new List<int>(commaSeparatedSequence.Split(',').Select(int.Parse));
                Assert.That(Monkey.DetectInfiniteLoop(sequence), Is.False);
            }

            [TestCase("1, 1, 1, 1")] // pattern (1, 1) is repeated twice
            [TestCase("1, 2, 1, 2")]
            [TestCase("1, 2, 3, 1, 2, 3")]
            [TestCase("1, 2, 3, 1, 2, 3, 1")] // one loop and unfinished loop
            public void DetectInfiniteLoop_RepeatingSequence_ReturnsTrue(string commaSeparatedSequence)
                // Note: If a parameter type is `int[]`, all test names will contain `System.Int32[]` will be indistinguishable, so pass it as a comma-separated string and parse it.
            {
                var sequence = new List<int>(commaSeparatedSequence.Split(',').Select(int.Parse));
                Assert.That(Monkey.DetectInfiniteLoop(sequence), Is.True);
            }
        }
    }
}
