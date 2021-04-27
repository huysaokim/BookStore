using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using IdGen;
using Microsoft.Ajax.Utilities;
using Project_PRN.Models;

namespace Project_PRN.Controllers {
    public class BillsController : Controller {
        private ProjectPRNEntities3 db = new ProjectPRNEntities3();

        public ActionResult CheckOut() {
            return View();
        }

        public ActionResult Bill() {
            return View();
        }

        public JsonResult GetLocalJson() {
            string json = "";
            using (StreamReader r = new StreamReader(Path.Combine(Server.MapPath("~/Content/data.json")))) {

                json = r.ReadToEnd();
            }
            Console.WriteLine(json);
            return Json(json, JsonRequestBehavior.AllowGet);
        }


        public JsonResult AddBill(string name, string address, string phone, string email, int payment) {
            try {
                Bill bill = new Bill();

                //Declare snowflake algorithm
                DateTime epoch = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                //41 bit for time
                //12 bit for number of shard
                //10 bit for sequence
                IdStructure structure = new IdStructure(41, 12, 10);
                IdGeneratorOptions option = new IdGeneratorOptions(structure, new DefaultTimeSource(epoch));
                IdGenerator generator = new IdGenerator(0, option);

                //create new id and declare properties to bill
                long billID = generator.CreateId();

                bill.BillID = billID;
                bill.userName = name;
                bill.address = address;
                bill.phoneNumber = phone;
                bill.email = email;
                bill.payment = payment;
                bill.status = 1;
                bill.orderTime = DateTime.Now;

                //email content
                string content = $"You have successfully booked your order, the ready-made product will be delivered within {DateTime.Now.AddDays(3).Date} to {DateTime.Now.AddDays(7).Date}<br/><br/>";
                content += $"Order ID: {billID}<br/><br/>";
                //head row of table
                content += "<table><tr style=\"color: white; background-color: #7fad39;\"><td style=\"padding: 5px 10px 5px 10px; font-size: 15px;\">Product</td><td style=\"padding: 5px 10px 5px 10px; font-size: 15px;\">Price</td><td style=\"padding: 5px 10px 5px 10px; font-size: 15px;\">Quantity</td><td style=\"padding: 5px 10px 5px 10px; font-size: 15px;\">Total</td></tr>";
                decimal totalValue = 0;

                //is loged User
                if (Session["user"] == null) {
                    //didn't log in case, storage cart in cookies

                    Dictionary<string, int> cart;

                    //check is exsisted cart in cookies
                    if (Session["cart"] != null) {
                        //exsisted case, pick up it
                        cart = (Dictionary<string, int>)Session["cart"];

                        Dictionary<string, int>.KeyCollection keys = cart.Keys;


                        //add bill to database
                        foreach (string key in keys) {
                            bill.productid = Int32.Parse(key);
                            bill.quantity = cart[key];
                            Product p = db.Products.Find(bill.productid = Int32.Parse(key));
                            bill.amount = p.price;

                            //add into bill
                            db.Bills.Add(bill);
                            db.SaveChanges();

                            //add middle row of table
                            decimal total = p.price * cart[key];
                            content += $"<tr style=\"background - color: #eeeeee;\"><td style=\"padding: 5px 10px 5px 10px; font-size: 15px;\">{p.title}</td><td style=\"padding: 5px 10px 5px 10px; font-size: 15px;\">{p.price.ToString("C")}</td><td style=\"padding: 5px 10px 5px 10px; font-size: 15px;\">{cart[key]}</td><td style=\"padding: 5px 10px 5px 10px; font-size: 15px;\">{total.ToString("C")}</td>";
                            totalValue += total;
                        }
                        Session.Remove("cart");
                    } else {
                        //in null case of cart
                        return Json("Please put item into cart before check out!", JsonRequestBehavior.AllowGet);
                    }

                } else {
                    //loged in case
                    Account account = db.Accounts.Find(Int32.Parse(Session["user"].ToString()));
                    bill.userid = account.userID;

                    //load cart infor from database
                    List<Cart> carts = db.Carts.ToList().Select(cart => new Cart {
                        cartid = cart.cartid,
                        userid = cart.userid,
                        productid = cart.productid,
                        quantity = cart.quantity,
                        Account = db.Accounts.Find(cart.userid),
                        Product = db.Products.Find(cart.productid)
                    }).Where(c => c.userid == account.userID).ToList();

                    //check is exsisted any item in cart in database
                    if (carts.Count > 0) {
                        foreach (Cart cart in carts) {
                            bill.productid = cart.productid;
                            bill.quantity = cart.quantity;
                            bill.amount = cart.Product.price;

                            //add bill
                            db.Bills.Add(bill);

                            //add middle row of table
                            decimal total = cart.Product.price * cart.quantity;
                            content += $"<tr style=\"background - color: #eeeeee;\"><td style=\"padding: 5px 10px 5px 10px; font-size: 15px;\">{cart.Product.title}</td><td style=\"padding: 5px 10px 5px 10px; font-size: 15px;\">{cart.Product.price.ToString("C")}</td><td style=\"padding: 5px 10px 5px 10px; font-size: 15px;\">{cart.quantity}</td><td style=\"padding: 5px 10px 5px 10px; font-size: 15px;\">{total.ToString("C")}</td>";
                            totalValue += total;

                            //remove item from cart
                            db.Carts.Remove(db.Carts.Find(cart.cartid));
                            db.SaveChanges();
                        }
                    } else {
                        //in null case of cart
                        return Json(new {
                            type = 2,
                            message = "Please put item into cart before check out!"
                        }, JsonRequestBehavior.AllowGet);
                    }
                }

                Session["tempBillID"] = billID;

                //last row of table
                content += $"<tr style=\"background-color: #F5F5F5;\"><td style=\"padding: 5px 10px 5px 10px; font-size: 15px;\"></td><td style=\"padding: 5px 10px 5px 10px; font-size: 15px;\"></td><td style=\"padding: 5px 10px 5px 10px; font-size: 15px;\">Total order value</td><td style=\"padding: 5px 10px 5px 10px; font-size: 15px;\">{totalValue.ToString("C")}</td></tr></table>";

                try {
                    //send email to user
                    MailAddress senderEmail = new MailAddress("pes2020testing@gmail.com", "BookStore");
                    MailAddress receiverEmail = new MailAddress(email, "Receiver");
                    string password = "pes2020test";
                    string subject = "Order successfull";
                    string body = content;
                    SmtpClient smtp = new SmtpClient {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(senderEmail.Address, password)
                    };
                    using (MailMessage mess = new MailMessage(senderEmail, receiverEmail) {

                        Subject = subject,
                        Body = body
                    }) {
                        mess.IsBodyHtml = true;
                        smtp.Send(mess);
                    }

                    return Json(new {
                        type = 1,
                        message = "Check Out Success!"
                    }, JsonRequestBehavior.AllowGet);
                } catch {
                    return Json(new {
                        type = 2,
                        message = "Send email fail!"
                    }, JsonRequestBehavior.AllowGet);
                }
                
            } catch {
                return Json(new {
                    type = 2,
                    message = "Check Out Fail!"
                }, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult AdminBillManagerJson(int? type, DateTime? date) {
            db.Configuration.ProxyCreationEnabled = false;
            int success = 0;
            int pending = 0;
            int cancel = 0;
            decimal total = 0;
            List<Bill> listBill = db.Bills.ToList().Select(Bill => new Bill {
                BillID = Bill.BillID,
                quantity = Bill.quantity,
                orderTime = Bill.orderTime,
                amount = Bill.amount,
                status = Bill.status,
                userName = Bill.userName,
                Account = db.Accounts.Find(Bill.userid),
                Product = db.Products.ToList().Select(product => new Product {
                    productID = product.productID,
                    title = product.title,
                    author = product.author,
                    description = product.description,
                    shortDescription = product.shortDescription,
                    image = product.fullImagePath(),
                    price = product.price,
                    quantity = product.quantity,
                    sold = product.sold,
                    postTime = product.postTime,
                    categoriesID = product.categoriesID,
                    userID = product.userID,
                }).Where(p => p.productID == Bill.productid).FirstOrDefault(),
            }).Where(b => b.orderTime >= date.Value.AddDays(-1) && b.orderTime < date.Value.AddDays(1) && b.status == type).ToList();
            success = listBill.Where(b => b.status == 2).Count();
            pending = listBill.Where(b => b.status == 1).Count();
            cancel = listBill.Where(b => b.status == 0).Count();
            total = listBill.Sum(b => b.amount);
            return Json(new {
                listBill = listBill,
                success = success,
                pending = pending,
                cancel = cancel,
                total = total.ToString("C")
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BillManagerJson() {
            int userId = Int32.Parse(Session["user"].ToString());
            db.Configuration.ProxyCreationEnabled = false;

            var listBill = new ArrayList();

            List<Bill> listBillID = db.Bills.DistinctBy(b => b.BillID).Where(b => b.userid == userId).ToList();

            foreach (Bill b in listBillID) {
                long id = b.BillID;
                int status = b.status;
                List<Bill> bills = db.Bills.ToList().Select(Bill => new Bill {
                    BillID = Bill.BillID,
                    quantity = Bill.quantity,
                    orderTime = Bill.orderTime,
                    amount = Bill.amount,
                    status = Bill.status,
                    userName = Bill.userName,
                    Product = db.Products.ToList().Select(product => new Product {
                        productID = product.productID,
                        title = product.title,
                        author = product.author,
                        description = product.description,
                        shortDescription = product.shortDescription,
                        image = product.fullImagePath(),
                        price = product.price,
                        quantity = product.quantity,
                        sold = product.sold,
                        postTime = product.postTime,
                        categoriesID = product.categoriesID,
                        userID = product.userID,
                    }).Where(p => p.productID == Bill.productid).FirstOrDefault(),
                }).Where(c => c.BillID == id).ToList();

                listBill.Add(new {
                    ID = id,
                    bills = bills,
                    status = status
                });
            }

            return Json(listBill, JsonRequestBehavior.AllowGet);
        }

        public JsonResult StaffBillManagerJson(long? billId) {
            db.Configuration.ProxyCreationEnabled = false;

            if (billId != null) {
                List<Bill> bills = db.Bills.ToList().Select(Bill => new Bill {
                    BillID = Bill.BillID,
                    quantity = Bill.quantity,
                    orderTime = Bill.orderTime,
                    amount = Bill.amount,
                    userName = Bill.userName,
                    status = Bill.status,
                    Product = db.Products.ToList().Select(product => new Product {
                        productID = product.productID,
                        title = product.title,
                        author = product.author,
                        description = product.description,
                        shortDescription = product.shortDescription,
                        image = product.fullImagePath(),
                        price = product.price,
                        quantity = product.quantity,
                        sold = product.sold,
                        postTime = product.postTime,
                        categoriesID = product.categoriesID,
                        userID = product.userID,
                    }).Where(p => p.productID == Bill.productid).FirstOrDefault(),
                }).Where(c => c.BillID == billId).ToList();

                if (bills.Count > 0) {
                    return Json(new {
                        ID = billId,
                        bills = bills,
                        status = bills[0].status,
                        type = 1
                    }, JsonRequestBehavior.AllowGet);
                } else {
                    return Json(new {
                        message = "Not Found!",
                        type = 2
                    }, JsonRequestBehavior.AllowGet);
                }
            } else {
                return Json(new {
                    message = "BillID is null or wrong format!",
                    type = 2
                }, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult GetCurrentBill() {
            long billID = Convert.ToInt64(Session["tempBillID"].ToString());

            db.Configuration.ProxyCreationEnabled = false;
            List<Bill> listBill = db.Bills.ToList().Select(Bill => new Bill {
                BillID = Bill.BillID,
                quantity = Bill.quantity,
                orderTime = Bill.orderTime,
                amount = Bill.amount,
                status = Bill.status,
                userName = Bill.userName,
                Account = db.Accounts.Find(Bill.userid),
                Product = db.Products.ToList().Select(product => new Product {
                    productID = product.productID,
                    title = product.title,
                    author = product.author,
                    description = product.description,
                    shortDescription = product.shortDescription,
                    image = product.fullImagePath(),
                    price = product.price,
                    quantity = product.quantity,
                    sold = product.sold,
                    postTime = product.postTime,
                    categoriesID = product.categoriesID,
                    userID = product.userID,
                }).Where(p => p.productID == Bill.productid).FirstOrDefault(),
            }).Where(b => b.BillID == billID).ToList();

            Session.Remove("tempBillID");
            return Json(listBill, JsonRequestBehavior.AllowGet);

        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
