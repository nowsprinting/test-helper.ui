// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.TestDoubles
{
    /// <summary>
    /// OnPointerDown event handler
    /// </summary>
    [AddComponentMenu("/")] // Hide from "Add Component" picker
    public class SpyOnPointerDownHandler : MonoBehaviour, IPointerDownHandler
    {
        public bool WasOnPointerDown { get; private set; }

        private void Log([CallerMemberName] string member = null)
        {
            Debug.Log($"{this.name}.{member}");
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Log();
            WasOnPointerDown = true;
        }
    }
}
