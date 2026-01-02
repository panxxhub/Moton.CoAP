using System;
using System.IO;

namespace Moton.CoAP.Internal
{
    public sealed class MemoryBuffer : IDisposable
    {
        readonly MemoryStream _memoryStream;
        
        public MemoryBuffer(int size)
        {
            _memoryStream = new MemoryStream(size);
        }

        /// <summary>
        /// Gets the current position (bytes written) in the buffer.
        /// </summary>
        public long Position => _memoryStream.Position;

        public void Write(byte buffer)
        {
            _memoryStream.WriteByte(buffer);
        }

        public void Write(byte[] buffer)
        {
            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            _memoryStream.Write(buffer, 0, buffer.Length);
        }

        public void Write(ArraySegment<byte> buffer)
        {
            if (buffer.Array is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            _memoryStream.Write(buffer.Array, buffer.Offset, buffer.Count);
        }

        public ArraySegment<byte> GetBuffer()
        {
            return new ArraySegment<byte>(_memoryStream.ToArray(), 0, (int)_memoryStream.Length);
        }

        public void Dispose()
        {
            _memoryStream.Dispose();
        }
    }
}
