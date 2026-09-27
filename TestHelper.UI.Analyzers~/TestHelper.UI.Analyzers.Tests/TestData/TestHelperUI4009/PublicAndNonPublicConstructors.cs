using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4009
{
    public class PublicAndNonPublicConstructors : IPaginator<MonoBehaviour>
    {
        public PublicAndNonPublicConstructors()
        {
        }

        private PublicAndNonPublicConstructors(int value)
        {
        }

        protected PublicAndNonPublicConstructors(string text)
        {
        }

        internal PublicAndNonPublicConstructors(bool flag)
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
