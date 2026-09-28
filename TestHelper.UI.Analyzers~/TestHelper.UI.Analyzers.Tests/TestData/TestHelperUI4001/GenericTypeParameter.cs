using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Operators;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4001
{
    public class GenericTypeParameter
    {
        public void Register<T>(OperatorPool pool) where T : class, IOperator
        {
            pool.Register<T>();
        }
    }
}
