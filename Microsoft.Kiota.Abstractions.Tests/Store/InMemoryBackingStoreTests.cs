using System.Collections.Generic;
using System.Linq;
using Microsoft.Kiota.Abstractions.Store;
using Microsoft.Kiota.Abstractions.Tests.Mocks;
using Xunit;

namespace Microsoft.Kiota.Abstractions.Tests.Store
{
    public class InMemoryBackingStoreTests
    {
        [Fact]
        public void SetsAndGetsValueFromStore()
        {
            // Arrange
            var testBackingStore = new InMemoryBackingStore();
            // Act
            Assert.Empty(testBackingStore.Enumerate());
            testBackingStore.Set("name", "Peter");
            // Assert
            Assert.NotEmpty(testBackingStore.Enumerate());
            Assert.Equal("Peter",testBackingStore.Enumerate().First().Value);
        }

        [Fact]
        public void PreventsDuplicatesInStore()
        {
            // Arrange
            var testBackingStore = new InMemoryBackingStore();
            // Act
            Assert.Empty(testBackingStore.Enumerate());
            testBackingStore.Set("name", "Peter");
            testBackingStore.Set("name", "Peter Pan");// modify a second time
            // Assert
            Assert.NotEmpty(testBackingStore.Enumerate());
            Assert.Single(testBackingStore.Enumerate());
            Assert.Equal("Peter Pan", testBackingStore.Enumerate().First().Value);
        }

        [Fact]
        public void EnumeratesValuesChangedToNullInStore()
        {
            // Arrange
            var testBackingStore = new InMemoryBackingStore();
            // Act
            Assert.Empty(testBackingStore.Enumerate());
            testBackingStore.Set("name", "Peter Pan");
            testBackingStore.Set("email", "peterpan@neverland.com");
            testBackingStore.Set<string>("phone", null); // phone changes to null
            // Assert
            Assert.NotEmpty(testBackingStore.EnumerateKeysForValuesChangedToNull());
            Assert.Single(testBackingStore.EnumerateKeysForValuesChangedToNull());
            Assert.Equal(3, testBackingStore.Enumerate().Count());// all values come back
            Assert.Equal("phone", testBackingStore.EnumerateKeysForValuesChangedToNull().First());
        }

