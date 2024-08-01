using System.IO;
using Microsoft.Kiota.Abstractions.Serialization;
using Moq;
using Xunit;

namespace Microsoft.Kiota.Abstractions.Tests;

public class MultipartBodyTests
{
    [Fact]
    public void KeepsFilename()
    {
        var writerMock = new Mock<ISerializationWriter>();
        var jsonWriterMock = new Mock<ISerializationWriter>();
        var requestAdapterMock = new Mock<IRequestAdapter>();
        var serializationFactoryMock = new Mock<ISerializationWriterFactory>();

        var body = new MultipartBody();
        jsonWriterMock.Setup(w => w.WriteStringValue("", "fileContent"));
        using var ms = new MemoryStream();
        using var sr = new StreamWriter(ms);

        sr.Write("fileContent");
        sr.Flush();
        jsonWriterMock.Setup(w => w.GetSerializedContent()).Returns(ms);

        serializationFactoryMock
            .Setup(r => r.GetSerializationWriter("application/json", true))
            .Returns(jsonWriterMock.Object);

        requestAdapterMock
            .Setup(r => r.SerializationWriterFactory)
            .Returns(serializationFactoryMock.Object);

        body.RequestAdapter = requestAdapterMock.Object;

        writerMock.Setup(w => w.WriteStringValue("", "--" + body.Boundary));
        writerMock.Setup(w => w.WriteStringValue("Content-Type", "application/json"));
        writerMock.Setup(w => w.WriteStringValue("Content-Disposition", "form-data; name=\"file\"; filename=\"file.json\""));
        writerMock.Setup(w => w.WriteByteArrayValue("", ms.ToArray()));
        writerMock.Setup(w => w.WriteStringValue("", ""));
        writerMock.Setup(w => w.WriteStringValue("", "--" + body.Boundary + "--"));

        body.AddOrReplacePart("file", "application/json", "fileContent", "file.json");
        body.Serialize(writerMock.Object);

        writerMock.VerifyAll();
        requestAdapterMock.VerifyAll();
        serializationFactoryMock.VerifyAll();
    }

    [Fact]
    public void WorksWithoutFilename()
    {
        var writerMock = new Mock<ISerializationWriter>();
        var jsonWriterMock = new Mock<ISerializationWriter>();
        var requestAdapterMock = new Mock<IRequestAdapter>();
        var serializationFactoryMock = new Mock<ISerializationWriterFactory>();

        var body = new MultipartBody();
        jsonWriterMock.Setup(w => w.WriteStringValue("", "fileContent"));
        using var ms = new MemoryStream();
        using var sr = new StreamWriter(ms);

        sr.Write("fileContent");
        sr.Flush();
        jsonWriterMock.Setup(w => w.GetSerializedContent()).Returns(ms);

        serializationFactoryMock
            .Setup(r => r.GetSerializationWriter("application/json", It.IsAny<bool>()))
            .Returns(jsonWriterMock.Object);

        requestAdapterMock
            .Setup(r => r.SerializationWriterFactory)
            .Returns(serializationFactoryMock.Object);

        body.RequestAdapter = requestAdapterMock.Object;

        writerMock.Setup(w => w.WriteStringValue("", "--" + body.Boundary));
        writerMock.Setup(w => w.WriteStringValue("Content-Type", "application/json"));
        writerMock.Setup(w => w.WriteStringValue("Content-Disposition", "form-data; name=\"file\""));
        writerMock.Setup(w => w.WriteByteArrayValue("", ms.ToArray()));
        writerMock.Setup(w => w.WriteStringValue("", ""));
        writerMock.Setup(w => w.WriteStringValue("", "--" + body.Boundary + "--"));

        body.AddOrReplacePart("file", "application/json", "fileContent");
        body.Serialize(writerMock.Object);

        writerMock.VerifyAll();
        requestAdapterMock.VerifyAll();
        serializationFactoryMock.VerifyAll();
    }
}
