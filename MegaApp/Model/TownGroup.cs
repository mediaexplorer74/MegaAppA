using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaApp
{
    public class ProductGroup : ObservableCollection<SourceData>
    {
        public ProductGroup(IEnumerable<SourceData> items)
            : base(items)
        {
        }

        public string Product { get; set; }
    }

}
