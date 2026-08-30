// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using UnityEngine;

namespace TestHelper.UI.GameObjectMatchers
{
    /// <summary>
    /// <see cref="GameObject"/> matcher that matchers by name.
    /// </summary>
    public class NameMatcher : IGameObjectMatcher
    {
        private readonly string _name;

        /// <inheritdoc/>
        public Type ComponentType => typeof(Transform);

        /// <summary>
        /// Constructor with name.
        /// </summary>
        /// <param name="name"><see cref="GameObject"/> name</param>
        public NameMatcher(string name)
        {
            _name = name;
        }

        /// <inheritdoc/>
        public override string ToString() => $"name={_name}";

        /// <inheritdoc/>
        public bool IsMatch(GameObject gameObject)
        {
            return gameObject.name == _name;
        }
    }
}
