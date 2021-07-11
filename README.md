# Unity3D-MonocularDepthEstimation
![gif](https://i.imgur.com/T04Wrng.gif)
Unity3D Monocular depth estimation using Barracuda
based on: https://medium.com/xrpractices/monocular-depth-sensing-point-cloud-from-webcam-feed-using-unity-barracuda-d9f1496b5932

### DephSensor.cs
This script computes depth estimation on RenderTexture source image.
Please note the model return a depth image at 224×244 RFloat.
You can grab the RenderTexture containing the Depth value from script by calling ```DephSensor.GetRawOutput()```

### Install Package
This package uses the scoped registry feature to import dependent packages.
Please add the following sections to the package manifest file (Packages/manifest.json).

To the scopedRegistries section:
```
{
    "name": "Bonjour-lab",
    "url": "https://registry.npmjs.com",
    "scopes": [
    "com.bonjour-lab"
    ]
}
```

To the dependencies section:

```
"com.bonjour-lab.monoculardepth": "1.0.0-preview",
```

After changes, the manifest file should look like below:
```
{
  "scopedRegistries": [
    {
      "name": "Bonjour-lab",
      "url": "https://registry.npmjs.com",
      "scopes": [
        "com.bonjour-lab"
      ]
    }
  ],
  "dependencies": {
    "com.bonjour-lab.monoculardepth": "1.0.0-preview",
    ...
```


Tested on
-------
* Unity 2020 & 2019

