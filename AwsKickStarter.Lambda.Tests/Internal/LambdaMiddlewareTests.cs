using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AwsKickStarter.Lambda.Tests.Internal;

public class LambdaMiddlewareTests
{
    private readonly Fixture _fixture;
    private readonly ILogger<LambdaMiddleware> _mockLogger;
    private readonly LambdaMiddleware _sut;

    public LambdaMiddlewareTests()
    {
        _fixture = new();
        _mockLogger = Substitute.For<ILogger<LambdaMiddleware>>();
        _sut = new(_mockLogger);
    }

    [Fact]
    public void Decode_S3NoS3_ReturnsOriginal()
    {
        var record = _fixture.Build<S3Event.S3EventNotificationRecord>()
                             .Without(_ => _.S3)
                             .Create();
        var originalJson = JsonSerializer.Serialize(record);
        var result = _sut.Decode(record);
        JsonSerializer.Serialize(result).ShouldBe(originalJson);
    }

    [Fact]
    public void Decode_S3NoObject_ReturnsOriginal()
    {
        var record = _fixture.Build<S3Event.S3EventNotificationRecord>()
                             .With(_ => _.S3, _fixture.Build<S3Event.S3Entity>()
                                                      .Without(_ => _.Object)
                                                      .Create())
                             .Create();
        var originalJson = JsonSerializer.Serialize(record);
        var result = _sut.Decode(record);
        JsonSerializer.Serialize(result).ShouldBe(originalJson);
    }

    [Fact]
    public void Decode_S3NoKey_ReturnsOriginal()
    {
        var record = _fixture.Build<S3Event.S3EventNotificationRecord>()
                             .With(_ => _.S3, _fixture.Build<S3Event.S3Entity>()
                                                      .With(_ => _.Object, _fixture.Build<S3Event.S3ObjectEntity>()
                                                                                   .Without(_ => _.Key)
                                                                                   .Create())
                                                      .Create())
                             .Create();
        var originalJson = JsonSerializer.Serialize(record);
        var result = _sut.Decode(record);
        JsonSerializer.Serialize(result).ShouldBe(originalJson);
    }

    [Fact]
    public void Decode_S3SimpleKey_ReturnsOriginal()
    {
        var record = _fixture.Create<S3Event.S3EventNotificationRecord>();
        var originalJson = JsonSerializer.Serialize(record);
        var result = _sut.Decode(record);
        JsonSerializer.Serialize(result).ShouldBe(originalJson);
    }

    [Fact]
    public void Decode_S3EncodedKey_UrlDecodesKey()
    {
        var record = _fixture.Create<S3Event.S3EventNotificationRecord>();
        record.S3.Object.Key = "My%3AKey+With+Spaces";
        var result = _sut.Decode(record);
        result.S3.Object.Key.ShouldBe("My:Key With Spaces");
    }

    [Fact]
    public void Decode_SnsWithNoSns_ReturnsOriginal()
    {
        var record = _fixture.Build<SNSEvent.SNSRecord>()
                             .Without(_ => _.Sns)
                             .Create();
        var originalJson = JsonSerializer.Serialize(record);
        var result = _sut.Decode(record);
        JsonSerializer.Serialize(result).ShouldBe(originalJson);
    }

    [Fact]
    public void Decode_SnsWithNoMessageAttributes_ReturnsOriginal()
    {
        var record = _fixture.Build<SNSEvent.SNSRecord>()
                             .With(_ => _.Sns, _fixture.Build<SNSEvent.SNSMessage>()
                                                       .Without(_ => _.MessageAttributes)
                                                       .Create())
                             .Create();
        var originalJson = JsonSerializer.Serialize(record);
        var result = _sut.Decode(record);
        JsonSerializer.Serialize(result).ShouldBe(originalJson);
    }

