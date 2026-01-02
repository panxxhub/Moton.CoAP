using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moton.CoAP.Protocol.Options;

namespace Moton.CoAP.Tests
{
    [TestClass]
    public class CoapContentFormatHelper_Tests
    {
        #region IsCborFormat Tests

        [TestMethod]
        public void IsCborFormat_Should_ReturnTrue_ForApplicationCbor()
        {
            Assert.IsTrue(CoapContentFormatHelper.IsCborFormat(CoapMessageContentFormat.ApplicationCbor));
        }

        [TestMethod]
        public void IsCborFormat_Should_ReturnTrue_ForApplicationSenmlCbor()
        {
            Assert.IsTrue(CoapContentFormatHelper.IsCborFormat(CoapMessageContentFormat.ApplicationSenmlCbor));
        }

        [TestMethod]
        public void IsCborFormat_Should_ReturnTrue_ForApplicationSensmlCbor()
        {
            Assert.IsTrue(CoapContentFormatHelper.IsCborFormat(CoapMessageContentFormat.ApplicationSensmlCbor));
        }

        [TestMethod]
        public void IsCborFormat_Should_ReturnTrue_ForApplicationCwt()
        {
            Assert.IsTrue(CoapContentFormatHelper.IsCborFormat(CoapMessageContentFormat.ApplicationCwt));
        }

        [TestMethod]
        public void IsCborFormat_Should_ReturnFalse_ForApplicationJson()
        {
            Assert.IsFalse(CoapContentFormatHelper.IsCborFormat(CoapMessageContentFormat.ApplicationJson));
        }

        [TestMethod]
        public void IsCborFormat_Should_ReturnFalse_ForTextPlain()
        {
            Assert.IsFalse(CoapContentFormatHelper.IsCborFormat(CoapMessageContentFormat.TextPlain));
        }

        [TestMethod]
        public void IsCborFormat_Should_ReturnFalse_ForApplicationOctetStream()
        {
            Assert.IsFalse(CoapContentFormatHelper.IsCborFormat(CoapMessageContentFormat.ApplicationOctetStream));
        }

        #endregion

        #region IsJsonFormat Tests

        [TestMethod]
        public void IsJsonFormat_Should_ReturnTrue_ForApplicationJson()
        {
            Assert.IsTrue(CoapContentFormatHelper.IsJsonFormat(CoapMessageContentFormat.ApplicationJson));
        }

        [TestMethod]
        public void IsJsonFormat_Should_ReturnTrue_ForApplicationSenmlJson()
        {
            Assert.IsTrue(CoapContentFormatHelper.IsJsonFormat(CoapMessageContentFormat.ApplicationSenmlJson));
        }

        [TestMethod]
        public void IsJsonFormat_Should_ReturnFalse_ForApplicationCbor()
        {
            Assert.IsFalse(CoapContentFormatHelper.IsJsonFormat(CoapMessageContentFormat.ApplicationCbor));
        }

        #endregion

        #region IsXmlFormat Tests

        [TestMethod]
        public void IsXmlFormat_Should_ReturnTrue_ForApplicationXml()
        {
            Assert.IsTrue(CoapContentFormatHelper.IsXmlFormat(CoapMessageContentFormat.ApplicationXml));
        }

        [TestMethod]
        public void IsXmlFormat_Should_ReturnTrue_ForApplicationSenmlXml()
        {
            Assert.IsTrue(CoapContentFormatHelper.IsXmlFormat(CoapMessageContentFormat.ApplicationSenmlXml));
        }

        [TestMethod]
        public void IsXmlFormat_Should_ReturnFalse_ForApplicationJson()
        {
            Assert.IsFalse(CoapContentFormatHelper.IsXmlFormat(CoapMessageContentFormat.ApplicationJson));
        }

        #endregion

        #region GetMediaType Tests

        [TestMethod]
        public void GetMediaType_Should_ReturnCorrectValue_ForApplicationCbor()
        {
            Assert.AreEqual("application/cbor", CoapContentFormatHelper.GetMediaType(CoapMessageContentFormat.ApplicationCbor));
        }

