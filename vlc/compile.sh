#! /bin/sh

# 1/ libvlc, libvlccore and its plugins
TESTED_HASH=aaf991f
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

TARGET_TUPLE=i686-w64-mingw32

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
    --enable-faad2 \
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

make fetch
make

cd ../.. && mkdir -p winrt && cd winrt

echo "Bootstraping"
../bootstrap

echo "Configuring"
../../configure.sh --host=${TARGET_TUPLE}

echo "Building"
make

echo "Package"
make install

mkdir tmp
find _win32/bin -name "*.dll" -exec cp -v {} tmp \;

cp -r _win32/lib/vlc/plugins tmp/
find tmp -name "*.la" -exec rm -v {} \;
find tmp -name "*.a" -exec rm -v {} \;

cd tmp && zip -r -9 ../vlc.zip * && cd ..
rm -rf tmp
