using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace BankSync.Model
{
    public class Subcategory
    {
        public Subcategory(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }
    }

    public class Category
    {
        public Category(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }
        public List<Subcategory> Subcategories { get; set; } = new List<Subcategory>();
    }
}