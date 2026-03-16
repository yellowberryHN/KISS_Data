# KISS Data

A collection of metadata harvested from the updates and DLC of [KISS](http://kisskiss.tv) software titles.

The current data set includes the following software:
- CM3D2
  - CM3D2 Chu-B Lip
  - CM3D2 VR β
  - CM3D2 Chu-B Lip VR β
  - CM3D2 Edit Trial
- COM3D2
  - COM3D2 Chu-B Lip
  - COM3D2.5
  - COM3D2.5 Chu-B Lip
  - COM3D2 EN (R18)
  - COM3D2.5 EN (R18)
  - COM3D2 INM
- KISSCharacter EditSystem/CR EditSystem
- CustomStudio α

The following software exists in the data set, but are currently invalid:
- CM3D
  - CM3D Ju-C AIR
  - CM3D Chu-B Lip

# NOTICE: This repo does not contain any game or DLC assets! This is not the place for such things.

## Download

 1. Check the [latest release](https://github.com/YellowberryHN/KISS_Data/releases/latest)
 2. Download `kiss.json` or `kiss.min.json`.

## Usage

The data is presented in JSON format.
If you are trying to preserve bandwidth or storage, use the .min version.
Same data, no whitespace.

A sample of what it looks like:
```json
{
  "version": "2026-03-03",
  "games": [
    {
      "game": "cm3d2",
      "files": [
        {
          "filepath": "CM3D2.exe",
          "updates": [
            {
              "size": 6044160,
              "hash": "70FC4DED",
              "version": 100,
              "source": "cm3d2"
            },
            {
              "size": 6248448,
              "hash": "E5768F9F",
              "version": 101,
              "source": "cm3d2_up101_x64"
            },
            {
              "size": 6257152,
              "hash": "9C5FAD73",
              "version": 1003,
              "source": "cm3d2_up104_x64"
            },
            ...
          ]
        },
        ...
      ]
    },
    ...
  ]
}
```

## MeidoDataParser

Included is a basic .NET 8.0 project which is used to generate the .json files.
**If you have to ask how to use this, it's not for you.**

## Contributing

### ***I'm still missing a lot of content. Please contribute what you can.***

When a new update is released or if you have files this repo is missing,
you may submit a pull request to add them, or send them to me on Discord at
**yellowberryhn**. I am also looking for DLC update metadata files,
so those are welcome as well.

The easiest way for you to contribute is to run the `7z x -o"lists" -r *.zip *.lst`
command from inside the folder you have your downloaded update and DLC .zip files,
and then zip up the created `lists` folder and send that to me.

### [List of the files that are confirmed missing](MISSING.md)

**Please ensure that the directory name is the same as the .zip file it came from,
your PR may be rejected otherwise. Some directory names might be wrong, as I've sourced
this data from many places, so some names are educated guesses.**

## License

MeidoDataParser is available under the [Unlicense](https://choosealicense.com/licenses/unlicense/).

The .json files in the releases are available under the [CC0 PDD](http://creativecommons.org/publicdomain/zero/1.0/)