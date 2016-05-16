#! /bin/sh

set -e

usage()
{
    echo "Usage: compile <arch> <TargetOS>"
    echo "archs: i686,x86_64,armv7"
    echo "os: win81,win10"
}

using()
{
    echo "preparing for MSVC target: $MSVC_TUPLE"
}

if [ "$1" != "" ]; then

case "$1" in

i686)
    MSVC_TUPLE="Win32"
    using
    ;;
x86_64)
    MSVC_TUPLE="x64"
    using
    ;;
armv7)
    MSVC_TUPLE="ARM"
    using
    ;;
*) echo "Unknown arch: $1"
   usage
   exit 1
   ;;
esac

case "$2" in
    win10)
        WINVER=0xA00
        RUNTIME=msvcr120_app
        ;;
    win81)
        WINVER=0x602
        RUNTIME=msvcr120_app
        ;;
    *)
        echo "Unknown OS: $2"
        usage
        exit 1
        ;;
esac

echo Using runtime $RUNTIME

# 1/ libvlc, libvlccore and its plugins
TESTED_HASH=45df8a6415
if [ ! -d "vlc" ]; then
    echo "VLC source not found, cloning"
    git clone git://git.videolan.org/vlc.git vlc
    cd vlc
    git am -3 ../patches/*.patch
    if [ $? -ne 0 ]; then
        git am --abort
        echo "Applying the patches failed, aborting git-am"
        exit 1
    fi
else
    echo "VLC source found"
    cd vlc
    if ! git cat-file -e ${TESTED_HASH}; then
        cat << EOF
***
*** Error: Your vlc checkout does not contain the latest tested commit ***
***

EOF
        exit 1
    fi
fi

MAKEFLAGS=
if which nproc >/dev/null
then
MAKEFLAGS=-j`nproc`
fi

TARGET_TUPLE=${1}-w64-mingw32
case "${1}" in
    armv7)
        COMPILER="clang -target armv7-windows-gnu"
        COMPILERXX="clang++ -target armv7-windows-gnu"
        # Clang will yield armv7-windows-gnu as build arch, which seems
        # to confuse some configure scripts
        BUILD_ARCH=x86_64-linux-gnu
        ;;
    *)
        COMPILER=${TARGET_TUPLE}-gcc
        COMPILERXX=${TARGET_TUPLE}-g++
        ${COMPILER} -dumpspecs | sed -e 's/-lmingwex/-lwinstorecompat -lmingwex -lwinstorecompat -lole32 -lruntimeobject/' -e "s/-lmsvcrt/-l$RUNTIME/" > ../newspecfile
        NEWSPECFILE="`pwd`/../newspecfile"
        COMPILER="${COMPILER} -specs=$NEWSPECFILE"
        COMPILERXX="${COMPILERXX} -specs=$NEWSPECFILE"
        BUILD_ARCH=`gcc -dumpmachine`
        ;;
esac


EXTRA_CPPFLAGS="-D_WIN32_WINNT=$WINVER -DWINVER=$WINVER -DWINSTORECOMPAT -D_UNICODE -DUNICODE -DWINAPI_FAMILY=WINAPI_FAMILY_APP"
EXTRA_LDFLAGS="-lnormaliz -lwinstorecompat -lruntimeobject"

echo "Building the contribs"
CONTRIB_FOLDER=contrib/winrt-$1-$RUNTIME
mkdir -p $CONTRIB_FOLDER
cd $CONTRIB_FOLDER
../bootstrap --host=${TARGET_TUPLE} --build=$BUILD_ARCH --disable-disc --disable-sout \
    --disable-sdl \
    --disable-schroedinger \
    --disable-vncserver \
    --disable-chromaprint \
    --disable-modplug \
    --disable-SDL_image \
    --disable-fontconfig \
    --disable-zvbi \
    --disable-caca \
    --disable-gettext \
    --disable-gme \
    --disable-tremor \
    --enable-vorbis \
    --enable-mad \
    --disable-sidplay2 \
    --enable-samplerate \
    --disable-faad2 \
    --enable-iconv \
    --disable-goom \
    --enable-dca \
    --disable-fontconfig \
    --disable-gpg-error \
    --disable-projectM \
    --enable-ass \
    --disable-qt \
    --disable-protobuf \
    --disable-aribb25 \
    --disable-gpl \
    --disable-libarchive \
    --enable-ssh2 \
    --disable-vncclient

echo "EXTRA_CFLAGS=${EXTRA_CPPFLAGS}" >> config.mak
echo "EXTRA_LDFLAGS=${EXTRA_LDFLAGS}" >> config.mak
echo "CC=${COMPILER}" >> config.mak
echo "CXX=${COMPILERXX}" >> config.mak
export PKG_CONFIG_LIBDIR="`pwd`/contrib/${TARGET_TUPLE}/lib/pkgconfig"

make $MAKEFLAGS

BUILD_FOLDER=winrt-$1-$RUNTIME
cd ../.. && mkdir -p ${BUILD_FOLDER} && cd ${BUILD_FOLDER}

echo "Bootstraping"
../bootstrap

echo "Configuring"
CPPFLAGS="${EXTRA_CPPFLAGS}" \
LDFLAGS="${EXTRA_LDFLAGS}" \
CC="${COMPILER}" \
CXX="${COMPILERXX}" \
ac_cv_search_connect="-lws2_32" \
../../configure.sh --host=${TARGET_TUPLE}

echo "Building"
make $MAKEFLAGS

echo "Package"
make install

rm -rf tmp && mkdir tmp

# Compiler shared DLLs, when using compilers built with --enable-shared
# The shared DLLs may not necessarily be in the first LIBRARY_PATH, we
# should check them all.
library_path_list=`${TARGET_TUPLE}-g++ -v /dev/null 2>&1 | grep ^LIBRARY_PATH|cut -d= -f2` ;

find _win32/bin -name "*.dll" -exec cp -v {} tmp \;
cp -r _win32/include tmp/
cp -r _win32/lib/vlc/plugins tmp/

find tmp -name "*.la" -exec rm -v {} \;
find tmp -name "*.a" -exec rm -v {} \;
blacklist="
wingdi
waveout
dshow
directdraw
windrive
globalhotkeys
direct2d
ntservice
dxva2
dtv
vcd
cdda
quicktime
atmo
oldrc
dmo
panoramix
screen
win_msg
win_hotkeys
crystalhd
smb
"
regexp=
for i in ${blacklist}
do
    if [ -z "${regexp}" ]
    then
        regexp="${i}"
    else
        regexp="${regexp}|${i}"
    fi
done
rm `find tmp/plugins -name 'lib*plugin.dll' | grep -E "lib(${regexp})_plugin.dll"`

find tmp \( -name "*.dll" -o -name "*.exe" \) -exec ${TARGET_TUPLE}-strip {} \;
find tmp \( -name "*.dll" -o -name "*.exe" \) -exec ../../appcontainer.pl {} \;

cp lib/.libs/libvlc.dll.a tmp/libvlc.lib
cp src/.libs/libvlccore.dll.a tmp/libvlccore.lib

CURRENTDATE="$(date +%Y%m%d-%H%M)"

cd tmp
7z a ../vlc-${1}-${2}-${CURRENTDATE}.7z *
cd ..
rm -rf tmp

else
    usage
fi
