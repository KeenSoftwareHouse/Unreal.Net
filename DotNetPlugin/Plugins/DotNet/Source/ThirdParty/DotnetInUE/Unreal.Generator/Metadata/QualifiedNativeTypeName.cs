// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;

namespace Unreal.Metadata
{
    /// <summary>
    /// Qualified name for a native type.
    /// </summary>
    public readonly struct QualifiedNativeTypeName : IEquatable<QualifiedNativeTypeName>
    {
        public readonly string Module;
        public readonly string Name;

        public QualifiedNativeTypeName(string module, string name)
        {
            Module = module;
            Name = name;
        }

        public override string ToString()
        {
            return $"{Module}/{Name}";
        }

        #region Equality Members

        public bool Equals(QualifiedNativeTypeName other)
        {
            return Module == other.Module
                   && Name == other.Name;
        }

        public override bool Equals(object? obj)
        {
            return obj is QualifiedNativeTypeName other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Module.GetHashCode() * 397) ^ Name.GetHashCode();
            }
        }

        public static bool operator ==(QualifiedNativeTypeName left, QualifiedNativeTypeName right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(QualifiedNativeTypeName left, QualifiedNativeTypeName right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}