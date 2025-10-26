using QuanLib.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Screens
{
    public struct ScreenSnapshot(Screen screen, int maxBackLayers, int maxFrontLayers) : IEquatable<ScreenSnapshot>
    {
        public Screen Screen { get; set; } = screen;

        public readonly CubeRange ScreenRange => Screen.GetRange(MaxBackLayers, MaxFrontLayers);

        public int MaxBackLayers { get; set; } = maxBackLayers;

        public int MaxFrontLayers { get; set; } = maxFrontLayers;

        public readonly bool Equals(ScreenSnapshot other)
        {
            return this == other;
        }

        public readonly override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is ScreenSnapshot other && Equals(other);
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Screen, MaxBackLayers, MaxFrontLayers);
        }
        public static bool operator ==(ScreenSnapshot left, ScreenSnapshot right)
        {
            return left.Screen == right.Screen && left.MaxBackLayers == right.MaxBackLayers && left.MaxFrontLayers == right.MaxFrontLayers;
        }

        public static bool operator !=(ScreenSnapshot left, ScreenSnapshot right)
        {
            return !(left == right);
        }
    }
}
