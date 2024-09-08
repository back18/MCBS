using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Desktop
{
    public readonly struct IconIdentifier(string type, string value) : IEquatable<IconIdentifier>, IParsable<IconIdentifier>
    {
        public readonly string Type = type;

        public readonly string Value = value;

        public static IconIdentifier Parse(string s)
        {
            return Parse(s, null);
        }

        public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out IconIdentifier result)
        {
            return TryParse(s, null, out result);
        }

        public static IconIdentifier Parse(string s, IFormatProvider? provider)
        {
            if (!TryParse(s, provider, out var result))
                throw new FormatException(s);

            return result;
        }

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out IconIdentifier result)
        {
            if (string.IsNullOrEmpty(s))
            {
                result = default;
                return false;
            }

            string[] subStrings = s.Split(':', 2);
            if (subStrings.Length != 2)
            {
                result = default;
                return false;
            }

            result = new(subStrings[0], subStrings[1]);
            return true;
        }

        public bool Equals(IconIdentifier other)
        {
            return Type == other.Type && Value == other.Value;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is IconIdentifier other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Value);
        }

        public override string ToString()
        {
            return $"{Type}:{Value}";
        }

        public static bool operator ==(IconIdentifier left, IconIdentifier right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IconIdentifier left, IconIdentifier right)
        {
            return !left.Equals(right);
        }
    }
}
