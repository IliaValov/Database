using AutoMapper;
using CarDealer.Data;
using CarDealer.Dtos.Import;
using CarDealer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            Mapper.Initialize(x =>
            {

                x.AddProfile<CarDealerProfile>();
            });

            string path = @"D:\Projects\C# Database Advance\CarDealer\Datasets\parts.xml";

            var inputXml = File.ReadAllText(path);

            using (CarDealerContext context = new CarDealerContext())
            {
                Console.WriteLine(ImportParts(context, inputXml));
            }
        }

        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PartDto[]), new XmlRootAttribute("Parts"));

            List<Part> parts = new List<Part>();
            using (var reader = new StringReader(inputXml))
            {

                var xmlUsers = (PartDto[])xmlSerializer.Deserialize(reader);
                foreach (var item in xmlUsers)
                {
                    if (!context.Suppliers.Any(x => x.Id == item.SupplierId) || item.SupplierId == 0)
                    {
                        continue;
                    }

                    var part = Mapper.Map<Part>(item);
                    parts.Add(part);
                }

            }
            context.Parts.AddRange(parts);
            context.SaveChanges();


            return $"Successfully imported {parts.Count}";
        }

        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(SupplierDto[]), new XmlRootAttribute("Suppliers"));


            List<Supplier> suppliers = new List<Supplier>();
            using (var reader = new StringReader(inputXml))
            {

                var xmlUsers = (SupplierDto[])xmlSerializer.Deserialize(reader);
                foreach (var item in xmlUsers)
                {
                    var supplier = Mapper.Map<Supplier>(item);
                    suppliers.Add(supplier);
                }

            }
            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();


            return $"Successfully imported {suppliers.Count}";
        }
    }
}