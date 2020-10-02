### 0.5.99
* updated to xenko beta2-741 fixed build issues
* updated VL to preview-0810

### 0.5.94
* Added texture/matrix tooltips and first version of TextureFX 

### 0.5.78
Switched VL to Xenko math, no more conversion nodes required

### 0.5.63
First version with custom content loader that can read Xenko projects
* added LoadProject node
* added FileTexturePooled node

### 0.5.30
Work on FileTexture and material modification with [sebllll](https://github.com/sebllll)'s contribution.
* added VL.Xenko.Windows project/package to work with windows specific nodes
* added FileTexture, windows specific texture loading using [Xenko's TextureConverter](https://www.nuget.org/packages/Xenko.TextureConverter/) tool 
* added SetTexture for materials
* added Get/SetMaterial for ModelComponent
* added GetModelComponent

### 0.5.21
* Xenko profiler gets started by PerfMeter
* Compute shader and Drawers have a "Profiler Name" input that is visible on the GPU timings page
* added PBREmissive and PBRThinGlass material nodes
* added PointLight, SpotLight and AmbientLight nodes
* Entity and Group nodes have "Transformation" and "Name" inputs
* transformation hierarchy of scene graph gets properly passed on to the shader WORLD matrix
* SceneGraphExplorer updates names of entities on change
