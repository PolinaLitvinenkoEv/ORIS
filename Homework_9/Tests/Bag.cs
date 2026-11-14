using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class Bag
    {
        string Name { get; set; }
        Size Size { get; set; }
        Color Color { get; set; }
        Material Material { get; set; }

        public Bag(string name, Size size, Color color, Material material)
        {
            Name = name;
            Size = size;
            Color = color;
            Material = material;
        }
    }
}
