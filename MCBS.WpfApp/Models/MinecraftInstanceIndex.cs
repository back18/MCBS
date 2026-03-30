using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MCBS.WpfApp.Models
{
    public class MinecraftInstanceIndex : IDataViewModel<MinecraftInstanceIndex>, IEquatable<MinecraftInstanceIndex>, IEquatable<MinecraftInstanceIndex.Model>
    {
        public static readonly MinecraftInstanceIndex Empty = FromDataModel(Model.CreateDefault());

        public MinecraftInstanceIndex(Model model)
        {
            NullValidator.ValidateObject(model, nameof(model));

            CurrentInstance = model.CurrentInstance;
            InstanceList = model.InstanceList.AsReadOnly();
        }

        public string CurrentInstance { get; }

        public ReadOnlyCollection<string> InstanceList { get; }

        public static MinecraftInstanceIndex FromDataModel(object model)
        {
            return new MinecraftInstanceIndex((Model)model);
        }

        public bool Equals(MinecraftInstanceIndex? other)
        {
            if (other is null)
                return false;

            return CurrentInstance == other.CurrentInstance &&
                   InstanceList.SequenceEqual(other.InstanceList);
        }

        public bool Equals(Model? other)
        {
            if (other is null)
                return false;

            return CurrentInstance == other.CurrentInstance &&
                   InstanceList.SequenceEqual(other.InstanceList);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;

            if (obj is MinecraftInstanceIndex index)
                return Equals(index);

            if (obj is Model model)
                return Equals(model);

            return false;
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new();
            hashCode.Add(CurrentInstance);

            foreach (string instance in InstanceList)
                hashCode.Add(instance);

            return hashCode.ToHashCode();
        }

        public object ToDataModel()
        {
            return new Model()
            {
                CurrentInstance = CurrentInstance,
                InstanceList = InstanceList.ToList()
            };
        }

        public class Model : IDataModel<Model>
        {
            public string CurrentInstance { get; set; } = string.Empty;

            public List<string> InstanceList { get; set; } = [];

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
                return Enumerable.Empty<IValidatable>();
            }
        }
    }
}
