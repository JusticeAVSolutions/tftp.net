using System;

namespace Tftp.Net;

/// <summary>
/// A single transfer options according to RFC2347.
/// </summary>
public class TransferOption
{
    public string Name { get; }
    public string Value { get; set; }
    public bool IsAcknowledged { get; internal set; }

    internal TransferOption(string name, string value)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("name must not be null or empty.");

        Name = name;
        Value = value ?? throw new ArgumentNullException(nameof(value), "value must not be null.");
    }

    public override string ToString() => $"{Name}={Value}";
}