namespace DataContractGeneratorUnitTests.Datas
{
    public class FakeImplementation1 : IFakeInterface
    {
        public string Value { get; set; }
        public string Value2 { get; set; }
        public string Value3 { get; set; }

        public FakeImplementation1(string v)
            : this(v, null, null) { }

        public FakeImplementation1(string v, string v2)
            : this(v, v2, null) { }

        public FakeImplementation1(string v, string v2, string v3)
        {
            Value = v;
            Value2 = v2;
            Value3 = v3;
        }
    }
}