        [TestMethod]
        public void GetMediaType_Should_ReturnCorrectValue_ForApplicationJson()
        {
            Assert.AreEqual("application/json", CoapContentFormatHelper.GetMediaType(CoapMessageContentFormat.ApplicationJson));
        }

        [TestMethod]
        public void GetMediaType_Should_ReturnCorrectValue_ForTextPlain()
        {
            Assert.AreEqual("text/plain; charset=utf-8", CoapContentFormatHelper.GetMediaType(CoapMessageContentFormat.TextPlain));
        }

        [TestMethod]
        public void GetMediaType_Should_ReturnCorrectValue_ForSenmlCbor()
        {
            Assert.AreEqual("application/senml+cbor", CoapContentFormatHelper.GetMediaType(CoapMessageContentFormat.ApplicationSenmlCbor));
        }

        #endregion

        #region TryParseMediaType Tests

        [TestMethod]
        public void TryParseMediaType_Should_ParseApplicationCbor()
        {
            var result = CoapContentFormatHelper.TryParseMediaType("application/cbor", out var format);
            Assert.IsTrue(result);
            Assert.AreEqual(CoapMessageContentFormat.ApplicationCbor, format);
        }

        [TestMethod]
        public void TryParseMediaType_Should_ParseApplicationJson()
        {
            var result = CoapContentFormatHelper.TryParseMediaType("application/json", out var format);
            Assert.IsTrue(result);
            Assert.AreEqual(CoapMessageContentFormat.ApplicationJson, format);
        }

        [TestMethod]
        public void TryParseMediaType_Should_ParseTextPlainWithCharset()
        {
            var result = CoapContentFormatHelper.TryParseMediaType("text/plain; charset=utf-8", out var format);
            Assert.IsTrue(result);
            Assert.AreEqual(CoapMessageContentFormat.TextPlain, format);
        }

        [TestMethod]
        public void TryParseMediaType_Should_BeCaseInsensitive()
        {
            var result = CoapContentFormatHelper.TryParseMediaType("APPLICATION/CBOR", out var format);
            Assert.IsTrue(result);
            Assert.AreEqual(CoapMessageContentFormat.ApplicationCbor, format);
        }

        [TestMethod]
        public void TryParseMediaType_Should_ReturnFalse_ForUnknownType()
        {
            var result = CoapContentFormatHelper.TryParseMediaType("application/unknown", out var format);
            Assert.IsFalse(result);
            Assert.AreEqual(CoapMessageContentFormat.ApplicationOctetStream, format);
        }

        [TestMethod]
        public void TryParseMediaType_Should_ReturnFalse_ForNullInput()
        {
            var result = CoapContentFormatHelper.TryParseMediaType(null, out var format);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryParseMediaType_Should_ReturnFalse_ForEmptyInput()
        {
            var result = CoapContentFormatHelper.TryParseMediaType("", out var format);
            Assert.IsFalse(result);
        }

        #endregion

        #region IsBinaryFormat Tests

        [TestMethod]
        public void IsBinaryFormat_Should_ReturnTrue_ForCbor()
        {
            Assert.IsTrue(CoapContentFormatHelper.IsBinaryFormat(CoapMessageContentFormat.ApplicationCbor));
        }

        [TestMethod]
        public void IsBinaryFormat_Should_ReturnTrue_ForOctetStream()
        {
            Assert.IsTrue(CoapContentFormatHelper.IsBinaryFormat(CoapMessageContentFormat.ApplicationOctetStream));
        }

        [TestMethod]
        public void IsBinaryFormat_Should_ReturnFalse_ForJson()
        {
            Assert.IsFalse(CoapContentFormatHelper.IsBinaryFormat(CoapMessageContentFormat.ApplicationJson));
        }

        [TestMethod]
        public void IsBinaryFormat_Should_ReturnFalse_ForTextPlain()
        {
            Assert.IsFalse(CoapContentFormatHelper.IsBinaryFormat(CoapMessageContentFormat.TextPlain));
        }

        #endregion
    }
}
