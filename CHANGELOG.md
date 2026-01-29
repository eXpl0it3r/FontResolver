# Changelog

## [Unreleased]

### Changed

- [Breaking] `FontResolver` namespace renamed to `FontResolution`
- [Breaking] `FontStyle` renamed to `FontAttributes`
- [Breaking] `FontAttributes` uses `Weight`, `Style`, and `Width`
- [Breaking] `DiscoverFontFamilies()` renamed to `RetrieveAvailableFonts()`
- [Breaking] `Resolve(...)` returns font metadata
- [Breaking] `RetrieveAvailableFonts()` returns font metadata
- `Resolve()` uses the internal font cache

### Added

- [Linux] Using Fontconfig (`fc list`) if available
- Basic TTF/OTF parser to extract font metadata
- Available fonts and metadata are cached
- Font resolve strategy to allow for broader font matching

## [1.1.0] - 2025-12-31

### Added

- Basic font families discovery

## [1.0.0] - 2025-12-30

### Added

- Resolve font file path by font name
- Basic directory search
- Register custom font directory
- [Windows] Query the registry for fonts