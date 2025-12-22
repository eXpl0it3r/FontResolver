#if NETSTANDARD2_0
namespace System.Runtime.CompilerServices
{
    // Polyfill to enable C# 9 records on .NET Standard 2.0
    internal static class IsExternalInit { }
}
#endif
