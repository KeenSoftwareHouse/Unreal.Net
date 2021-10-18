// Copyright (c) 2021 Keen Software House
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Unreal.Generation
{
    public static class TemplateWriter
    {
        private static readonly Dictionary<Type, Model> Models = new();

        public static string WriteTemplate(string template, params object[] arguments)
        {
            var models = arguments.Select(x => (Model: GetModel(x), Values: x)).ToList();

            var patterns = models.SelectMany(x => x.Model.Values).Select(x => @$"\{{{x.Name}\}}");
            var regex = new Regex(string.Join("|", patterns));

            var evaluator = new Evaluator(models);

            return regex.Replace(template, evaluator.MatchEvaluator);
        }

        private static Model GetModel(object instance)
        {
            var type = instance.GetType();
            if (!Models.TryGetValue(type, out var model))
                Models[type] = model = new Model(type);

            return model;
        }

        private class Model : Dictionary<string, IValueProvider>
        {
            public Model(Type type)
            {
                var bindingFlags = BindingFlags.Instance
                                   | BindingFlags.Static
                                   | BindingFlags.Public
                                   | BindingFlags.NonPublic;

                // TODO: This ignores values from parent types.

                foreach (var property in type.GetProperties(bindingFlags))
                {
                    if (property.GetMethod != null)
                        Add(new PropertyGetter(property));
                }

                foreach (var field in type.GetFields(bindingFlags))
                {
                    Add(new FieldGetter(field));
                }

                foreach (var method in type.GetMethods(bindingFlags))
                {
                    if (method.ReturnType != typeof(void) && method.GetParameters().Length == 0)
                        Add(new MethodGetter(method));
                }
            }

            private void Add(IValueProvider provider)
            {
                var key = $"{{{provider.Name}}}";
                base.Add(key, provider);
            }
        }

        private class Evaluator
        {
            private readonly List<(Model Model, object Instance)> m_models;

            public Evaluator(List<(Model Model, object Instance)> models)
            {
                m_models = models;
            }

            public string MatchEvaluator(Match match)
            {
                foreach (var (model, instance) in m_models)
                {
                    if (model.TryGetValue(match.Value, out var value))
                        return value.Get(instance);
                }

                // Not found, do nothing.
                return match.Value;
            }
        }

        private interface IValueProvider
        {
            string Name { get; }

            string Get(object instance);
        }

        private class PropertyGetter : IValueProvider
        {
            private readonly PropertyInfo m_property;

            public string Name => m_property.Name;

            public PropertyGetter(PropertyInfo property)
            {
                m_property = property;
            }

            public string Get(object instance)
            {
                return m_property.GetValue(instance)?.ToString() ?? string.Empty;
            }
        }

        private class FieldGetter : IValueProvider
        {
            private readonly FieldInfo m_field;

            public string Name => m_field.Name;

            public FieldGetter(FieldInfo field)
            {
                m_field = field;
            }

            public string Get(object instance)
            {
                return m_field.GetValue(instance)?.ToString() ?? string.Empty;
            }
        }

        private class MethodGetter : IValueProvider
        {
            private readonly MethodInfo m_method;

            public string Name => m_method.Name;

            public MethodGetter(MethodInfo method)
            {
                m_method = method;
            }

            public string Get(object instance)
            {
                return m_method.Invoke(instance, null)?.ToString() ?? string.Empty;
            }
        }
    }
}