using QuanLib.Core;
using QuanLib.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.ObjectModel
{
    public static class GuidHelper
    {
        public static string GetShortId(this Guid guid)
        {
            return guid.GetFirstChars(4);
        }

        public static Guid GenerateShortId(IList<Guid> existing, int maxRetries)
        {
            ArgumentNullException.ThrowIfNull(existing, nameof(existing));
            ThrowHelper.ArgumentOutOfMin(-1, maxRetries, nameof(maxRetries));
            const string MESSAGE = "已达最大重试次数未成功生成GUID";

            Guid guid = Guid.NewGuid();
            if (existing.Count == 0)
                return guid;

            if (!ContainsShortId(existing, guid))
                return guid;

            if (maxRetries == 0)
                throw new InvalidOperationException(MESSAGE);

            HashSet<string> hashset = [];
            foreach (Guid item in existing)
                hashset.Add(item.GetFirstChars(4));

            if (hashset.Count >= 65536)
                throw new InvalidOperationException(MESSAGE);

            Func<int, bool> condition = maxRetries == -1 ? ((count) => true) : ((count) => count < maxRetries);
            for (int i = 0; condition(i); i++)
            {
                guid = Guid.NewGuid();
                if (!hashset.Contains(guid.GetFirstChars(4)))
                    return guid;
            }

            throw new InvalidOperationException(MESSAGE);
        }

        public static bool ContainsShortId(IList<Guid> existing, Guid guid)
        {
            ArgumentNullException.ThrowIfNull(existing, nameof(existing));

            if (existing.Count == 0)
                return false;

            foreach (Guid item in existing)
            {
                if (guid.FirstEquals(item, 4))
                    return true;
            }

            return false;
        }
    }
}
