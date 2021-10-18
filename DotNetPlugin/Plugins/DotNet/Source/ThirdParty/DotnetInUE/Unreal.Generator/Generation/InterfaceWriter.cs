// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System.Collections.Generic;
using Unreal.Metadata;

namespace Unreal.Generation
{
    public class InterfaceWriter : TypeWriter
    {
        public InterfaceWriter(TypeDefinition type, IEnumerable<MemberWriter> members)
            : base(type, members)
        { }
    }
}