using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;
using UnityEngine.Scripting;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4008
{
    public class MultiplePublicConstructors : IPaginator<MonoBehaviour>
    {
        [Preserve]
        public MultiplePublicConstructors()
        {
        }

        public MultiplePublicConstructors(int value) // TestHelperUI4008
        {
        }

        public MultiplePublicConstructors(string text) // TestHelperUI4008
        {
        }

        public MonoBehaviour? TargetComponent { get; set; }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
