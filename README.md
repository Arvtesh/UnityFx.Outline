# UnityFx.Outline

Channel | UnityFx.Outline |
--------|-----------------|
Github | [![GitHub release](https://img.shields.io/github/release/Arvtesh/UnityFx.Outline.svg?logo=github)](https://github.com/Arvtesh/UnityFx.Outline/releases)
Npm (core + built-in RP) | [![Npm release](https://img.shields.io/npm/v/com.unityfx.outline.svg)](https://www.npmjs.com/package/com.unityfx.outline) ![npm](https://img.shields.io/npm/dt/com.unityfx.outline)
Npm (Post-processing v2) | [![Npm release](https://img.shields.io/npm/v/com.unityfx.outline.postprocessing.svg)](https://www.npmjs.com/package/com.unityfx.outline.postprocessing) ![npm](https://img.shields.io/npm/dt/com.unityfx.outline.postprocessing)
Npm (URP) | [![Npm release](https://img.shields.io/npm/v/com.unityfx.outline.urp.svg)](https://www.npmjs.com/package/com.unityfx.outline.urp) ![npm](https://img.shields.io/npm/dt/com.unityfx.outline.urp)
Npm (HDRP) | TODO

**Requires Unity 2018.4 or higher.**<br/>
**Compatible with [Unity Post-processing Stack v2](https://github.com/Unity-Technologies/PostProcessing/tree/v2).**<br/>
**Compatible with [Universal Render Pipeline](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@8.1/manual/index.html).**</br>
**Compatible with [XR](https://docs.unity3d.com/Manual/XR.html) (Multi Pass, Single Pass Instanced).**

**Please ask any questions and leave feedback at the [Unity forums](https://forum.unity.com/threads/screen-space-outline-effect-for-unity-free.836908/).**

## Synopsis
![Outline demo](Docs/OutlineSamples.png "Outline demo")
![Outline demo](Docs/MotusOutline.png "Outline demo")

*UnityFx.Outline* implements configurable per-object and per-camera outlines. Both solid and blurred outline modes are supported (Gauss blur). The outlines can be easily customized either through scripts or with Unity editor (both in edit-time or runtime).

Implementation is based on Unity [command buffers](https://docs.unity3d.com/ScriptReference/Rendering.CommandBuffer.html), compatible with [Unity Post-processing Stack v2](https://github.com/Unity-Technologies/PostProcessing/tree/v2) and [Universal Render Pipeline](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@8.1/manual/index.html), extendable and has no external dependencies.

Supported outline parameters are:
- Color;
- Width (in pixels);
- Type (solid or blurred);
- Intensity (for blurred outlines);
- Depth testing;
- Alpha testing.

Supported platforms:
- Windows/Mac standalone;
- Android;
- iOS;
- Other platforms (untested).

Please see [CHANGELOG](CHANGELOG.md) for information on recent changes.

## Getting Started
### Prerequisites
You may need the following software installed in order to build/use the library:
- [Unity3d 2018.4+](https://store.unity.com/).

### Getting the code
You can get the code by cloning the github repository using your preffered git client UI or you can do it from command line as follows:
```cmd
git clone https://github.com/Arvtesh/UnityFx.Outline.git
```

### Npm packages
[![NPM](https://nodei.co/npm/com.unityfx.outline.png)](https://www.npmjs.com/package/com.unityfx.outline)<br/>
[![NPM](https://nodei.co/npm/com.unityfx.outline.postprocessing.png)](https://www.npmjs.com/package/com.unityfx.outline.postprocessing)<br/>
[![NPM](https://nodei.co/npm/com.unityfx.outline.urp.png)](https://www.npmjs.com/package/com.unityfx.outline.urp)<br/>

Npm core package is available at [npmjs.com](https://www.npmjs.com/package/com.unityfx.outline). There are dedicated packages for [Post-processing Stack v2](https://github.com/Unity-Technologies/PostProcessing/tree/v2) and [Universal Render Pipeline](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@8.0/manual/index.html). To use the packages, add the following line to dependencies section of your `manifest.json`. Unity should download and link the package automatically:
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
    "com.unityfx.outline": "0.8.3",
    "com.unityfx.outline.urp": "0.3.0",
  }
}
```

## Usage
Install the package and import the namespace:
```csharp
using UnityFx.Outline;
```

### Per-camera outlines (built-in RP)
![Outline demo](Docs/OutlineEffectInspector.png "OutlineEffect Inspector")

Add `OutlineEffect` script to a camera that should render outlines. Then add and configure as many layers as you need. An outline layer is a group of game objects that share the same outline settings:
```csharp
var outlineEffect = Camera.main.GetComponent<OutlineEffect>();
var layer = new OutlineLayer("MyOutlines");

layer.OutlineColor = Color.red;
layer.OutlineWidth = 7;
layer.OutlineRenderMode = OutlineRenderFlags.Blurred;
layer.Add(myGo);

outlineEffect.OutlineLayers.Add(layer);
```
or
```csharp
var outlineEffect = Camera.main.GetComponent<OutlineEffect>();

// This adds layer 0 (if it is not there) and then adds myGo.
outlineEffect.AddGameObject(myGo);

// Now setup the layer.
var layer = outlineEffect[0];

layer.OutlineColor = Color.red;
layer.OutlineWidth = 7;
layer.OutlineRenderMode = OutlineRenderFlags.Blurred;
layer.Add(myGo);
```

This can be done at runtime or while editing a scene. If you choose to assign the script in runtime make sure `OutlineEffect.OutlineResources` is initialized. Disabling `OutlineEffect` script disables outlining for the camera (and frees all resources used).

Multiple `OutlineEffect` scripts can share outline layers rendered. To achieve that assign the same layer set to all `OutlineEffect` instances:

```csharp
var effect1 = camera1.GetComponent<OutlineEffect>();
var effect2 = camera2.GetComponent<OutlineEffect>();

// Make effect1 share its layers with effect2.
effect1.ShareLayersWith(effect2);
```

### Per-object outlines (built-in RP)
![Outline demo](Docs/OutlineBehaviourInspector.png "OutlineBehaviour Inspector")

Add `OutlineBehaviour` script to objects that should be outlined (in edit mode or in runtime). Make sure `OutlineBehaviour.OutlineResources` is initialized. You can customize outline settings either via Unity inspector or via script. Objects with `OutlineBehaviour` assigned render outlines in all cameras.

```csharp
var outlineBehaviour = GetComponent<OutlineBehaviour>();

// Make sure to set this is OutlineBehaviour was added at runtime.
outlineBehaviour.OutlineResources = myResources;

outlineBehaviour.OutlineColor = Color.green;
outlineBehaviour.OutlineWidth = 2;
outlineBehaviour.OutlineIntensity = 10;
```

### Depth testing
By default depth testing is disabled when rendering outlines. This behaviour can be overriden by setting `EnableDepthTesting` flag of `Rander Flags` (either via scripting API or with editor).
```csharp
var outlineSettings = GetComponent<OutlineBehaviour>();

outlineSettings.OutlineColor = Color.green;
outlineSettings.OutlineWidth = 2;
outlineSettings.OutlineRenderMode = OutlineRenderFlags.Blurred | OutlineRenderFlags.EnableDepthTesting;
```

### Alpha testing
By default alpha testing is disabled when rendering outlines. This behaviour can be overriden by setting `EnableAlphaTesting` flag of `Rander Flags` (either via scripting API or with editor).
```csharp
outlineSettings.OutlineRenderMode = OutlineRenderFlags.EnableAlphaTesting;
```

### Ignore layers
When adding a `GameObject` to outline collection it is often desirable to ignore child renderers in specific layers (for instance, `TransparentFX`). This can be achieved by settings the `IgnoreLayers` mask in outline settings (or through corresponding API).
```csharp
var outlineSettings = GetComponent<OutlineBehaviour>();
outlineSettings.IgnoreLayerMask = LayerMask.GetMask("TransparentFX", "UI");
```

### Extensibility
There are a number of helper classes that can be used for writing highly customized outline implementations (if neither `OutlineBehaviour` nor `OutlineEffect` does not suit your needs).
All outline implementations use following helpers:
- `OutlineRenderer` is basically a wrapper around `CommandBuffer` for low-level outline rendering.
- `OutlineSettings` is a set of outline settings.

Using these helpers is quite easy to create new outline tools. For instance, the following code renders a blue outline around object the script is attached to in `myCamera`:

```csharp
var commandBuffer = new CommandBuffer();
var renderers = GetComponentsInChildren<Renderer>();

// Any implementation of `IOutlineSettings` interface can be used here instead of `OutlineSettings`.
var settings = ScriptableObject.CreateInstance<OutlineSettings>();

settings.OutlineColor = Color.blue;
settings.OutlineWidth = 12;

// Get outline assets instance. In real app this usually comes from MonoBehaviour's serialized fields.
var resources = GetMyResources();

using (var renderer = new OutlineRenderer(commandBuffer, resources))
{
  renderer.Render(renderers, settings, myCamera.actualRenderingPath);
}

myCamera.AddCommandBuffer(OutlineRenderer.RenderEvent, commandBuffer);
```

## Integration with Unity post-processing v2.
[![NPM](https://nodei.co/npm/com.unityfx.outline.postprocessing.png)](https://www.npmjs.com/package/com.unityfx.outline.postprocessing)

Install the package, add `Outline` effect to `PostProcessProfile`'s overrides list. Configure the effect parameters, make sure outline resources and layer collection are set:

![Post processing outlinesettings](Docs/PpOutlineSettings.png "Post processing outlinesettings")

Assign the configured `PostProcessProfile` to `PostProcessVolume` and that's it!

More info on writing custom post processing effects can be found [here](https://docs.unity3d.com/Packages/com.unity.postprocessing@2.3/manual/Writing-Custom-Effects.html).

## Integration with Universal Render Pipeline (URP).
[![NPM](https://nodei.co/npm/com.unityfx.outline.urp.png)](https://www.npmjs.com/package/com.unityfx.outline.urp)

Install the package, add `OutlineFeature` to `ScriptableRendererData`'s list of features. Configure the feature parameters (make sure outline resources reference is set). Outline objects can be selected by layer or explixitly using `OutlineLayerCollection`:

![URP outline settings](Docs/UrpOutlineSettings.png "URP outline settings")

Enable depth texture rendering in `UniversalRenderPipelineAsset` and that's it!

### Integration with High Definition Render Pipeline (HDRP).
[![NPM](https://nodei.co/npm/com.unityfx.outline.hdrp.png)](https://www.npmjs.com/package/com.unityfx.outline.hdrp)

TODO

## Motivation
The project was initially created to help author with his [Unity3d](https://unity3d.com) projects. There are not many reusable open-source examples of it, so here it is. Hope it will be useful for someone.

## Documentation
Please see the links below for extended information on the product:
- [Unity forums](https://forum.unity.com/threads/screen-space-outline-effect-for-unity-free.836908/).
- [CHANGELOG](CHANGELOG.md).
- [SUPPORT](.github/SUPPORT.md).

## Useful links
- [A great outline tutorial](https://willweissman.wordpress.com/tutorials/shaders/unity-shaderlab-object-outlines/).
- [Command buffers tutorial](https://lindenreid.wordpress.com/2018/09/13/using-command-buffers-in-unity-selective-bloom/).
- [Gaussian blur tutorial](https://www.ronja-tutorials.com/2018/08/27/postprocessing-blur.html).
- [Excellent post-processing tutorial](https://catlikecoding.com/unity/tutorials/scriptable-render-pipeline/post-processing/).

## Contributing
Please see [contributing guide](.github/CONTRIBUTING.md) for details.

## Versioning
The project uses [SemVer](https://semver.org/) versioning pattern. For the versions available, see [tags in this repository](https://github.com/Arvtesh/UnityFx.Outline/tags).

## License
Please see the [![license](https://img.shields.io/github/license/Arvtesh/UnityFx.Outline.svg)](LICENSE.md) for details.
