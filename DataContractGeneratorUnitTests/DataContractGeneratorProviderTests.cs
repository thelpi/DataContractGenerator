using System;
using System.Collections.Generic;
using DataContractGenerator;
using DataContractGeneratorUnitTests.Datas;
using Xunit;

namespace DataContractGeneratorUnitTests
{
    public class DataContractGeneratorProviderTests
    {
        const string _expectedForFakeSpecValue = "SALUT";

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
            Assert.NotEmpty(instance.ArrayOfIntThreeDim);
            Assert.NotNull(instance.BasicList);
            Assert.NotEmpty(instance.BasicList);
            Assert.InRange(instance.BasicList.Count, GenerationOptions.MIN_LIST_COUNT, GenerationOptions.MAX_LIST_COUNT);
            Assert.NotNull(instance.BigTuple);
            Assert.NotNull(instance.Dictionary);
            Assert.NotEmpty(instance.Dictionary);
            Assert.InRange(instance.Dictionary.Keys.Count, GenerationOptions.MIN_LIST_COUNT, GenerationOptions.MAX_LIST_COUNT);
            Assert.NotNull(instance.Enumerable);
            Assert.NotEmpty(instance.Enumerable);
            Assert.NotNull(instance.FakeAbstract);
            Assert.NotNull(instance.FakeConcreteWithoutCtor);
            Assert.NotNull(instance.FakeSpec);
            Assert.Equal(_expectedForFakeSpecValue, instance.FakeSpec.Value);
            Assert.NotNull(instance.Interface);
            Assert.NotNull(instance.ReadOnlyDictionary);
            Assert.NotEmpty(instance.ReadOnlyDictionary);
            Assert.NotNull(instance.ReadOnlyList);
            Assert.NotEmpty(instance.ReadOnlyList);
            Assert.NotNull(instance.SmallTuple);
            Assert.NotNull(instance.String);
            Assert.NotNull(instance.FakeGeneric);
            Assert.Null(instance.Unparsable);
            // can't happen for now (but will later)
            Assert.NotEmpty(instance.String);
            Assert.InRange(instance.String.Length, GenerationOptions.MIN_STRING_LENGTH, GenerationOptions.MAX_STRING_LENGTH);
            Assert.NotNull(instance.NullableInt);
            // unlikely to happen
            Assert.NotEqual(default(TimeSpan), instance.TimeSpan);
            Assert.NotEqual(default(DateTime), instance.DateTime);
            Assert.NotEqual(default(DateTimeOffset), instance.DateTimeOffset);
            Assert.NotEqual(default(Guid), instance.Guid);
            Assert.NotEqual(default(KeyValuePair<byte, string>), instance.KeyValuePair);
            Assert.Equal(GenerationOptions.MAX_RECURSION_DEPTH, GetRecursionDepth(instance));
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
