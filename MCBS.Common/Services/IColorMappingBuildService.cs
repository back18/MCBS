using MCBS.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface IColorMappingBuildService
    {
        public Task<Rgba32[]> BuildAsync(IColorFinder colorFinder, CancellationToken cancellationToken = default);

        public Task<Rgba32[]> BuildAsync(IColorFinder colorFinder, IProgress<BuildProgress> progress, CancellationToken cancellationToken = default);

        public Task<Rgba32[]> BuildAsync(IColorFinder colorFinder, IColorToIndexConverter colorToIndexConverter, CancellationToken cancellationToken = default);

        public Task<Rgba32[]> BuildAsync(IColorFinder colorFinder, IColorToIndexConverter colorToIndexConverter, IProgress<BuildProgress>? progress, CancellationToken cancellationToken = default);
    }
}
