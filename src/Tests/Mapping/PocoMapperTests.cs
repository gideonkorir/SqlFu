﻿using System;
using System.Linq;
using System.Reflection;
using CavemanTools.Logging;
using CavemanTools.Testing;
using DomainBus.Tests;
using FluentAssertions;
using SqlFu.Mapping;
using SqlFu.Mapping.Internals;
using SqlFu.Tests._Fakes;
using Xunit;
using Xunit.Abstractions;

namespace SqlFu.Tests.Mapping
{
    public class PocoMapperTests
    {
        private Mapper<MapperPost> _sut;

        public PocoMapperTests(ITestOutputHelper x)
        {
            x.Logger();
         _sut = Setup.MapperFactory().CreateMapper<MapperPost>("1") as Mapper<MapperPost>;
         
        }

        [Fact]
        public void map_simple_properties()
        {
            var guid = Guid.NewGuid();
            var post = MapWithReader(data =>
            {
                data["Name"] = "bla";
                data["Id"] = guid;
                data["Decimal"] = 34m;              
            });
           
            post.Id.Should().Be(guid);
            post.Decimal.Should().Be(34);

            
        }

        class SimplePost
        {
            public string Name { get; set; } 
            public Guid Id { get; set; }
            public int Number { get; private set; }

            public void SetNumber(int r)
            {
                Number = r;
            }

            private SimplePost()
            {
                
            }

            public SimplePost(int i)
            {
                
            }
        }

        [Fact]
        public void map_to_single_property()
        {
            var post=_sut.Map(Setup.FakeReader(r =>
            {
                r["Name"] = "hey00";
            }));
            post.Title.Should().Be("hey00");
            post.Version.ShouldAllBeEquivalentTo(new byte[] {0,1});
        }


       // [Fact]
        public void Benchmark()
        {
           
            var manual = new ManualMapper<SimplePost>(r =>
            {
               
                var dt = new SimplePost(34);

                dt.Id = (Guid)r["Id"];
                dt.Name = r["Name"].ToString();
                dt.SetNumber((int)r["Number"]);
                return dt;

            });
            var data = Setup.FakeReader(r =>
            {
                r.Clear();
                r["Id"] = Guid.NewGuid();
                r["Name"] = "bla";
                r["Number"] = 23;
            });

      
            var sut = Setup.MapperFactory().CreateMapper<SimplePost>("1") as Mapper<SimplePost>;
            sut.Map(data);
            

            Setup.DoBenchmark(500, new[]{new BenchmarkAction(i =>
            {
                sut.Map(data, "");
            })
            , new BenchmarkAction(i =>
            {
                manual.Map(data);
            }),
            });

        
        }

        private MapperPost MapWithReader(Action<FakeReader> config)
        {
            var data = Setup.FakeReader(config);

          
            return _sut.Map(data, "");
         
        }

        [Fact]
        public void map_value_using_user_converter()
        {
            var post = MapWithReader(data => data["Email"]= "bla@example.com");
             
             post.Email.Value.Should().Be("bla@example.com");
        }


        [Fact]
        public void map_using_custom_mapper()
        {
            var post = MapWithReader(data => data["Address"]= "street");

            post.Address.Street.Should().Be("street");
        }

        [Fact]
        public void map_to_complex_type()
        {
            var post = MapWithReader(data =>
            {
                data["Author_Id"] = 25;
                data["Author_Name"] = "Hio";
            });
            post.Author.Id.Should().Be(25);
            post.Author.Name.Should().Be("Hio");
        }

        [Fact]
        public void map_field_value_to_column_of_type_object()
        {
            var post = MapWithReader(data =>
            {
                data["Dyno"] = 25;                
            });
            AssertionExtensions.Should((object) post.Dyno).Be(25);
        }

        [Fact]
        public void map_int_to_enum()
        {
            var post = MapWithReader(data =>
            {
                data["Order"] = SomeEnum.Last;
            });
            post.Order.Should().Be(SomeEnum.Last);
        }
        
        [Fact]
        public void map_string_to_enum()
        {
            var post = MapWithReader(data =>
            {
                data["Order"] = "Last";
            });
            post.Order.Should().Be(SomeEnum.Last);
        }

    }
}