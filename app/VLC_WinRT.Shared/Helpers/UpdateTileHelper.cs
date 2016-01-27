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
using VLC_WinRT.Model;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Utils;
#if WINDOWS_UWP
using NotificationsExtensions.Tiles;
#endif
namespace VLC_WinRT.Helpers
{
    public class UpdateTileHelper
    {
        #region Windows UWP
#if WINDOWS_UWP
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
            
            if (!string.IsNullOrEmpty(Locator.MusicPlayerVM.CurrentAlbum.AlbumCoverFullUri))
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
                                        Text = "Now Playing",
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
#endif
        #endregion
        public static void UpdateMediumTileWithMusicInfo()
        {
            const TileTemplateType template = TileTemplateType.TileSquare150x150PeekImageAndText02;
            var tileXml = TileUpdateManager.GetTemplateContent(template);

            var tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = Strings.NowPlaying;
            if (Locator.MusicPlayerVM.CurrentTrack != null)
            {
                tileTextAttributes[1].InnerText = Locator.MusicPlayerVM.CurrentTrack.Name + " - " + Locator.MusicPlayerVM.CurrentTrack.ArtistName;

                var tileImgAttribues = tileXml.GetElementsByTagName("image");
                if (Locator.MusicPlayerVM.CurrentAlbum != null)
                    tileImgAttribues[0].Attributes[1].NodeValue = Locator.MusicPlayerVM.CurrentAlbum.AlbumCoverFullUri;
            }

            var tileNotification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
            UpdateBigTileWithMusicInfo();
        }

        public static void UpdateBigTileWithMusicInfo()
        {
            const TileTemplateType template = TileTemplateType.TileWide310x150PeekImage05;
            var tileXml = TileUpdateManager.GetTemplateContent(template);

            var tileTextAttributes = tileXml.GetElementsByTagName("text");
#if WINDOWS_APP
            tileTextAttributes[0].InnerText = Strings.NowPlaying;
#endif
            if (Locator.MusicPlayerVM.CurrentTrack != null)
            {
#if WINDOWS_APP
                tileTextAttributes[1].InnerText = Locator.MusicPlayerVM.CurrentTrack.Name + " - " + Locator.MusicPlayerVM.CurrentTrack.ArtistName;
#else
                tileTextAttributes[0].InnerText = Locator.MusicPlayerVM.CurrentTrack.Name ?? "";
                tileTextAttributes[1].InnerText = Locator.MusicPlayerVM.CurrentTrack.AlbumName;
                tileTextAttributes[1].InnerText = Locator.MusicPlayerVM.CurrentTrack.ArtistName;
#endif
                var tileImgAttribues = tileXml.GetElementsByTagName("image");
#if WINDOWS_APP
                if (Locator.MusicPlayerVM.CurrentArtist != null)
                    tileImgAttribues[0].Attributes[1].NodeValue = Locator.MusicPlayerVM.CurrentArtist.Picture;
                
                if (Locator.MusicPlayerVM.CurrentAlbum != null)
                    tileImgAttribues[1].Attributes[1].NodeValue = Locator.MusicPlayerVM.CurrentAlbum.AlbumCoverFullUri;
#else
                if (Locator.MusicPlayerVM.CurrentAlbum != null)
                    tileImgAttribues[0].Attributes[1].NodeValue = Locator.MusicPlayerVM.CurrentAlbum.AlbumCoverFullUri;
#endif
            }

            var tileNotification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }

        public static void UpdateMediumTileWithVideoInfo()
        {
            LogHelper.Log("PLAYVIDEO: Updating Live Tile");
            const TileTemplateType template = TileTemplateType.TileSquare150x150PeekImageAndText02;
            var tileXml = TileUpdateManager.GetTemplateContent(template);
            var tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = Strings.NowPlaying;
            if (Locator.VideoPlayerVm.CurrentVideo != null)
            {
                tileTextAttributes[1].InnerText = Locator.VideoPlayerVm.CurrentVideo.Name;

                var tileImgAttribues = tileXml.GetElementsByTagName("image");
                if (Locator.VideoPlayerVm.CurrentVideo != null)
                    tileImgAttribues[0].Attributes[1].NodeValue = Strings.VideoPicFolderPath + Locator.VideoPlayerVm.CurrentVideo.Id + ".jpg";
            }
            var tileNotification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
            UpdateBigTileWithVideoInfo();
        }

        public static void UpdateBigTileWithVideoInfo()
        {
            const TileTemplateType template = TileTemplateType.TileWide310x150PeekImage05;
            var tileXml = TileUpdateManager.GetTemplateContent(template);
            var tileTextAttributes = tileXml.GetElementsByTagName("text");
#if WINDOWS_APP
            tileTextAttributes[0].InnerText = Strings.NowPlaying;
#endif
            if (Locator.VideoPlayerVm.CurrentVideo != null)
            {
                tileTextAttributes[0].InnerText = Locator.VideoPlayerVm.CurrentVideo.Name;
                var tileImgAttribues = tileXml.GetElementsByTagName("image");
                if (Locator.VideoPlayerVm.CurrentVideo != null)
                    tileImgAttribues[0].Attributes[1].NodeValue = Strings.VideoPicFolderPath + Locator.VideoPlayerVm.CurrentVideo.Id + ".jpg";
            }
            var tileNotification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }

        public static async void CreateOrReplaceSecondaryTile(VLCItemType type, int id, string title)
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
                await tileData.RequestCreateAsync();
            }
            else
            {
                SecondaryTile secondaryTile = new SecondaryTile(tileId);
                await secondaryTile.RequestDeleteForSelectionAsync(Window.Current.Bounds, Placement.Default);
                ToastHelper.Basic(Strings.TileRemoved);
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
            catch
            {
            }
        }
    }
}
