namespace AwsKickStarter.Lambda;

/// <summary>
/// Represents an exception that occurs during deserialization.
/// </summary>
/// <typeparam name="TTargetType">The target type that the input was being deserialized to.</typeparam>
public class DeserializeException<TTargetType> : DeserializeException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeserializeException{TTargetType}"/> class.
    /// </summary>
    /// <param name="input">The input that caused the exception.</param>
    public DeserializeException(string? input) : base(input, typeof(TTargetType)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeserializeException{TTargetType}"/> class.
    /// </summary>
    /// <param name="input">The input that caused the exception.</param>
    /// <param name="innerException">The underlying <see cref="Exception"/>.</param>
    public DeserializeException(string? input, Exception? innerException) : base(input, typeof(TTargetType), innerException) { }
}
