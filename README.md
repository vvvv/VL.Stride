# VL.Stride

[Stride 3d Engine](http://stride3d.net) for VL.

Try it with vvvv, the visual live-programming environment for .NET  
Download: http://visualprogramming.net

## Contributing
- [Fork this repo](https://github.com/vvvv/VL.Stride/fork)
- Make sure [git lfs](https://git-lfs.github.com/) is available (check by typing `git lfs` into the git bash)
- Checkout this repository into a folder which doesn't have spaces
- (Run `git lfs install` and `git lfs pull` in the git bash (high-level git tools like GitExtensions do this automatically))
- Open the solution `packages\VL.Stride.sln`, switch it to `Release` mode, set VL.Stride as startup project and press `Ctrl+F5` to start vvvv. (VL.Stride is configured as a source package automatically)

The build process will download and install the required vvvv gamma version in case it's not installed yet.

Compiling and running in `Debug` requires the [graphic diagnostic tools](https://docs.microsoft.com/en-us/windows/uwp/gaming/use-the-directx-runtime-and-visual-studio-graphics-diagnostic-features) to be installed.

In case one wants to use a different vvvv gamma installation its path can be set with the `VVVV_BinPath` property in `packages\Directory.Build.props`.

Follow the [GitHub Flow](https://guides.github.com/introduction/flow/) to contrinute your changes back to the main repo. Also don't hesitate to open a [pull request early](https://carlosperez.medium.com/pull-request-first-f6bb667a9b6) to let everyone know what you are working on.

## Credits

A deep bow before those who believed in VL.Stride from the beginning and substantially supported its development:

* [Marshmallow Laser Feast](http://marshmallowlaserfeast.com)
* [schnellebuntebilder](http://schnellebuntebilder.de)
* [m box](http://m-box.de)
* [Refik Anadol](http://refikanadol.com)
* Jarrad Hope
