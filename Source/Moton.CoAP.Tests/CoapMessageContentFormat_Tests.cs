using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moton.CoAP.Protocol.Options;

namespace Moton.CoAP.Tests
{
    [TestClass]
    public class CoapMessageContentFormat_Tests
    {
        [TestMethod]
        public void ApplicationCbor_Should_HaveCorrectValue()
        {
            Assert.AreEqual(60, (int)CoapMessageContentFormat.ApplicationCbor);
        }

        [TestMethod]
        public void ApplicationCwt_Should_HaveCorrectValue()
        {
            Assert.AreEqual(61, (int)CoapMessageContentFormat.ApplicationCwt);
        }

        [TestMethod]
        public void ApplicationSenmlCbor_Should_HaveCorrectValue()
        {
            Assert.AreEqual(112, (int)CoapMessageContentFormat.ApplicationSenmlCbor);
        }

        [TestMethod]
        public void ApplicationSensmlCbor_Should_HaveCorrectValue()
        {
            Assert.AreEqual(113, (int)CoapMessageContentFormat.ApplicationSensmlCbor);
        }

        [TestMethod]
        public void TextPlain_Should_HaveCorrectValue()
        {
            Assert.AreEqual(0, (int)CoapMessageContentFormat.TextPlain);
        }

        [TestMethod]
        public void ApplicationJson_Should_HaveCorrectValue()
        {
            Assert.AreEqual(50, (int)CoapMessageContentFormat.ApplicationJson);
        }

        [TestMethod]
        public void ApplicationOctetStream_Should_HaveCorrectValue()
        {
            Assert.AreEqual(42, (int)CoapMessageContentFormat.ApplicationOctetStream);
        }
    }
}
