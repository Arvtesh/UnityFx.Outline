# UnityFx.Outline changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/); this project adheres to [Semantic Versioning](http://semver.org/).

## [0.5.0] - unreleased

### Added
- Added `OutlineSettings`, that can be shared between dfferent `OutlineLayer` and `OutlineBehaviour` instances.
- Added custom inspectors for `OutlineSettings`, `OutlineLayerCollection`.
- Added undo/redo support to all custom inspectors.
- Added unit-tests.

### Changed
- Improved inspectors for `OutlineBehaviour` and `OutlineEffect`.

## [0.4.0] - 2019.08.31

### Added
- Added Gauss blurring to outlines.
- Added outline mode parameter (possible values are `Solid` and `Blurred`).
- Added outline intensity parameter (for blurred outlines only).
- Added `IOutlineSettings` interface to make outline settings the same for `OutlineBehaviour` and `OutlineLayer`.
- Added `OutlineMaterialSet` helper.

### Changed
- Changed solid outline to use Gauss sampling (to achieve smoother outlines).
- Changed outline implementation to use different passed for horizontal and vertical sampling (to make algorithm complexity linear instead of quadric).

### Fixed
- Fixed an issue with `OutlineBehaviour` not rendering outlines if attached to a `GameObject` with no renderers.

### Removed
- Removed `OutlineResourceCache` class.

## [0.3.0] - 2019.08.27

### Added
- Added support for sharing outline layers between `OutlineEffect` instances.
- Added custom editors for `OutlineEffect` and `OutlineBehaviour`.
- Added possibility to setup outline layers as `ScriptableObject` asset.

### Fixed
- Fixed profiler error 'BeginSample and EndSample count must match'.

## [0.2.0] - 2019.08.19

### Added
- Added `OutlineBehaviour` for rendering per-object outlines.
- Added `OutlineResources` to help initialize outline effects in runtime.
- Added `OutlineRenderer` as low-level helper for outline rendering.

## [0.1.0] - 2019.08.18

### Added
- Initial release.