    [Fact]
    public void Decode_SnsWithNoContentEncoding_ReturnsOriginal()
    {
        var record = _fixture.Build<SNSEvent.SNSRecord>()
                             .With(_ => _.Sns, _fixture.Build<SNSEvent.SNSMessage>()
                                                       .With(_ => _.MessageAttributes, new Dictionary<string, SNSEvent.MessageAttribute>())
                                                       .Create())
                             .Create();
        var originalJson = JsonSerializer.Serialize(record);
        var result = _sut.Decode(record);
        JsonSerializer.Serialize(result).ShouldBe(originalJson);
    }

    [Fact]
    public void Decode_SnsWithUnsupportedContentEncoding_ReturnsOriginalMessage()
    {
        var record = _fixture.Build<SNSEvent.SNSRecord>()
                             .With(_ => _.Sns, _fixture.Build<SNSEvent.SNSMessage>()
                                                       .With(_ => _.MessageAttributes, new Dictionary<string, SNSEvent.MessageAttribute>()
                                                       {
                                                           ["Content-Encoding"] = new SNSEvent.MessageAttribute { Type = "String", Value = "unsupported" }
                                                       })
                                                       .Create())
                             .Create();
        var originalMessage = record.Sns.Message;
        var result = _sut.Decode(record);
        result.Sns.Message.ShouldBe(originalMessage);
    }

    [Fact]
    public void Decode_SnsWithUnsupportedContentEncoding_RemovesContentEncoding()
    {
        var record = _fixture.Build<SNSEvent.SNSRecord>()
                             .With(_ => _.Sns, _fixture.Build<SNSEvent.SNSMessage>()
                                                       .With(_ => _.MessageAttributes, new Dictionary<string, SNSEvent.MessageAttribute>()
                                                       {
                                                           ["Content-Encoding"] = new SNSEvent.MessageAttribute { Type = "String", Value = "unsupported" }
                                                       })
                                                       .Create())
                             .Create();
        var result = _sut.Decode(record);
        result.Sns.MessageAttributes.ShouldNotContainKey("Content-Encoding");
    }

    [Fact]
    public void Decode_SnsWithGzipContentEncoding_ReturnsDecoded()
    {
        var record = _fixture.Build<SNSEvent.SNSRecord>()
                             .With(_ => _.Sns, _fixture.Build<SNSEvent.SNSMessage>()
                                                       .With(_ => _.Message, "H4sIAAAAAAAACqtW8kxRsjLUUfJLzE1VslJyTEnMVaoFAM6IgUgWAAAA")
                                                       .With(_ => _.MessageAttributes, new Dictionary<string, SNSEvent.MessageAttribute>()
                                                       {
                                                           ["Content-Encoding"] = new SNSEvent.MessageAttribute { Type = "String", Value = "gzip" }
                                                       })
                                                       .Create())
                             .Create();
        var result = _sut.Decode(record);
        result.Sns.Message.ShouldBe("{\"Id\":1,\"Name\":\"Adam\"}");
    }

    [Fact]
    public void Decode_SnsWithGzipContentEncoding_RemovesContentEncoding()
    {
        var record = _fixture.Build<SNSEvent.SNSRecord>()
                             .With(_ => _.Sns, _fixture.Build<SNSEvent.SNSMessage>()
                                                       .With(_ => _.Message, "H4sIAAAAAAAACqtW8kxRsjLUUfJLzE1VslJyTEnMVaoFAM6IgUgWAAAA")
                                                       .With(_ => _.MessageAttributes, new Dictionary<string, SNSEvent.MessageAttribute>()
                                                       {
                                                           ["Content-Encoding"] = new SNSEvent.MessageAttribute { Type = "String", Value = "gzip" }
                                                       })
                                                       .Create())
                             .Create();
        var result = _sut.Decode(record);
        result.Sns.MessageAttributes.ShouldNotContainKey("Content-Encoding");
    }

    [Fact]
    public void Decode_SqsWithNoMessageAttributes_ReturnsOriginal()
    {
        var record = _fixture.Build<SQSEvent.SQSMessage>()
                             .Without(_ => _.MessageAttributes)
                             .Create();
        var originalJson = JsonSerializer.Serialize(record);
        var result = _sut.Decode(record);
        JsonSerializer.Serialize(result).ShouldBe(originalJson);
    }

