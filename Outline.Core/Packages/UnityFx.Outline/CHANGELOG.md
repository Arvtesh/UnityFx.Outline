# UnityFx.Outline changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/); this project adheres to [Semantic Versioning](http://semver.org/).

## [0.8.3] - 2021.01.25

Misc improvements and bugfixes.

### Fixed
- Fixed `OutlineBehaviour` not working in edit mode after disabling and enabling it again.

### Changed
- `OutlineEffect` now works in edit-mode.
- `OutlineEffect` now exposes `OutlineLayerCollection` instead of `IList`.
- `OutlineEffect` now uses `OnPreRender` to update its command buffer.
- Moved `MergeLayerObjects` flag to `OutlineLayer` from `OutlineLayerCollection`.
- Multiple `OutlineEffect` component instances can now be added to a camera.

## [0.8.2] - 2020.11.10

Misc improvements.

### Added
- Added support for Single Pass Instanced XR rendering for built-in render pipeline ([#13](https://github.com/Arvtesh/UnityFx.Outline/issues/13)).

### Changed
- Misc inspector improvements.

## [0.8.1] - 2020.09.21

Alpha test support, bugfixes and misc improvements.

### Added
- Added support for alpha-testing ([#10](https://github.com/Arvtesh/UnityFx.Outline/issues/10)).
- Added support for merging outline layer objects ([#12](https://github.com/Arvtesh/UnityFx.Outline/issues/12)).
- Added `RemoveGameObject` helper methof to `OutlineEffect` ([#15](https://github.com/Arvtesh/UnityFx.Outline/issues/15)).
- Added ability to customize render event in `OutlineBehaviour`.
- Added ability to render outlines to the specified camera only for `OutlineBehaviour`.
- Added warning for unsupported render pipelines for `OutlineBehaviour` and `OutlineEffect`.

### Changed
- Misc inspector improvements.
- Changed default render event to `AfterSkybox`.

### Fixed
- Fixed incorrect condition for selection of render method, which sometimes caused problems with outline rendering on mobiles ([#14](https://github.com/Arvtesh/UnityFx.Outline/issues/14)).

## [0.8.0] - 2020.05.30

Major refactoring and bugfixes.

### Added
- Use procedural geometry ([DrawProcedural](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.DrawProcedural.html)) on SM3.5+.
- Added support for both forward and deferred renderers.
- Added ignore layer mask settings to `OutlineLayerCollection` (previously the ignore layers were specified when adding game objects to layers).
- Added `OutlineBuilder` helper script for managinf `OutlineLayerCollection` content from editor ([#8](https://github.com/Arvtesh/UnityFx.Outline/issues/8)).

### Changed
- Changed `OutlineSettings` to display enum mask instead of checkboxes.
- Changed inspector look and feel for `OutlineLayerCollection` assets.
- Merged shaders for the 2 outline passes into one multi-pass shader.
- `OutlineLayerCollection` doe not depend on `OutlineRenderer` now.

### Fixed
- Fixed outline rendering on mobiles ([#7](https://github.com/Arvtesh/UnityFx.Outline/issues/7)).
- Fixed outline shader error on low-end devices.

### Removed
- Dropped .NET 3.5 support, minimal Unity version is set to 2018.4.
- Removed `IOutlineSettingsEx` interface.

## [0.7.2] - 2020.04.08

Depth testing support and performance optimizations.

### Added
- Added support for depth testing when rendering outlines. When enabled, outlines are only rendered around the visible object parts ([#1](https://github.com/Arvtesh/UnityFx.Outline/issues/1)).
- Added a few convenience methods to `OutlineEffect`.
- Added editor tooltips for outline component fileds.

### Fixed
- Get rid of GC allocatinos during command buffer updates.
- Fixed `IndexOutOfRangeException` when setting outline width to max value ([#4](https://github.com/Arvtesh/UnityFx.Outline/issues/4)).

### Removed
- Removed change tracking support in package entities ([#2](https://github.com/Arvtesh/UnityFx.Outline/issues/2)).

## [0.7.1] - 2020.01.28

Bugfixes and project layout changes.

### Fixed
- Fixed `OutlineBehaviour` to allow changing its state while its `GameObject` is inactive.

## [0.7.0] - 2019.11.26

`MaterialPropertyBlock`-based rendering and [Unity Post-processing Stack v2](https://github.com/Unity-Technologies/PostProcessing/tree/v2) compatibility.

### Added
- Moved to for `MaterialPropertyBlock`-based rendering. This is in-line with Unity post-processing Stack and is more performant approach.
- Significant optimizations made to `OutlineRenderer`.

### Changed
- `IOutlineSettings` now implements `IEquatable`.
- Changed all outline shaders to use HLSL-based macros.
- Modified all shaders to ignore MVP vertex transform to be compatible with the new rendering model.
- Exposed rendering APIs for `OutlineLayer` and `OutlineLayerCollection`.

### Fixed
- Fixed `TiledGPUPerformanceWarning` on mobile targets.

### Removed
- Removed `OutlineMaterialSet` class. It is not used in `MaterialPropertyBlock`-based effect rendering.

## [0.6.0] - 2019.09.26

Quality of life improvements.

### Added
- Added `OutlineLayer.Enabled`.
- Added `OutlineLayer.Name`.
- Added possibility to change render order of layers via `OutlineLayer.Priority`.
- Added possibility to edit renderers of an `OutlineLayer`.
- Added possibility to alter `CameraEvent` used to render `OutlineEffect`.
- Added more info to the `OutlineLayer` preview inspector.

### Changed
- `IOutilneSettings` setters now throw if overriden.

### Fixed
- Fixed `OutlineLayer.Add` not filtering renderers by the mask passed.

## [0.5.0] - 2019.09.09

Editor UI improvements and unit tests.

### Added
- Added `OutlineSettings`, that can be shared between dfferent `OutlineLayer` and `OutlineBehaviour` instances.
- Added custom inspectors for `OutlineSettings`, `OutlineLayerCollection`.
- Added undo/redo support to all custom inspectors.
- Added unit-tests.

### Changed
- Improved inspectors for `OutlineBehaviour` and `OutlineEffect`.

## [0.4.0] - 2019.08.31

Blurred outlines.

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

