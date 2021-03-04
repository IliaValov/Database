using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ProductShop.Dtos.Export
{
    [XmlType("Product")]
    public class ExportGetProductsInRangeDto
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("price")]
        public decimal Price { get; set; }

        [XmlElement("buyer")]
        public string Buyer { get; set; }

        //<Product>
        //    <name>Care One Hemorrhoidal</name>
        //    <price>932.18</price>
        //    <sellerId>25</sellerId>
        //    <buyerId>24</buyerId>
        //</Product>
    }
}
