using MCBS.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public class ColorMappingBuildService : IColorMappingBuildService
    {
        public ColorMappingBuildService(IColorToIndexConverter defaultConverter)
        {
            ArgumentNullException.ThrowIfNull(defaultConverter, nameof(defaultConverter));

            _defaultConverter = defaultConverter;
        }

        private readonly IColorToIndexConverter _defaultConverter;

        public Task<Rgba32[]> BuildAsync(IColorFinder colorFinder, CancellationToken cancellationToken = default)
        {
            return BuildAsync(colorFinder, _defaultConverter, cancellationToken);
        }

        public Task<Rgba32[]> BuildAsync(IColorFinder colorFinder, IProgress<BuildProgress>? progress, CancellationToken cancellationToken = default)
        {
            return BuildAsync(colorFinder, _defaultConverter, progress, cancellationToken);
        }

        public Task<Rgba32[]> BuildAsync(IColorFinder colorFinder, IColorToIndexConverter colorToIndexConverter, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(colorFinder, nameof(colorFinder));
            ArgumentNullException.ThrowIfNull(colorToIndexConverter, nameof(colorToIndexConverter));

            ColorMappingBuilder builder = CreateBuilder(colorFinder, colorToIndexConverter);
            return builder.BuildAsync(cancellationToken);
        }

        public Task<Rgba32[]> BuildAsync(IColorFinder colorFinder, IColorToIndexConverter colorToIndexConverter, IProgress<BuildProgress>? progress, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(colorFinder, nameof(colorFinder));
            ArgumentNullException.ThrowIfNull(colorToIndexConverter, nameof(colorToIndexConverter));

            ColorMappingBuilder builder = CreateBuilder(colorFinder, colorToIndexConverter, progress);
            return builder.BuildAsync(cancellationToken);
        }

        private static ColorMappingBuilder CreateBuilder(IColorFinder colorFinder, IColorToIndexConverter colorToIndexConverter, IProgress<BuildProgress>? progress = null)
        {
            return new ColorMappingBuilder(colorFinder, colorToIndexConverter, progress);
        }
    }
}
