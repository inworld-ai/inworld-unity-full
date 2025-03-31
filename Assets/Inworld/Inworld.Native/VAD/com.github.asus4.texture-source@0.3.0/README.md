# Texture Source

[![upm](https://img.shields.io/npm/v/com.github.asus4.texture-source?label=upm)](https://www.npmjs.com/package/com.github.asus4.texture-source)

TextureSource is a utility that provides a consistent API to get the texture from various sources.

![virtual-texture](https://github.com/asus4/TextureSource/assets/357497/e52f80d2-b1be-4cfa-81f7-76cdafe271bc)

## Example API Usage

```c#
using TextureSource;
using UnityEngine;

[RequireComponent(typeof(VirtualTextureSource))]
public class TextureSourceSample: MonoBehaviour
{
    private void Start()
    {
        // Listen to OnTexture event from VirtualTextureSource
        // Also able to bind in the inspector
        if (TryGetComponent(out VirtualTextureSource source))
        {
            source.OnTexture.AddListener(OnTexture);
        }
    }

    private void OnDestroy()
    {
        if (TryGetComponent(out VirtualTextureSource source))
        {
            source.OnTexture.RemoveListener(OnTexture);
        }
    }

    public void OnTexture(Texture texture)
    {
        // Do whatever 🥳
        // You don't need to think about webcam texture rotation.
    }   
}
```

## Install via UPM

Add the following setting to `Packages/manifest.json`

```json
{
  "scopedRegistries": [
    {
      "name": "npm",
      "url": "https://registry.npmjs.com",
      "scopes": [
        "com.github.asus4"
      ]
    }
  ],
  "dependencies": {
    "com.github.asus4.texture-source": "0.3.0",
    ...// other dependencies
  }
}
```

## How To Use

After installing the library, attach `VirtualTextureSource` to the GameObject.

![virtual-texture](https://github.com/asus4/TextureSource/assets/357497/e52f80d2-b1be-4cfa-81f7-76cdafe271bc)

Then, right-click on the project panel and create the TextureSource scriptable object that you want to use. You can set different sources for the Editor and Runtime.

![scriptable-object](https://github.com/asus4/TextureSource/assets/357497/6c4862e2-5298-4f4e-8cd5-076d54d46db8)

Currently provides the following sources:

### WebCam Texture Source

Includes collecting device rotation.

![webcam-texture-source](https://github.com/asus4/TextureSource/assets/357497/407f7372-b214-4ba9-9093-2b39755b905b)

### Video Texture Source

Useful when using test videos only in the Editor.

![video-texture-source](https://github.com/asus4/TextureSource/assets/357497/8e38ed1a-d2d8-4e16-9fc4-e5d4c6d0a888)

### Image Texture Source

Test with static images.

`OnTexture` event is invoked every frame if the `sendContinuousUpdate` is enabled.

![image-texture-source](https://github.com/asus4/TextureSource/assets/357497/3d7eef4b-40c5-40b4-8403-b70f394ce938)

### AR Foundation Texture Source

Provides AR camera texture access. It supports both ARCore/ARKit.

![ar-foundation-texture-source](https://github.com/asus4/TextureSource/assets/357497/5ac82a8a-0554-41a2-b9ef-c03ebd60c6ff)

## Acknowledgement

Inspired from [TestTools](https://github.com/keijiro/TestTools)

## License

[MIT](https://github.com/asus4/TextureSource/blob/main/Packages/com.github.asus4.texture-source/LICENSE)
