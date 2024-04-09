using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCMDump
{
    public class GlobalVars : ObservableObject
    {
        public static GlobalVars Configs = new();

        public bool DownloadCoverImage
        {
            get;
            set;
        } = false;

        public bool DownloadLyric 
        {
            get;
            set;
        } = false;

        private GlobalVars() { }

    }
}
