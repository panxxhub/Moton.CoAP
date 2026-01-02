using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moton.CoAP.Protocol.BlockTransfer;

namespace Moton.CoAP.Tests
{
    [TestClass]
    public class CoapBlockTransfer_Tests
    {
        #region Encode/Decode Round-Trip Tests

        [TestMethod]
        public void BlockTransfer_EncodeAndDecode_Should_PreserveValues_Block0_1024_NoMore()
        {
            var original = new CoapBlockTransferOptionValue
            {
                Number = 0,
                Size = 1024,
                HasFollowingBlocks = false
            };

            var encoded = CoapBlockTransferOptionValueEncoder.Encode(original);
            var decoded = CoapBlockTransferOptionValueDecoder.Decode(encoded);

            Assert.AreEqual(original.Number, decoded.Number);
            Assert.AreEqual(original.Size, decoded.Size);
            Assert.AreEqual(original.HasFollowingBlocks, decoded.HasFollowingBlocks);
        }

        [TestMethod]
        public void BlockTransfer_EncodeAndDecode_Should_PreserveValues_Block5_512_HasMore()
        {
            var original = new CoapBlockTransferOptionValue
            {
                Number = 5,
                Size = 512,
                HasFollowingBlocks = true
            };

            var encoded = CoapBlockTransferOptionValueEncoder.Encode(original);
            var decoded = CoapBlockTransferOptionValueDecoder.Decode(encoded);

            Assert.AreEqual(original.Number, decoded.Number);
            Assert.AreEqual(original.Size, decoded.Size);
            Assert.AreEqual(original.HasFollowingBlocks, decoded.HasFollowingBlocks);
        }

        [TestMethod]
        public void BlockTransfer_EncodeAndDecode_Should_PreserveValues_Block100_16_NoMore()
        {
            var original = new CoapBlockTransferOptionValue
            {
                Number = 100,
                Size = 16,
                HasFollowingBlocks = false
            };

            var encoded = CoapBlockTransferOptionValueEncoder.Encode(original);
            var decoded = CoapBlockTransferOptionValueDecoder.Decode(encoded);

            Assert.AreEqual(original.Number, decoded.Number);
            Assert.AreEqual(original.Size, decoded.Size);
            Assert.AreEqual(original.HasFollowingBlocks, decoded.HasFollowingBlocks);
        }

        #endregion

        #region Block Size Encoding Tests (SZX values)

        [TestMethod]
        public void BlockSize_16_Should_EncodeTo_SZX0()
        {
            var value = new CoapBlockTransferOptionValue { Number = 0, Size = 16, HasFollowingBlocks = false };
            var encoded = CoapBlockTransferOptionValueEncoder.Encode(value);
            // SZX = 0 for 16 bytes
            Assert.AreEqual(0U, encoded & 0x7);
        }

        [TestMethod]
        public void BlockSize_32_Should_EncodeTo_SZX1()
        {
            var value = new CoapBlockTransferOptionValue { Number = 0, Size = 32, HasFollowingBlocks = false };
            var encoded = CoapBlockTransferOptionValueEncoder.Encode(value);
            // SZX = 1 for 32 bytes
            Assert.AreEqual(1U, encoded & 0x7);
        }

        [TestMethod]
        public void BlockSize_64_Should_EncodeTo_SZX2()
        {
            var value = new CoapBlockTransferOptionValue { Number = 0, Size = 64, HasFollowingBlocks = false };
            var encoded = CoapBlockTransferOptionValueEncoder.Encode(value);
            // SZX = 2 for 64 bytes
            Assert.AreEqual(2U, encoded & 0x7);
        }

        [TestMethod]
        public void BlockSize_128_Should_EncodeTo_SZX3()
        {
            var value = new CoapBlockTransferOptionValue { Number = 0, Size = 128, HasFollowingBlocks = false };
            var encoded = CoapBlockTransferOptionValueEncoder.Encode(value);
            // SZX = 3 for 128 bytes
            Assert.AreEqual(3U, encoded & 0x7);
        }

        [TestMethod]
        public void BlockSize_256_Should_EncodeTo_SZX4()
        {
            var value = new CoapBlockTransferOptionValue { Number = 0, Size = 256, HasFollowingBlocks = false };
            var encoded = CoapBlockTransferOptionValueEncoder.Encode(value);
            // SZX = 4 for 256 bytes
            Assert.AreEqual(4U, encoded & 0x7);
        }

