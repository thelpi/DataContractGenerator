using System;
using System.Collections.Generic;
using DataContractGenerator;
using Xunit;

namespace DataContractGeneratorUnitTests
{
    public class DataContractGeneratorProviderTests
    {
        [Fact]
        public void CreateInstanceOf_A_Success()
        {
            var objectOf = new DataContractGeneratorProvider().GenerateRandom<A>();

            Assert.NotNull(objectOf);
        }
    }

    class A
    {
        public int Toto { get; set; }
        public string Tutu { get; set; }
        public B Titi { get; set; }
        public IReadOnlyDictionary<DateTimeKind, D> Dz { get; set; }
    }

    class B
    {
        public IReadOnlyCollection<C> Cs { get; set; }
        public int? Bee { get; set; }
        public decimal? Koala { get; set; }
        public DateTime[] Pingus { get; set; }
    }

    class C
    {
        public decimal Machin { get; set; }
        public DateTime Bidule { get; set; }
        public DateTimeKind Dtk { get; set; }
    }

    class D
    {
        public Tuple<string, bool> Rien { get; set; }
        public KeyValuePair<DateTime, C> Tout { get; set; }
    }
}
