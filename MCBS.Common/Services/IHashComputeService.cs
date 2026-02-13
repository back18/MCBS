using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface IHashComputeService
    {
        public byte[] GetHashValue(byte[] bytes, HashType hashType);

        public byte[] GetHashValue(Stream stream, HashType hashType);

        public byte[] GetHashValue(string path, HashType hashType);

        public string GetHashString(byte[] bytes, HashType hashType);

        public string GetHashString(Stream stream, HashType hashType);

        public string GetHashString(string path, HashType hashType);

        public bool HashEquals(byte[] bytes1, byte[] bytes2, HashType hashType);

        public bool HashEquals(Stream stream1, Stream stream2, HashType hashType);

        public bool HashEquals(string path1, string path2, HashType hashType);
    }
}
