#!/bin/sh

OPTIONS="
      --enable-winstore-app
      --disable-vlc
      --enable-lua
      --disable-vlm --disable-sout
      --disable-faad
      --disable-flac
      --enable-theora
      --disable-twolame
      --enable-quicktime
      --enable-avcodec --enable-merge-ffmpeg
      --enable-dca
      --enable-mpc
      --enable-libass
      --disable-x264
      --enable-realrtsp
      --enable-live555
      --enable-shout
      --disable-goom
      --disable-caca
      --disable-sdl
      --disable-qt
      --disable-skins2
      --enable-sse --enable-mmx
      --disable-libcddb
      --disable-zvbi --disable-telx
      --disable-mad
      --disable-a52
      --enable-vlc
      --disable-libgcrypt
      --disable-taglib
      --disable-dirac"

if gcc -v 2>/dev/null -a echo | gcc -mno-cygwin -E -2>/dev/null 2>&1
then
    echo Cygwin detected, adjusting options
    export CC="gcc -mno-cygwin"
    export CXX="g++ -mno-cygwin"
    OPTIONS="${OPTIONS} --disable-mkv"
fi

sh ../configure ${OPTIONS} $*
