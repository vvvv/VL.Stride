## Install Xenko
* Before running the Xenko launcher, edit the file **%AppData%\NuGet\NuGet.Config** to add the vvvv nuget feed:
```xml
<add key="vvvv-public-feed" value="http://teamcity.vvvv.org/guestAuth/app/nuget/v1/FeedService.svc/" />
```
* Running the Xenko launcher now shows the vvvv builds of Xenko
* Install the Xenko 3.2.x build
* Then start the Xenko Game Studio by pressing the red button
<p align="center">
<img src="images/xenkolauncher2.png" width="50%" alt="Visual Studio Installer">
</p>

* Also install the Visual Studio extension

## Get VL.Xenko sources
* Clone repository: https://github.com/vvvv/VL.Stride.git
* Switch to `external-mainloop` branch
* Open VL.Xenko\packages-xenko\VL.Xenko.sln
* Make sure TestGame.Windows is the startup project
* Rebuild and run 
* Save Main.vl as your own project document
* Quit game

## Set the Startup .vl Document
* Open the project TestGame.Windows
* Open the TestGameApp.cs
* Modify the following line to reference your main project .vl document  
`game.AttachVL(VLScript.MainVLDocSrc);`    
e.g.  
`game.AttachVL(@"C:\Users\joreg\Documents\repos\PolyGround\vl\Polyground.vl");`

## Run Game
* Make sure T**estGame.Windows** is set as startup project
## Additional Tools
These tools greatly improve the workflow with Xenko: [Additional Tools](Additional-Tools.md)