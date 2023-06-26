namespace Microsoft.Kiota.Abstractions.Serialization;

/// <summary>
/// Defines the additional information for a composed type.
/// </summary>
public interface IComposedTypeWrapper
{
    /// <summary>
    /// Whether the type of the composed type is scalar.
    /// </summary>
    public bool IsScalarValue();
}