Emby.AniDbMetaStructure
==========================

Originally a fork of [MediaBrowser.Plugins.AniMetadata](https://github.com/Randomage/MediaBrowser.Plugins.AniDbForTvDb), AniDbMetaStructure turned into an alternative plugin for loading metadata for anime libraries that are organised using [AniDb](http://anidb.net) style naming, and uses the AniDb structure in the Emby library.


## Supported Naming Styles ##

TO BE FILED   

## How Shows Appear in the Emby Library ##

TO BE FILLED

## Configuration ##

There are a number of configuration options:

### Title Language ###
This controls what names are used in the Emby library, it does not have to match the names of the files themselves.

The title is always taken from AniDb, using other titles breaks metadata refreshes as Emby provides the existing title rather than the file name the second time around which can cause finding a match in AniDb to fail.

| Option    | Description                                               | Example                                           |
|-----------|-----------------------------------------------------------|---------------------------------------------------|
| Localized | Use the title in the language that Emby is configured for | Konosuba: God`s Blessing on This Wonderful World! |
| Romaji    | Use romaji titles                                         | Kono Subarashii Sekai ni Shukufuku o!             |
| Japanese  | Use japanese titles                                       | この素晴らしい世界に祝福を!                             |

### Move Excess Genres to Tags & Max Genres ###

If Move Excess Genres to Tags is ticked, and Max Genres is set to a number greater than zero, then any genres over that number will be added as tags instead.

Genres are taken from AniDb, with the highest weighting taken first.

## First Run ##

The best approach to the first run is to clear any series from the library and start from fresh, otherwise any existing incorrect matches may be kept.

For example if your file structure contains:

    Full Metal Panic\S01E01 - The Guy I Kinda Like is a Sergeant.mp4
	Full Metal Panic Fumoffu\S01E01 - The Man from the South A Fruitless Lunchtime.mp4

And this has been incorrectly added to your Emby library as Full Metal Panic S01E01 twice, this will not be fixed as Emby will report the series name 'Full Metal Panic' to AniMetadata for both episodes even though this doesn't match the file structure.

Removing the series and rescanning uses the folder names which should give the correct result.

## Compiling ##

There are a number of dependencies that are IL merged to produce a single output assembly, but this shouldn't need any additional setup.

After a successful build the assembly is copied to %AppData%\Emby-Server\plugins\MediaBrowser.Plugins.AniMetadata.dll
