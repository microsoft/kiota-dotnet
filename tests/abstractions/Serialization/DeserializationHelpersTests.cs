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
    public async Task DefensiveObject()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeAsync<TestEntity>(null!, (Stream)null!, null!, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeAsync<TestEntity>(_jsonContentType, (Stream)null!, null!, TestContext.Current.CancellationToken));
        using var stream = new MemoryStream();
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeAsync<TestEntity>(_jsonContentType, stream, null!, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeAsync<TestEntity>(_jsonContentType, "", null!, TestContext.Current.CancellationToken));
    }
    [Fact]
    public async Task DefensiveObjectCollection()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeCollectionAsync<TestEntity>(null!, (Stream)null!, null!, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeCollectionAsync<TestEntity>(_jsonContentType, (Stream)null!, null!, TestContext.Current.CancellationToken));
        using var stream = new MemoryStream();
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeCollectionAsync<TestEntity>(_jsonContentType, stream, null!, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeCollectionAsync<TestEntity>(_jsonContentType, "", null!, TestContext.Current.CancellationToken));
    }
    [Fact]
    public async Task DeserializesObjectWithoutReflection()
    {
        var strValue = "{'id':'123'}";
        var mockParseNode = new Mock<IParseNode>();
        mockParseNode.Setup(x => x.GetObjectValue(It.IsAny<ParsableFactory<TestEntity>>())).Returns(new TestEntity()
        {
            Id = "123"
        });
        var mockJsonParseNodeFactory = new Mock<IParseNodeFactory>();
        mockJsonParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockParseNode.Object);
        mockJsonParseNodeFactory.Setup(x => x.ValidContentType).Returns(_jsonContentType);
        ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[_jsonContentType] = mockJsonParseNodeFactory.Object;

        var result = await KiotaSerializer.DeserializeAsync(_jsonContentType, strValue, TestEntity.CreateFromDiscriminatorValue, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }
    [Fact]
    public async Task DeserializesObjectWithReflection()
    {
        var strValue = "{'id':'123'}";
        var mockParseNode = new Mock<IParseNode>();
        mockParseNode.Setup(x => x.GetObjectValue(It.IsAny<ParsableFactory<TestEntity>>())).Returns(new TestEntity()
        {
            Id = "123"
        });
        var mockJsonParseNodeFactory = new Mock<IParseNodeFactory>();
        mockJsonParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockParseNode.Object);
        mockJsonParseNodeFactory.Setup(x => x.ValidContentType).Returns(_jsonContentType);
        ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[_jsonContentType] = mockJsonParseNodeFactory.Object;

        var result = await KiotaSerializer.DeserializeAsync<TestEntity>(_jsonContentType, strValue, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }
    [Fact]
    public async Task DeserializesCollectionOfObject()
    {
        var strValue = "{'id':'123'}";
        var mockParseNode = new Mock<IParseNode>();
        mockParseNode.Setup(x => x.GetCollectionOfObjectValues(It.IsAny<ParsableFactory<TestEntity>>())).Returns(new List<TestEntity> {
            new TestEntity()
            {
                Id = "123"
            }
        });
        var mockJsonParseNodeFactory = new Mock<IParseNodeFactory>();
        mockJsonParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockParseNode.Object);
        mockJsonParseNodeFactory.Setup(x => x.ValidContentType).Returns(_jsonContentType);
        ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[_jsonContentType] = mockJsonParseNodeFactory.Object;

        var result = await KiotaSerializer.DeserializeCollectionAsync(_jsonContentType, strValue, TestEntity.CreateFromDiscriminatorValue, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task DefensiveObjectAsync()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeAsync<TestEntity>(null!, (Stream)null!, null!, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeAsync<TestEntity>(_jsonContentType, (Stream)null!, null!, TestContext.Current.CancellationToken));
        using var stream = new MemoryStream();
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeAsync<TestEntity>(_jsonContentType, stream, null!, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeAsync<TestEntity>(_jsonContentType, "", null!, TestContext.Current.CancellationToken));
    }
    [Fact]
    public async Task DefensiveObjectCollectionAsync()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeCollectionAsync<TestEntity>(null!, (Stream)null!, null!, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeCollectionAsync<TestEntity>(_jsonContentType, (Stream)null!, null!, TestContext.Current.CancellationToken));
        using var stream = new MemoryStream();
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeCollectionAsync<TestEntity>(_jsonContentType, stream, null!, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await KiotaSerializer.DeserializeCollectionAsync<TestEntity>(_jsonContentType, "", null!, TestContext.Current.CancellationToken));
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
        var mockJsonParseNodeFactory = new Mock<IParseNodeFactory>();
        mockJsonParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockParseNode.Object);
        mockJsonParseNodeFactory.Setup(x => x.ValidContentType).Returns(_jsonContentType);
        ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[_jsonContentType] = mockJsonParseNodeFactory.Object;

        var result = await KiotaSerializer.DeserializeAsync(_jsonContentType, strValue, TestEntity.CreateFromDiscriminatorValue, TestContext.Current.CancellationToken);

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
        var mockJsonParseNodeFactory = new Mock<IParseNodeFactory>();
        mockJsonParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockParseNode.Object);
        mockJsonParseNodeFactory.Setup(x => x.ValidContentType).Returns(_jsonContentType);
        ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[_jsonContentType] = mockJsonParseNodeFactory.Object;

        var result = await KiotaSerializer.DeserializeAsync<TestEntity>(_jsonContentType, strValue, TestContext.Current.CancellationToken);

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
        var mockJsonParseNodeFactory = new Mock<IParseNodeFactory>();
        mockJsonParseNodeFactory.Setup(x => x.GetRootParseNodeAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>())).ReturnsAsync(mockParseNode.Object);
        mockJsonParseNodeFactory.Setup(x => x.ValidContentType).Returns(_jsonContentType);
        ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[_jsonContentType] = mockJsonParseNodeFactory.Object;

        var result = await KiotaSerializer.DeserializeCollectionAsync(_jsonContentType, strValue, TestEntity.CreateFromDiscriminatorValue, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Single(result);
    }
}

