// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Reflection;

namespace Unreal.Attributes.Meta
{
    public readonly struct PropertyAttributeInfo
    {
        public readonly string UE4Name;

        public readonly string FullName;

        public readonly Type AttributeType;

        public bool HasArgument => ArgumentType != null;

        public readonly Type? ArgumentType;

        public readonly MetaTypeFlags ValidMetaTargets;

        public readonly bool IsMeta;

        public PropertyAttributeInfo(Type type)
        {
            FullName = type.FullName!; // These are all explicit user defined types, FullName will always be non-null. 
            AttributeType = type;

            const string Attribute = "Attribute";
            var name = type.Name;
            if (!name.EndsWith(Attribute))
                throw new AttributeFormatException($"{FullName}: Attribute class names should end with '{Attribute}'.");

            // Attributes and their UE4 names should match.
            UE4Name = name.Substring(0, name.Length - Attribute.Length);

            if (type.GetConstructors(BindingFlags.Public).Length > 1)
                throw new AttributeFormatException(
                    $"Attribute {FullName} should only have one public constructor.");

            var valueField = type.GetField("Value");

            MetaPropertyAttributeBase instance;

            // Check value.
            if (valueField == null)
            {
                ArgumentType = null;

                if (type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Array.Empty<Type>(), null)
                    == null)
                    throw new AttributeFormatException(
                        $"Attribute {FullName} should have public a parameterless constructor.");

                instance = (MetaPropertyAttributeBase) Activator.CreateInstance(type)!;
            }
            else
            {
                ArgumentType = valueField.FieldType;

                if (!valueField.IsInitOnly || !valueField.IsPublic)
                    throw new AttributeFormatException(
                        $"Attribute {FullName} has a Value member but it's not a public readonly field.");

                if (type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new[] {ArgumentType}, null)
                    == null)
                {
                    throw new AttributeFormatException(
                        $"Attribute {FullName} has a Value member but it's not a public readonly field.");
                }

                var argDefault = ArgumentType.IsValueType ? Activator.CreateInstance(ArgumentType) : null;
                instance = (MetaPropertyAttributeBase) Activator.CreateInstance(type, argDefault)!;
            }

            ValidMetaTargets = instance.ValidTargets;
            IsMeta = instance.IsMeta;
        }
    }
}