// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TestHelper.Monkey.Operators
{
    /// <summary>
    /// Matcher and Operator pair for text input component.
    /// Added specify input string method for scenario testing.
    /// </summary>
    public interface ITextInputOperator : IOperator
    {
        /// <summary>
        /// Text input with specified text.
        /// </summary>
        /// <param name="gameObject">Operation target <c>GameObject</c></param>
        /// <param name="text">Text to input</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        UniTask OperateAsync(GameObject gameObject, string text, CancellationToken cancellationToken = default);

        [Obsolete("Use OperateAsync(GameObject, string, CancellationToken) and properties instead.")]
        UniTask OperateAsync(GameObject gameObject, string text,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null,
            CancellationToken cancellationToken = default);
    }
}
