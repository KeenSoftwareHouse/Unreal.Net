// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using SystemModule = System.Reflection.Module;

namespace Unreal.Generation
{
    public class Module
    {
        public readonly string Name;
        public readonly string ModuleId;

        public readonly string ModuleHeader;
        public readonly string NameUpperCamelCase;

        public readonly string ModuleExport;

        public readonly string ModuleApi;

        // Module ticket for generated modules.
        public readonly ulong Ticket;
        
        // Module ticket for generated modules.
        public readonly string Timespamp;

        public Module(GeneratorExecutionContext context)
            : this(context.Compilation.AssemblyName!, true)
        { }

        public Module(string name, bool generateTicket = false)
        {
            Name = name;

            ModuleId = Name.Replace(" ", "").Replace(".", "");

            ModuleHeader = ModuleId + ".h";

            NameUpperCamelCase = ToUpperCamelCase(Name);

            ModuleExport = NameUpperCamelCase + "_MANAGED_EXPORT";
            ModuleApi = ModuleId.ToUpper() + "_API";

            if (generateTicket)
            {
                var bytes = new byte[8];
                RandomNumberGenerator.Create().GetBytes(bytes);

                Ticket = BitConverter.ToUInt64(bytes, 0);
            }
            
            Timespamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
        }

        private static string ToUpperCamelCase(string nameCamelCase)
        {
            StringBuilder buffer = new();

            bool lastLower = false;
            for (int i = 0; i < nameCamelCase.Length; i++)
            {
                if (!char.IsLetterOrDigit(nameCamelCase[i]))
                {
                    lastLower = true;
                    continue;
                }

                if (char.IsUpper(nameCamelCase[i]))
                {
                    if (lastLower)
                        buffer.Append("_");
                    lastLower = false;
                }
                else
                {
                    lastLower = true;
                }

                buffer.Append(Char.ToUpperInvariant(nameCamelCase[i]));
            }

            return buffer.ToString();
        }

        private static readonly ConditionalWeakTable<SystemModule, Module> CachedModules = new();

        public static Module GetModule(SystemModule systemModule)
        {
            return CachedModules.GetValue(systemModule,
                x => new Module(Path.GetFileNameWithoutExtension(x.Name)));
        }
    }
}