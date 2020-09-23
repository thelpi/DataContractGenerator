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

        [Theory]
        [InlineData(100)]
        public void GenerateRandom_Success(int testsCount)
        {
            var logger = new TestLogger();

            Dictionary<Type, Delegate> convertes = new Dictionary<Type, Delegate>
            {
                { typeof(FakeSpecType), new Func<FakeSpecType>(RandomFakeSpecType) }
            };

            var provider = new DataContractGeneratorProvider(logger, convertes);

            for (int j = 0; j < testsCount; j++)
            {
                logger.Clear();

                FakeContract instance = provider.GenerateRandom<FakeContract>();

                Assert.NotNull(instance);
                Assert.Equal(0, logger.ErrorsCount);
                Assert.NotNull(instance.ArrayOfIntThreeDim);
                Assert.NotEmpty(instance.ArrayOfIntThreeDim);
                Assert.NotNull(instance.BasicList);
                Assert.NotEmpty(instance.BasicList);
                Assert.NotNull(instance.BigTuple);
                Assert.NotNull(instance.Dictionary);
                Assert.NotEmpty(instance.Dictionary);
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
                // can't happen for now (but will later)
                Assert.NotEmpty(instance.String);
                Assert.NotNull(instance.NullableInt);
                // unlikely to happen
                Assert.NotEqual(default(TimeSpan), instance.TimeSpan);
                Assert.NotEqual(default(DateTime), instance.DateTime);
                Assert.NotEqual(default(DateTimeOffset), instance.DateTimeOffset);
                Assert.NotEqual(default(Guid), instance.Guid);
                Assert.NotEqual(default(KeyValuePair<byte, string>), instance.KeyValuePair);

                var recursionInstance = instance;
                int i = -1;
                while (recursionInstance != null)
                {
                    i++;
                    recursionInstance = recursionInstance.Recursion;
                }
                Assert.Equal(DataContractGeneratorProvider.MAX_RECURSION_DEPTH, i);
            }
        }

        private FakeSpecType RandomFakeSpecType()
        {
            return new FakeSpecType { Value = _expectedForFakeSpecValue };
        }
    }
}
