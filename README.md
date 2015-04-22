This is a plugin for CamBam. CamBam is a "2.5D" CAM tool that takes 2D drawings, for example in dxf format, and allows you to specify operations such as pocketing and profiling and generate g-code to execute those operations on a machine tool such as a
vertical milling machine.

This plugin analyzes letter outlines, for example from TrueType fonts, and computes a 3D path to engrave those letters, using the depth of the cutter
to control the stroke width and cut features such as serifs.

# Download and installation #

The current version can be downloaded from [Google Cloud Storage](https://v-engrave-plugin.storage.googleapis.com/v-engrave-plugin-latest.zip). A listing of the available versions is at [Downloads](wiki/Downloads).

To install, unzip the download, close CamBam if you have it open, and copy the file VEngrave\_Plugin.dll to the CamBam plugins directory. Restart CamBam, and the V-Engrave operation will appear in the Machining menu.  The V-Engrave operation works with text, regions, and closed polylines.

# Notes #

See http://www.cambam.info/ for more info on CamBam. You will need a licensed copy of CamBam to use this plugin.

**DISCLAIMER**: The V-Engrave Plugin for CamBam is not a product of Google or HexRay, Ltd. CamBam is a product of HexRay, Ltd, who owns all rights to the CamBam software. This project is not affiliated with HexRay, Ltd.