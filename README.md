# KunstuniLinz-DeepSpace-Unity
A base/template Unity project as starting point for creating interactive content for the Kunstuni Linz Deep Space, including TUIO protocol cursor tracking.

# Quick Start
- Make a copy of the scene "Starter - Deep Space - Wall, Floor and UI (dual displays)" and start from there.
- Typically, the DeepSpace is configured as two separate displays, so each Camera in the scene is set to render to a different display. To preview both the wall and the floor, you may want to have two Game tabs, one set to "Display 1" and the other to "Display 2" and stack them vertically. Each Game tab should be set to an aspect of 16:9, or a fixed resolution of 4K 3840x2160 (DeepSpace single display); or 1920x1080 (half of that) on less powerful computers. 
- In Project Settings > Player > Resolution and Presentation, make sure "Run in Background" is checked, so that the Editor still runs while you are working on another window, e.g. a TUIO simulator.
- The on-screen Log UI can be toggled with the "L" key.
- De-activate or delete the Game Objects you won't use. However, even if you only want to work with wall visuals, keeping "Floor UI" and "Floor Camera" can be useful for visual feedback while creating and testing.
- What your cursors look like and what they do depends on the prefab assigned in each *Deep Space Cursor Manager* in the scene. The cursors move in X and Y **in relation to their parent Transform**, as set in each *Deep Space Cursor Manager*.  
- When running in the Editor, a *Deep Space Settings Loader* component loads settings from an asset assigned to it. 
- When you build and run the application, the *Deep Space Settings Loader* will instead look for a settings file in "[Application name]_Data\Settings\DeepSpaceSettings.txt" relative to the executable file. An example settings file is provided for you to use.

