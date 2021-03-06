// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Unreal.Attributes.Meta
{
    /// <summary>
    /// Index of all Unreal Engine attributes defined in this assembly. 
    /// </summary>
    public class AttributeIndex
    {
        /// <summary>
        /// Known attributes keyed by their UE4 name.
        /// </summary>
        public ImmutableDictionary<string, PropertyAttributeInfo> MetaAttributeProperties { get; }

        /// <summary>
        /// Known attributes keyed by their UE4 name.
        /// </summary>
        public ImmutableArray<MetaAttributeInfo> MetaAttributes { get; }

        ///<Summary>Info about the <see cref="UClassAttribute"/> attribute.</Summary>
        public readonly MetaAttributeInfo UClassAttributeType;

        ///<Summary>Info about the <see cref="UEnumAttribute"/> attribute.</Summary>
        public readonly MetaAttributeInfo UEnumAttributeType;

        ///<Summary>Info about the <see cref="UInterfaceAttribute"/> attribute.</Summary>
        public readonly MetaAttributeInfo UInterfaceAttributeType;

        ///<Summary>Info about the <see cref="UStructAttribute"/> attribute.</Summary>
        public readonly MetaAttributeInfo UStructAttributeType;

        ///<Summary>Info about the <see cref="UFunctionAttribute"/> attribute.</Summary>
        public readonly MetaAttributeInfo UFunctionAttributeType;

        ///<Summary>Info about the <see cref="UPropertyAttribute"/> attribute.</Summary>
        public readonly MetaAttributeInfo UPropertyAttributeType;

        public readonly ImmutableArray<Type> AllAttributes;

        public AttributeIndex()
        {
            var attrs = typeof(AttributeIndex).Assembly.GetTypes()
                .Where(type => typeof(MetaPropertyAttributeBase).IsAssignableFrom(type) && !type.IsAbstract)
                .Select(x => new PropertyAttributeInfo(x));

            MetaAttributeProperties =
                attrs.ToImmutableDictionary(x => x.UE4Name, StringComparer.InvariantCultureIgnoreCase);

            UClassAttributeType = new MetaAttributeInfo(typeof(UClassAttribute));
            UEnumAttributeType = new MetaAttributeInfo(typeof(UEnumAttribute));
            UInterfaceAttributeType = new MetaAttributeInfo(typeof(UInterfaceAttribute));
            UStructAttributeType = new MetaAttributeInfo(typeof(UStructAttribute));
            UFunctionAttributeType = new MetaAttributeInfo(typeof(UFunctionAttribute));
            UPropertyAttributeType = new MetaAttributeInfo(typeof(UPropertyAttribute));

            MetaAttributes = ImmutableArray.Create(UClassAttributeType, UEnumAttributeType, UInterfaceAttributeType,
                UStructAttributeType, UFunctionAttributeType, UPropertyAttributeType);

            AllAttributes = typeof(AttributeIndex).Assembly.GetTypes()
                .Where(x => typeof(Attribute).IsAssignableFrom(x)
                            // Skip attributes generated by the compiler.
                            && x.GetCustomAttribute<CompilerGeneratedAttribute>() == null)
                .ToImmutableArray();
        }

        public bool IsMetaProperty(string attributeName)
        {
            return !MetaAttributeProperties.TryGetValue(attributeName, out var info) || info.IsMeta;
        }
    }
}