namespace AwsKickStarter.Lambda;

/// <summary>
/// Represents an exception that occurs during deserialization.
/// </summary>
public class DeserializeException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeserializeException"/> class.
    /// </summary>
    /// <param name="input">The input that caused the exception.</param>
    /// <param name="targetType">The target type that the input was being deserialized to.</param>
    public DeserializeException(string? input, Type targetType) : base(DefaultMessage(targetType))
    {
        Input = input;
        TargetType = targetType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeserializeException"/> class.
    /// </summary>
    /// <param name="input">The input that caused the exception.</param>
    /// <param name="targetType">The target type that the input was being deserialized to.</param>
    /// <param name="innerException">The underlying <see cref="Exception"/>.</param>
    public DeserializeException(string? input, Type targetType, Exception? innerException) : base(DefaultMessage(targetType), innerException)
    {
        Input = input;
        TargetType = targetType;
    }

    /// <summary>
    /// Gets or sets the input that caused the exception.
    /// </summary>
    public string? Input { get; set; }

    /// <summary>
    /// Gets or sets the target type that the input was being deserialized to.
    /// </summary>
    public Type TargetType { get; set; }

    private static string? DefaultMessage(Type targetType) => $"Failed to deserialize message to {targetType}";
}
