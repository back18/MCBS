using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MCBS.Common
{
    public class FileMetadata : IDataViewModel<FileMetadata>
    {
        public FileMetadata(Model model)
        {
            NullValidator.ValidateObject(model, nameof(model));

            Name = model.Name;
            Length = model.Length;
            Hash = model.Hash;
            HashType = Enum.Parse<HashType>(model.HashType, true);
        }

        public FileMetadata(string name, int length, string hash, HashType hashType)
        {
            ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
            ArgumentException.ThrowIfNullOrEmpty(hash, nameof(hash));

            Name = name;
            Length = length;
            Hash = hash;
            HashType = hashType;
        }

        public string Name { get; }

        public int Length { get; }

        public string Hash { get; }

        public HashType HashType { get; }

        public static FileMetadata FromDataModel(object model)
        {
            return new FileMetadata((Model)model);
        }

        public virtual object ToDataModel()
        {
            return new Model()
            {
                Name = Name,
                Length = Length,
                Hash = Hash,
                HashType = HashType.ToString()
            };
        }

        public class Model : IDataModel<Model>
        {
            public Model()
            {
                Name = string.Empty;
                Length = 0;
                Hash = string.Empty;
                HashType = nameof(QuanLib.Core.HashType.SHA1);
            }

            public string Name { get; set; }

            public int Length { get; set; }

            public string Hash { get; set; }

            public string HashType { get; set; }

            public static Model CreateDefault()
            {
                return new Model();
            }

            public IValidatableObject GetValidator()
            {
                return new ValidatableObject(this);
            }

            public IEnumerable<IValidatable> GetValidatableProperties()
            {
                return Array.Empty<IValidatable>();
            }
        }
    }
}
