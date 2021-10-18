// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Diagnostics;
using Unreal.Generation;
using Unreal.Metadata;

namespace Unreal.Marshalling
{
    public class CustomTypeMarshaller : CustomTypeMarshallerBase
    {
        public override string? AdditionalHeader => m_formats.RequiredHeader;
        public override string? AdditionalNamespace => m_formats.RequiredNamespace;

        private readonly MarshalFormats m_formats;
        private readonly ITypeInfo m_intermediateType;

        public CustomTypeMarshaller(in MarshalFormats formats, ITypeInfo intermediateType)
        {
            m_formats = formats;
            m_intermediateType = intermediateType;
        }

        protected override ITypeInfo GetIntermediateType(ITypeInfo type) => m_intermediateType;

        protected override string FormatMarshalled(QualifiedTypeReference type, Codespace space, Order order,
            string field)
        {
            if (space == Codespace.Managed)
            {
                if (order == Order.Before)
                    return string.Format(m_formats.FromManagedToIntermediate, field);
                else
                    return string.Format(m_formats.FromIntermediateToManaged, field);
            }
            else
            {
                if (order == Order.Before)
                    return string.Format(m_formats.FromNativeToIntermediate, field);
                else
                    return string.Format(m_formats.FromIntermediateToNative, field);
            }
        }
    }
}