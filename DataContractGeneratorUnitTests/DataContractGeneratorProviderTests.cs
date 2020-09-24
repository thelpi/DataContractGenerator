using System;
using System.Collections.Generic;
using System.Linq;
using DataContractGenerator;
using DataContractGeneratorUnitTests.Datas;
using Xunit;

namespace DataContractGeneratorUnitTests
{
    public class DataContractGeneratorProviderTests
    {
        const string _expectedForFakeSpecValue = "SALUT";
        // replace "options.MinListCount" by zero as minimal count for dictionary
        // because random keys can trigger an early fill loop ending
        // to avoid keys duplicate
        const int minimalForDictionary = 0;

        [Fact]
        public void GenerateRandom_Success_DefaultOptions()
        {
            // ARRANGE
            var logger = new TestLogger();
            var convertes = new Dictionary<Type, Delegate>
            {
                {
                    typeof(FakeSpecType),
                    new Func<FakeSpecType>(() =>
                        new FakeSpecType { Value = _expectedForFakeSpecValue })
                }
            };
            var options = new GenerationOptions();
            var provider = new DataContractGeneratorProvider(logger, convertes, options);

            // ACT
            FakeContract instance = provider.GenerateRandom<FakeContract>();

            // ASSERT
            Assert.NotNull(instance);
            Assert.Equal(0, logger.ErrorsCount);
            Assert.NotNull(instance.ArrayOfIntThreeDim);
            Assert.InRange(instance.ArrayOfIntThreeDim.GetLength(0), options.MinListCount, options.MaxListCount);
            Assert.InRange(instance.ArrayOfIntThreeDim.GetLength(1), options.MinListCount, options.MaxListCount);
            Assert.InRange(instance.ArrayOfIntThreeDim.GetLength(2), options.MinListCount, options.MaxListCount);
            Assert.NotNull(instance.BasicList);
            Assert.InRange(instance.BasicList.Count, options.MinListCount, options.MaxListCount);
            Assert.NotNull(instance.BigTuple);
            Assert.NotNull(instance.Dictionary);
            Assert.InRange(instance.Dictionary.Keys.Count, minimalForDictionary, options.MaxListCount);
            Assert.NotNull(instance.Enumerable);
            Assert.InRange(instance.Enumerable.Count(), options.MinListCount, options.MaxListCount);
            Assert.NotNull(instance.FakeAbstract);
            Assert.NotNull(instance.FakeConcreteWithoutCtor);
            Assert.NotNull(instance.FakeSpec);
            Assert.Equal(_expectedForFakeSpecValue, instance.FakeSpec.Value);
            Assert.NotNull(instance.Interface);
            Assert.NotNull(instance.ReadOnlyDictionary);
            Assert.InRange(instance.ReadOnlyDictionary.Keys.Count(), minimalForDictionary, options.MaxListCount);
            Assert.NotNull(instance.ReadOnlyList);
            Assert.InRange(instance.ReadOnlyList.Count, options.MinListCount, options.MaxListCount);
            Assert.NotNull(instance.SmallTuple);
            Assert.NotNull(instance.String);
            Assert.NotNull(instance.FakeGeneric);
            Assert.Null(instance.Unparsable);
            // can't happen for now (but will later)
            Assert.InRange(instance.String.Length, options.MinStringLength, options.MaxStringLength);
            Assert.NotNull(instance.NullableInt);
            // unlikely to happen
            Assert.NotEqual(default(TimeSpan), instance.TimeSpan);
            Assert.NotEqual(default(DateTime), instance.DateTime);
            Assert.NotEqual(default(DateTimeOffset), instance.DateTimeOffset);
            Assert.NotEqual(default(Guid), instance.Guid);
            Assert.NotEqual(default(KeyValuePair<string, string>), instance.KeyValuePair);
            Assert.Equal(options.MaximalRecursionDepth, GetRecursionDepth(instance));
        }

        [Fact]
        public void GenerateRandom_Success_StringPropertyOptions()
        {
            // ARRANGE
            var logger = new TestLogger();
            var convertes = new Dictionary<Type, Delegate>();
            var options = new GenerationOptions { StringAsPropertyName = true };
            var provider = new DataContractGeneratorProvider(logger, convertes, options);

            // ACT
            FakeContract instance = provider.GenerateRandom<FakeContract>();

            // ASSERT
            Assert.NotNull(instance);
            Assert.Equal(0, logger.ErrorsCount);
            Assert.NotEmpty(instance.BigTuple.Item2);
            Assert.Equal("Key", instance.KeyValuePair.Key);
            Assert.Equal("Value", instance.KeyValuePair.Value);
            Assert.Equal("Value", instance.ReadOnlyDictionary.ElementAt(0).Value);
            Assert.Equal("String", instance.String);
            Assert.Equal("Value", instance.FakeSpec.Value);
        }

        private static int GetRecursionDepth(FakeContract instance)
        {
            var recursionInstance = instance;
            int i = -1;
            while (recursionInstance != null)
            {
                i++;
                recursionInstance = recursionInstance.Recursion;
            }
            return i;
        }
    }
}
