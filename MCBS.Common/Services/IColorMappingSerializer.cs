using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface IColorMappingSerializer
    {
        public void Serialize(Rgba32[] mapping, byte[] buffer);

        public void Serialize(Rgba32[] mapping, byte[] buffer, int startIndex);

        public void Serialize(Rgba32[] mapping, Stream outputStream);

        public Task SerializeAsync(Rgba32[] mapping, Stream outputStream);

        public Rgba32[] Deserialize(byte[] buffer);

        public Rgba32[] Deserialize(byte[] buffer, int startIndex, int count);

        public Rgba32[] Deserialize(Stream inputStream);

        public Task<Rgba32[]> DeserializeAsync(Stream inputStream);
    }
}
