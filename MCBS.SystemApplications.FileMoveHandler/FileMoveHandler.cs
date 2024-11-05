using MCBS.BlockForms.FileSystem;
using MCBS.BlockForms.FileSystem.IO;
using QuanLib.Core;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.FileMoveHandler
{
    public class FileMoveHandler
    {
        public FileMoveHandler(IList<IOStream> fileMoveStreams)
        {
            ArgumentNullException.ThrowIfNull(fileMoveStreams, nameof(fileMoveStreams));

            _fileMoveStreams = fileMoveStreams.AsReadOnly();
            TotalBytes = fileMoveStreams.Sum(s => s.Source.Length);
            CompletedBytes = 0;
            CurrentFile = string.Empty;
            CurrentFileTotalBytes = 0;
            CurrentFileCompletedBytes = 0;
        }

        private readonly ReadOnlyCollection<IOStream> _fileMoveStreams;

        private int _position;

        public int TotalFiles => _fileMoveStreams.Count;

        public int CompletedFiles => _position;

        public long TotalBytes { get; }

        public long CompletedBytes { get; private set; }

        public string CurrentFile { get; private set; }

        public long CurrentFileTotalBytes { get; private set; }

        public long CurrentFileCompletedBytes { get; private set; }

        public async Task StartAsync()
        {
            await StartAsync(CancellationToken.None);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            for (_position = 0; _position < _fileMoveStreams.Count; _position++)
            {
                IOStream fileMoveStream = _fileMoveStreams[_position];
                CurrentFile = fileMoveStream.Source.Name;
                CurrentFileTotalBytes = fileMoveStream.Source.Length;

                if (FileSystemUtil.DriveEquals(fileMoveStream.Source.Name, fileMoveStream.Destination.Name))
                {
                    long length = fileMoveStream.Source.Length;
                    fileMoveStream.Source.Close();
                    fileMoveStream.Destination.Close();
                    File.Move(fileMoveStream.Source.Name, fileMoveStream.Destination.Name, true);
                    CompletedBytes += length;
                    CurrentFileCompletedBytes = length;
                }
                else
                {
                    long completedBytes = CompletedBytes;
                    Progress<long> progress = new((bytes) =>
                    {
                        CompletedBytes = completedBytes + bytes;
                        CurrentFileCompletedBytes = bytes;
                    });

                    await CopyAsync(fileMoveStream.Source, fileMoveStream.Destination, progress, cancellationToken);
                    fileMoveStream.Source.Close();
                    File.Delete(fileMoveStream.Source.Name);
                }

                if (cancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException();
            }
        }

        public IOStream[] GetCompleted()
        {
            if (_position <= 0)
                return [];

            IOStream[] result = new IOStream[_position];
            for (int i = 0; i < _position; i++)
                result[i] = _fileMoveStreams[i];

            return result;
        }

        private static async Task CopyAsync(FileStream source, FileStream destination, IProgress<long> progress, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(destination, nameof(destination));
            ArgumentNullException.ThrowIfNull(progress, nameof(progress));

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

                progress.Report(destination.Length);

                if (cancellationToken.IsCancellationRequested)
                    break;
            }

            ArrayPool<byte>.Shared.Return(buffer);
            await destination.FlushAsync(CancellationToken.None);
        }
    }
}
