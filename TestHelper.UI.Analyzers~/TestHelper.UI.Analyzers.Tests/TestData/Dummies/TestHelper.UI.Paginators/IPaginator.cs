using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TestHelper.UI.Paginators
{
    public interface IPaginator
    {
        MonoBehaviour TargetComponent { set; }

        UniTask ResetAsync(CancellationToken cancellationToken = default);

        UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default);

        bool HasNextPage();
    }
}
