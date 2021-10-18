// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using Unreal.NativeMetadata;

namespace Unreal.MetaAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = args[0];

            var collector = new MetadataCollector(path);

            Write("Generic Types", collector.GetAllProperties()
                .GroupBy(x => x.GetCleanRawType())
                .OrderByDescending(x => x.Count())
                .Select(x => new
                {
                    Type = x.Key,
                    PropertyType = x.First().PropertyType,
                    Count = x.Count()
                }));

            Write("Concrete Types", collector.GetAllProperties()
                .GroupBy(x => x.GetPrettyType())
                .OrderByDescending(x => x.Count())
                .Select(x => new
                {
                    Type = x.Key,
                    PropertyType = x.First().PropertyType,
                    Count = x.Count()
                }));

            Write("Property Meta Types", collector.GetAllProperties()
                .GroupBy(x => x.PropertyType)
                .OrderByDescending(x => x.Count())
                .Select(x => new
                {
                    Type = x.Key,
                    IsKnown = !x.First().IsUnknown,
                    Count = x.Count()
                }));
        }

        private static void Write<T>(string name, IEnumerable<T> enumeration)
        {
            using (var writer = new StreamWriter(name + ".csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                csv.WriteRecords(enumeration);
        }

        private static void ListByRefParameters(MetadataCollector collector)
        {
            var byRef = collector.GetAllProperties()
                .Where(x => (x.Flags & PropertyFlags.ReferenceParm) != 0
                            && (x.Flags & PropertyFlags.ConstParm) == 0);

            foreach (var prop in byRef)
            {
                Console.WriteLine(prop.Function != null
                    ? prop.Struct.Name + "::" + prop.Function.Name + "::" + prop.Name
                    : prop.Struct.Name + "::" + prop.Name);
            }
        }
    }
}