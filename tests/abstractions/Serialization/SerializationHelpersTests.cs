using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Tests.Mocks;
using Moq;
using Xunit;

namespace Microsoft.Kiota.Abstractions.Tests.Serialization;

public class SerializationHelpersTests
{
    private const string _jsonContentType = "application/json";
    [Fact]
    public void DefensiveObject()
    {
        Assert.Throws<ArgumentNullException>(() => KiotaSerializer.SerializeAsStream(null!, (TestEntity)null!));
        Assert.Throws<ArgumentNullException>(() => KiotaSerializer.SerializeAsStream(_jsonContentType, (TestEntity)null!));
    }
    [Fact]
    public void DefensiveObjectCollection()
    {
        Assert.Throws<ArgumentNullException>(() => KiotaSerializer.SerializeAsStream(null!, (IEnumerable<TestEntity>)null!));
        Assert.Throws<ArgumentNullException>(() => KiotaSerializer.SerializeAsStream(_jsonContentType, (IEnumerable<TestEntity>)null!));
    }
    [Fact]
    public async Task SerializesObject()
    {
        var mockSerializationWriter = new Mock<ISerializationWriter>();
        mockSerializationWriter.Setup(x => x.GetSerializedContent()).Returns(new MemoryStream(UTF8Encoding.UTF8.GetBytes("{'id':'123'}")));
        var mockSerializationWriterFactory = new Mock<ISerializationWriterFactory>();
        mockSerializationWriterFactory.Setup(x => x.GetSerializationWriter(It.IsAny<string>())).Returns(mockSerializationWriter.Object);
        SerializationWriterFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[_jsonContentType] = mockSerializationWriterFactory.Object;

        var result = await KiotaSerializer.SerializeAsStringAsync(_jsonContentType, new TestEntity()
        {
            Id = "123"
        }, TestContext.Current.CancellationToken);

        Assert.Equal("{'id':'123'}", result);

        mockSerializationWriterFactory.Verify(x => x.GetSerializationWriter(It.IsAny<string>()), Times.Once);
        mockSerializationWriter.Verify(x => x.WriteObjectValue(It.IsAny<string>(), It.IsAny<TestEntity>()), Times.Once);
        mockSerializationWriter.Verify(x => x.GetSerializedContent(), Times.Once);
    }
    [Fact]
    public async Task SerializesObjectCollection()
    {
        var mockSerializationWriter = new Mock<ISerializationWriter>();
        mockSerializationWriter.Setup(x => x.GetSerializedContent()).Returns(new MemoryStream(UTF8Encoding.UTF8.GetBytes("[{'id':'123'}]")));
        var mockSerializationWriterFactory = new Mock<ISerializationWriterFactory>();
        mockSerializationWriterFactory.Setup(x => x.GetSerializationWriter(It.IsAny<string>())).Returns(mockSerializationWriter.Object);
        SerializationWriterFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories[_jsonContentType] = mockSerializationWriterFactory.Object;

        var result = await KiotaSerializer.SerializeAsStringAsync(_jsonContentType, new List<TestEntity> {
            new()
            {
                Id = "123"
            }
        }, TestContext.Current.CancellationToken);

        Assert.Equal("[{'id':'123'}]", result);

        mockSerializationWriterFactory.Verify(x => x.GetSerializationWriter(It.IsAny<string>()), Times.Once);
        mockSerializationWriter.Verify(x => x.WriteCollectionOfObjectValues(It.IsAny<string>(), It.IsAny<IEnumerable<TestEntity>>()), Times.Once);
        mockSerializationWriter.Verify(x => x.GetSerializedContent(), Times.Once);
    }
}