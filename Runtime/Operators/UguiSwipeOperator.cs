// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Random;
using TestHelper.UI.Extensions;
using TestHelper.UI.Operators.Utils;
using TestHelper.UI.Random;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.UI.Operators
{
    /// <summary>
    /// Swipe operator for Unity UI (uGUI) components.
    /// </summary>
    public class UguiSwipeOperator : ISwipeOperator, IRandomizable, IScreenPointCustomizable
    {
        private readonly float _swipeSpeed;
        private readonly float _swipeDistance;
        
        /// <inheritdoc/>
        public ILogger Logger { private get; set; }

        /// <inheritdoc/>
        public ScreenshotOptions ScreenshotOptions { private get; set; }

        /// <inheritdoc/>
        public Func<GameObject, Vector2> GetScreenPoint { private get; set; }
        
        /// <inheritdoc/>
        public IRandom Random
        {
            get => _random ?? new RandomWrapper();
            set => _random = value;
        }
        private IRandom _random;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="swipeSpeed">Swipe speed (units/sec). Default is 2400.</param>
        /// <param name="swipeDistance">Swipe distance (units). Default is 400.</param>
        /// <param name="random">PRNG instance. Default is <see cref="TestHelper.Random.Random.Shared"/>.</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="screenshotOptions">Screenshot options.</param>
        public UguiSwipeOperator(
            float swipeSpeed = 2400f,
            float swipeDistance = 400f,
            IRandom random = null,
            ILogger logger = null,
            ScreenshotOptions screenshotOptions = null)
        {
            if (swipeSpeed <= 0)
            {
                throw new ArgumentException("Swipe speed must be positive", nameof(swipeSpeed));
            }
            if (swipeDistance <= 0)
            {
                throw new ArgumentException("Swipe distance must be positive", nameof(swipeDistance));
            }
            
            _swipeSpeed = swipeSpeed;
            _swipeDistance = swipeDistance;
            Random = random;
            Logger = logger;
            ScreenshotOptions = screenshotOptions;
        }
        
        /// <inheritdoc/>
        public bool CanOperate(GameObject gameObject)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public async UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public async UniTask OperateAsync(GameObject gameObject, Vector2 direction, RaycastResult raycastResult = default, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}