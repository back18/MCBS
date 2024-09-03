using QuanLib.Core;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.FileCopyHandler
{
    public class FileCopyHandler
    {
        public FileCopyHandler(IList<IOStream> fileCopyStreams)
        {
            ArgumentNullException.ThrowIfNull(fileCopyStreams, nameof(fileCopyStreams));

            _fileCopyStreams = fileCopyStreams.AsReadOnly();
        }

        private readonly ReadOnlyCollection<IOStream> _fileCopyStreams;

        private int _position;

        public int TotalFiles => _fileCopyStreams.Count;

        public int CompletedFiles => _position;

        public long TotalBytes => _fileCopyStreams.Sum(s => s.Source.Length);

        public long CompletedBytes => _fileCopyStreams.Sum(s => s.Destination.Length);

        public IOStream? CurrentFile => CheckHelper.Range(0, _fileCopyStreams.Count - 1, _position) ? _fileCopyStreams[_position] : null;

        public async Task StartAsync()
        {
            await StartAsync(CancellationToken.None);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            for (_position = 0; _position < _fileCopyStreams.Count; _position++)
            {
                IOStream fileCopyStream = _fileCopyStreams[_position];
                await CopyAsync(fileCopyStream.Source, fileCopyStream.Destination, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    break;
            }
        }

        private static async Task CopyAsync(FileStream source, FileStream destination, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(destination, nameof(destination));

            if (source.CanSeek && source.Position != 0)
                source.Seek(0, SeekOrigin.Begin);
            if (destination.CanSeek && destination.Position != 0)
                destination.Seek(0, SeekOrigin.Begin);

            byte[] buffer = ArrayPool<byte>.Shared.Rent(1024 * 1024);
            while (true)
            {
                int length = await source.ReadAsync(buffer, CancellationToken.None);
                if (length <= 0)
                    break;

                await destination.WriteAsync(buffer.AsMemory(0, length), CancellationToken.None);

                if (cancellationToken.IsCancellationRequested)
                    break;
            }

            ArrayPool<byte>.Shared.Return(buffer);
            await destination.FlushAsync(CancellationToken.None);
        }
    }
}
