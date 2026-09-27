using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4008
{
    public class DerivedFromPreserveAttribute : IPaginator<MonoBehaviour>
    {
        [DerivedPreserve]
        public DerivedFromPreserveAttribute(int value = 0)
        {
        }

        public MonoBehaviour? TargetComponent { get; set; }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();

        public class IntermediatePreserveAttribute : UnityEngine.Scripting.PreserveAttribute
        {
        }

        public class DerivedPreserveAttribute : IntermediatePreserveAttribute
        {
        }
    }
}
