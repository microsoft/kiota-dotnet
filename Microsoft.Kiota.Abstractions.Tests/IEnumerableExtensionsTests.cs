using System;
using System.Collections.Generic;
using Microsoft.Kiota.Abstractions.Extensions;
using Xunit;

namespace Microsoft.Kiota.Abstractions.Tests
{
    public class IEnumerableExtensionsTests
    {
        [Fact]
        public void AsList_WithNullInput_ThrowsArgumentNullException()
        {
            IEnumerable<int> nullEnumerable = null;
            Assert.Throws<ArgumentNullException>(() => nullEnumerable.AsList());
        }

        [Fact]
        public void AsList_WithListInput_ReturnsSameList()
        {
            List<int> originalList = [1, 2, 3];
            var resultList = originalList.AsList();
            Assert.Same(originalList, resultList);
        }

        [Fact]
        public void AsList_WithEnumerableInput_ReturnsNewList()
        {
            IEnumerable<int> enumerable = [1, 2, 3];
            var resultList = enumerable.AsList();
            Assert.NotSame(enumerable, resultList);
            Assert.Equal(enumerable, resultList);
        }

        [Fact]
        public void AsArray_WithNullInput_ThrowsArgumentNullException()
        {
            IEnumerable<int> nullEnumerable = null;
            Assert.Throws<ArgumentNullException>(() => nullEnumerable.AsArray());
        }

        [Fact]
        public void AsArray_WithArrayInput_ReturnsSameArray()
        {
            int[] originalArray = [1, 2, 3];
            var resultArray = originalArray.AsArray();
            Assert.Same(originalArray, resultArray);
        }

        [Fact]
        public void AsArray_WithListInput_ReturnsNewArray()
        {
            List<int> list = [1, 2, 3];
            var resultArray = list.AsArray();
            Assert.NotSame(list, resultArray);
            Assert.Equal(list, resultArray);
        }

        [Fact]
        public void AsArray_WithEnumerableInput_ReturnsNewArray()
        {
            IEnumerable<int> enumerable = [1, 2, 3];
            var resultArray = enumerable.AsArray();
            // We expect a new array only if the input is not already an array
            if(enumerable is not int[])
            {
                Assert.NotSame(enumerable, resultArray);
            }
            Assert.Equal(enumerable, resultArray);
        }

        [Fact]
        public void AsArray_WithNonCollectionEnumerableInput_ReturnsNewArray()
        {
            IEnumerable<int> enumerable = GetEnumerable();
            var resultArray = enumerable.AsArray();
            int[] expected = [1, 2, 3];
            Assert.Equal(expected, resultArray);
        }

        private IEnumerable<int> GetEnumerable()
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }
    }
}
