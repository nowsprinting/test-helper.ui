// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.UI.Annotations.Enums;
using UnityEngine;

namespace TestHelper.UI.Annotations
{
    [DisallowMultipleComponent]
    public class InputFieldAnnotation : MonoBehaviour
    {
        /// <summary>
        /// Character kind.
        /// </summary>
        public CharactersKind charactersKind = CharactersKind.Alphanumeric;

        /// <summary>
        /// Minimum length of generated strings. Length of the strings must be greater than or equal the value.
        /// </summary>
        public uint minimumLength = 5;

        /// <summary>
        /// Maximum length of generated strings. Length of the strings must be lower than or equal the value.
        /// </summary>
        public uint maximumLength = 10;
    }
}
