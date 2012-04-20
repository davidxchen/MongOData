﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using MongoDB.Driver;
using NUnit.Framework;
using Simple.Data;
using Simple.Data.OData;

namespace Mongo.Context.Tests
{
    public abstract class QueryTests<T>
    {
        protected TestService service;
        protected dynamic ctx;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            TestData.Populate();
            service = new TestService(typeof(T));
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            if (service != null)
            {
                service.Dispose();
                service = null;
            }

            TestData.Clean();
        }

        [SetUp]
        public void SetUp()
        {
            ctx = Database.Opener.Open(service.ServiceUri);
        }

        [Test]
        public void Metadata()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(service.ServiceUri + "/$metadata");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "The $metadata didn't return success.");
        }

        [Test]
        public void AllEntitiesVerifyResultCount()
        {
            var result = ctx.Products.All().ToList();
            Assert.AreEqual(3, result.Count, "The service returned unexpected number of results.");
        }

        [Test]
        public void AllEntitiesTakeOneVerifyResultCount()
        {
            var result = ctx.Products.All().Take(1).ToList();
            Assert.AreEqual(1, result.Count, "The service returned unexpected number of results.");
        }

        [Test]
        public void AllEntitiesSkipOneVerifyResultCount()
        {
            var result = ctx.Products.All().Skip(1).ToList();
            Assert.AreEqual(2, result.Count, "The service returned unexpected number of results.");
        }

        [Test]
        public void AllEntitiesCountVerifyResult()
        {
            var result = ctx.Products.All().Count();
            Assert.AreEqual(3, result, "The count is not correctly computed.");
        }

        [Test]
        public void AllEntitiesVerifyID()
        {
            var result = ctx.Products.All().ToList();
            Assert.AreEqual(3, result[2].ID, "The ID is not correctly filled.");
        }

        [Test]
        public void AllEntitiesVerifyName()
        {
            var result = ctx.Products.All().ToList();
            Assert.AreEqual("Milk", result[1].Name, "The Name is not correctly filled.");
        }

        [Test]
        public void AllEntitiesVerifyReleaseDate()
        {
            var result = ctx.Products.All().ToList();
            Assert.AreEqual(new DateTime(1992, 1, 1), result[0].ReleaseDate, "The ReleaseDate is not correctly filled.");
        }

        [Test]
        public void AllEntitiesOrderby()
        {
            var result = ctx.Products.All().OrderBy(ctx.Products.Name).ToList();
            for (int i = 0; i < 2; i++)
            {
                Assert.Greater(result[i + 1].Name, result[i].Name, "Names are not in correct order.");
            }
        }

        [Test]
        public void AllEntitiesOrderbyDescending()
        {
            var result = ctx.Products.All().OrderByDescending(ctx.Products.Name).ToList();
            for (int i = 0; i < 2; i++)
            {
                Assert.Less(result[i + 1].Name, result[i].Name, "Names are not in correct order.");
            }
        }

        [Test]
        public void AllEntitiesVerifyQuantityValue()
        {
            var result = ctx.Products.All().ToList();
            Assert.AreEqual(12, result[0].Quantity.Value, "Unexpected quantity value.");
        }

        [Test]
        public void FilterEqualID()
        {
            var result = ctx.Products.FindAll(ctx.Products.ID == 1).ToList();
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void FilterEqualName()
        {
            var result = ctx.Products.FindAll(ctx.Products.Name == "Bread").ToList();
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void FilterEqualNameCountVerifyResult()
        {
            var result = ctx.Products.FindAll(ctx.Products.Name == "Bread").Count();
            Assert.AreEqual(1, result, "The count is not correctly computed.");
        }

        [Test]
        public void FilterEqualIDAndEqualName()
        {
            var result = ctx.Products.FindAll(ctx.Products.ID == 1 && ctx.Products.Name == "Bread").ToList();
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void FilterGreaterID()
        {
            var result = ctx.Products.FindAll(ctx.Products.ID > 0).ToList();
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void FilterNameContainsEqualsTrue()
        {
            var result = ctx.Products.FindAll(ctx.Products.Name.Contains("i") == true).ToList();
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void FilterNameContainsEqualsFalse()
        {
            var result = ctx.Products.FindAll(ctx.Products.Name.Contains("i") == false).ToList();
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void FilterGreaterRating()
        {
            var result = ctx.Products.FindAll(ctx.Products.Rating > 3).ToList();
            Assert.AreEqual(2, result.Count);
        }
        [Test]
        public void FilterNameLength()
        {
            var result = ctx.Products.FindAll(ctx.Products.Name.Length() == 4).ToList();
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void FilterGreaterIDAndNameLength()
        {
            var result = ctx.Products.FindAll(ctx.Products.ID > 0 && ctx.Products.Name.Length() == 4).ToList();
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void ProjectionVerifyExcluded()
        {
            var product = ctx.Products.All().Select(ctx.Products.ID).First();
            var id = product.ID;
            try
            {
                var name = product.Name;
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
            {
            }
        }

        [Test]
        public void ProjectionVerifyID()
        {
            var products = ctx.Products.FindAll(ctx.Products.ID > 0).Select(ctx.Products.ID).ToList();
            foreach (var product in products)
            {
                Assert.Greater(product.ID, 0, "The ID is not correctly filled.");
            }
        }

        [Test]
        public void ProjectionVerifyName()
        {
            var products = ctx.Products.All().Select(ctx.Products.Name).ToList();
            foreach (var product in products)
            {
                Assert.IsNotNull(product.Name, "The Name is not correctly filled.");
            }
        }

        [Test]
        public void ProjectionVerifyQuantity()
        {
            var products = ctx.Products.All().Select(ctx.Products.Quantity).ToList();
            foreach (var product in products)
            {
                Assert.Greater(product.Quantity.Value, 0, "The Quantity is not correctly filled.");
            }
        }

        [Test]
        public void ProjectionVerifyNameDescriptionRating()
        {
            var products = ctx.Products.All().Select(ctx.Products.Name, ctx.Products.Description, ctx.Products.Rating).ToList();
            foreach (var product in products)
            {
                Assert.IsNotNull(product.Name, "The Name is not correctly filled.");
                Assert.IsNotNull(product.Description, "The Description is not correctly filled.");
                Assert.Greater(product.Rating, 0, "The Rating is not correctly filled.");
            }
        }

        [Test]
        public void VerifyClrTypes()
        {
            var clr = ctx.ClrTypes.All().First();
            Assert.AreEqual(new[] { (byte)1 }, clr.BinaryValue, "The BinaryValue is not correctly filled.");
            Assert.AreEqual(true, clr.BoolValue, "The BoolValue is not correctly filled.");
            Assert.AreEqual(new DateTime(2012, 1, 1), clr.DateTimeValue, "The DateTimeValue is not correctly filled.");
            Assert.AreEqual("01:02:03", clr.TimeSpanValue, "The TimeSpan is not correctly filled.");
            Assert.AreEqual(Guid.Empty, clr.GuidValue, "The GuidValue is not correctly filled.");
            Assert.AreEqual(1, clr.ByteValue, "The ByteValue is not correctly filled.");
            Assert.AreEqual(2, clr.SByteValue, "The SByteValue is not correctly filled.");
            Assert.AreEqual(3, clr.Int16Value, "The Int16Value is not correctly filled.");
            Assert.AreEqual(4, clr.UInt16Value, "The UInt16Value is not correctly filled.");
            Assert.AreEqual(5, clr.Int32Value, "The Int32Value is not correctly filled.");
            Assert.AreEqual(6, clr.UInt32Value, "The UInt32Value is not correctly filled.");
            Assert.AreEqual(7, clr.Int64Value, "The Int64Value is not correctly filled.");
            Assert.AreEqual(8, clr.UInt64Value, "The UInt64Value is not correctly filled.");
            Assert.AreEqual(9, clr.SingleValue, "The SingleValue is not correctly filled.");
            Assert.AreEqual(10, clr.DoubleValue, "The DoubleValue is not correctly filled.");
            Assert.AreEqual("11", clr.DecimalValue, "The DecimalValue is not correctly filled.");
            Assert.AreEqual("abc", clr.StringValue, "The StringValue is not correctly filled.");
        }

        //[Test]
        //public void ResourceReferenceProperty()
        //{
        //    List<ClientCategory> categories = ctx.CreateQuery<ClientCategory>("Categories").ToList();
        //    var category = RunSingleResultQuery<ClientCategory>("/Products(0)/Category");
        //    Assert.AreEqual(categories[0], category, "The product ID 0 should be in the first category.");
        //    category = RunSingleResultQuery<ClientCategory>("/Products(1)/Category");
        //    Assert.AreEqual(categories[1], category, "The product ID 1 should be in the second category.");
        //    category = RunSingleResultQuery<ClientCategory>("/Products(2)/Category");
        //    Assert.AreEqual(categories[1], category, "The product ID 2 should be in the second category.");
        //}

        //[Test]
        //public void ResourceSetReferenceProperty()
        //{
        //    List<ClientProduct> products = ctx.CreateQuery<ClientProduct>("Products").ToList();
        //    var relatedProducts = RunQuery<ClientProduct>("/Categories(0)/Products").ToList();
        //    Assert.AreEqual(1, relatedProducts.Count, "Category 0 should have just one product.");
        //    Assert.IsTrue(relatedProducts.Contains(products[0]), "The category 0 should have product 0 in it.");
        //    relatedProducts = RunQuery<ClientProduct>("/Categories(1)/Products").ToList();
        //    Assert.AreEqual(2, relatedProducts.Count, "Category 1 should have two products.");
        //    Assert.IsTrue(relatedProducts.Contains(products[1]), "The category 1 should have product 1 in it.");
        //    Assert.IsTrue(relatedProducts.Contains(products[2]), "The category 1 should have product 2 in it.");
        //    relatedProducts = RunQuery<ClientProduct>("/Categories(2)/Products").ToList();
        //    Assert.AreEqual(0, relatedProducts.Count, "Category 2 should have no products.");
        //}
    }

    [TestFixture]
    public class InMemoryServiceQueryTests : QueryTests<ProductInMemoryService>
    {
    }

    [TestFixture]
    public class QueryableServiceQueryTests : QueryTests<ProductQueryableService>
    {
    }
}
