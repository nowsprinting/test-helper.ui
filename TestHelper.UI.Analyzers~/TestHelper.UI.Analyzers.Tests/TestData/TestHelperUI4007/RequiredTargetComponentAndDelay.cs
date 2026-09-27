using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4007
{
    public class RequiredTargetComponentAndDelay : IPaginator<MonoBehaviour>
    {
        public RequiredTargetComponentAndDelay(MonoBehaviour carousel, float pageDelaySeconds) // TestHelperUI4007
        {
            TargetComponent = carousel;
        }

        public MonoBehaviour? TargetComponent { get; set; }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
