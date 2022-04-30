# GelbooruImageTagger
Universal Windows Platform (UWP) app that paints metadata properties to images downloaded from Gelbooru.

Currently, this app paints metadata such as any artists, copyrights / intellectual properties, keywords / tags and image source.

This makes images easier to search for a particular tag or author.

## How to use

Typically, images downloaded from Gelbooru are in the form of an MD5 hash (e.g. 5f97439d98b85503c6419c2aa7521a01.jpg) or prefixed with "sample_" if it's a thumbnail.

Add the downloaded files to the list and click 'Tag' when ready.

You can also go into the settings to skip images that have already been tagged.

As this is a UWP app, you will need to install or modify additional tools to build in the Visual Studio installer.

Visual Studio is required (Visual Studio 2022 recommended) and under "Universal Windows Platform development" select the latest Windows SDK (Windows 11 SDK recommended).

This app isn't currently on the Microsoft Store but is under consideration.

## Notes

This project and repository is not affiliated with nor endorsed by Gelbooru in any way. I made the project as a hobby and for personal use.
