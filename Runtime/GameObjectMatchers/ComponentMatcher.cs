// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Text;
using TestHelper.UI.Extensions;
using UnityEngine;

namespace TestHelper.UI.GameObjectMatchers
{
    /// <summary>
    /// <see cref="GameObject"/> matcher that matches by name, path, and <c>Component</c> type.
    /// </summary>
    public class ComponentMatcher : IGameObjectMatcher
    {
        private readonly Type _componentType;
        private readonly string _name;
        private readonly string _path;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="componentType">Component type. If omitted, <see cref="Component"/> is used.</param>
        /// <param name="name"><see cref="GameObject"/> name</param>
        /// <param name="path"><see cref="GameObject"/> hierarchy path separated by `/`. Can specify wildcards of glob pattern (`?`, `*`, and `**`).</param>
        /// <seealso href="https://en.wikipedia.org/wiki/Glob_(programming)"/>
        public ComponentMatcher(Type componentType = null, string name = null, string path = null)
        {
            _componentType = componentType ?? typeof(Component);
            _name = name;
            _path = path;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder($"type={_componentType}");
            if (_name != null)
            {
                builder.Append($", name={_name}");
            }

            if (_path != null)
            {
                builder.Append($", path={_path}");
            }

            return builder.ToString();
        }

        /// <inheritdoc/>
        public bool IsMatch(GameObject gameObject)
        {
            if (!gameObject.TryGetComponent(_componentType, out _))
            {
                return false;
            }

            if (_name != null && gameObject.name != _name)
            {
                return false;
            }

            if (_path != null && !gameObject.transform.MatchPath(_path))
            {
                return false;
            }

            return true;
        }
    }
}