    [Fact]
    public void Decode_SqsWithNoContentEncoding_ReturnsOriginal()
    {
        var record = _fixture.Build<SQSEvent.SQSMessage>()
                             .With(_ => _.MessageAttributes, new Dictionary<string, SQSEvent.MessageAttribute>())
                             .Create();
        var originalJson = JsonSerializer.Serialize(record);
        var result = _sut.Decode(record);
        JsonSerializer.Serialize(result).ShouldBe(originalJson);
    }

    [Fact]
    public void Decode_SqsWithUnsupportedContentEncoding_ReturnsOriginalMessage()
    {
        var record = _fixture.Build<SQSEvent.SQSMessage>()
                             .With(_ => _.MessageAttributes, new Dictionary<string, SQSEvent.MessageAttribute>()
                             {
                                 ["Content-Encoding"] = new SQSEvent.MessageAttribute { DataType = "String", StringValue = "unsupported" }
                             })
                             .Create();
        var originalMessage = record.Body;
        var result = _sut.Decode(record);
        result.Body.ShouldBe(originalMessage);
    }

    [Fact]
    public void Decode_SqsWithUnsupportedContentEncoding_RemovesContentEncoding()
    {
        var record = _fixture.Build<SQSEvent.SQSMessage>()
                             .With(_ => _.MessageAttributes, new Dictionary<string, SQSEvent.MessageAttribute>()
                             {
                                 ["Content-Encoding"] = new SQSEvent.MessageAttribute { DataType = "String", StringValue = "unsupported" }
                             })
                             .Create();
        var result = _sut.Decode(record);
        result.MessageAttributes.ShouldNotContainKey("Content-Encoding");
    }

    [Fact]
    public void Decode_SqsWithGzipContentEncoding_ReturnsDecoded()
    {
        var record = _fixture.Build<SQSEvent.SQSMessage>()
                             .With(_ => _.Body, "H4sIAAAAAAAACqtW8kxRsjLUUfJLzE1VslJyTEnMVaoFAM6IgUgWAAAA")
                             .With(_ => _.MessageAttributes, new Dictionary<string, SQSEvent.MessageAttribute>()
                             {
                                 ["Content-Encoding"] = new SQSEvent.MessageAttribute { DataType = "String", StringValue = "gzip" }
                             })
                             .Create();
        var result = _sut.Decode(record);
        result.Body.ShouldBe("{\"Id\":1,\"Name\":\"Adam\"}");
    }

    [Fact]
    public void Decode_SqsWithGzipContentEncoding_RemovesContentEncoding()
    {
        var record = _fixture.Build<SQSEvent.SQSMessage>()
                             .With(_ => _.Body, "H4sIAAAAAAAACqtW8kxRsjLUUfJLzE1VslJyTEnMVaoFAM6IgUgWAAAA")
                             .With(_ => _.MessageAttributes, new Dictionary<string, SQSEvent.MessageAttribute>()
                             {
                                 ["Content-Encoding"] = new SQSEvent.MessageAttribute { DataType = "String", StringValue = "gzip" }
                             })
                             .Create();
        var result = _sut.Decode(record);
        result.MessageAttributes.ShouldNotContainKey("Content-Encoding");
    }

    [Fact]
    public void Deserialize_SnsNoSns_ThrowsException()
    {
        var record = _fixture.Build<SNSEvent.SNSRecord>()
                             .Without(_ => _.Sns)
                             .Create();
        var ex = Should.Throw<DeserializeException<TestInput>>(() => _sut.Deserialize<TestInput>(record));
        ex.TargetType.ShouldBe(typeof(TestInput));
        ex.Input.ShouldBeNull();
        var inner = ex.InnerException.ShouldBeOfType<ArgumentNullException>();
        inner.ParamName.ShouldBe("body");
    }

    [Fact]
    public void Deserialize_SnsNoMessage_ThrowsException()
    {
        var record = _fixture.Build<SNSEvent.SNSRecord>()
                             .With(_ => _.Sns, new SNSEvent.SNSMessage())
                             .Create();
        var ex = Should.Throw<DeserializeException<TestInput>>(() => _sut.Deserialize<TestInput>(record));
        ex.TargetType.ShouldBe(typeof(TestInput));
        ex.Input.ShouldBeNull();
        var inner = ex.InnerException.ShouldBeOfType<ArgumentNullException>();
        inner.ParamName.ShouldBe("body");
    }

