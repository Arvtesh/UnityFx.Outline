# UnityFx.Outline

Channel  | UnityFx.Outline |
---------|---------------|
Github | [![GitHub release](https://img.shields.io/github/release/Arvtesh/UnityFx.Outline.svg?logo=github)](https://github.com/Arvtesh/UnityFx.Outline/releases)
Npm | [![Npm release](https://img.shields.io/npm/v/com.unityfx.outline.svg)](https://www.npmjs.com/package/com.unityfx.outline) ![npm](https://img.shields.io/npm/dt/com.unityfx.outline)

**Requires Unity 2017 or higher.**

## Synopsis
![Outline demo](OutlineDemo.png "Outline demo")

*UnityFx.Outline* defines tools that can be used to render configurable object outlines for specific cameras. The outlines configuration can be easily customized either through scripts or with Unity editor.

Implementation is based on Unity [command buffers](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.html), does not require putting objects into layers and has no dependencies.

Please see [CHANGELOG](CHANGELOG.md) for information on recent changes.

## Getting Started
### Prerequisites
You may need the following software installed in order to build/use the library:
- [Unity3d 2017+](https://store.unity.com/).

### Getting the code
You can get the code by cloning the github repository using your preffered git client UI or you can do it from command line as follows:
```cmd
git clone https://github.com/Arvtesh/UnityFx.Outline.git
```

### Npm package
[![NPM](https://nodei.co/npm/com.unityfx.outline.png)](https://www.npmjs.com/package/com.unityfx.outline)

Npm package is available at [npmjs.com](https://www.npmjs.com/package/com.unityfx.outline). To use it, add the following line to dependencies section of your `manifest.json`. Unity should download and link the package automatically:
```json
{
  "scopedRegistries": [
    {
      "name": "Arvtesh",
      "url": "https://registry.npmjs.org/",
      "scopes": [
        "com.unityfx"
      ]
    }
  ],
  "dependencies": {
    "com.unityfx.outline": "0.1.0"
  }
}
```

## Usage
Install the package and import the namespace:
```csharp
using UnityFx.Outline;
```

Add `OutlineEffect` script to a camera that should render outlines. Then add and configure as many layers as you need:
```csharp
var outlineEffect = Camera.main.GetComponent<OutlineEffect>();
var layer = outlineEffect.AddLayer();

layer.OutlineColor = Color.red;
layer.OutlineWidth = 7;
layer.Add(myGo);
```

This can be done at runtime or while editing a scene. Disabling `OutlineEffect` script disables outlining for the camera (and frees all resources used).

## Motivation
The project was initially created to help author with his [Unity3d](https://unity3d.com) projects. There are not many reusable open-source examples of it, so here it is. Hope it will be useful for someone.

## Documentation
Please see the links below for extended information on the product:
- [Unity forums](https://forum.unity.com/threads/TODO/).
- [CHANGELOG](CHANGELOG.md).
- [SUPPORT](.github/SUPPORT.md).

## Useful links
- [A great outline tutorial](https://willweissman.wordpress.com/tutorials/shaders/unity-shaderlab-object-outlines/).
- [Command buffers tutorial](https://lindenreid.wordpress.com/2018/09/13/using-command-buffers-in-unity-selective-bloom/).
- [Gaussian blur tutorial](https://www.ronja-tutorials.com/2018/08/27/postprocessing-blur.html).
- [Gaussian blur 2](http://rastergrid.com/blog/2010/09/efficient-gaussian-blur-with-linear-sampling/).

## Contributing
Please see [contributing guide](.github/CONTRIBUTING.md) for details.

## Versioning
The project uses [SemVer](https://semver.org/) versioning pattern. For the versions available, see [tags in this repository](https://github.com/Arvtesh/UnityFx.Outline/tags).

## License
Please see the [![license](https://img.shields.io/github/license/Arvtesh/UnityFx.Outline.svg)](LICENSE.md) for details.
