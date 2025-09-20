// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.TestDoubles
{
    /// <summary>
    /// OnPointerUp event handler
    /// </summary>
    [AddComponentMenu("/")] // Hide from "Add Component" picker
    public class SpyOnPointerUpHandler : MonoBehaviour, IPointerUpHandler
    {
        private void Log([CallerMemberName] string member = null)
        {
            Debug.Log($"{this.name}.{member}");
        }

        public void OnPointerUp(PointerEventData eventData) => Log();
    }
}
