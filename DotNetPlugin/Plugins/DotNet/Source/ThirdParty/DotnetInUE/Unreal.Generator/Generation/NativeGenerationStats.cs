// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

namespace Unreal.Generation
{
    public struct NativeGenerationStats
    {
        public int TotalEnums;
        public int TotalStructs;
        public int TotalClasses;
        public int TotalInterfaces;

        public int TotalTypes => TotalClasses + TotalEnums + TotalInterfaces + TotalStructs;
        public int TotalFunctions;
        public int TotalProperties;

        public float GeneratedPropertyRatio
            => TotalProperties > 0 ? 1 - (float) SkippedProperties / TotalProperties : 0;

        public float GeneratedFunctionRatio => TotalFunctions > 0 ? 1 - (float) SkippedFunctions / TotalFunctions : 0;

        /// <summary>
        /// Structs not generated because they contained reference types.
        /// </summary>
        public int SkippedStructReferenceType;

        /// <summary>
        /// Functions that failed to generate.
        /// </summary>
        public int SkippedFunctions;

        /// <summary>
        /// Member properties that failed to generate.
        /// </summary>
        public int SkippedProperties;

        /// <summary>
        /// Number of structs missing properties.
        /// </summary>
        public int StructsMissingProperties;

        /// <summary>
        /// Number of classes missing properties.
        /// </summary>
        public int ClassesMissingProperties;

        /// <summary>
        /// Number of classes missing functions.
        /// </summary>
        public int ClassesMissingFunctions;

        /// <summary>
        /// Number of classes missing functions because those were not exported.
        /// </summary>
        public int ClassesMissingFunctionsNoExport;

        /// <summary>
        /// Whether these stats have recorded any missing types and or members.
        /// </summary>
        public bool AnyMissing
            => SkippedStructReferenceType + SkippedFunctions + SkippedProperties + ClassesMissingFunctionsNoExport > 0;

        public override string ToString()
        {
            return $@"Types
    Total: {TotalTypes}
    Enums: {TotalEnums}
    Structs: {TotalStructs}
        Skipped Non-Blitable: {SkippedStructReferenceType}
    Classes: {TotalClasses}
Properties:
    Total: {TotalProperties}
        Generated: {GeneratedPropertyRatio:P}
        Skipped: {SkippedProperties}
    Structs Missing: {StructsMissingProperties}
    Classes Missing: {ClassesMissingProperties}
Functions:
    Total: {TotalFunctions}
        Generated: {GeneratedFunctionRatio:P}
        Skipped: {SkippedFunctions}
    Classes Missing: {ClassesMissingFunctions}
    Classes Missing NoExport: {ClassesMissingFunctionsNoExport}
";
        }
    }
}