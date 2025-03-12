using System.Text;
using System.Xml;
using System.Xml.Linq;
using KbinXml.Net;
using Xunit;
using Xunit.Abstractions;

namespace GeneralUnitTests
{
    public class SpecialCaseTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public SpecialCaseTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [Fact]
        public void TestEmptyNode()
        {
            var xml = "<root><empty></empty><self_closing /></root>";
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            var result2 = new StableKbin.XmlReader(kbin).ReadLinq();
            Assert.Equal(result2.ToString(SaveOptions.DisableFormatting), result.ToString(SaveOptions.DisableFormatting));
        }

        [Theory]
        [InlineData("<root><node __type=\"str\">特殊文字：&amp;&lt;&gt;'\"</node></root>", KnownEncodings.UTF8)]
        [InlineData("<root><node __type=\"str\">日本語テスト</node></root>", KnownEncodings.ShiftJIS)]
        public void TestSpecialCharactersAndEncoding(string xml, KnownEncodings encoding)
        {
            var kbin = KbinConverter.Write(xml, encoding);
            var result = KbinConverter.ReadXmlLinq(kbin);
            Assert.Equal(xml, result.ToString(SaveOptions.DisableFormatting));
        }

        [Fact]
        public void TestDeepNestedNodes()
        {
            var xml = "<root><a><b><c><d><e __type=\"str\">深层嵌套测试</e></d></c></b></a></root>";
            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            Assert.Equal(xml, result.ToString(SaveOptions.DisableFormatting));
        }

        [Fact]
        public void TestLargeXml()
        {
            var builder = new StringBuilder("<root>");
            for (int i = 0; i < 1000; i++)
            {
                builder.AppendFormat("<item __type=\"u32\" id=\"{0}\">{0}</item>", i);
            }
            builder.Append("</root>");
            var xml = builder.ToString();

            var kbin = KbinConverter.Write(xml, KnownEncodings.UTF8);
            var result = KbinConverter.ReadXmlLinq(kbin);
            Assert.Equal(xml, result.ToString(SaveOptions.DisableFormatting));
        }

        [Fact]
        public void TestInvalidXmlThrowsException()
        {
            var invalidXml = "<root><unclosed>";
            Assert.Throws<XmlException>(() => KbinConverter.Write(invalidXml, KnownEncodings.UTF8));
        }
    }
}