# About This Project
This Unity project was created by Tiago Martins, with reference to an example kindly provided by Axel Bräuer, and a few test runs in the actual space with feedback from Holunder Heiß. OSC code is heavily based on [UnityOSC by Thomas Fredericks](https://thomasfredericks.github.io/UnityOSC/).

The project was created in Unity 2021.3 starting from the base 3D template, and then adding the URP. It should be possible to open the project with later versions of the Editor; and it should be straightforward enough to recreate the project for earlier versions of the editor, or to use the built-in renderer or HDRP. The code should be version-independent in principle. Assets include materials and textures from the free [Gridbox Prototype Materials package by Cyathiza](https://assetstore.unity.com/packages/2d/textures-materials/gridbox-prototype-materials-129127).

# Setting up for Deep Space - Dual Displays
Typically, the DeepSpace is configured as two separate displays. Assuming you wish to render to both the wall and floor, you will have two cameras in your scene, each set to render to a different display, "Display 1" and "Display 2" respectively. Typically, the Wall Camera comes first and is tagged as "MainCamera", and the Floor Camera next. You can assign a custom tag "FloorCamera" to it.

To preview your scene, you may want to add a second "Game" tab in the Editor, set it to "Display 2", and stack both Game tabs vertically. Each Game tab should be set to a fixed aspect of 16:9. You can also use a fixed resolution of 3840x2160 (DeepSpace single display); or 1920x1080 (half of that) to save up on rendering power.

When running the app, Unity will not, by default, use additional displays beyond the main diplay (as indicated by the operating system). A script is provided and used in the example scene to do just that. You can also refer to the [Unity documentation on multiple displays](https://docs.unity3d.com/Manual/MultiDisplay.html).

# Setting up for Deep Space - Single Display
The DeepSpace at Kunstuni can also run as a single display with a resolution of 3840x4320, as wall and floor stacked vertically (wall on top, floor on bottom). If you plan on running your project in the Ars Electronica Center's DeepSpace however, **this is not recommended**. An example scene for this configuration is provided.

You should create one or more resolution modes for Deep Space in the Game tab. I suggest creating a "Deep Space Full-res" setting with a "fixed resolution" of 3840x4320; and a "Deep Space Scaled" setting with an "aspect ratio" of 16:18. Having a scaled resolution can be better for testing on less powerful computers, since Unity won't be rendering at a full 3840x4320. 

Both cameras should be set to render on "Display 1", but each will render to a different area of the display (viewport). The "Viewport Rect" (X/Y/W/H) for Wall and Floor cameras should be respectively (0.0, 0.5, 1.0, 0.5) and (0.0, 0.0, 1.0, 0.5).

The Floor UI Canvas can be set to "Scale With Screen Size" with a reference resolution of 3840x4320. To host the UI cursors, you should have a UI Panel taking up the lower half of the canvas (e.g. anchored to the bottom, stretched horizontally, and height set to 2160).

# Configuration File
If you wish to load a configuration file when a scene starts, add a *Deep Space Settings Loader* to the top of the Hierarchy. This is already the case in the Starter scenes. Make sure you assign all the component fields properly, check the Starter scenes for reference. 

At runtime, the settings are held by a *Deep Space Settings* asset (scriptable object). You can create one in the the Project tab with Create > Deep Space > Deep Space Settings. While working in the Editor, the *Deep Space Settings Loader* will not load settings from a file, but rather use whatever is in the *Deep Space Settings* asset that you assigned to it. When running a built application, it will look for the file "[Application name]_Data\Settings\DeepSpaceSettings.txt" relative to the location of the executable.

Basic settings for a built app running on the DeepSpace can be kept in a JSON-formatted configuration file named "DeepSpaceSettings.txt". A sample is provided in the project, and also below. Fields and values should be mostly self-explanatory.

    {
      "showDebugUi": 1,
      "wallCameraDisplayIndex": 0,
      "floorCameraDisplayIndex": 1,
      "tuioPort": 3333
    }

# Setting up the Cursor Components

If you plan on using the tracking system, the first thing you should do is to **ensure that Unity scripts keep running even when the application window is not focused**. In *Project Settings > Player > Resolution and Presentation*, make sure that "Run in Background" is checked. Otherwise, for instance, if you go into play mode in Unity and then go and click on the TUIO Simulator's window, the Unity Editor loses focus and stops updating scripts, and your TUIO cursor messages will not be received. They will later be received (all at once) when you click back on the Editor window. 

Create an empty Game Object in your scene, name it "Cursor Manager" and add the *Tuio Cursor Manager* script to it. There should be only one such component in the scene. Make sure the *Tuio Cursor Manager*'s port is set to 3333 (the default port for TUIO), and "Open On Start" is checked (so that the manger begins listening for messages when you enter Play mode). You can also check "Debug Messages" for initial testing - you could already enter Play mode, and try out the TUIO simulator, while keeping an eye on the Console to make sure that cursor data is coming in.

The *Tuio Cursor Manager* will receive and handle TUIO 2D-cursor messages, to keep track of cursors and call events when cursors are added, updated or removed. The *Tuio Cursor Manager* won't create anything in the scene by itself. You could write your own script that subscribes to the *Tuio Cursor Manager* events, creating and manipulating Game Objects in the scene as intended. A *Deep Space Cursor Manager* script is provided, which does just that and should suit most purposes. If you ever need to write your own specific behaviour, feel free to take a look inside the script and/or use it as a starting point.

We will start with displaying cursors on the wall cursors as the base example. Create an empty Game Object named "Wall Cursors". This object's position should be aligned with the camera. Typically your camera is looking forward in the Z axis, so your "Wall Cursors" object should be on the same line along the Z axis, in front of the camera. Make sure that the "Wall Cursors" object is not rotated or scaled. Add a *Deep Space Cursor Manager* script to the object. 

Create an empty Game Object in the scene and name it "My Cursor". Make sure that the object's transform is reset (all 0's in position and rotation, all 1's in scale). As a child of this object, create a Unity 3D primitive such as a sphere or cube; or add another visual of your choice, such as a 3D model. Depending on the distance from your "Wall Cursors" object to the camera, you may want to make your cursor's visuals larger (if it is very far) or smaller (if it is very close). To the "My Cursor" object, add the *Deep Space Cursor* script. For now, make sure "Destroy When Removed" is checked. This will cause the cursor to disappear (self-destruct) as soon as it is removed by the *Deep Space Cursor Manager*. Later on you may want to animate the cursor's visuals in some way, instead of having it immediately disappear, but for now we want to be sure that things are working properly, and cursors aren't left hanging in the scene like zombies. Now create a prefab based on "My Cursor" by dragging the Game Object from the Hierarchy tab into the project tab, ideally to a "Prefabs" folder. You can now delete the original "My Cursor" from the scene Hierarchy.

Back to the *Deep Space Cursor Manager*, assign the cursor prefab you just created to "Cursor Prefab". When a TUIO cursor is added, the manager will create a Game Object in the scene based on this prefab; and when the TUIO cursor's position is updated, the manager will update the object's position in the scene. You could already press play and give it a try - hopefully you can see that cursors are being created, moved and removed when you use the TUIO simulator.

In the Deep Space Cursor Manager, choose a range for X and Y positions. The manager script will set the X and Y position of cursors relative to the parent - in this case the "Wall Cursors" object. The manager script won't change the object's relative Z position. 

Your cursors will always move in an XY plane **relative to their parent transform**. So if you wanted to make floor cursors, or cursors that move in the XZ (horizontal) plane, you could simply rotate the parent transform 90 degrees in the X axis. The parent transform can be assigned in the *Deep Space Cursor Manager* as you wish, and does not have to be the same Game Object that the *Deep Space Cursor Manager* script is on. Ideally, make sure that neither the cursors' parent or its progenitors/ancestors are scaled. You can have multiple *Deep Space Cursor Managers* if you need, each with different position ranges, cursor parents and cursor prefabs. For instance, if you wanted to have cursors on the wall and floor, you will likely have two *Deep Space Cursor Managers*. If your floor area is not a 3D scene but rather a UI canvas, you will have a different prefab for the floor cursors, typically using a UI image. Check the example scenes to see how the managers, parent transforms and prefabs are set up in each case.