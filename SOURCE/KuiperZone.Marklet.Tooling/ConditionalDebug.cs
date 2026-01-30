// -----------------------------------------------------------------------------
// PROJECT   : KuiperZone.Marklet
// AUTHOR    : Andrew Thomas
// COPYRIGHT : Andrew Thomas © 2025-2026 All rights reserved
// LICENSE   : AGPL-3.0-only
// -----------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace KuiperZone.Marklet.Tooling;

/// <summary>
/// A static debug utility class for internal use only.
/// </summary>
public static class ConditionalDebug
{
    private static readonly Lock _syncObj = new();
    private static HashSet<string>? _enabled = new();

    /// <summary>
    /// Equivalent to <see cref="ArgumentOutOfRangeException.ThrowIfZero"/>, but conditional on DEBUG build.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowIfZero<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : INumberBase<T>
    {
        ArgumentOutOfRangeException.ThrowIfZero(value, paramName);
    }

    /// <summary>
    /// Equivalent to <see cref="ArgumentOutOfRangeException.ThrowIfNegative"/>, but conditional on DEBUG build.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowIfNegative<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : INumberBase<T>
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value, paramName);
    }

    /// <summary>
    /// Equivalent to <see cref="ArgumentOutOfRangeException.ThrowIfNegativeOrZero"/>, but conditional on DEBUG build.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowIfNegativeOrZero<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : INumberBase<T>
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, paramName);
    }

    /// <summary>
    /// Equivalent to <see cref="ArgumentOutOfRangeException.ThrowIfLessThan"/>, but conditional on DEBUG build.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowIfLessThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T>
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, other, paramName);
    }

    /// <summary>
    /// Equivalent to <see cref="ArgumentOutOfRangeException.ThrowIfLessThanOrEqual"/>, but
    /// conditional on DEBUG build.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowIfLessThanOrEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T>
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, other, paramName);
    }

    /// <summary>
    /// Equivalent to <see cref="ArgumentOutOfRangeException.ThrowIfGreaterThan"/>, but conditional on DEBUG build.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowIfGreaterThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T>
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other, paramName);
    }

    /// <summary>
    /// Equivalent to <see cref="ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual"/>, but
    /// conditional on DEBUG build.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowIfGreaterThanOrEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T>
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, other, paramName);
    }

    /// <summary>
    /// Asserts that values are equal. Conditional on DEBUG build.
    /// </summary>
    /// <exception cref="ArgumentException">Value a != b</exception>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowIfNotEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (!Equals(value, other))
        {
            throw new ArgumentException($"Value {value} != {other}", paramName);
        }
    }

    /// <summary>
    /// Asserts that values not are equal. Conditional on DEBUG build.
    /// </summary>
    /// <exception cref="ArgumentException">Value a != b</exception>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowIfEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (Equals(value, other))
        {
            throw new ArgumentException($"Value {value} == {other}", paramName);
        }
    }

    /// <summary>
    /// Asserts that values are not same instance. Conditional on DEBUG build.
    /// </summary>
    /// <exception cref="ArgumentException">Instance is same as other</exception>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowIfSame(object? value, object? other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (ReferenceEquals(value, other))
        {
            throw new ArgumentException($"Instance {value?.GetType().Name ?? "null"} is same as other", paramName);
        }
    }

    /// <summary>
    /// Asserts that values are same instance. Conditional on DEBUG build.
    /// </summary>
    /// <exception cref="ArgumentException">Instance is same as other</exception>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowIfNotSame(object? value, object? other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (!ReferenceEquals(value, other))
        {
            throw new ArgumentException($"Instance {value?.GetType().Name ?? "null"} is not same as other", paramName);
        }
    }

    /// <summary>
    /// Asserts that value is true. Conditional on DEBUG build.
    /// </summary>
    /// <exception cref="ArgumentException">Not true</exception>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowIfFalse(bool value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (!value)
        {
            throw new ArgumentException($"Not true", paramName);
        }
    }

    /// <summary>
    /// Asserts that value is false. Conditional on DEBUG build.
    /// </summary>
    /// <exception cref="ArgumentException">Not true</exception>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowIfTrue(bool value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value)
        {
            throw new ArgumentException($"Not false", paramName);
        }
    }

    /// <summary>
    /// Asserts that string value is not null or empty. Conditional on DEBUG build.
    /// </summary>
    /// <exception cref="ArgumentException">Null or empty</exception>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowIfNullOrEmpty(string? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, nameof(paramName));
    }

    /// <summary>
    /// Asserts that the thing is null. Conditional on DEBUG build.
    /// </summary>
    /// <exception cref="ArgumentException">Value not null</exception>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowIfNotNull(object? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value != null)
        {
            throw new ArgumentException($"Value not null", paramName);
        }
    }

    /// <summary>
    /// Asserts that the thing is null. Conditional on DEBUG build.
    /// </summary>
    /// <exception cref="ArgumentException">Value not null</exception>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void ThrowIfNull(object? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
    }

    /// <summary>
    /// Throws. Conditional on DEBUG build.
    /// </summary>
    /// <exception cref="InvalidOperationException">Message</exception>
    [System.Diagnostics.Conditional("DEBUG")]
    [DoesNotReturn]
    public static void Fail(string msg)
    {
        throw new InvalidOperationException(msg);
    }

    /// <summary>
    /// Writes message to Console. Conditional on DEBUG build.
    /// </summary>
    /// <remarks>
    /// Unwelcome control codes are replaced if "safe" is true, otherwise it is written verbatim. The call is ignored
    /// unless the namespace has been enabled with a call to <see cref="EnableNamespace(string?)"/>.
    /// </remarks>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void WriteLine(object? msg, bool safe = true)
    {
        if (IsNamespaceEnabled(null))
        {
            WriteLine("", msg, safe);
        }
    }

    /// <summary>
    /// Overload with exception.
    /// </summary>
    public static void WriteLine(Exception e)
    {
        if (IsNamespaceEnabled(null))
        {
            WriteLine("", e.ToString(), false);
        }
    }

    /// <summary>
    /// Writes message to Console with leading namespace. Conditional on DEBUG build.
    /// </summary>
    /// <remarks>
    /// Unwelcome control codes are replaced if "safe" is true, otherwise it is written verbatim. The call is ignored
    /// unless the namespace has been enabled with a call to <see cref="EnableNamespace(string?)"/>.
    /// </remarks>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void WriteLine(string? nspace, object? msg, bool safe = true)
    {
        if (!IsNamespaceEnabled(nspace))
        {
            return;
        }

        if (!string.IsNullOrEmpty(nspace))
        {
            nspace += " : ";
        }

        if (safe)
        {
            msg = Sanitizer.ToDebugSafe(msg?.ToString());
        }

        Console.WriteLine(nspace + msg);
    }

    /// <summary>
    /// Overload with exception.
    /// </summary>
    public static void WriteLine(string? nspace, Exception e)
    {
        WriteLine(nspace, e.ToString());
    }

    /// <summary>
    /// Enables writing with the given "StartsWith" namespace.
    /// </summary>
    /// <remarks>
    /// Enabling "ClassName" will allow namespaces of "ClassName.MethodName". A "nspace" value of null will clear all
    /// previous entries and enable all further logging calls. An empty string will allow <see cref="WriteLine(object?,
    /// bool)"/> calls without a namespace. The default is no logging.
    /// </remarks>
    [System.Diagnostics.Conditional("DEBUG")]
    public static void EnableNamespace(string? nspace)
    {
        lock (_syncObj)
        {
            if (nspace == null)
            {
                // Enable all
                _enabled = null;
                return;
            }

            if (nspace.Length != 0 && !nspace.Contains('.'))
            {
                nspace += ".";
            }

            _enabled ??= new();
            _enabled.Add(nspace);
        }
    }

    private static bool IsNamespaceEnabled(string? nspace)
    {
        lock (_syncObj)
        {
            if (_enabled == null)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(nspace))
            {
                foreach (var item in _enabled)
                {
                    if (item.Length != 0 && nspace.StartsWith(item))
                    {
                        return true;
                    }
                }

                return false;
            }

            if (nspace != null)
            {
                return _enabled.Contains("");
            }

            return false;
        }
    }

}