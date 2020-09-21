using System;
using System.Collections.Generic;

namespace DataContractGeneratorUnitTests.Datas
{
    public class FakeContract
    {
        public string String { get; set; }
        public char Char { get; set; }
        public short Short { get; set; }
        public ushort UShort { get; set; }
        public long Long { get; set; }
        public ulong ULong { get; set; }
        public int Int { get; set; }
        public uint UInt { get; set; }
        public byte Byte { get; set; }
        public sbyte SByte { get; set; }
        public float Float { get; set; }
        public double Double { get; set; }
        public bool Bool { get; set; }
        public decimal Decimal { get; set; }
        public int? NullableInt { get; set; }
        public Tuple<int> SmallTuple { get; set; }
        public Tuple<int, string, bool, char, double, float, decimal, Tuple<string>> BigTuple { get; set; }
        public KeyValuePair<byte, string> KeyValuePair { get; set; }
        public FakeEnum Enum { get; set; }
        public DateTime DateTime { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
        public Guid Guid { get; set; }
        public IFakeInterface Interface { get; set; }
        public int[] ArrayOfInt { get; set; }
        public Dictionary<bool, IFakeInterface> Dictionary { get; set; }
        public List<IFakeInterface> BasicList { get; set; }
        public IReadOnlyList<IFakeInterface> ReadOnlyList { get; set; }
        public IEnumerable<IFakeInterface> Enumerable { get; set; }
        public IReadOnlyDictionary<FakeEnum, string> ReadOnlyDictionary { get; set; }
        public FakeSpecType FakeSpec { get; set; }
        public FakeContract Recursion { get; set; }
    }
}
