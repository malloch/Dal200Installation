# Dal 200 Installation: Documentation

There are currently three computers running the installation: two mac minis running projectors and a tablet running the management software. Websockets are used for communicating between machines.

When connected to the Installation wifi network you can use VNC to interact with the machines using the following names:

* Dal200Wall
* Dal200Floor
* lowerkinect

## Windows Tablet

The Windows tablet has several functions:

1. communicating with the Kinect tracker
2. running the top-down tracking software, which sends tracked positions via OSC to the Manager porgram
3. running the Manager program

## Mac Mini #1: Floor Display

This mini is connected to the floor projector; its role is to display dwell targets and a round 'cursor' that follows the user on the floor.

### Start-up instructions

1. start up machine
2. start Google Chrome
3. open URL [file:///Users/dal200floor/Desktop/Dal200Installation/Software/SVGRenderer/SVGRenderer.html](file:///Users/dal200floor/Desktop/Dal200Installation/Software/SVGRenderer/SVGRenderer.html)
4. make Chrome full-screen using the `view` menu or the keyboard shortcut `CMD`+`CTL`+`F`

## Mac Mini #1: Wall Display

### Start-up instructions

1. start up machine
2. start Safari
3. open URL [file:///Users/dal200floor/Desktop/Dal200Installation/Content/displayer.html](file:///Users/dal200floor/Desktop/Dal200Installation/Content/displayer.html)
4. open NewTekScanconverter
5. select 'Safari' from the File menu in NewTekScanconverter
6. open Syphon2NDIClient
7. open Isadora
8. in Isadora, open most recent file
9. open Isadora stage window using the shortcut `CMD`+`G`
