#!/bin/sh

OPTIONS="
      --enable-winstore-app
      --disable-vlc
      --enable-lua
      --disable-vlm
      --disable-faad
      --enable-theora
      --disable-twolame
      --enable-avcodec --enable-merge-ffmpeg
      --enable-dca
      --enable-mpc
      --enable-libass
      --disable-x264
      --enable-realrtsp
      --enable-shout
      --disable-goom
      --disable-caca
      --disable-qt
      --disable-skins2
      --enable-sse --enable-mmx
      --disable-libcddb
      --enable-zvbi --disable-telx
      --enable-mad
      --disable-a52
      --enable-dsm
      --disable-smbclient
      --disable-vcd
      --disable-crystalhd
      --disable-dxva2
      --enable-flac
      --enable-jpeg
      --enable-vpx
      --enable-postproc
      --enable-gme"

sh ../configure ${OPTIONS} $*
