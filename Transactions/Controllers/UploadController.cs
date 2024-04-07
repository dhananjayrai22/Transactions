using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Util;
using System.Xml.Linq;
using Transactions.Models;

namespace Transactions.Controllers
{
    public class UploadController : Controller
    {
        private readonly AssignmentDbEntities _context;

        public UploadController()
        {
            _context = new AssignmentDbEntities(); // Initialize _context
        }

        

        [HttpGet]
        public ActionResult UploadFile()
        {
            return View();
        }
        //[HttpPost]
        //public ActionResult UploadFile(HttpPostedFileBase file)
        //{
        //    try
        //    {
        //        if (file.ContentLength > 0)
        //        {
        //            string _FileName = Path.GetFileName(file.FileName);
        //            string _path = Path.Combine(Server.MapPath("~/UploadedFiles"), _FileName);
        //            file.SaveAs(_path);
        //        }
        //        ViewBag.Message = "File Uploaded Successfully!!";
        //        return View();
        //    }
        //    catch
        //    {
        //        ViewBag.Message = "File upload failed!!";
        //        return View();
        //    }
        //}

        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                return View("No file selected.");
            }

            if (file.ContentType != "text/csv" && file.ContentType != "text/xml")
            {
                return View("Invalid file format. Please upload a CSV or XML file.");
            }

            if (file.ContentLength > 1 * 1024 * 1024) // 1 MB
            {
                return View("File size exceeds the maximum limit of 1 MB.");
            }

            using (var reader = new StreamReader(file.InputStream))
            {
                if (file.ContentType == "text/csv")
                {
                    
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        var records = csv.GetRecords<Transaction>();
                        foreach (var record in records)
                        {
                            
                            if (string.IsNullOrEmpty(record.Id) ||
                                record.Amount == 0 ||
                                string.IsNullOrEmpty(record.CurrencyCode) ||
                                record.TransactionDate == default(DateTime) ||
                                string.IsNullOrEmpty(record.Status))
                            {
                                return View("Invalid record format.");
                            }

                            var transaction = new Transaction
                            {
                                Id = record.Id,
                                Amount = record.Amount,
                                CurrencyCode = record.CurrencyCode,
                                TransactionDate = record.TransactionDate,
                                Status = record.Status
                            };

                            _context.Transactions.Add(transaction);
                        }

                        _context.SaveChanges();
                    }
                }
                
                    
                    else if (file.ContentType == "text/xml")
                    {
                        
                        try
                        {
                            XDocument xmlDoc = XDocument.Load(reader);

                           
                            foreach (var element in xmlDoc.Descendants("Transaction"))
                            {
                                var transaction = new Transaction
                                {
                                    Id = element.Element("Id")?.Value,
                                    Amount = Convert.ToDecimal(element.Element("Amount")?.Value),
                                    CurrencyCode = element.Element("CurrencyCode")?.Value,
                                    TransactionDate = Convert.ToDateTime(element.Element("TransactionDate")?.Value),
                                    Status = element.Element("Status")?.Value
                                };

                                
                                if (string.IsNullOrEmpty(transaction.Id) ||
                                    transaction.Amount == 0 ||
                                    string.IsNullOrEmpty(transaction.CurrencyCode) ||
                                    transaction.TransactionDate == default(DateTime) ||
                                    string.IsNullOrEmpty(transaction.Status))
                                {
                                    return View("Invalid record format.");
                                }

                                _context.Transactions.Add(transaction);
                            }

                            _context.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            return View("Error processing XML file: " + ex.Message);
                        }
                    }
                }
            
            

            return View();
        }

    }
}