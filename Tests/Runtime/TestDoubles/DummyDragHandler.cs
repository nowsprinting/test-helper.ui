// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.TestDoubles
{
    [AddComponentMenu("/")] // Hide from "Add Component" picker
    public class DummyDragHandler : MonoBehaviour, IDragHandler
    {
        public void OnDrag(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }
    }
}
