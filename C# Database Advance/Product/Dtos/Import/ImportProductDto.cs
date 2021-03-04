using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ProductShop.Dtos.Import
{
    [XmlType("Product")]
    public class ImportProductDto
    {
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "price")]
        public decimal Price { get; set; }

        [XmlElement(ElementName = "sellerId")]
        public int SellerId { get; set; }

        [XmlElement(ElementName = "buyerId")]
        public int? BuyerId { get; set; }

        //<Product>
        //    <name>Care One Hemorrhoidal</name>
        //    <price>932.18</price>
        //    <sellerId>25</sellerId>
        //    <buyerId>24</buyerId>
        //</Product>
    }
}
