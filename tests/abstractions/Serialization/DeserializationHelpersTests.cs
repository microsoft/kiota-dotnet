using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Tests.Mocks;
using Moq;
using Xunit;

namespace Microsoft.Kiota.Abstractions.Tests.Serialization;

public partial class DeserializationHelpersTests
{
    private const string _jsonContentType = "application/json";
    [Fact]
    [Obsolete]
    public void DefensiveObject()
    {
        Assert.Throws<ArgumentNullException>(() => KiotaSerializer.Deserialize<TestEntity>(null!, (Stream)null!, null!));
        Assert.Throws<ArgumentNullException>(() => KiotaSerializer.Deserialize<TestEntity>(_jsonContentType, (Stream)null!, null!));
        using var stream = new MemoryStream();
        Assert.Throws<ArgumentNullException>(() => KiotaSerializer.Deserialize<TestEntity>(_jsonContentType, stream, null!));
        Assert.Throws<ArgumentNullException>(() => KiotaSerializer.Deserialize<TestEntity>(_jsonContentType, "", null!));
    }
    [Fact]
    [Obsolete]
    public void DefensiveObjectCollection()
    {
        Assert.Throws<ArgumentNullException>(() => KiotaSerializer.DeserializeCollection<TestEntity>(null!, (Stream)null!, null!));
        Assert.Throws<ArgumentNullException>(() => KiotaSerializer.DeserializeCollection<TestEntity>(_jsonContentType, (Stream)null!, null!));
        using var stream = new MemoryStream();
        Assert.Throws<ArgumentNullException>(() => KiotaSerializer.DeserializeCollection<TestEntity>(_jsonContentType, stream, null!));
        Assert.Throws<ArgumentNullException>(() => KiotaSerializer.DeserializeCollection<TestEntity>(_jsonContentType, "", null!));
    }
    [Fact]
    [Obsolete]
    public void DeserializesObjectWithoutReflection()
    {
        var strValue = "{'id':'123'}";
        var mockParseNode = new Mock<IParseNode>();
        mockParseNode.Setup(x => x.GetObjectValue(It.IsAny<ParsableFactory<TestEntity>>())).Returns(new TestEntity()
        {
            Id = "123"
        });
        var mockJsonParseNodeFactory = new Mock<IAsyncParseNodeFactory>();
        mockJsonParseNodeFactory.Setup(x => x.GetRootParseNode(It.IsAny<string>(), It.IsAny<Stream>())).Returns(mockParseNode.Object);
        mockJsonParseNodeFactory.Setup(x => x.ValidContentType).Returns(_jsonContentType);
        ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[_jsonContentType] = mockJsonParseNodeFactory.Object;

        var result = KiotaSerializer.Deserialize(_jsonContentType, strValue, TestEntity.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
    }
    [Fact]
    [Obsolete]
    public void DeserializesObjectWithReflection()
    {
        var strValue = "{'id':'123'}";
        var mockParseNode = new Mock<IParseNode>();
        mockParseNode.Setup(x => x.GetObjectValue(It.IsAny<ParsableFactory<TestEntity>>())).Returns(new TestEntity()
        {
            Id = "123"
        });
        var mockJsonParseNodeFactory = new Mock<IAsyncParseNodeFactory>();
        mockJsonParseNodeFactory.Setup(x => x.GetRootParseNode(It.IsAny<string>(), It.IsAny<Stream>())).Returns(mockParseNode.Object);
        mockJsonParseNodeFactory.Setup(x => x.ValidContentType).Returns(_jsonContentType);
        ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[_jsonContentType] = mockJsonParseNodeFactory.Object;

        var result = KiotaSerializer.Deserialize<TestEntity>(_jsonContentType, strValue);

        Assert.NotNull(result);
    }
    [Fact]
    [Obsolete]
    public void DeserializesCollectionOfObject()
    {
        var strValue = "{'id':'123'}";
        var mockParseNode = new Mock<IParseNode>();
        mockParseNode.Setup(x => x.GetCollectionOfObjectValues(It.IsAny<ParsableFactory<TestEntity>>())).Returns(new List<TestEntity> {
            new TestEntity()
            {
                Id = "123"
            }
        });
        var mockJsonParseNodeFactory = new Mock<IAsyncParseNodeFactory>();
        mockJsonParseNodeFactory.Setup(x => x.GetRootParseNode(It.IsAny<string>(), It.IsAny<Stream>())).Returns(mockParseNode.Object);
        mockJsonParseNodeFactory.Setup(x => x.ValidContentType).Returns(_jsonContentType);
        ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[_jsonContentType] = mockJsonParseNodeFactory.Object;

        var result = KiotaSerializer.DeserializeCollection(_jsonContentType, strValue, TestEntity.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task DefensiveObjectAsync()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeAsync<TestEntity>(null!, (Stream)null!, null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeAsync<TestEntity>(_jsonContentType, (Stream)null!, null!));
        using var stream = new MemoryStream();
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeAsync<TestEntity>(_jsonContentType, stream, null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeAsync<TestEntity>(_jsonContentType, "", null!));
    }
    [Fact]
    public async Task DefensiveObjectCollectionAsync()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeCollectionAsync<TestEntity>(null!, (Stream)null!, null!, default));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeCollectionAsync<TestEntity>(_jsonContentType, (Stream)null!, null!));
        using var stream = new MemoryStream();
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeCollectionAsync<TestEntity>(_jsonContentType, stream, null!));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeCollectionAsync<TestEntity>(_jsonContentType, "", null!));
    }


    [Fact]
    public async Task DeserializesObjectWithoutReflectionAsync()
    {
        var strValue = "{'id':'123'}";
        var mockParseNode = new Mock<IParseNode>();
        mockParseNode.Setup(x => x.GetObjectValue(It.IsAny<ParsableFactory<TestEntity>>())).Returns(new TestEntity()
        {
            Id = "123"
        });
        var mockJsonParseNodeFactory = new Mock<IAsyncParseNodeFactory>();
        mockJsonParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(mockParseNode.Object));
        mockJsonParseNodeFactory.Setup(x => x.ValidContentType).Returns(_jsonContentType);
        ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[_jsonContentType] = mockJsonParseNodeFactory.Object;

        var result = await KiotaSerializer.DeserializeAsync(_jsonContentType, strValue, TestEntity.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
    }
    [Fact]
    public async Task DeserializesObjectWithReflectionAsync()
    {
        var strValue = "{'id':'123'}";
        var mockParseNode = new Mock<IParseNode>();
        mockParseNode.Setup(x => x.GetObjectValue(It.IsAny<ParsableFactory<TestEntity>>())).Returns(new TestEntity()
        {
            Id = "123"
        });
        var mockJsonParseNodeFactory = new Mock<IAsyncParseNodeFactory>();
        mockJsonParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(mockParseNode.Object));
        mockJsonParseNodeFactory.Setup(x => x.ValidContentType).Returns(_jsonContentType);
        ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[_jsonContentType] = mockJsonParseNodeFactory.Object;

        var result = await KiotaSerializer.DeserializeAsync<TestEntity>(_jsonContentType, strValue);

        Assert.NotNull(result);
    }
    [Fact]
    public async Task DeserializesCollectionOfObjectAsync()
    {
        var strValue = "{'id':'123'}";
        var mockParseNode = new Mock<IParseNode>();
        mockParseNode.Setup(x => x.GetCollectionOfObjectValues(It.IsAny<ParsableFactory<TestEntity>>())).Returns(new List<TestEntity> {
            new TestEntity()
            {
                Id = "123"
            }
        });
        var mockJsonParseNodeFactory = new Mock<IAsyncParseNodeFactory>();
        mockJsonParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(mockParseNode.Object));
        mockJsonParseNodeFactory.Setup(x => x.ValidContentType).Returns(_jsonContentType);
        ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[_jsonContentType] = mockJsonParseNodeFactory.Object;

        var result = await KiotaSerializer.DeserializeCollectionAsync(_jsonContentType, strValue, TestEntity.CreateFromDiscriminatorValue);

        Assert.NotNull(result);
        Assert.Single(result);
    }
}
