using System;
using System.Collections.Generic;
using System.Text;

namespace VLC_WINRT_APP.Model.Search
{
    public class SearchResult
    {
        private string _text;
        private string _picture;
        private int _id;
        private VLCItemType _searchItemType;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public string Picture
        {
            get { return _picture; }
            set { _picture = value; }
        }

        public VLCItemType SearchItemType
        {
            get { return _searchItemType; }
            set { _searchItemType = value; }
        }

        public SearchResult(string text, string pic, VLCItemType itemType, int? id = null)
        {
            Picture = pic;
            Text = text;
            SearchItemType = itemType;
            if (id != null) 
                Id = id.Value;
        }
    }
}
