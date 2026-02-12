using QuanLib.Core;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MCBS.Common
{
    public class ColorMappingInfo : FileMetadata, IDataViewModel<ColorMappingInfo>
    {
        public ColorMappingInfo(Model model) : base(model)
        {
            ColorSet = new HashSet<Rgba32>(model.ColorSet.Select(hex => Rgba32.ParseHex(hex))).AsReadOnly();
        }

        public ColorMappingInfo(string name, int length, string hash, HashType hashType, IEnumerable<Rgba32> colorSet) : base(name, length, hash, hashType)
        {
            ArgumentNullException.ThrowIfNull(colorSet, nameof(colorSet));

            ColorSet = new HashSet<Rgba32>(colorSet).AsReadOnly();
        }

        public ReadOnlySet<Rgba32> ColorSet { get; set; }

        public static new ColorMappingInfo FromDataModel(object model)
        {
            return new ColorMappingInfo((Model)model);
        }

        public override object ToDataModel()
        {
            return new Model()
            {
                Name = Name,
                Length = Length,
                Hash = Hash,
                HashType = HashType.ToString(),
                ColorSet = ColorSet.Select(color => color.ToHex()).ToArray()
            };
        }

        public new class Model : FileMetadata.Model, IDataModel<Model>
        {
            public Model()
            {
                ColorSet = [];
            }

            public string[] ColorSet { get; set; }

            public static new Model CreateDefault()
            {
                return new Model();
            }
        }
    }
}
