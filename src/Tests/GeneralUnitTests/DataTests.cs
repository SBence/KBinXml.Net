using System;
using KbinXml.Net.Internal.TypeConverters;
using KbinXml.Net.Utils;
using Xunit;

namespace GeneralUnitTests
{
    public class DataTests
    {
        [Theory]
        [ClassData(typeof(ByteTestData))]
        public void ByteTest(byte value)
        {
            DoWorks(value, x => StableKbin.Converters.U8ToBytes(x).ToArray(),
                x => StableKbin.Converters.U8ToString(x),
                str =>
                {
                    var builder = new ValueListBuilder<byte>(stackalloc byte[4]);
                    U8Converter.Instance.WriteString(ref builder, str);
                    return builder.AsSpan().ToArray();
                }, bytes => U8Converter.Instance.ToString(bytes));
        }

        [Theory]
        [ClassData(typeof(SbyteTestData))]
        public void SbyteTest(sbyte value)
        {
            DoWorks(value, x => StableKbin.Converters.S8ToBytes(x).ToArray(),
                x => StableKbin.Converters.S8ToString(x),
                str =>
                {
                    var builder = new ValueListBuilder<byte>(stackalloc byte[4]);
                    S8Converter.Instance.WriteString(ref builder, str);
                    return builder.AsSpan().ToArray();
                }, bytes => S8Converter.Instance.ToString(bytes));
        }

        [Theory]
        [ClassData(typeof(Int16TestData))]
        public void Int16Test(short value)
        {
            DoWorks(value, x => StableKbin.Converters.S16ToBytes(x).ToArray(),
                x => StableKbin.Converters.S16ToString(x),
                str =>
                {
                    var builder = new ValueListBuilder<byte>(stackalloc byte[4]);
                    S16Converter.Instance.WriteString(ref builder, str);
                    return builder.AsSpan().ToArray();
                }, bytes => S16Converter.Instance.ToString(bytes));
        }

        [Theory]
        [ClassData(typeof(Int32TestData))]
        public void Int32Test(int value)
        {
            DoWorks(value, x => StableKbin.Converters.S32ToBytes(x).ToArray(),
                x => StableKbin.Converters.S32ToString(x),
                str =>
                {
                    var builder = new ValueListBuilder<byte>(stackalloc byte[4]);
                    S32Converter.Instance.WriteString(ref builder, str);
                    return builder.AsSpan().ToArray();
                }, bytes => S32Converter.Instance.ToString(bytes));
        }

        [Theory]
        [ClassData(typeof(Int64TestData))]
        public void Int64Test(long value)
        {
            DoWorks(value, x => StableKbin.Converters.S64ToBytes(x).ToArray(),
                x => StableKbin.Converters.S64ToString(x),
                str =>
                {
                    var builder = new ValueListBuilder<byte>(stackalloc byte[4]);
                    S64Converter.Instance.WriteString(ref builder, str);
                    return builder.AsSpan().ToArray();
                }, bytes => S64Converter.Instance.ToString(bytes));
        }

        [Theory]
        [ClassData(typeof(UInt16TestData))]
        public void UInt16Test(ushort value)
        {
            DoWorks(value, x => StableKbin.Converters.U16ToBytes(x).ToArray(),
                x => StableKbin.Converters.U16ToString(x),
                str =>
                {
                    var builder = new ValueListBuilder<byte>(stackalloc byte[4]);
                    U16Converter.Instance.WriteString(ref builder, str);
                    return builder.AsSpan().ToArray();
                }, bytes => U16Converter.Instance.ToString(bytes));
        }

        [Theory]
        [ClassData(typeof(UInt32TestData))]
        public void UInt32Test(uint value)
        {
            DoWorks(value, x => StableKbin.Converters.U32ToBytes(x).ToArray(),
                x => StableKbin.Converters.U32ToString(x),
                str =>
                {
                    var builder = new ValueListBuilder<byte>(stackalloc byte[4]);
                    U32Converter.Instance.WriteString(ref builder, str);
                    return builder.AsSpan().ToArray();
                }, bytes => U32Converter.Instance.ToString(bytes));
        }

        [Theory]
        [ClassData(typeof(UInt64TestData))]
        public void UInt64Test(ulong value)
        {
            DoWorks(value, x => StableKbin.Converters.U64ToBytes(x).ToArray(),
                x => StableKbin.Converters.U64ToString(x),
                str =>
                {
                    var builder = new ValueListBuilder<byte>(stackalloc byte[4]);
                    U64Converter.Instance.WriteString(ref builder, str);
                    return builder.AsSpan().ToArray();
                }, bytes => U64Converter.Instance.ToString(bytes));
        }

        [Theory]
        [ClassData(typeof(SingleTestData))]
        public void SingleTest(float value)
        {
            DoWorks(value, x => StableKbin.Converters.SingleToBytes(x).ToArray(),
                x => StableKbin.Converters.SingleToString(x),
                str =>
                {
                    var builder = new ValueListBuilder<byte>(stackalloc byte[4]);
                    FloatConverter.Instance.WriteString(ref builder, str);
                    return builder.AsSpan().ToArray();
                }, bytes => FloatConverter.Instance.ToString(bytes));
        }

        [Theory]
        [ClassData(typeof(DoubleTestData))]
        public void DoubleTest(double value)
        {
            DoWorks(value, x => StableKbin.Converters.DoubleToBytes(x).ToArray(),
                x => StableKbin.Converters.DoubleToString(x),
                str =>
                {
                    var builder = new ValueListBuilder<byte>(stackalloc byte[4]);
                    DoubleConverter.Instance.WriteString(ref builder, str);
                    return builder.AsSpan().ToArray();
                }, bytes => DoubleConverter.Instance.ToString(bytes));
        }

        [Theory]
        [ClassData(typeof(Ip4TestData))]
        public void Ip4Test(string value)
        {
            DoWorks(value, x => StableKbin.Converters.Ip4ToBytes(x).ToArray(),
                x => StableKbin.Converters.Ip4ToString(x),
                str =>
                {
                    var builder = new ValueListBuilder<byte>(stackalloc byte[4]);
                    Ip4Converter.Instance.WriteString(ref builder, str);
                    return builder.AsSpan().ToArray();
                }, bytes => Ip4Converter.Instance.ToString(bytes));
        }

        private static void DoWorks(object value,
            Func<string, byte[]> toBytesOld,
            Func<byte[], string> toStringOld,
            Func<string, byte[]> toBytesNew,
            Func<byte[], string> toStringNew)
        {
            var str = value.ToString();

            var bytes = toBytesOld(str);
            var bytes2 = toBytesNew(str);

            var output = toStringOld(bytes);
            var output2 = toStringNew(bytes2);

            Assert.Equal(bytes, bytes2);
            Assert.Equal(output, output2);
        }
    }
}
