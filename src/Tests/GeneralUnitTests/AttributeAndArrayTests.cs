using System.Linq;
using System.Text;
using System.Xml.Linq;
using KbinXml.Net;
using Xunit;
using Xunit.Abstractions;

namespace GeneralUnitTests
{
    public class AttributeAndArrayTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public AttributeAndArrayTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [Fact]
        public void TestMultipleAttributes()
        {
            var xml = "<root id=\"1\" name=\"test\" value=\"123\"><node attr1=\"val1\" attr2=\"val2\" /></root>";
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            Assert.Equal(xml, result.ToString(SaveOptions.DisableFormatting));
        }

        [Fact]
        public void TestEmptyArray()
        {
            var xml = "<root><array __type=\"s32\" __count=\"0\"></array></root>";
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            Assert.Equal(xml, result.ToString(SaveOptions.DisableFormatting));
        }

        [Fact]
        public void TestLargeArray()
        {
            var values = string.Join(" ", Enumerable.Range(1, 10000));
            var xml = $"<root><array __type=\"s32\" __count=\"10000\">{values}</array></root>";
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            Assert.Equal(xml, result.ToString(SaveOptions.DisableFormatting));
        }

        [Fact]
        public void TestMixedArrayTypes()
        {
            var xml = "<root>" +
                      "<array1 __type=\"s8\" __count=\"3\">1 2 3</array1>" +
                      "<array2 __type=\"u16\" __count=\"3\">1000 2000 3000</array2>" +
                      "<array3 __type=\"s32\" __count=\"3\">-1 -2 -3</array3>" +
                      "</root>";
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            Assert.Equal(xml, result.ToString(SaveOptions.DisableFormatting));
        }

        [Fact]
        public void TestInvalidArrayCount()
        {
            var xml = "<root><array __type=\"s32\" __count=\"3\">1 2</array></root>";
            Assert.Throws<KbinArrayCountMissMatchException>(() => KbinConverter.Write(xml, KnownEncodings.UTF8));
        }

        [Fact]
        public void TestInvalidArrayType()
        {
            var xml = "<root><array __type=\"invalid\" __count=\"1\">1</array></root>";
            Assert.Throws<KbinTypeNotFoundException>(() => KbinConverter.Write(xml, KnownEncodings.UTF8));
        }
    }
}