        [TestMethod]
        public void BlockSize_512_Should_EncodeTo_SZX5()
        {
            var value = new CoapBlockTransferOptionValue { Number = 0, Size = 512, HasFollowingBlocks = false };
            var encoded = CoapBlockTransferOptionValueEncoder.Encode(value);
            // SZX = 5 for 512 bytes
            Assert.AreEqual(5U, encoded & 0x7);
        }

        [TestMethod]
        public void BlockSize_1024_Should_EncodeTo_SZX6()
        {
            var value = new CoapBlockTransferOptionValue { Number = 0, Size = 1024, HasFollowingBlocks = false };
            var encoded = CoapBlockTransferOptionValueEncoder.Encode(value);
            // SZX = 6 for 1024 bytes
            Assert.AreEqual(6U, encoded & 0x7);
        }

        #endregion

        #region HasFollowingBlocks (M bit) Tests

        [TestMethod]
        public void HasFollowingBlocks_True_Should_SetMBit()
        {
            var value = new CoapBlockTransferOptionValue { Number = 0, Size = 1024, HasFollowingBlocks = true };
            var encoded = CoapBlockTransferOptionValueEncoder.Encode(value);
            // M bit is bit 3 (0x8)
            Assert.IsTrue((encoded & 0x8) != 0);
        }

        [TestMethod]
        public void HasFollowingBlocks_False_Should_ClearMBit()
        {
            var value = new CoapBlockTransferOptionValue { Number = 0, Size = 1024, HasFollowingBlocks = false };
            var encoded = CoapBlockTransferOptionValueEncoder.Encode(value);
            // M bit is bit 3 (0x8)
            Assert.IsTrue((encoded & 0x8) == 0);
        }

        #endregion

        #region Block Number Tests

        [TestMethod]
        public void BlockNumber_Should_BeEncodedInUpperBits()
        {
            var value = new CoapBlockTransferOptionValue { Number = 15, Size = 1024, HasFollowingBlocks = false };
            var encoded = CoapBlockTransferOptionValueEncoder.Encode(value);
            // Block number is in bits 4+ (shifted left by 4)
            Assert.AreEqual(15U, encoded >> 4);
        }

        [TestMethod]
        public void BlockNumber_Large_Should_BeEncodedCorrectly()
        {
            var value = new CoapBlockTransferOptionValue { Number = 1000, Size = 1024, HasFollowingBlocks = false };
            var encoded = CoapBlockTransferOptionValueEncoder.Encode(value);
            var decoded = CoapBlockTransferOptionValueDecoder.Decode(encoded);
            Assert.AreEqual(1000, decoded.Number);
        }

        #endregion

        #region Decoder Tests

        [TestMethod]
        public void Decoder_Should_DecodeBlockSize_16()
        {
            // SZX = 0 means 16 bytes
            var encoded = 0U; // NUM=0, M=0, SZX=0
            var decoded = CoapBlockTransferOptionValueDecoder.Decode(encoded);
            Assert.AreEqual(16, decoded.Size);
        }

        [TestMethod]
        public void Decoder_Should_DecodeBlockSize_1024()
        {
            // SZX = 6 means 1024 bytes
            var encoded = 6U; // NUM=0, M=0, SZX=6
            var decoded = CoapBlockTransferOptionValueDecoder.Decode(encoded);
            Assert.AreEqual(1024, decoded.Size);
        }

        [TestMethod]
        public void Decoder_Should_DecodeHasFollowingBlocks_True()
        {
            // M bit set (0x8)
            var encoded = 0x8U; // NUM=0, M=1, SZX=0
            var decoded = CoapBlockTransferOptionValueDecoder.Decode(encoded);
            Assert.IsTrue(decoded.HasFollowingBlocks);
        }

        [TestMethod]
        public void Decoder_Should_DecodeHasFollowingBlocks_False()
        {
            // M bit clear
            var encoded = 0U; // NUM=0, M=0, SZX=0
            var decoded = CoapBlockTransferOptionValueDecoder.Decode(encoded);
            Assert.IsFalse(decoded.HasFollowingBlocks);
        }

        #endregion
    }
}