        [Fact]
        public void TestsBackingStoreEmbeddedInModel()
        {
            // Arrange dummy user with initialized backingstore
            var testUser = new TestEntity
            {
                Id = "84c747c1-d2c0-410d-ba50-fc23e0b4abbe",
            };
            testUser.BackingStore.InitializationCompleted = true;
            // Act on the data by making a change
            testUser.BusinessPhones = new List<string> 
            {
                "+1 234 567 891"
            };
            // Assert by retrieving only changed values
            testUser.BackingStore.ReturnOnlyChangedValues = true;
            var changedValues = testUser.BackingStore.Enumerate();
            Assert.NotEmpty(changedValues);
            Assert.Single(changedValues);
            Assert.Equal("businessPhones", changedValues.First().Key);
        }
        [Fact]
        public void TestsBackingStoreEmbeddedInModelWithAdditionDataValues()
        {
            // Arrange dummy user with initialized backingstore
            var testUser = new TestEntity
            {
                Id = "84c747c1-d2c0-410d-ba50-fc23e0b4abbe",
                AdditionalData = new Dictionary<string, object> 
                {
                    { "extensionData" , null }
                }
            };
            testUser.BackingStore.InitializationCompleted = true;
            // Act on the data by making a change to a property and additionalData
            testUser.BusinessPhones = new List<string>
            {
                "+1 234 567 891"
            };
            testUser.AdditionalData.Add("anotherExtension", null);
            // Assert by retrieving only changed values
            testUser.BackingStore.ReturnOnlyChangedValues = true;
            var changedValues = testUser.BackingStore.Enumerate();
            Assert.NotEmpty(changedValues);
            Assert.Equal(2, changedValues.Count());
            var resultList = changedValues.ToList();
            Assert.Equal("additionalData", resultList[0].Key);
            Assert.Equal("businessPhones", resultList[1].Key);
        }
        [Fact]
        public void TestsBackingStoreEmbeddedInModelWithCollectionPropertyReplacedWithNewCollection()
        {
            // Arrange dummy user with initialized backingstore
            var testUser = new TestEntity
            {
                Id = "84c747c1-d2c0-410d-ba50-fc23e0b4abbe",
                BusinessPhones = new List<string>
                {
                    "+1 234 567 891"
                }
            };
            testUser.BackingStore.InitializationCompleted = true;
            // Act on the data by making a change
            testUser.BusinessPhones = new List<string>
            {
                "+1 234 567 891"
            };
            // Assert by retrieving only changed values
            testUser.BackingStore.ReturnOnlyChangedValues = true;
            var changedValues = testUser.BackingStore.Enumerate();
            Assert.NotEmpty(changedValues);
            Assert.Single(changedValues);
            Assert.Equal("businessPhones", changedValues.First().Key);
        }
        [Fact]
        public void TestsBackingStoreEmbeddedInModelWithCollectionPropertyReplacedWithNull()
        {
            // Arrange dummy user with initialized backingstore
            var testUser = new TestEntity
            {
                Id = "84c747c1-d2c0-410d-ba50-fc23e0b4abbe",
                BusinessPhones = new List<string>
                {
                    "+1 234 567 891"
                }
            };
            testUser.BackingStore.InitializationCompleted = true;
            // Act on the data by making a change
            testUser.BusinessPhones = null;
            // Assert by retrieving only changed values
            testUser.BackingStore.ReturnOnlyChangedValues = true;
            var changedValues = testUser.BackingStore.Enumerate();
            Assert.NotEmpty(changedValues);
            Assert.Single(changedValues);
            Assert.Equal("businessPhones", changedValues.First().Key);
            var changedValuesToNull = testUser.BackingStore.EnumerateKeysForValuesChangedToNull();
            Assert.NotEmpty(changedValuesToNull);
            Assert.Single(changedValuesToNull);
            Assert.Equal("businessPhones", changedValues.First().Key);
        }
        [Fact]
        public void TestsBackingStoreEmbeddedInModelWithCollectionPropertyModifiedByAdd()
        {
            // Arrange dummy user with initialized backingstore
            var testUser = new TestEntity
            {
                Id = "84c747c1-d2c0-410d-ba50-fc23e0b4abbe",
                BusinessPhones = new List<string>
                {
                    "+1 234 567 891"
                }
            };
            testUser.BackingStore.InitializationCompleted = true;
            // Act on the data by making a change
            testUser.BusinessPhones.Add("+1 234 567 891");
            // Assert by retrieving only changed values
            testUser.BackingStore.ReturnOnlyChangedValues = true;
            var changedValues = testUser.BackingStore.Enumerate();
            Assert.NotEmpty(changedValues);
            Assert.Single(changedValues);
            Assert.Equal("businessPhones", changedValues.First().Key);
        }
        [Fact]
        public void TestsBackingStoreEmbeddedInModelWithBySettingNestedIBackedModel()
        {
            // Arrange dummy user with initialized backingstore
            var testUser = new TestEntity
            {
                Id = "84c747c1-d2c0-410d-ba50-fc23e0b4abbe"
            };
            testUser.BackingStore.InitializationCompleted = true;
            // Act on the data by making a change
            testUser.Manager = new TestEntity 
            {
                Id = "2fe22fe5-1132-42cf-90f9-1dc17e325a74"
            };
            // Assert by retrieving only changed values
            testUser.BackingStore.ReturnOnlyChangedValues = true;
            var changedValues = testUser.BackingStore.Enumerate();
            Assert.NotEmpty(changedValues);
            Assert.Single(changedValues);
            Assert.Equal("manager", changedValues.First().Key);
        }
        [Fact]
        public void TestsBackingStoreEmbeddedInModelWithByUpdatingNestedIBackedModel()
        {
            // Arrange dummy user with initialized backingstore
            var testUser = new TestEntity
            {
                Id = "84c747c1-d2c0-410d-ba50-fc23e0b4abbe",
                Manager = new TestEntity
                {
                    Id = "2fe22fe5-1132-42cf-90f9-1dc17e325a74"
                }
            };
            testUser.BackingStore.InitializationCompleted = testUser.Manager.BackingStore.InitializationCompleted = true;
            // Act on the data by making a change in the nested Ibackedmodel
            testUser.Manager.BusinessPhones = new List<string>
            {
                "+1 234 567 891"
            };
            // Assert by retrieving only changed values
            testUser.BackingStore.ReturnOnlyChangedValues = true;
            var changedValues = testUser.BackingStore.Enumerate();
            Assert.NotEmpty(changedValues);
            Assert.Single(changedValues);
            Assert.Equal("manager", changedValues.First().Key);//Backingstore should detect manager property changed
        }
    }
}
