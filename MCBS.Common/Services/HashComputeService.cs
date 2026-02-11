using QuanLib.Core;
using QuanLib.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public class HashComputeService : IHashComputeService
    {
        public byte[] GetHashValue(byte[] bytes, HashType hashType)
        {
            return HashUtil.GetHashValue(bytes, hashType);
        }

        public byte[] GetHashValue(Stream stream, HashType hashType)
        {
            return HashUtil.GetHashValue(stream, hashType);
        }

        public byte[] GetHashValue(string path, HashType hashType)
        {
            return HashUtil.GetHashValue(path, hashType);
        }

        public string GetHashString(byte[] bytes, HashType hashType)
        {
            return HashUtil.GetHashString(bytes, hashType);
        }

        public string GetHashString(Stream stream, HashType hashType)
        {
            return HashUtil.GetHashString(stream, hashType);
        }

        public string GetHashString(string path, HashType hashType)
        {
            return HashUtil.GetHashString(path, hashType);
        }

        public bool HashEquals(byte[] bytes1, byte[] bytes2, HashType hashType)
        {
            return HashUtil.HashEquals(bytes1, bytes2, hashType);
        }

        public bool HashEquals(Stream stream1, Stream stream2, HashType hashType)
        {
            return HashUtil.HashEquals(stream1, stream2, hashType);
        }

        public bool HashEquals(string path1, string path2, HashType hashType)
        {
            return HashUtil.HashEquals(path1, path2, hashType);
        }
    }
}
