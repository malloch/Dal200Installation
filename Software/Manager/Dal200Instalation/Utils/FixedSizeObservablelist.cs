using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dal200Instalation.Utils
{
    public class FixedSizeObservablelist<T> : MTObservableCollection<T>
    {
        private readonly int capacity;

        public new void Add(T item)
        {
            if (Count == capacity)
            {
                RemoveAt(0);
            }
            base.Add(item);
        }

        public FixedSizeObservablelist(int cap) : base()
        {
            capacity = cap;

        }
    }
}
