# ONNX Runtime Plugin for Unity

[![upm](https://img.shields.io/npm/v/com.github.asus4.onnxruntime?label=upm)](https://www.npmjs.com/package/com.github.asus4.onnxruntime)

Pre-built ONNX Runtime libraries for Unity.

## [See Examples](https://github.com/asus4/onnxruntime-unity-examples)

[https://github.com/asus4/onnxruntime-unity-examples](https://github.com/asus4/onnxruntime-unity-examples)

Yolox

<https://github.com/asus4/onnxruntime-unity-examples/assets/357497/96ed9913-41b7-401d-a634-f0e2de4fc3c7>

NanoSAM  

<https://github.com/asus4/onnxruntime-unity-examples/assets/357497/5e2b8712-87cc-4a3a-82b7-f217087a0ed1>

## Tested environment

- Unity: 2022.3.19f1 (LTS)
- ONNX Runtime: [1.17.1](https://github.com/microsoft/onnxruntime/releases/tag/v1.17.1)
- ONNX Runtime Extensions: [0.10.0](https://github.com/microsoft/onnxruntime-extensions/releases/tag/v0.10.0)

### Execution Providers & Extensions

#### [Execution Providers](https://onnxruntime.ai/docs/execution-providers/)

Execution Providers are hardware acceleration libraries for each platform. See [official docs](https://onnxruntime.ai/docs/execution-providers/) for more details.

| Platform | CPU | CoreML | NNAPI | CUDA | TensorRT | DirectML | XNNPACK |
| --- | --- | --- | --- | --- | --- | --- | --- |
| macOS | :white_check_mark: | :white_check_mark: | | | | | |
| iOS | :white_check_mark: | :white_check_mark: | | | | | :construction: |
| Android | :white_check_mark: | | :white_check_mark: | | | | :construction: |
| Windows | :white_check_mark: | | | :construction: | :construction: | :white_check_mark: | |
| Linux | :white_check_mark: | | | :construction: | :construction: | | |

#### [ONNX Runtime Extensions](https://github.com/microsoft/onnxruntime-extensions)

ONNX Runtime Extensions are a set of pre/post-processing.

| Platform | Extensions |
| --- | --- |
| macOS | :construction: |
| iOS | :construction: |
| Android | :construction: |
| Windows | :construction: |
| Linux | :construction: |

:white_check_mark: : Supported in Unity Core library  
:construction: : Experimental Preview

## How to Install

Pre-built libraries are available on [NPM](https://www.npmjs.com/package/com.github.asus4.onnxruntime). Add the following `scopedRegistries` and `dependencies` in `Packages/manifest.json`.

```json
  "scopedRegistries": [
    {
      "name": "NPM",
      "url": "https://registry.npmjs.com",
      "scopes": [
        "com.github.asus4"
      ]
    }
  ]
  "dependencies": {
    "com.github.asus4.onnxruntime": "0.1.14",
    "com.github.asus4.onnxruntime.unity": "0.1.14",
    "com.github.asus4.onnxruntime.win-x64-gpu": "0.1.14",
    "com.github.asus4.onnxruntime.linux-x64-gpu": "0.1.14",
    "com.github.asus4.onnxruntime-extensions": "0.1.14",
    ... other dependencies
  }
```

### What is included in each package

- `com.github.asus4.onnxruntime` : Core library
  - CPU provider for all platforms
  - GPU provider for iOS, Android, macOS and Windows(only DirectML)
- `com.github.asus4.onnxruntime.unity` : (Optional) Utilities for Unity
- `com.github.asus4.onnxruntime.win-x64-gpu` : (Optional) GPU provider for Windows
- `com.github.asus4.onnxruntime.linux-x64-gpu` : (Optional) GPU provider for Linux
- `com.github.asus4.onnxruntime-extensions` : (Optional) ONNX Runtime Extensions
