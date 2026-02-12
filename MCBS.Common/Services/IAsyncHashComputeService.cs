using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface IAsyncHashComputeService
    {
        public Task<byte[]> GetHashValueAsync(byte[] bytes, HashType hashType, CancellationToken cancellationToken = default);

        public Task<byte[]> GetHashValueAsync(Stream stream, HashType hashType, CancellationToken cancellationToken = default);

        public Task<byte[]> GetHashValueAsync(string path, HashType hashType, CancellationToken cancellationToken = default);

        public Task<string> GetHashStringAsync(byte[] bytes, HashType hashType, CancellationToken cancellationToken = default);

        public Task<string> GetHashStringAsync(Stream stream, HashType hashType, CancellationToken cancellationToken = default);

        public Task<string> GetHashStringAsync(string path, HashType hashType, CancellationToken cancellationToken = default);

        public Task<bool> HashEqualsAsync(byte[] bytes1, byte[] bytes2, HashType hashType, CancellationToken cancellationToken = default);

        public Task<bool> HashEqualsAsync(Stream stream1, Stream stream2, HashType hashType, CancellationToken cancellationToken = default);

        public Task<bool> HashEqualsAsync(string path1, string path2, HashType hashType, CancellationToken cancellationToken = default);
    }
}
