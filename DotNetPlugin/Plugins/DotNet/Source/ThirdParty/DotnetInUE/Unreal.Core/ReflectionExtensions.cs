// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

// ReSharper disable once CheckNamespace

namespace System.Reflection
{
    /// <summary>
    /// Extension methods for reflection.
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Whether a given attribute type is defined on this member, or optionally in any of it's parent types.
        /// </summary>
        /// <param name="member">The current member instance.</param>
        /// <param name="inherit">Whether to also look for definition on parent members.</param>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <returns></returns>
        public static bool IsDefined<TAttribute>(this MemberInfo member, bool inherit = true)
            where TAttribute : Attribute
        {
            return Attribute.IsDefined(member, typeof(TAttribute), inherit);
        }
    }
}