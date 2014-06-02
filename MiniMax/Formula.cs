using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MiniMax.Attributes;
using Boolean = MyLibrary.Types.Boolean;

namespace MiniMax
{
    public class Formula
    {
        public Formula(Type type)
        {
            Type = type;
        }

        private Type Type { get; set; }

        public bool IsValid(IEnumerable<object> dataEnumerable)
        {
            return dataEnumerable.All(IsValid);
        }

        public bool IsValid(object obj)
        {
            return
                GetProperties(typeof (MiniMaxRestrictionAttribute))
                    .Select(property => property.GetValue(obj, null))
                    .Cast<bool>()
                    .All(Boolean.IsTrue);
        }

        public bool IsInvalid(object obj)
        {
            return
                GetProperties(typeof (MiniMaxRestrictionAttribute))
                    .Select(property => property.GetValue(obj, null))
                    .Cast<bool>()
                    .Any(Boolean.IsFalse);
        }

        public IEnumerable<PropertyInfo> GetProperties(Type attribute)
        {
            return
                Type.GetProperties()
                    .Where(
                        property => property.GetCustomAttributes(attribute, false).Any());
        }
    }
}