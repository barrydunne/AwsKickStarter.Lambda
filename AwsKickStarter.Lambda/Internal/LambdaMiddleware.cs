using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AwsKickStarter.Lambda.Internal;

/// <inheritdoc/>
internal class LambdaMiddleware : ILambdaMiddleware
{
    private readonly ILogger _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="LambdaMiddleware"/> class.
    /// </summary>
    /// <param name="logger">The logger to write to.</param>
    public LambdaMiddleware(ILogger<LambdaMiddleware> logger)
    {
        _logger = logger;
        _jsonSerializerOptions = JsonSerializerOptions.Web;
    }

    /// <inheritdoc/>
    public S3Event.S3EventNotificationRecord Decode(S3Event.S3EventNotificationRecord record)
    {
        _logger.LogTrace("Decoding {S3Key}", record.S3?.Object?.Key);
        if (record.S3?.Object?.Key is not null)
            record.S3.Object.Key = WebUtility.UrlDecode(record.S3.Object.Key);
        return record;
    }

    /// <inheritdoc/>
    public SNSEvent.SNSRecord Decode(SNSEvent.SNSRecord record)
    {
        if (!string.IsNullOrWhiteSpace(record.Sns?.Message))
        {
            if (record.Sns.MessageAttributes?.TryGetValue("Content-Encoding", out var contentEncodingAttribute) == true)
            {
                if (!string.IsNullOrWhiteSpace(contentEncodingAttribute.Value))
                {
                    record.Sns.Message = Decode(record.Sns.Message, contentEncodingAttribute.Value);
                    record.Sns.MessageAttributes.Remove("Content-Encoding");
                }
            }
        }
        return record;
    }

    /// <inheritdoc/>
    public SQSEvent.SQSMessage Decode(SQSEvent.SQSMessage message)
    {
        if (!string.IsNullOrWhiteSpace(message.Body))
        {
            if (message.MessageAttributes?.TryGetValue("Content-Encoding", out var contentEncodingAttribute) == true)
            {
                if (!string.IsNullOrWhiteSpace(contentEncodingAttribute.StringValue))
                {
                    message.Body = Decode(message.Body, contentEncodingAttribute.StringValue);
                    message.MessageAttributes.Remove("Content-Encoding");
                }
            }
        }
        return message;
    }

    private string Decode(string message, string contentEncoding)
    {
        return contentEncoding switch
        {
            "gzip" => DecodeGzip(message),
            _ => message
        };
    }

    private string DecodeGzip(string message)
    {
        _logger.LogTrace("Decoding gzip message {Message}", message);
        var base64EncodedBytes = Convert.FromBase64String(message);
        using var inputStream = new MemoryStream(base64EncodedBytes);
        using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
        using var reader = new StreamReader(gzipStream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    /// <inheritdoc/>
    public TMessage Deserialize<TMessage>(SNSEvent.SNSRecord record)
        => Deserialize<TMessage>(record.Sns?.Message);

    /// <inheritdoc/>
    public TMessage Deserialize<TMessage>(SQSEvent.SQSMessage message)
        => Deserialize<TMessage>(message.Body);

    private TMessage Deserialize<TMessage>(string? body)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(body, nameof(body));
            _logger.LogTrace("Deserializing {TMessage} from {Body}", typeof(TMessage), body);
            var deserialized = JsonSerializer.Deserialize<TMessage>(body, _jsonSerializerOptions);
            return deserialized ?? throw new DeserializeException<TMessage>(body);
        }
        catch (Exception ex) when (ex is not DeserializeException<TMessage>)
        {
            throw new DeserializeException<TMessage>(body, ex);
        }
    }
}
