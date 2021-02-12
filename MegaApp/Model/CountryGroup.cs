using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaApp
{
    public class CategoryGroup : ObservableCollection<ProductGroup>
    {
        public CategoryGroup(IEnumerable<ProductGroup> items)
            : base(items)
        {
        }

        public string Category { get; set; }
    }

}
