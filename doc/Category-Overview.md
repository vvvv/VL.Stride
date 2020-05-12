## Assets
Nodes that work with the xenko asset system
* (Managed)Asset, URL → generic xenko resource type
## Audio
Manage audio files and playback
* AudioComponents
* Audio manager nodes (add/remove file/sound)
## EffectLib
vl wrapped shader nodes lib, dynamic pins on recompile etc., would be nice to get categories into it, use namespace keyword and check whether shader reflection contains it
* All shaders
## Engine
Basic nodes to work with the scene graph
* Entity
* Component
* Scene
GroupEntity (+ Spectral)
## Input
Handle user input
* OrbitCamera controls
* Input manager for unified input events
## Layer
VL invented category for lowlevel api functional layer nodes → move to LowLevelAPI
* Group (LowLevelAPI) (+Spectral)
## LowLevelAPI
Xenkos unified low level graphics API, abstraction over DX11/12, Vulkan, OpenGL etc.
* Buffers
* Rendering
* Draw calls
* Shader things 
* Paramater management imports/operations
* SpoutSender, should move into textures maybe
* Textures
## Mathematics
Xenkos math lib, will be used as VL's default vectors and matrices instead of SharpDX
* Vector, Matrix
* BoundingBox
* Frustum
* ...
## Rendering
Xenko’s category that contains rendering functionality
* OrbitCamera
* PostFX (monolithic node, needs proper split per effect)
* Prefab
* MeshDraw
* Stream out to MeshDraw/Model
## Utils
Currently a sink category for util Operations that help with patching
* Getters for important instances/services (game, graphics device, contexts, commandlist)
* Converters (vectors, matrix etc.), will be removed once we switched to xenko math for VL
* Math utils
## VirtualReality
Nodes to work with VR specific functionality
* GetVRDevice
* Controller input
* Tracker
