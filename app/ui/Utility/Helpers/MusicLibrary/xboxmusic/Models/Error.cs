/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;

namespace XboxMusicLibrary.Models
{
    // These error structures can be optionally embedded in any Xbox Music Platform service response. 
    // If an error is reported, the Error.ErrorCode and Error.Description elements will be provided.
    public class Error
    {
        // The error code, as described in the following table of error codes.
        public string ErrorCode { get; set; }
        // A user-friendly description of the error code.
        public string Description { get; set; }
        // A more contextual message describing what may have gone wrong.
        public string Message { get; set; }
    }

//  Name                                        Default HTTP code           Description
//  -----------------------------------------------------------------------------------
//  CATALOG_UNAVAILABLE                         502 Bad Gateway             No response from catalog. 
//  CATALOG_NO_RESULT                           404 Not Found               Item does not exist. 
//  CATALOG_INVALID_DATA                                                    Error while reading catalog data; some results may be missing. 
//  ACCESS_TOKEN_MISSING                        401 Unauthorized            Azure Marketplace access token required. 
//  ACCESS_TOKEN_INVALID                        401 Unauthorized            Invalid Azure Marketplace token. 
//  ACCESS_TOKEN_EXPIRED                        401 Unauthorized            Expired Azure Marketplace token. 
//  ACCESS_TOKEN_VALIDATION_ERROR               500 Internal Server Error   Unexpected error while validating Azure Marketplace token. 
//  ACCESS_TOKEN_INVALID_SUBSCRIPTION           401 Unauthorized            Azure Marketplace client ID is not a subscriber to the Xbox Music data offer. 
//  ACCESS_TOKEN_SUBSCRIPTION_VALIDATION_ERROR  500 Internal Server Error   Unexpected error while validating Azure Marketplace subscription status. 
//  CONTINUATION_TOKEN_INVALID_ERROR            400 Bad Request             The continuation token provided is incorrect. 
//  MISSING_INPUT_PARAMETER                     400 Bad Request             Missing or empty mandatory parameter. 
//  INVALID_INPUT_PARAMETER                     400 Bad Request             Invalid parameter value. 
//  INCOMPATIBLE_INPUT_PARAMETERS               400 Bad Request             Incompatible parameters. 
//  INTERNAL_SERVER_ERROR                       500 Internal Server Error   Oops, something went seriously wrong. 

    public delegate void ErrorEventHandler(object sender, ErrorEventArgs e);

    public class ErrorEventArgs : EventArgs
    {
        private string _errorcode;
        private string _description;
        private string _message;

        public ErrorEventArgs(Error error)
        {
            this._errorcode = error.ErrorCode;
            this._description = error.Description;
            this._message = error.Message;
        }

        public string ErrorCode
        {
            get { return _errorcode; }
        }

        public string Description
        {
            get { return _description; }
        }

        public string Message
        {
            get { return _message; }
        }
    }
}
