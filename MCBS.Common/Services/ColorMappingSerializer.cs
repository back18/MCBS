using QuanLib.Core;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public class ColorMappingSerializer : IColorMappingSerializer
    {
        private const int BUFFER_SIZE = 65536;

        public void Serialize(Rgba32[] mapping, byte[] buffer)
        {
            Serialize(mapping, buffer, 0);
        }

        public void Serialize(Rgba32[] mapping, byte[] buffer, int startIndex)
        {
            ArgumentNullException.ThrowIfNull(mapping, nameof(mapping));
            ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
            ThrowHelper.ArgumentOutOfRange(0, buffer.Length - 1, startIndex, nameof(startIndex));
            ThrowHelper.ArrayLengthOutOfMin(mapping.Length * 4 + startIndex, buffer, nameof(buffer));

            int index = startIndex;
            foreach (Rgba32 color in mapping)
            {
                buffer[index++] = color.R;
                buffer[index++] = color.G;
                buffer[index++] = color.B;
                buffer[index++] = color.A;
            }
        }

        public void Serialize(Rgba32[] mapping, Stream outputStream)
        {
            ArgumentNullException.ThrowIfNull(mapping, nameof(mapping));
            ArgumentNullException.ThrowIfNull(outputStream, nameof(outputStream));
            ThrowHelper.StreamNotSupportWrite(outputStream);

            byte[] buffer = ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
            int bufferLength = (buffer.Length / 4) * 4;
            int index = 0;

            try
            {
                foreach (Rgba32 color in mapping)
                {
                    buffer[index++] = color.R;
                    buffer[index++] = color.G;
                    buffer[index++] = color.B;
                    buffer[index++] = color.A;

                    if (index == bufferLength)
                    {
                        outputStream.Write(buffer, 0, bufferLength);
                        index = 0;
                    }
                }

                outputStream.Flush();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public async Task SerializeAsync(Rgba32[] mapping, Stream outputStream)
        {
            ArgumentNullException.ThrowIfNull(mapping, nameof(mapping));
            ArgumentNullException.ThrowIfNull(outputStream, nameof(outputStream));
            ThrowHelper.StreamNotSupportWrite(outputStream);

            byte[] buffer = ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
            int bufferLength = (buffer.Length / 4) * 4;
            int index = 0;

            try
            {
                foreach (Rgba32 color in mapping)
                {
                    buffer[index++] = color.R;
                    buffer[index++] = color.G;
                    buffer[index++] = color.B;
                    buffer[index++] = color.A;

                    if (index == bufferLength)
                    {
                        await outputStream.WriteAsync(buffer.AsMemory(0, bufferLength)).ConfigureAwait(false);
                        index = 0;
                    }
                }

                await outputStream.FlushAsync().ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public Rgba32[] Deserialize(byte[] buffer)
        {
            return Deserialize(buffer, 0, buffer.Length);
        }

        public Rgba32[] Deserialize(byte[] buffer, int startIndex, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
            ThrowHelper.ArgumentOutOfRange(0, buffer.Length - 1, startIndex, nameof(startIndex));
            if (count % 4 != 0)
                throw new ArgumentException($"读取长度应该为4的倍数，实际长度为 {count}", nameof(count));
            ThrowHelper.ArrayLengthOutOfMin(startIndex + count, buffer, nameof(buffer));

            Rgba32[] mapping = new Rgba32[count / 4];
            int index = 0;
            int endIndex = startIndex + count;

            for (int i = startIndex; i < endIndex; i += 4)
                mapping[index++] = new Rgba32(buffer[i], buffer[i + 1], buffer[i + 2], buffer[i + 3]);

            return mapping;
        }

        public Rgba32[] Deserialize(Stream inputStream)
        {
            ArgumentNullException.ThrowIfNull(inputStream, nameof(inputStream));
            ThrowHelper.StreamNotSupportRead(inputStream);

            if (inputStream.CanSeek)
                return DeserializeWhereCanSeek(inputStream);
            else
                return DeserializeWhereCanRead(inputStream);
        }

        private static Rgba32[] DeserializeWhereCanSeek(Stream inputStream)
        {
            int total = (int)(inputStream.Length - inputStream.Position);
            total = (total / 4) * 4;
            int count = 0;

            byte[] buffer = ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
            Rgba32[] mapping = new Rgba32[total / 4];
            int index = 0;

            try
            {
                while (count < total)
                {
                    int bytesRead = Math.Min(total - count, BUFFER_SIZE);
                    inputStream.ReadExactly(buffer, 0, bytesRead);

                    for (int i = 0; i < bytesRead; i += 4)
                        mapping[index++] = new Rgba32(buffer[i], buffer[i + 1], buffer[i + 2], buffer[i + 3]);

                    count += bytesRead;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return mapping;
        }

        private static Rgba32[] DeserializeWhereCanRead(Stream inputStream)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
            int bufferLength = (buffer.Length / 4) * 4;
            List<Rgba32> mapping = [];

            try
            {
                int bytesRead = 0;
                int offset = 0;

                while ((bytesRead = inputStream.Read(buffer, offset, bufferLength - offset)) != 0)
                {
                    int totalBytes = bytesRead + offset;
                    offset = ProcessColorChunkWithRemainder(mapping, buffer, totalBytes);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return mapping.ToArray();
        }

        public Task<Rgba32[]> DeserializeAsync(Stream inputStream)
        {
            ArgumentNullException.ThrowIfNull(inputStream, nameof(inputStream));
            ThrowHelper.StreamNotSupportRead(inputStream);

            if (inputStream.CanSeek)
                return DeserializeWhereCanSeekAsync(inputStream);
            else
                return DeserializeWhereCanReadAsync(inputStream);
        }

        private static async Task<Rgba32[]> DeserializeWhereCanSeekAsync(Stream inputStream)
        {
            int total = (int)(inputStream.Length - inputStream.Position);
            total = (total / 4) * 4;
            int count = 0;

            byte[] buffer = ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
            Rgba32[] mapping = new Rgba32[total / 4];
            int index = 0;

            try
            {
                while (count < total)
                {
                    int bytesRead = Math.Min(total - count, BUFFER_SIZE);
                    await inputStream.ReadExactlyAsync(buffer, 0, bytesRead).ConfigureAwait(false);

                    for (int i = 0; i < bytesRead; i += 4)
                        mapping[index++] = new Rgba32(buffer[i], buffer[i + 1], buffer[i + 2], buffer[i + 3]);

                    count += bytesRead;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return mapping;
        }

        private static async Task<Rgba32[]> DeserializeWhereCanReadAsync(Stream inputStream)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
            int bufferLength = (buffer.Length / 4) * 4;
            List<Rgba32> mapping = [];

            try
            {
                int bytesRead = 0;
                int offset = 0;

                while ((bytesRead = await inputStream.ReadAsync(buffer.AsMemory(offset, bufferLength - offset)).ConfigureAwait(false)) != 0)
                {
                    int totalBytes = bytesRead + offset;
                    offset = ProcessColorChunkWithRemainder(mapping, buffer, totalBytes);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return mapping.ToArray();
        }

        private static int ProcessColorChunkWithRemainder(List<Rgba32> mapping, byte[] buffer, int totalBytes)
        {
            for (int i = 0; i + 4 <= totalBytes; i += 4)
                mapping.Add(new Rgba32(buffer[i], buffer[i + 1], buffer[i + 2], buffer[i + 3]));

            if (totalBytes % 4 != 0)
            {
                for (int i = 0, j = (totalBytes / 4) * 4; i < totalBytes % 4; i++, j++)
                    buffer[i] = buffer[j];
                return totalBytes % 4;
            }

            return 0;
        }
    }
}
