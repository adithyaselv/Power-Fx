﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Microsoft.PowerFx.Connectors.Execution;
using Microsoft.PowerFx.Functions;
using Microsoft.PowerFx.Types;

namespace Microsoft.PowerFx.Connectors.Tests
{
    internal static class OpenApiHelperFunctions
    {
        internal static OpenApiSchema SchemaInteger => new () { Type = "integer" };

        internal static OpenApiSchema SchemaNumber => new () { Type = "number" };

        internal static OpenApiSchema SchemaString => new () { Type = "string" };

        internal static OpenApiSchema SchemaBoolean => new () { Type = "boolean" };

        internal static OpenApiSchema SchemaArrayInteger => new () { Type = "array", Format = "int" };

        internal static OpenApiSchema SchemaArrayString => new () { Type = "array", Format = "string" };

        internal static OpenApiSchema SchemaArrayObject => new () { Type = "array", Format = "object" };

        internal static OpenApiSchema SchemaArrayDateTime => new () { Type = "array", Format = "date-time" };

        internal static OpenApiSchema SchemaDateTime => new () { Type = "string", Format = "date-time" };

        internal static OpenApiSchema SchemaObject(params (string PropertyName, OpenApiSchema Schema, bool Required)[] properties) => 
            new () 
            { 
                Type = "object", 
                Required = properties.Where(p => p.Required).Select(p => p.PropertyName).ToHashSet(), 
                Properties = properties.ToDictionary(prop => prop.PropertyName, prop => prop.Schema)
            };

        internal static RecordValue GetRecord(params (string Name, FormulaValue Value)[] properties) => FormulaValue.NewRecordFromFields(properties.Select(prop => new NamedValue(prop.Name, prop.Value)).ToArray());

        internal static TableValue GetArray(params string[] values) => FormulaValue.NewSingleColumnTable(values.Select(v => FormulaValue.New(v)));

        internal static TableValue GetArray(params int[] values) => FormulaValue.NewSingleColumnTable(values.Select(v => FormulaValue.New(v)));

        internal static TableValue GetArray(params bool[] values) => FormulaValue.NewSingleColumnTable(values.Select(v => FormulaValue.New(v)));

        internal static TableValue GetArray(params DateTime[] values) => FormulaValue.NewSingleColumnTable(values.Select(v => FormulaValue.New(v)));

        internal static TableValue GetArray(params RecordValue[] values) => FormulaValue.NewTable(values.First().Type, values);

        internal static TableValue GetTable(RecordValue recordValue) => FormulaValue.NewTable(recordValue.Type, recordValue);

        internal static string SerializeJson(Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)> parameters, IConvertToUTC utcConverter = null) => Serialize<OpenApiJsonSerializer>(parameters, false, utcConverter);

        internal static string SerializeUrlEncoder(Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)> parameters, IConvertToUTC utcConverter = null) => Serialize<OpenApiFormUrlEncoder>(parameters, false, utcConverter);

        internal static string Serialize<T>(Dictionary<string, (OpenApiSchema Schema, FormulaValue Value)> parameters, bool schemaLessBody, IConvertToUTC utcConverter = null)
            where T : FormulaValueSerializer
        {
            var jsonSerializer = (FormulaValueSerializer)Activator.CreateInstance(typeof(T), new object[] { utcConverter, schemaLessBody });
            jsonSerializer.StartSerialization(null);

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    jsonSerializer.SerializeValue(parameter.Key, parameter.Value.Schema, parameter.Value.Value);
                }
            }

            jsonSerializer.EndSerialization();
            return jsonSerializer.GetResult();
        }
    }
}
