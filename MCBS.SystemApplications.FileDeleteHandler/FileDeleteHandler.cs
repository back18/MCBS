using MCBS.BlockForms.FileSystem;
using QuanLib.Core;
using QuanLib.Core.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.FileDeleteHandler
{
    public class FileDeleteHandler
    {
        public FileDeleteHandler(IList<string> files)
        {
            ArgumentNullException.ThrowIfNull(files, nameof(files));

            _files = files.AsReadOnly();

            ThrowException += OnThrowException;
        }

        private readonly ReadOnlyCollection<string> _files;

        private int _position;

        public int TotalFiles => _files.Count;

        public int CompletedFiles => _position;

        public string? CurrentFile => CheckHelper.Range(0, _files.Count - 1, _position) ? _files[_position] : null;

        public event EventHandler<FileDeleteHandler, EventArgs<AggregateException>> ThrowException;

        protected virtual void OnThrowException(FileDeleteHandler sender, EventArgs<AggregateException> e) { }

        public async Task StartAsync()
        {
            await StartAsync(CancellationToken.None);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            for (_position = 0; _position < _files.Count; _position++)
            {
                string file = _files[_position];
                AggregateException? aggregateException = null;

                try
                {
                    await DeleteAsync(_files[_position]);
                }
                catch (Exception ex)
                {
                    if (ex is AggregateException aex && aex.InnerException is not null)
                        ex = aex.InnerException;

                    StringBuilder stringBuilder = new();
                    stringBuilder.AppendLine("文件删除时遇到了错误：");
                    stringBuilder.AppendLine(ExceptionMessageFactory.GetLocalMessag(ex));
                    stringBuilder.AppendLine("文件路径：");
                    stringBuilder.AppendLine(file);
                    stringBuilder.AppendLine("错误信息：");
                    aggregateException = new(stringBuilder.ToString(), ex);
                }

                if (aggregateException is not null)
                    ThrowException.Invoke(this, new(aggregateException));

                if (cancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException();
            }
        }

        private static async Task DeleteAsync(string path)
        {
            await Task.Run(() => File.Delete(path));
        }
    }
}
