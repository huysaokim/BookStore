using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Project_PRN.Models;

namespace Project_PRN.Controllers {
    public class AccountsController : Controller {
        private ProjectPRNEntities3 db = new ProjectPRNEntities3();

        // GET: Accounts
        public ActionResult SignIn() {
            if (Session["user"] == null) {
                return View();
            } else {
                return RedirectToRoute(new {
                    controller = "Home",
                    action = "Index",
                    id = UrlParameter.Optional
                });
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignIn([Bind(Include = "email,password")] Account account) {
            db.Configuration.ProxyCreationEnabled = false;
            if (ModelState.IsValid) {
                string checkEmail = account.email;
                string checkPassword = account.password;

                //get user's information from database
                Account checkAccount = db.Accounts.Where(a => a.email.Equals(checkEmail) && a.role != 0).FirstOrDefault();
                //check is exsisted account
                if (checkAccount != null) {
                    //check if password matches
                    if (BCrypt.Net.BCrypt.Verify(checkPassword, checkAccount.password)) {
                        HttpSessionStateBase session = HttpContext.Session;
                        //add user to session
                        session.Add("user", checkAccount.userID);
                        session.Add("role", checkAccount.role);
                        //reload cart
                        if (Session["cart"] != null) {
                            CartsController cartsController = new CartsController();
                            int userId = Int32.Parse(Session["user"].ToString());
                            Dictionary<string, int> cookieCart = (Dictionary<string, int>)Session["cart"];
                            cartsController.AddToCartWhenLogin(cookieCart, userId);
                            Session.Remove("cart");
                        }
                        return RedirectToRoute(new {
                            controller = "Home",
                            action = "Index",
                            id = UrlParameter.Optional
                        });
                    } else {
                        ViewBag.Message = "Wrong Password!";
                        ViewData["email"] = account.email;
                    }
                } else {
                    ViewBag.Message = "Not exsisted account!";
                }
            }
            return View();
        }

        public ViewResult SignUp() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignUp([Bind(Include = "userID,email,password,userName,role,address,phoneNumber")] Account account) {
            try {
                db.Configuration.ProxyCreationEnabled = false;
                if (ModelState.IsValid) {
                    List<Account> list = db.Accounts.Where(a => a.email.Equals(account.email)).ToList();
                    if (list.Count == 0) {
                        account.role = 0;
                        string pass = account.password;
                        int cost = 12;
                        string newPassword = BCrypt.Net.BCrypt.HashPassword(pass, cost);
                        account.password = newPassword;
                        Guid captcha = Guid.NewGuid();
                        account.captcha = captcha;
                        db.Accounts.Add(account);
                        db.SaveChanges();

                        string content = $"Wellcome to my Bookstore, to active your account," +
                            $" please click here https://localhost:44368/Accounts/ActiveAcount?captcha={captcha}";
                        var senderEmail = new MailAddress("pes2020testing@gmail.com", "Active Account");
                        var receiverEmail = new MailAddress(account.email, "Receiver");
                        var password = "pes2020test";
                        var subject = "Active Account";
                        var body = content;
                        var smtp = new SmtpClient {
                            Host = "smtp.gmail.com",
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential(senderEmail.Address, password)
                        };
                        using (var mess = new MailMessage(senderEmail, receiverEmail) {

                            Subject = subject,
                            Body = body
                        }) {
                            mess.IsBodyHtml = true;
                            smtp.Send(mess);
                        }

                        return RedirectToAction("SignIn");
                    } else {
                        ViewBag.Message = "Exsisted Account! Please register again";
                        ViewData["email"] = account.email;
                        ViewData["userName"] = account.userName;
                        ViewData["phoneNumber"] = account.phoneNumber;
                        ViewData["address"] = account.address;
                    }
                }
                return View();
            } catch (Exception e) {
                return RedirectToRoute(new {
                    controller = "Home",
                    action = "Error",
                    id = UrlParameter.Optional
                });
            }

        }

        public ActionResult ActiveAcount(Guid? captcha) {
            Account ac = db.Accounts.Where(a => a.captcha == captcha).FirstOrDefault();

            //get account with captcha
            if (ac != null) {
                ac.role = 2;
                ac.captcha = null;
                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            return null;
        }

        public ActionResult SignOut() {
            try {
                if (Session["user"] != null) {
                    HttpSessionStateBase session = HttpContext.Session;
                    session.Remove("user");
                    session.Remove("role");
                    if (Session["cart"] != null) {
                        session.Remove("cart");
                    }
                }
            } catch (Exception e) {
                //chuyen toi trang bao loi
            }
            return RedirectToRoute(new {
                controller = "Home",
                action = "Index",
                id = UrlParameter.Optional
            });
        }

        public ActionResult Edit() {
            if (Session["user"] != null && Session["Role"] != null && Session["Role"].ToString().Equals("2")) {
                return View();
            } else {
                return RedirectToAction("SignIn");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "password,userName,address,phoneNumber")] Account account) {
            if (ModelState.IsValid) {
                db.Configuration.ProxyCreationEnabled = false;
                string checkPassword = account.password;
                var userId = Int32.Parse(Session["user"].ToString());
                List<Account> list = db.Accounts.Where(a => a.userID == userId).ToList();
                if (BCrypt.Net.BCrypt.Verify(checkPassword, list[0].password)) {
                    int cost = 12;
                    string newPassword = BCrypt.Net.BCrypt.HashPassword(checkPassword, cost);
                    Account accountUpdated = db.Accounts.Find(userId);
                    accountUpdated.password = newPassword;
                    accountUpdated.userName = account.userName;
                    accountUpdated.address = account.address;
                    accountUpdated.phoneNumber = account.phoneNumber;
                    db.Entry(accountUpdated).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToRoute(new {
                        controller = "Home",
                        action = "Index",
                        id = UrlParameter.Optional
                    });
                } else {
                    return RedirectToAction("Edit");
                }
            }
            return View(account);
        }

        public JsonResult GetAccountInfor() {
            db.Configuration.ProxyCreationEnabled = false;
            if (Session["user"] != null) {
                var userId = Int32.Parse(Session["user"].ToString());
                var infor = db.Accounts.Where(a => a.userID == userId).ToList();
                return Json(infor, JsonRequestBehavior.AllowGet);
            } else {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetCheckOutInfor() {
            db.Configuration.ProxyCreationEnabled = false;
            if (Session["user"] != null) {
                var userId = Int32.Parse(Session["user"].ToString());
                var infor = db.Accounts.Where(a => a.userID == userId).FirstOrDefault();
                string[] nameWords = infor.userName.Split(' ');
                string firstname = nameWords[0];
                string lastname = "";
                for (int i = 1; i < nameWords.Length; i++) {
                    lastname += nameWords[i];
                }
                return Json(new {
                    firstname = firstname,
                    lastname = lastname,
                    phone = infor.phoneNumber,
                    email = infor.email
                }, JsonRequestBehavior.AllowGet);
            } else {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
