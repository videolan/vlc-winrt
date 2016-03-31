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
      --enable-mad
      --disable-libgcrypt
      --disable-dirac"

sh ../configure ${OPTIONS} $*
