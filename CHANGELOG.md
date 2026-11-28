# Changelog

## [Unreleased]

### Changed

- `System.Security.Cryptography.Pkcs` updated from 8.0.1 to 10.0.15, PDFsharp 6.2.4 depends on the vulnerable 8.0.1
- PDFsharp stays on 6.2.4, the last MIT release, as PDFsharp switched to AGPL-3.0 with 6.5.0
- `ClearCache()` and `RegisterCustomFontDirectory()` wait for a running font scan to finish

### Fixed

- `FontResolverPdfSharp` can be used from several threads at once
- `Resolve()` could fall back to no match while the cache was being cleared
- A font directory registered during a font scan could be missing from the cache
- The Windows English names are preferred over localized ones and over the Mac ones, which are only read as ASCII
- Names in the Windows symbol encoding and on the Unicode platform are read, fonts with only those names were skipped
- Fonts without an OS/2 table read SemiCondensed, ExtraLight and spaced names like "Extra Bold" from the subfamily correctly
- Font files are read completely when the file system returns them in parts
- `Closest` picks the regular face of a family instead of an extra weight like Light, which matched the family name better
- An empty or blank font name resolves to no font instead of an arbitrary one
- Font directories are scanned for upper case extensions like `.TTF` on Linux and macOS, past subfolders that can't be read and without following linked folders
- A system without any fonts is scanned once instead of on every call

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