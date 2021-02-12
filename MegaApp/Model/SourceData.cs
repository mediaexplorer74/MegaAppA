using MegaApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegaApp
{
    public class SourceData
    {
        public string Category { get; set; }

        public string Product { get; set; }

        public string Address { get; set; }

        // static
        public IEnumerable<SourceData> GetData()
        {
           
            /*
            var handMadeData = new[]
            {
                new SourceData { Category = "Group", Product = "Item", Address = "-" },
                //new SourceData { Category = "Games", Product = "Doom 2", Address = "-" },
                //new SourceData { Category = "Games", Product = "Doom 3", Address = "-" },
                //new SourceData { Category = "Multimedia Apps", Product = "VLC", Address = "-" },
            };
            */


                      
            var generatedData =
                from i in Enumerable.Range(0, Contact.MegaCount)
                select new SourceData
                {
                    Category = Contact.MegaFCategory[i],
                    Product = Contact.MegaFName[i],
                    Address = i.ToString()
                };


            return generatedData; //handMadeData.Concat(generatedData);
        }

        public static IEnumerable<SourceData> GetData2()
        {
            var handMadeData = new[]
            {
                new SourceData { Category = "Group", Product = "Doom 1", Address = "-" },
                new SourceData { Category = "Games", Product = "Doom 2", Address = "-" },
                new SourceData { Category = "Games", Product = "Doom 3", Address = "-" },
                new SourceData { Category = "Multimedia Apps", Product = "VLC", Address = "-" },
            };

            /*
            var r = new Random(1);
            var generatedData =
                from i in Enumerable.Range(0, 1000)
                select new SourceData
                {
                    Category = "Category " + r.Next(1, 9),
                    Product = "Product " + r.Next(1, 9),
                    Address = "Address " + i
                };
            */

            return handMadeData;//.Concat(generatedData);
        }

    }
}
