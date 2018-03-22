using System.Collections.Generic;
using FileHelpers;

namespace Dal200Instalation.Model.Files
{
   
    [DelimitedRecord(",")]
    public class DwellableFileEntry
    {
        public int x;
        public int y;

        public DwellableFileEntry(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public DwellableFileEntry()
        {

        }
    }
}
