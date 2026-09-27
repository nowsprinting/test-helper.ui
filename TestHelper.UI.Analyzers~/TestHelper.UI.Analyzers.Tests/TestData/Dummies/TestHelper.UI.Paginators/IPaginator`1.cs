using UnityEngine;

namespace TestHelper.UI.Paginators
{
    public interface IPaginator<TComponent> : IPaginator where TComponent : MonoBehaviour
    {
    }
}
