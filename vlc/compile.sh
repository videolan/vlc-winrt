#! /bin/sh

set -e

# 1/ libvlc, libvlccore and its plugins
TESTED_HASH=66a5515e
if [ ! -d "vlc" ]; then
    echo "VLC source not found, cloning"
    git clone git://git.videolan.org/vlc.git vlc
    cd vlc
    #git am -3 ../patches/*.patch
    #if [ $? -ne 0 ]; then
    #    git am --abort
    #    echo "Applying the patches failed, aborting git-am"
    #    exit 1
    #fi
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

TARGET_TUPLE=i686-w64-mingw32
[ $# = 1 ] && TARGET_TUPLE=$1

EXTRA_CPPFLAGS="-D_WIN32_WINNT=0x602"
EXTRA_LDFLAGS="-lwinstorecompat"

echo "Building the contribs"
mkdir -p contrib/winrt
cd contrib/winrt
../bootstrap --host=${TARGET_TUPLE} --disable-disc --disable-sout \
    --disable-sdl \
    --disable-SDL_image \
    --disable-fontconfig \
    --disable-zvbi \
    --disable-kate \
    --disable-caca \
    --disable-gettext \
    --disable-mpcdec \
    --disable-upnp \
    --disable-gme \
    --disable-tremor \
    --disable-vorbis \
    --disable-sidplay2 \
    --disable-samplerate \
    --disable-faad2 \
    --disable-harfbuzz \
    --enable-iconv \
    --disable-goom \
    --disable-flac \
    --enable-dca \
    --disable-fontconfig \
    --disable-gcrypt \
    --disable-gpg-error \
    --disable-gnutls \
    --disable-projectM \
    --disable-ass \
    --disable-qt4 \
    --disable-gpl

echo "EXTRA_CFLAGS=-g ${EXTRA_CPPFLAGS}" >> config.mak
echo "EXTRA_LDFLAGS=${EXTRA_LDFLAGS}" >> config.mak


make fetch
make $MAKEFLAGS

cd ../.. && mkdir -p winrt && cd winrt

echo "Bootstraping"
../bootstrap

echo "Configuring"
CPPFLAGS="${EXTRA_CPPFLAGS}" \
LDFLAGS="${EXTRA_LDFLAGS}" \
../../configure.sh --host=${TARGET_TUPLE}

echo "Building"
make $MAKEFLAGS

echo "Package"
make install

find _win32 -name "*.dll" -exec ../extras/package/win32/peflags.pl {} \;

rm -rf tmp && mkdir tmp
find _win32/bin -name "*.dll" -exec cp -v {} tmp \;

cp -r _win32/include tmp/

cp -r _win32/lib/vlc/plugins tmp/
find tmp -name "*.la" -exec rm -v {} \;
find tmp -name "*.a" -exec rm -v {} \;

cd tmp
cp ../lib/.libs/libvlc.dll.def ../src/.libs/libvlccore.dll.def .

${TARGET_TUPLE}-dlltool libvlc.dll -l libvlc.lib -d libvlc.dll.def libvlc.dll
${TARGET_TUPLE}-dlltool libvlccore.dll -l libvlccore.lib -d libvlccore.dll.def libvlccore.dll

7z a ../vlc.7z *

cd ..
rm -rf tmp
