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
        public void GenerateRandom_Success()
        {
            var logger = new TestLogger();

            Dictionary<Type, Delegate> convertes = new Dictionary<Type, Delegate>
            {
                { typeof(FakeSpecType), new Func<FakeSpecType>(RandomFakeSpecType) }
            };

            FakeContract instance = new DataContractGeneratorProvider(logger, convertes)
                .GenerateRandom<FakeContract>();

            Assert.NotNull(instance);
            Assert.Equal(0, logger.ErrorsCount);
            Assert.Equal(_expectedForFakeSpecValue, instance.FakeSpec.Value);
        }

        private FakeSpecType RandomFakeSpecType()
        {
            return new FakeSpecType { Value = _expectedForFakeSpecValue };
        }
    }
}
