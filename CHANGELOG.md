# Changelog

## [2.0.0] - 2026-08-21

### Changed

- [Breaking] `FontResolver` namespace renamed to `FontResolution`
- [Breaking] `FontStyle` renamed to `FontAttributes`
- [Breaking] `FontAttributes` uses `Weight`, `Style`, and `Width`
- [Breaking] `RetrieveAvailableFonts()` renamed to `ResolveAll()`
- [Breaking] `Resolve(...)` and `ResolveAll()` return font metadata
- [Breaking] `FontResolver` and `FontResolverPdfSharp` are the only publicly accessible classes
- `Resolve()` and `ResolveAll()` use the internal font cache
- `FontResolverPdfSharp` only depends on `FontResolver` and no other implementation detail

### Added

- [Linux] Using Fontconfig (`fc list`) if available
- Basic TTF/OTF parser to extract font metadata
- Available fonts and metadata are cached
- `ClearCache()` methods to manually reset the font cache
- Synchronization protection for the cache
- Font resolve strategy to allow for broader font matching
- String comparison parameter to steer the font matching
- Basic CLI to test the library output on different systems

### Removed

- [Breaking] `DiscoverFontFamilies()` removed in favor of `ResolveAll()`

## [1.1.0] - 2025-12-31

### Added

- Basic font families discovery

## [1.0.0] - 2025-12-30

### Added

- Resolve font file path by font name
- Basic directory search
- Register custom font directory
- [Windows] Query the registry for fonts