﻿// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Text.Json;
using Xunit;

namespace ShadowsocksUriGenerator.Tests.SnakeCase
{
    public class SnakeCaseTests
    {
        private static readonly JsonSerializerOptions s_snakeCaseAndIndentedOption = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy(),
            WriteIndented = true,
        };

        [Fact]
        public void JsonSerializerSnakeCaseSettings()
        {
            Person person = new Person();
            person.BirthDate = new DateTime(2000, 11, 20, 23, 55, 44, DateTimeKind.Utc);
            person.LastModified = new DateTime(2000, 11, 20, 23, 55, 44, DateTimeKind.Utc);
            person.Name = "Name!";

            string json = JsonSerializer.Serialize(person, s_snakeCaseAndIndentedOption);

            Assert.Equal(@"{
  ""name"": ""Name!"",
  ""birth_date"": ""2000-11-20T23:55:44Z"",
  ""last_modified"": ""2000-11-20T23:55:44Z""
}", json);

            Person? deserializedPerson = JsonSerializer.Deserialize<Person>(json, s_snakeCaseAndIndentedOption);

            Assert.Equal(person.BirthDate, deserializedPerson?.BirthDate);
            Assert.Equal(person.LastModified, deserializedPerson?.LastModified);
            Assert.Equal(person.Name, deserializedPerson?.Name);

            json = JsonSerializer.Serialize(person, new JsonSerializerOptions { WriteIndented = true });
            Assert.Equal(@"{
  ""Name"": ""Name!"",
  ""BirthDate"": ""2000-11-20T23:55:44Z"",
  ""LastModified"": ""2000-11-20T23:55:44Z""
}", json);
        }

        [Fact]
        public void BlogPostExample()
        {
            Product product = new Product
            {
                ExpiryDate = new DateTime(2010, 12, 20, 18, 1, 0, DateTimeKind.Utc),
                Name = "Widget",
                Price = 9.99m,
                Sizes = new[] { "Small", "Medium", "Large" }
            };

            string json = JsonSerializer.Serialize(product, s_snakeCaseAndIndentedOption);

            Assert.Equal(@"{
  ""name"": ""Widget"",
  ""expiry_date"": ""2010-12-20T18:01:00Z"",
  ""price"": 9.99,
  ""sizes"": [
    ""Small"",
    ""Medium"",
    ""Large""
  ]
}", json);
        }
    }
}
