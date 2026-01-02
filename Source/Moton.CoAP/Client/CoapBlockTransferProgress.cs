using System;

namespace Moton.CoAP.Client
{
    /// <summary>
    /// Progress information for block-wise transfers (Block1 or Block2).
    /// </summary>
    public sealed class CoapBlockTransferProgress
    {
        /// <summary>
        /// The direction of the transfer.
        /// </summary>
        public CoapBlockTransferDirection Direction { get; init; }

        /// <summary>
        /// Current block number (0-based).
        /// </summary>
        public int BlockNumber { get; init; }

        /// <summary>
        /// Total number of blocks (estimated for Block2 until last block).
        /// </summary>
        public int TotalBlocks { get; init; }

        /// <summary>
        /// Block size in bytes.
        /// </summary>
        public int BlockSize { get; init; }

        /// <summary>
        /// Total bytes transferred so far.
        /// </summary>
        public int BytesTransferred { get; init; }

        /// <summary>
        /// Total payload size in bytes (known for Block1, estimated for Block2).
        /// </summary>
        public int TotalBytes { get; init; }

        /// <summary>
        /// Progress percentage (0-100).
        /// </summary>
        public double ProgressPercent => TotalBytes > 0 ? (BytesTransferred * 100.0 / TotalBytes) : 0;

        /// <summary>
        /// True if this is the last block.
        /// </summary>
        public bool IsComplete => BlockNumber + 1 >= TotalBlocks;
    }

    /// <summary>
    /// Direction of block-wise transfer.
    /// </summary>
    public enum CoapBlockTransferDirection
    {
        /// <summary>
        /// Block1: Uploading payload to server.
        /// </summary>
        Upload,

        /// <summary>
        /// Block2: Downloading payload from server.
        /// </summary>
        Download
    }

    /// <summary>
    /// Callback for block transfer progress notifications.
    /// </summary>
    /// <param name="progress">The progress information.</param>
    public delegate void CoapBlockTransferProgressHandler(CoapBlockTransferProgress progress);
}
