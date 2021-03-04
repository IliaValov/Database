using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;

namespace CarDealer.Dtos.Import
{
    [XmlType("Supplier")]
    public class SupplierDto
    {
        //    <Supplier>
        //    <name>3M Company</name>
        //    <isImporter>true</isImporter>
        //</Supplier>

        [Required]
        [XmlElement("name")]
        public string Name { get; set; }

        [Required]
        [XmlElement("isImporter")]
        public bool IsImporter { get; set; }
    }
}
