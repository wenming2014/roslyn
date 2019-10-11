
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.UnitTests.InternalUtilities
{
    public class FrozenDictionaryTests
    {
        [Fact]
        public void Test()
        {
            var builder = FrozenDictionary<string, string>.GetBuilder(StringOrdinalComparer.Instance);

            builder.Add("a", "b");
            builder.Add("c", "d");

            var frozen = builder.ToImmutable();
        }
    }
}
