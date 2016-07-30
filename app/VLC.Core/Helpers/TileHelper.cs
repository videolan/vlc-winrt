/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using VLC.Model;
using VLC.ViewModels;
using VLC.Utils;
using System.Threading.Tasks;
using System.Diagnostics;
using NotificationsExtensions.Tiles;

namespace VLC.Helpers
{
    public class TileHelper
    {
        public static void UpdateMusicTile()
        {
            if (Locator.MusicPlayerVM.CurrentTrack == null) return;
            var content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileMedium = CreateMediumMusicTileBinding(),
                    TileWide = CreateWideMusicTileBinding(),
                    TileLarge = CreateLargeMusicTileBinding()
                }
            };
            var tileXml = content.GetXml();
            var tileNotification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }

        private static TileBinding CreateMediumMusicTileBinding()
        {
            var bindingContent = new TileBindingContentAdaptive()
            {
                Children =
                {
                    new TileText()
                    {
                        Text = Strings.NowPlaying,
                        Style = TileTextStyle.Body,
                    },
                    new TileText()
                    {
                        Text = Locator.MusicPlayerVM.CurrentTrack.Name + " - " + Locator.MusicPlayerVM.CurrentTrack.ArtistName,
                        Wrap = true,
                        Style = TileTextStyle.CaptionSubtle
                    }
                }
            };
            
            if (!string.IsNullOrEmpty(Locator.MusicPlayerVM.CurrentAlbum?.AlbumCoverFullUri))
            {
                bindingContent.PeekImage = new TilePeekImage()
                {
                    Source = new TileImageSource(Locator.MusicPlayerVM.CurrentAlbum.AlbumCoverFullUri)
                };
            }
            
            return new TileBinding()
            {
                Branding = TileBranding.Logo,
                Content = bindingContent
            };
        }

        private static TileBinding CreateWideMusicTileBinding()
        {
            var bindingContent = new TileBindingContentAdaptive()
            {
                Children =
                {
                    new TileGroup()
                    {
                        Children =
                        {
                            new TileSubgroup()
                            {
                                Children =
                                {
                                   new TileText()
                                    {
                                        Text = Strings.NowPlaying,
                                        Style = TileTextStyle.Body,
                                    },
                                   new TileText()
                                    {
                                    Text = Locator.MusicPlayerVM.CurrentTrack.Name + " - " + Locator.MusicPlayerVM.CurrentTrack.ArtistName,
                                    Wrap = true,
                                    Style = TileTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        }
                    },
                }
            };

            if (!string.IsNullOrEmpty(Locator.MusicPlayerVM.CurrentAlbum?.AlbumCoverFullUri))
            {
                bindingContent.PeekImage = new TilePeekImage()
                {
                    Source = new TileImageSource(Locator.MusicPlayerVM.CurrentAlbum.AlbumCoverFullUri)
                };
            }

            if (!string.IsNullOrEmpty(Locator.MusicPlayerVM.CurrentArtist?.Picture))
            {
                var artistPic = new TileSubgroup()
                {
                    Weight = 33,
                    Children =
                                {
                                    new TileImage()
                                    {
                                        Crop = TileImageCrop.Circle,
                                        Source = new TileImageSource(Locator.MusicPlayerVM.CurrentArtist.Picture)
                                    }
                                }

                };
                (bindingContent.Children[0] as TileGroup).Children.Insert(0, artistPic);
            }

            return new TileBinding()
            {
                Branding = TileBranding.NameAndLogo,
                Content = bindingContent
            };
        }

        private static TileBinding CreateLargeMusicTileBinding()
        {
            var bindingContent = new TileBindingContentAdaptive()
            {
                Children =
                {
                    new TileGroup()
                    {
                        Children =
                        {
                            new TileSubgroup()
                            {
                              Weight = 1
                            },
                            new TileSubgroup()
                            {
                                Weight = 2,
                            },
                            new TileSubgroup()
                            {
                                Weight = 1
                            }
                        }
                    },
                    new TileText()
                    {
                        Text = Strings.NowPlaying,
                        Style = TileTextStyle.Title,
                        Align = TileTextAlign.Center
                    },
                    new TileText()
                    {
                        Text = Locator.MusicPlayerVM.CurrentTrack.Name + " - " + Locator.MusicPlayerVM.CurrentTrack.ArtistName,
                        Wrap = true,
                        Style = TileTextStyle.SubtitleSubtle,
                        Align = TileTextAlign.Center
                    }
                }
            };
            if (!string.IsNullOrEmpty(Locator.MusicPlayerVM.CurrentAlbum?.AlbumCoverFullUri))
            {
                bindingContent.PeekImage = new TilePeekImage()
                {
                    Source = new TileImageSource(Locator.MusicPlayerVM.CurrentAlbum.AlbumCoverFullUri)
                };
            }

            if (!string.IsNullOrEmpty(Locator.MusicPlayerVM.CurrentArtist?.Picture))
            {
                var artistPic = new TileImage()
                {
                    Crop = TileImageCrop.Circle,
                    Source = new TileImageSource(Locator.MusicPlayerVM.CurrentArtist.Picture)
                };
                (bindingContent.Children[0] as TileGroup).Children[1].Children.Add(artistPic);
            }
            return new TileBinding()
            {
                Branding = TileBranding.NameAndLogo,
                Content = bindingContent
            };
        }

        public static void UpdateVideoTile()
        {
            if (Locator.VideoPlayerVm.CurrentVideo == null)
                return;
            var content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileMedium = CreateMediumVideoTileBinding(),
                    TileWide = CreateWideVideoTileBinding(),
                    TileLarge = CreateLargeVideoTileBinding()
                }
            };
            var tileXml = content.GetXml();
            var tileNotification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }

        private static TileBinding CreateMediumVideoTileBinding()
        {
            var bindingContent = new TileBindingContentAdaptive()
            {
                Children =
                {
                    new TileText()
                    {
                        Text = Strings.NowPlaying,
                        Style = TileTextStyle.Body,
                    },
                    new TileText()
                    {
                        Text = Locator.VideoPlayerVm.CurrentVideo.Name,
                        Wrap = true,
                        Style = TileTextStyle.CaptionSubtle
                    }
                }
            };

            if (!string.IsNullOrEmpty(Locator.VideoPlayerVm.CurrentVideo.PictureUri))
            {
                bindingContent.PeekImage = new TilePeekImage()
                {
                    Source = new TileImageSource(Locator.VideoPlayerVm.CurrentVideo.PictureUri)
                };
            }

            return new TileBinding()
            {
                Branding = TileBranding.Logo,
                Content = bindingContent
            };
        }

        private static TileBinding CreateWideVideoTileBinding()
        {
            var bindingContent = new TileBindingContentAdaptive()
            {
                Children =
                {
                    new TileGroup()
                    {
                        Children =
                        {
                            new TileSubgroup()
                            {
                                Children =
                                {
                                   new TileText()
                                    {
                                        Text = Strings.NowPlaying,
                                        Style = TileTextStyle.Body,
                                    },
                                   new TileText()
                                    {
                                    Text = Locator.VideoPlayerVm.CurrentVideo.Name,
                                    Wrap = true,
                                    Style = TileTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        }
                    },
                }
            };

            if (!string.IsNullOrEmpty(Locator.VideoPlayerVm.CurrentVideo.PictureUri))
            {
                bindingContent.PeekImage = new TilePeekImage()
                {
                    Source = new TileImageSource(Locator.VideoPlayerVm.CurrentVideo.PictureUri)
                };
            }

            return new TileBinding()
            {
                Branding = TileBranding.NameAndLogo,
                Content = bindingContent
            };
        }

        private static TileBinding CreateLargeVideoTileBinding()
        {
            var bindingContent = new TileBindingContentAdaptive()
            {
                Children =
                {
                    new TileGroup()
                    {
                        Children =
                        {
                            new TileSubgroup()
                            {
                              Weight = 1
                            },
                            new TileSubgroup()
                            {
                                Weight = 2,
                            },
                            new TileSubgroup()
                            {
                                Weight = 1
                            }
                        }
                    },
                    new TileText()
                    {
                        Text = Strings.NowPlaying,
                        Style = TileTextStyle.Title,
                        Align = TileTextAlign.Center
                    },
                    new TileText()
                    {
                        Text = Locator.VideoPlayerVm.CurrentVideo.Name,
                        Wrap = true,
                        Style = TileTextStyle.SubtitleSubtle,
                        Align = TileTextAlign.Center
                    }
                }
            };
            if (!string.IsNullOrEmpty(Locator.VideoPlayerVm.CurrentVideo.PictureUri))
            {
                bindingContent.PeekImage = new TilePeekImage()
                {
                    Source = new TileImageSource(Locator.VideoPlayerVm.CurrentVideo.PictureUri)
                };
            }
            
            return new TileBinding()
            {
                Branding = TileBranding.NameAndLogo,
                Content = bindingContent
            };
        }

        public static async Task<bool> CreateOrReplaceSecondaryTile(VLCItemType type, int id, string title)
        {
            string tileId = "SecondaryTile-" + type.ToString() + "-" + id;
            if (!SecondaryTile.Exists(tileId))
            {
                var tileData = new SecondaryTile()
                {
                    TileId = tileId,
                    DisplayName = title,
                    Arguments = tileId
                };
                string subfolder = null;
                switch (type)
                {
                    case VLCItemType.Album:
                        subfolder = "albumPic";
                        break;
                    case VLCItemType.Artist:
                        subfolder = "artistPic";
                        break;
                }
                tileData.VisualElements.ShowNameOnSquare150x150Logo = true;
                tileData.DisplayName = title;
                tileData.VisualElements.Square150x150Logo =
                    new Uri("ms-appdata:///local/" + subfolder + "/" + id + ".jpg");
                bool success = await tileData.RequestCreateAsync();
                return success;
            }
            else
            {
                SecondaryTile secondaryTile = new SecondaryTile(tileId);
                await secondaryTile.RequestDeleteForSelectionAsync(Window.Current.Bounds, Placement.Default);
                ToastHelper.Basic(Strings.TileRemoved);
                return false;
            }
        }

        public static bool SecondaryTileExists(VLCItemType type, int id, string title)
        {
            string tileId = "SecondaryTile-" + type.ToString() + "-" + id;
            return SecondaryTile.Exists(tileId);
        }

        public static void ClearTile()
        {
            try
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Couldn't clear live tiles");
            }
        }
    }
}
