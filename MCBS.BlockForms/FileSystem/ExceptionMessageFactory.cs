using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.FileSystem
{
    public static class ExceptionMessageFactory
    {
        public static string GetLocalMessag(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception, nameof(exception));

            if (exception is PathTooLongException)
                return "路径过长";
            else if (exception is NotSupportedException)
                return "路径指向了不支持的非文件设备";
            else if (exception is DirectoryNotFoundException)
                return "目录不存在";
            else if (exception is FileNotFoundException)
                return "文件不存在";
            else if (exception is UnauthorizedAccessException)
                return "访问被拒绝";
            else if (exception is SecurityException)
                return "没有足够的权限访问";
            else if (exception is IOException)
                return "引发了I/O异常（例如文件已被其他进程占用）";
            else
                return $"引发了异常，异常消息：{Environment.NewLine}{ObjectFormatter.Format(exception)}";
        }
    }
}
