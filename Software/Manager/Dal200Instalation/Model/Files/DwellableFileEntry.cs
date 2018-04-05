using System.Collections.Generic;
using FileHelpers;

namespace Dal200Instalation.Model.Files
{
   
    [DelimitedRecord(",")]
    public class DwellableFileEntry
    {
        public int x;
        public int y;

        public string[] media;

        public DwellableFileEntry(int x, int y, string[] media)
        {
            this.x = x;
            this.y = y;
            media = this.media;

        }

        public DwellableFileEntry()
        {

        }
    }
}
