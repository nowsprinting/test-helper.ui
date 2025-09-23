// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Extensions
{
    public static class RaycastResultExtensions
    {
        /// <summary>
        /// Create <c>RaycastResult</c> for simulating an event.
        /// <c>screenPosition</c> is set pivot position of <c>GameObject</c> by default.
        /// </summary>
        /// <param name="gameObject">Event target <c>GameObject</c></param>
        /// <param name="getScreenPoint">Function returns the screen position of <c>GameObject</c></param>
        /// <returns>A <c>RaycastResult</c> with the specified <c>gameObject</c> and its screen position as determined by <paramref name="getScreenPoint"/></returns>
        public static RaycastResult CreateFrom(GameObject gameObject, Func<GameObject, Vector2> getScreenPoint)
        {
            return new RaycastResult
            {
                gameObject = gameObject,
                module = null,
                distance = 0,
                index = 0,
                depth = 0,
                sortingLayer = 0,
                sortingOrder = 0,
                worldNormal = Vector3.up,
                worldPosition = Vector3.zero,
                screenPosition = getScreenPoint(gameObject),
            };
        }
    }
}
