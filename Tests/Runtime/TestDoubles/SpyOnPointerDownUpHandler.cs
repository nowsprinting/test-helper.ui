// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.TestDoubles
{
    /// <summary>
    /// Spy that implements both IPointerDownHandler and IPointerUpHandler for testing.
    /// </summary>
    public class SpyOnPointerDownUpHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public List<PointerEventData> PointerDownEvents { get; } = new List<PointerEventData>();
        public List<PointerEventData> PointerUpEvents { get; } = new List<PointerEventData>();
        
        public void OnPointerDown(PointerEventData eventData)
        {
            PointerDownEvents.Add(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PointerUpEvents.Add(eventData);
        }
    }
}