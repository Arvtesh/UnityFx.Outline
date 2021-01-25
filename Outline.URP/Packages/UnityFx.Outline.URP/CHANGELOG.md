# UnityFx.Outline.URP changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/); this project adheres to [Semantic Versioning](http://semver.org/).

## [0.3.0] - 2021.01.25

Misc improvements and bugfixes.

### Added
- Added possibility to set custom shader tags for URP outlines.
- Added support for filtering URP outline renderers by [rendering layer mask](https://docs.unity3d.com/ScriptReference/Renderer-renderingLayerMask.html) ([#22](https://github.com/Arvtesh/UnityFx.Outline/issues/22)).

### Fixed
- Fixed URP outlines rendering issue when both depth-testing and MSAA are enabled ([#23](https://github.com/Arvtesh/UnityFx.Outline/issues/23)).

## [0.2.0] - 2020.11.10

### Added
- Added URP-specific shader versions.
- Added URP layer-based outline rendering ([#9](https://github.com/Arvtesh/UnityFx.Outline/issues/9)).
- Added support for URP Single Pass Instanced XR rendering ([#13](https://github.com/Arvtesh/UnityFx.Outline/issues/13)).

### Fixed
- Fixed URP outlines rendering issue in Unity 2020.2 ([#21](https://github.com/Arvtesh/UnityFx.Outline/issues/21)).

## [0.1.0] - 2020.05.30

### Added
- Initial release.
