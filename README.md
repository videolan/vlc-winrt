# VLC for Windows 10 Desktop, Mobile and Xbox

This is the official mirror repository of VLC for UWP.

_You can find the official repository [here](https://code.videolan.org/videolan/vlc-winrt)._

It's currently written in C# and uses [libvlcpp](https://code.videolan.org/videolan/libvlcpp) (C++) and 
[libvlcppcx](https://code.videolan.org/videolan/vlc-winrt/tree/master/modules/libvlcppcx) (C++/CX) for interop with managed code.

- [Requirements](#requirements)
- [Building](#building)
    - [VLC-WinRT](#vlc-winrt)
    - [LibVLC](#libvlc)
- [Contribute](#contribute)
- [Communication](#communication)
    - [Forum](#forum)
    - [Issues](#issues)
    - [IRC](#irc)
- [Code of Conduct](#code-of-conduct)
- [License](#license)
- [More](#more)


## Requirements
* A recent enough Windows 10 
* Visual Studio 2017 (UWP workload, C++ workload with VC++ 2017 and C++ runtime for UWP, Windows 10 SDK)
* [Multilingual App Toolkit](https://marketplace.visualstudio.com/items?itemName=MultilingualAppToolkit.MultilingualAppToolkit-18308)
* [SQLite for UWP](https://marketplace.visualstudio.com/items?itemName=SQLiteDevelopmentTeam.SQLiteforUniversalWindowsPlatform)


## Building

### VLC-WinRT

First, get the code
```cmd
git clone https://code.videolan.org/videolan/vlc-winrt
```

Enter the repository directory
```cmd 
cd vlc-winrt
```

Then, initialize `libvlcpp` 
```cmd 
git submodule update --init
```

After this, you need to get a recent nightly build of vlc-winrt: [x86](http://nightlies.videolan.org/build/winrt-i686) or [x64](http://nightlies.videolan.org/build/winrt-x86_64)

Extract the content from the archive to `vlc-winrt/libvlc/Universal/vlc-x86/Debug/` and create the directories as necessary (where `vlc-winrt` is the repositorty root).

If you have downloaded an x64 build, replace the `vlc-x86` folder name by `vlc-amd64`.

If you want to make a release build, replace the `Debug` folder name by `Release`.

### LibVLC

If you want to build libvlc yourself (instead of grabing a nightly build), aside from a lot of time and patience, you need to install and setup `mingw-w64`.
Then run `./compile.sh` with your target platform and configuration.

Use the docker image [here](https://code.videolan.org/videolan/docker-images/blob/master/vlc-winrt-x86_64/Dockerfile)

## Contribute

### Pull request

Pull request are more than welcome! If you do submit one, please make sure to use a descriptive title and description.

### Gitlab issues

You can look through issues we currently have on the [VideoLAN Gitlab](https://code.videolan.org/videolan/vlc-winrt/issues).

An [up for grabs](https://code.videolan.org/videolan/vlc-winrt/issues?label_name%5B%5D=up+for+grabs) tag is available if you don't know where to start.

## Communication

### Forum

If you have any question or you're not sure it's an issue please visit our [forum](https://forum.videolan.org/).

### Issues

You have encountered an issue and wish to report it to the VLC dev team?

You can create one on our [Gitlab](https://code.videolan.org/videolan/vlc-winrt/issues) or on our [bug tracker](https://trac.videolan.org/vlc/).

Before creating an issue or ticket, please double check of duplicates!

### IRC

Want to quickly get in touch with us for a question, or even just to talk?

You will always find someone of the VLC team on IRC, __#videolan__ channel on the freenode network.

If you don't have an IRC client, you can always use the [freenode webchat](https://webchat.freenode.net/).

## Code of Conduct

Please read and follow the [VideoLAN CoC](https://wiki.videolan.org/Code_of_Conduct/).

## License

VLC-WinRT is under the GPLv2 (or later) and the MPLv2 license.

See [LICENSE](./LICENSE) for more license info.

## More

For everything else, check our [wiki](https://wiki.videolan.org/) or our [support page](http://www.videolan.org/support/).

We're happy to help!