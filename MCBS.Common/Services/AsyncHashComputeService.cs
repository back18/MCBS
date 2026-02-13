using QuanLib.Core;
using QuanLib.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public class AsyncHashComputeService : IAsyncHashComputeService
    {
        public Task<byte[]> GetHashValueAsync(byte[] bytes, HashType hashType, CancellationToken cancellationToken = default)
        {
            return HashUtil.GetHashValueAsync(bytes, hashType, cancellationToken);
        }

        public Task<byte[]> GetHashValueAsync(Stream stream, HashType hashType, CancellationToken cancellationToken = default)
        {
            return HashUtil.GetHashValueAsync(stream, hashType, cancellationToken);
        }

        public Task<byte[]> GetHashValueAsync(string path, HashType hashType, CancellationToken cancellationToken = default)
        {
            return HashUtil.GetHashValueAsync(path, hashType, cancellationToken);
        }

        public Task<string> GetHashStringAsync(byte[] bytes, HashType hashType, CancellationToken cancellationToken = default)
        {
            return HashUtil.GetHashStringAsync(bytes, hashType, cancellationToken);
        }

        public Task<string> GetHashStringAsync(Stream stream, HashType hashType, CancellationToken cancellationToken = default)
        {
            return HashUtil.GetHashStringAsync(stream, hashType, cancellationToken);
        }

        public Task<string> GetHashStringAsync(string path, HashType hashType, CancellationToken cancellationToken = default)
        {
            return HashUtil.GetHashStringAsync(path, hashType, cancellationToken);
        }

        public Task<bool> HashEqualsAsync(byte[] bytes1, byte[] bytes2, HashType hashType, CancellationToken cancellationToken = default)
        {
            return HashUtil.HashEqualsAsync(bytes1, bytes2, hashType, cancellationToken);
        }

        public Task<bool> HashEqualsAsync(Stream stream1, Stream stream2, HashType hashType, CancellationToken cancellationToken = default)
        {
            return HashUtil.HashEqualsAsync(stream1, stream2, hashType, cancellationToken);
        }

        public Task<bool> HashEqualsAsync(string path1, string path2, HashType hashType, CancellationToken cancellationToken = default)
        {
            return HashUtil.HashEqualsAsync(path1, path2, hashType, cancellationToken);
        }
    }
}
