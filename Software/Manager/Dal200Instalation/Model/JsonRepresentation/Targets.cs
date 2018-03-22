using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dal200Instalation.Model.Dwellable;

namespace Dal200Instalation.Model.JsonRepresentation
{
    class Targets
    {
        public List<DwellableTarget> targets;

        public Targets()
        {
            targets = new List<DwellableTarget>();
        }

    }
}
