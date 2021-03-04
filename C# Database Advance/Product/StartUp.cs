using AutoMapper;
using ProductShop.Data;
using ProductShop.Dtos.Export;
using ProductShop.Dtos.Import;
using ProductShop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ProductShop
{
    public class StartUp
    {


        public static void Main(string[] args)
        {
            Mapper.Initialize(x =>
            {

                x.AddProfile<ProductShopProfile>();
            });

            string path = @"C:\Users\Ilia\Desktop\C#\Entity Framework\ProductShop - Skeleton\ProductShop\Datasets\categories-products.xml";

            var inputXml = File.ReadAllText(path);

            using (ProductShopContext context = new ProductShopContext())
            {
                Console.WriteLine(GetUsersWithProducts(context));
            }
        }
        public static string GetUsersWithProducts(ProductShopContext context)
        {


            var input = context.Users
                            .Where(p => p.ProductsSold.Any())
                            .OrderByDescending(u => u.ProductsSold.Count())
                            .Select(x => new ExportGetUsersWithProductsDto
                            {
                                FirstName = x.FirstName,
                                LastName = x.LastName,
                                Age = x.Age,
                                SoldProduts = new SoldProductDto
                                {
                                    Count = x.ProductsSold.Count,
                                    ProductsDto = x.ProductsSold.Select(s => new GetProductDto
                                    {
                                        Name = s.Name,
                                        Price = s.Price
                                    })
                                    .OrderByDescending(p => p.Price)
                                    .ToArray()
                                }
                            })
                            .Take(10)
                            .ToArray();

            var users = new ExportCustomUserXml
            {
                Count = context.Users.Count(u => u.ProductsSold.Any()),
                Users = input
            };

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("","")
            });

            var xmlSearializer = new XmlSerializer(typeof(ExportCustomUserXml), new XmlRootAttribute("Users"));

            var sb = new StringBuilder();

            xmlSearializer.Serialize(new StringWriter(sb), users, namespaces);

            return sb.ToString().TrimEnd();
        }


        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                .Select(x => new ExportGetCategoriesByProductsCountDto
                {
                    Name = x.Name,
                    Count = x.CategoryProducts.Count(),
                    AveragePrice = x.CategoryProducts.Average(a => a.Product.Price),
                    TotalRevenue = x.CategoryProducts.Sum(p => p.Product.Price)
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.TotalRevenue)
                .ToArray();

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("","")
            });

            var xmlSearializer = new XmlSerializer(typeof(ExportGetCategoriesByProductsCountDto[]), new XmlRootAttribute("Categories"));

            var sb = new StringBuilder();

            xmlSearializer.Serialize(new StringWriter(sb), categories, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            var sellers = context.Users
                            .Where(p => p.ProductsSold.Any(x => x.Buyer != null))
                            .Select(x => new ExportGetSoldProductsDto
                            {
                                FirstName = x.FirstName,
                                LastName = x.LastName,
                                Products = x.ProductsSold.Select(b => new GetProductDto
                                {
                                    Name = b.Name,
                                    Price = b.Price,
                                })
                                .ToArray()
                            })
                            .OrderBy(l => l.LastName)
                            .ThenBy(f => f.FirstName)
                            .Take(5)
                            .ToArray();

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                XmlQualifiedName.Empty
            });

            var xmlSearializer = new XmlSerializer(typeof(ExportGetSoldProductsDto[]), new XmlRootAttribute("Users"));

            var sb = new StringBuilder();

            xmlSearializer.Serialize(new StringWriter(sb), sellers, namespaces);

            return sb.ToString().TrimEnd();
        }


        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context.Products
                .Select(x => new ExportGetProductsInRangeDto()
                {
                    Name = x.Name,
                    Price = x.Price,
                    Buyer = x.Buyer.FirstName + " " + x.Buyer.LastName
                })
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .OrderBy(p => p.Price)
                .Take(10)
                .ToArray();



            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("","")
            });

            var xmlSearializer = new XmlSerializer(typeof(ExportGetProductsInRangeDto[]), new XmlRootAttribute("Products"));

            var sb = new StringBuilder();

            xmlSearializer.Serialize(new StringWriter(sb), products, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportCategoryProductDto[]), new XmlRootAttribute("CategoryProducts"));

            using (var reader = new StringReader(inputXml))
            {
                List<CategoryProduct> categoriesProducts = new List<CategoryProduct>();

                var importCategoriesProducts = (ImportCategoryProductDto[])xmlSerializer.Deserialize(reader);

                foreach (var currentCategoryProduct in importCategoriesProducts)
                {
                    if (!context.Products.Any(x => x.Id == currentCategoryProduct.ProductId) ||
                        !context.Categories.Any(x => x.Id == currentCategoryProduct.CategoryId))
                    {
                        continue;
                    }
                    var categoryProduct = Mapper.Map<CategoryProduct>(currentCategoryProduct);
                    categoriesProducts.Add(categoryProduct);
                }

                context.CategoryProducts.AddRange(categoriesProducts);
                context.SaveChanges();

                return $"Successfully imported {categoriesProducts.Count}";

            }
        }
        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportCategoryDto[]), new XmlRootAttribute("Categories"));

            List<Category> categories = new List<Category>();
            using (var reader = new StringReader(inputXml))
            {

                var importCategories = (ImportCategoryDto[])xmlSerializer.Deserialize(reader);
                foreach (var currentCategory in importCategories)
                {
                    var category = Mapper.Map<Category>(currentCategory);
                    categories.Add(category);
                }

            }
            context.Categories.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Count}";
        }

        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportProductDto[]), new XmlRootAttribute("Products"));


            List<Product> products = new List<Product>();
            using (var reader = new StringReader(inputXml))
            {

                var importProduct = (ImportProductDto[])xmlSerializer.Deserialize(reader);
                foreach (var currentProduct in importProduct)
                {
                    var product = Mapper.Map<Product>(currentProduct);
                    products.Add(product);
                }

            }
            context.Products.AddRange(products);
            context.SaveChanges();


            return $"Successfully imported {products.Count}";
        }

        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportUserDto[]), new XmlRootAttribute("Users"));


            List<User> users = new List<User>();
            using (var reader = new StringReader(inputXml))
            {

                var xmlUsers = (ImportUserDto[])xmlSerializer.Deserialize(reader);
                foreach (var item in xmlUsers)
                {
                    var user = Mapper.Map<User>(item);
                    users.Add(user);
                }

            }
            context.Users.AddRange(users);
            context.SaveChanges();


            return $"Successfully imported {users.Count}";

        }
    }
}