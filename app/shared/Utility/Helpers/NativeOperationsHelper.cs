/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.ServiceModel.Security;
using Windows.Storage;

namespace VLC_WINRT.Utility.Helpers
{
    internal class NativeOperationsHelper
    {
        // ReSharper disable InconsistentNaming
        /// <summary>
        /// La déclaration de l'API GetFileAttributesEx
        /// </summary>
        /// <param name="lpFileName">Le nom du fichier recherché</param>
        /// <param name="fInfoLevelId">Quelles types d'info on recherche</param>
        /// <param name="lpFileInformation">Structure contenant les informations une fois l'API appelée</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetFileAttributesEx(
            string lpFileName,
            GET_FILEEX_INFO_LEVELS fInfoLevelId,
            out WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);

        /// <summary>
        /// Type d'infos recherchées
        /// </summary>
        public enum GET_FILEEX_INFO_LEVELS
        {
            /// <summary>
            /// C'est la seule valeur autorisée avec WinRT
            /// </summary>
            GetFileExInfoStandard,
            /// <summary>
            /// Ne fonctionne pas sous WinRT => marqué Obsolete
            /// </summary>
            [Obsolete]
            GetFileExMaxInfoLevel
        }

        /// <summary>
        /// Structure contenant les informations sur le fichier
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct WIN32_FILE_ATTRIBUTE_DATA
        {
            public FileAttributes dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
        }

        // quelques valeurs d'erreurs succeptibles d'arriver
        private const int ERROR_FILE_NOT_FOUND = 2;
        private const int ERROR_PATH_NOT_FOUND = 3;
        private const int ERROR_ACCESS_DENIED = 5;

        public static bool FileExist(string fileName)
        {
            WIN32_FILE_ATTRIBUTE_DATA fileData;
            if (GetFileAttributesEx(fileName, GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out fileData))
                return true;

            // récupération de la dernière erreur (in the same thread of course ;-))
            var lastError = Marshal.GetLastWin32Error();
            if (lastError == ERROR_FILE_NOT_FOUND || lastError == ERROR_PATH_NOT_FOUND) return false;
            // si c'est pas un fichier non trouvé, on lance une exception
            if (lastError == ERROR_ACCESS_DENIED)
                throw new SecurityAccessDeniedException("Accès interdit");

            throw new InvalidOperationException(string.Format("Erreur pendant l'accès au fichier {0}, code {1}", fileName, lastError));
        }

    }
}
