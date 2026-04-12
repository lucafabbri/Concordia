// Questo file è un polyfill per System.Runtime.CompilerServices.IsExternalInit.
// È necessario per consentire l'uso di tipi 'record' (C# 9.0) in progetti .NET Standard 2.0.

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Questo tipo è un polyfill fornito dal compilatore C# 9.0 per abilitare
    /// l'uso di 'init' accessors e tipi 'record' in framework che non lo includono nativamente.
    /// Non deve essere utilizzato direttamente nel codice.
    /// </summary>
    internal static class IsExternalInit { }
}