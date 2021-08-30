# KISS Data

A collection of data harvested from the updates of [KISS](http://kisskiss.tv) software titles.

The current data set includes the following software:
- CM3D2
  - CM3D2 Chu-B Lip
  - CM3D2 VR β
  - CM3D2 Chu-B Lip VR β
- COM3D2
  - COM3D2 Chu-B Lip
  - COM3D2.5
  - COM3D2 EN (R18)
  - COM3D2 INM
- CR EditSystem

## Download

 1. Check the [latest release](https://github.com/YellowberryHN/KISS_Data/releases/latest)
 2. Download `kiss.json` or `kiss.min.json`.

## Usage

The data is presented in JSON format.
If you are trying to preserve bandwidth, use the .min version.
Same data, no whitespace.

A sample of what it looks like:
```json
[
  {
    "name": "cm3d2_up101_x64",
    "company": "KISS",
    "appName": "カスタムメイド3D2",
    "appExe": "CM3D2.exe",
    "category": "アップデータ",
    "registry": "カスタムメイド3D2",
    "files": [
      {
        "fileName": "CM3D2x64.exe",
        "size": 14913024,
        "hash": "267C4E4A",
        "version": 101
      },
      {
        "fileName": "CM3D2.exe",
        "size": 6248448,
        "hash": "E5768F9F",
        "version": 101
      },
      {
        "fileName": "CM3D2x64_Data\\level0",
        "size": 147760,
        "hash": "DC9A914F",
        "version": 101
      },
      ...
    ]
  },
  {
    "name": "cres_up1_10_0",
    "company": "KISS",
    "appName": "CR EditSystem",
    "appExe": "CR Launcher.exe",
    "category": "アップデータ",
    "registry": "CR EditSystem",
    "files": [
      {
        "fileName": "CR EditSystem.exe",
        "size": 653824,
        "hash": "4922FB81",
        "version": 11000
      },
      {
        "fileName": "CR Launcher.exe",
        "size": 6199296,
        "hash": "3043369E",
        "version": 110000
      },
      {
        "fileName": "UnityCrashHandler64.exe",
        "size": 1249672,
        "hash": "CAC3AE6D",
        "version": 11000
      },
      {
        "fileName": "UnityPlayer.dll",
        "size": 28066696,
        "hash": "7D5D0F91",
        "version": 11000
      },
      ...
    ]
  }
]
```

## MeidoDataParser

Included is a basic .NET 5.0 project which is used to generate the .json files.
**If you have to ask how to use this, it's not for you.**

## Contributing

### ***I'm still missing a lot of content. Please contribute what you can.***

When a new update is released or if you have files this repo is missing,
you may submit a pull request to add them, or send them to me on Discord at
**Yellow#7755**. I am also looking for DLC update metadata files,
so those are welcome as well.

**Please ensure that the directory name is the same as the .zip file it came from,
your PR may be rejected otherwise.**

## License

MeidoDataParser is available under the [Unlicense](https://choosealicense.com/licenses/unlicense/).

The .json files in the releases are available under the [CC0 PDD](http://creativecommons.org/publicdomain/zero/1.0/)