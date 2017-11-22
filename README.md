MediaBrowser.Plugins.AniMetadata
==========================

Originally a fork of [Emby.Plugins.Anime](https://github.com/MediaBrowser/Emby.Plugins.Anime), AniMetadata turned into an alternative plugin for loading metadata for anime libraries that are organised using [AniDb](http://anidb.net) style naming, and uses the AniDb structure in the Emby library.


## Supported Naming Styles ##

> **You MUST disable series merging for any library containing anime to get correct results**

AniMetadata follows all the naming schemes supported by Emby (more information [here](https://github.com/MediaBrowser/Wiki/wiki/TV%20naming)) but assumes that your files are organised using AniDb names, specifically:

- Series folders that match an AniDb title
- One season per series or no season specified
	- Except for specials which must be labelled season zero
- AniDb episode numbers or episode names matching an AniDb episode


For example, libraries arranged like the below will work:
    
    Full Metal Panic\S01E01 - The Guy I Kinda Like is a Sergeant.mp4
    Full Metal Panic\01 - The Guy I Kinda Like is a Sergeant.mp4
    Full Metal Panic\S01E01.mp4
    Full Metal Panic\Season 1\Ep01 - The Guy I Kinda Like is a Sergeant.mp4    

## How Shows Appear in the Emby Library ##
Normally Emby providers expect a single show to be broken down into seasons across a single series. AniDb breaks multiple seasons into series with different names. 

For example Full Metal Panic has 3 seasons on [TvDb](https://www.thetvdb.com/?tab=series&id=78914&lid=7) (plus a 4th yet to air that we can ignore). AniDb splits the 3 seasons into [three](http://anidb.net/perl-bin/animedb.pl?show=anime&aid=17) [separate](http://anidb.net/perl-bin/animedb.pl?show=anime&aid=959) [series](http://anidb.net/perl-bin/animedb.pl?show=anime&aid=2710) each with 1 season.

So a file structure:

    Full Metal Panic\S01E01 - The Guy I Kinda Like is a Sergeant.mp4
	Full Metal Panic Fumoffu\01 - The Man from the South A Fruitless Lunchtime.mp4
	Full Metal Panic Fumoffu\S00E01 - The Man from the South A Hostage with No Compromises.mp4
	Full Metal Panic - The Second Raid\S01E01 - The End of Day by Day.mp4

Will be correctly recognised and added to the Emby library:

![](https://raw.githubusercontent.com/Randomage/MediaBrowser.Plugins.AniDbForTvDb/master/EmbyLibrary.png)

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

### Series / Season / Episode Fields ###

Each of these sections provides a list of fields along with the data sources they can be populated from. Data is taken in priority order from top to bottom until a source with data is found.

So if you want episode overviews to come from TvDb rather than AniDb, move TvDb to the top under the Overview field in the Episode fields section.


## First Run ##

The best approach to the first run is to clear any series from the library and start from fresh, otherwise any existing incorrect matches may be kept.

For example if your file structure contains:

    Full Metal Panic\S01E01 - The Guy I Kinda Like is a Sergeant.mp4
	Full Metal Panic Fumoffu\S01E01 - The Man from the South A Fruitless Lunchtime.mp4

And this has been incorrectly added to your Emby library as Full Metal Panic S01E01 twice, this will not be fixed as Emby will report the series name 'Full Metal Panic' to AniMetadata for both episodes even though this doesn't match the file structure.

Removing the series and rescanning uses the folder names which should give the correct result.

## ToDo ##

### Handling false positives ###
The config page needs to have a list of series names that should be ignored as non-anime.

### Episodes spanning multiple indexes ###
Sometimes one file contains multiple episodes, or one episode in AniDb corresponds to multiple episodes in TvDb (or vice versa), this currently isn't handled anywhere. 

## Compiling ##

There are a number of dependencies that are IL merged to produce a single output assembly, but this shouldn't need any additional setup.

After a successful build the assembly is copied to %AppData%\Emby-Server\plugins\MediaBrowser.Plugins.AniMetadata.dll