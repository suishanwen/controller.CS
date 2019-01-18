using System;
using System.Collections.Generic;
using System.Text;

namespace controller.util
{
    class BlackMap
    {
        private string name;
        private double price;

        public string Name { get => name; set => name = value; }
        public double Price { get => price; set => price = value; }

        public BlackMap() { }

        public BlackMap(string name, double price)
        {
            this.name = name;
            this.price = price;
        }
    }
}
