using System.Collections.Generic;
using FileHelpers;

namespace Dal200Instalation.Model.Files
{
   
    [DelimitedRecord(",")]
    public class DwellableFileEntry
    {
        public int x;
        public int y;
        public string label;
        public int type;
        public int page;



        public DwellableFileEntry(int x, int y, string label, int type, int page)
        {
            this.x = x;
            this.y = y;
            label = this.label;
            type = this.type;
            page = this.page;

        }

        public DwellableFileEntry()
        {

        }
    }
}