    [Fact]
    public void Deserialize_SnsNull_ThrowsException()
    {
        var record = _fixture.Build<SNSEvent.SNSRecord>()
                             .With(_ => _.Sns, new SNSEvent.SNSMessage { Message = "null" })
                             .Create();
        var ex = Should.Throw<DeserializeException<TestInput>>(() => _sut.Deserialize<TestInput>(record));
        ex.TargetType.ShouldBe(typeof(TestInput));
        ex.Input.ShouldBe(record.Sns.Message);
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Deserialize_SnsInvalid_ThrowsException()
    {
        var record = _fixture.Build<SNSEvent.SNSRecord>()
                             .With(_ => _.Sns, new SNSEvent.SNSMessage { Message = "INVALID" })
                             .Create();
        var ex = Should.Throw<DeserializeException<TestInput>>(() => _sut.Deserialize<TestInput>(record));
        ex.TargetType.ShouldBe(typeof(TestInput));
        ex.Input.ShouldBe(record.Sns.Message);
        ex.InnerException.ShouldBeOfType<JsonException>();
    }

    [Fact]
    public void Deserialize_SnsValid_ReturnsDeserialized()
    {
        var entity = _fixture.Create<TestInput>();
        var record = _fixture.Build<SNSEvent.SNSRecord>()
                             .With(_ => _.Sns, new SNSEvent.SNSMessage { Message = JsonSerializer.Serialize(entity) })
                             .Create();
        var deserialized = _sut.Deserialize<TestInput>(record);
        deserialized.ShouldBe(entity);
    }

    [Fact]
    public void Deserialize_SqsNoBody_ThrowsException()
    {
        var record = _fixture.Build<SQSEvent.SQSMessage>()
                             .Without(_ => _.Body)
                             .Without(_ => _.MessageAttributes)
                             .Create();
        var ex = Should.Throw<DeserializeException<TestInput>>(() => _sut.Deserialize<TestInput>(record));
        ex.TargetType.ShouldBe(typeof(TestInput));
        ex.Input.ShouldBeNull();
        var inner = ex.InnerException.ShouldBeOfType<ArgumentNullException>();
        inner.ParamName.ShouldBe("body");
    }

    [Fact]
    public void Deserialize_SqsNull_ThrowsException()
    {
        var record = _fixture.Build<SQSEvent.SQSMessage>()
                             .With(_ => _.Body, "null")
                             .Without(_ => _.MessageAttributes)
                             .Create();
        var ex = Should.Throw<DeserializeException<TestInput>>(() => _sut.Deserialize<TestInput>(record));
        ex.TargetType.ShouldBe(typeof(TestInput));
        ex.Input.ShouldBe(record.Body);
        ex.InnerException.ShouldBeNull();
    }

    [Fact]
    public void Deserialize_SqsInvalid_ThrowsException()
    {
        var record = _fixture.Build<SQSEvent.SQSMessage>()
                             .With(_ => _.Body, "INVALID")
                             .Without(_ => _.MessageAttributes)
                             .Create();
        var ex = Should.Throw<DeserializeException<TestInput>>(() => _sut.Deserialize<TestInput>(record));
        ex.TargetType.ShouldBe(typeof(TestInput));
        ex.Input.ShouldBe(record.Body);
        ex.InnerException.ShouldBeOfType<JsonException>();
    }

    [Fact]
    public void Deserialize_SqsValid_ReturnsDeserialized()
    {
        var entity = _fixture.Create<TestInput>();
        var record = _fixture.Build<SQSEvent.SQSMessage>()
                             .With(_ => _.Body, JsonSerializer.Serialize(entity))
                             .Without(_ => _.MessageAttributes)
                             .Create();
        var deserialized = _sut.Deserialize<TestInput>(record);
        deserialized.ShouldBe(entity);
    }
}
