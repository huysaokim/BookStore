using PagedList;
using Project_PRN.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Project_PRN.Controllers {
    public class ManagerController : Controller {
        private ProjectPRNEntities3 db = new ProjectPRNEntities3();
        public ActionResult ContactManager() {
            return View();
        }

        public JsonResult ContactJson() {
            db.Configuration.ProxyCreationEnabled = false;
            List<Contact> contacts = db.Contacts.ToList().Select(contact => new Contact {
                userid = contact.userid,
                email = contact.email,
                content = contact.content,
                date = contact.date,
                contactid = contact.contactid,
                subject = contact.subject,
                status = contact.status,
                Account = db.Accounts.Find(contact.userid)
            }).Where(c => c.status == false).ToList();
            return Json(contacts, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AdminBillManager(int? type, DateTime? date) {
            db.Configuration.ProxyCreationEnabled = false;
            if (Session["user"] == null) {
                return RedirectToAction("SignIn", "Accounts");
            } else {
                int userID = Int32.Parse(Session["user"].ToString());
                Account account = db.Accounts.Find(userID);
                if (account.role == 1) {
                    if(type != null) {
                        ViewData["type"] = type;
                    } else {
                        ViewData["type"] = 1;
                    }
                    if (date != null) {
                        ViewData["date"] = date.Value.ToString("yyyy-MM-dd");
                    } else {
                        ViewData["date"] = DateTime.Now.ToString("yyyy-MM-dd");
                    }
                    return View();
                } else {
                    return RedirectToAction("Index", "Home");
                }
            }
        }

        public ActionResult StaffBillManager() {
            db.Configuration.ProxyCreationEnabled = false;
            if (Session["user"] == null) {
                return RedirectToAction("SignIn", "Accounts");
            } else {
                int userID = Int32.Parse(Session["user"].ToString());
                Account account = db.Accounts.Find(userID);
                if (account.role == 3) {
                    return View();
                } else {
                    return RedirectToAction("Index", "Home");
                }
            }
        }

        public ActionResult BillManager() {
            db.Configuration.ProxyCreationEnabled = false;
            if (Session["user"] == null) {
                return RedirectToAction("SignIn", "Accounts");
            } else {
                int userID = Int32.Parse(Session["user"].ToString());
                Account account = db.Accounts.Find(userID);
                if (account.role == 3 || account.role == 2) {
                    return View();
                } else {
                    return RedirectToAction("Index", "Home");
                }
            }
        }

        public ActionResult ReplyContact(int? contactId) {
            ViewData["contactId"] = contactId;
            return View();
        }

        public JsonResult GetContactByID(int? contactId) {
            db.Configuration.ProxyCreationEnabled = false;
            Contact ct = db.Contacts.ToList().Select(contact => new Contact {
                userid = contact.userid,
                email = contact.email,
                content = contact.content,
                date = contact.date,
                contactid = contact.contactid,
                subject = contact.subject,
                status = contact.status,
                Account = db.Accounts.Find(contact.userid)
            }).Where(c => c.contactid == contactId).FirstOrDefault();
            return Json(ct, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReplyContact(string email, string subject, string content, int? contactId) {
            try {
                if (ModelState.IsValid) {
                    MailAddress senderEmail = new MailAddress("pes2020testing@gmail.com", "BookStore");
                    MailAddress receiverEmail = new MailAddress(email, "Receiver");
                    string password = "pes2020test";
                    string sub = subject;
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

                    Contact contact = db.Contacts.Find(contactId);
                    contact.status = true;
                    db.Entry(contact).State = EntityState.Modified;
                    db.SaveChanges();
                }

            } catch (Exception) {
                ViewBag.Message = "Some Error";
                return View();
            }
            return RedirectToAction("ContactManager");
        }

        public ActionResult ChangeStatus(string sBillID, string sType) {
            if (sType.Equals("0")) {
                try {
                    long billId = Convert.ToInt64(sBillID);
                    List<Bill> bill = db.Bills.Where(b => b.BillID == billId).ToList();
                    foreach (Bill b in bill) {
                        b.status = 0;
                        db.Entry(b).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    return RedirectToAction("BillManager");
                } catch {
                    return null;
                }
            } else if (sType.Equals("1")) {
                try {

                    long billId = Convert.ToInt64(sBillID);
                    List<Bill> bill = db.Bills.Where(b => b.BillID == billId).ToList();
                    foreach (Bill b in bill) {
                        b.status = 2;
                        db.Entry(b).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                } catch {
                    return null;
                }
            }
            return null;

        }

        public ActionResult AccountManager(int? page) {
            db.Configuration.ProxyCreationEnabled = false;
            if (Session["user"] == null) {
                return RedirectToAction("SignIn", "Accounts");
            } else {
                int userID = Int32.Parse(Session["user"].ToString());
                Account account = db.Accounts.Find(userID);
                if (account.role == 1) {
                    if (page != null) {
                        ViewData["page"] = page;
                    } else {
                        ViewData["page"] = 1;
                    }
                    return View();
                } else {
                    return RedirectToAction("Index", "Home");
                }
            }
        }

        public JsonResult AccountJson(int? page) {
            db.Configuration.ProxyCreationEnabled = false;
            List<Account> listAccount;
            if (page == null) {
                page = 1;
            }
            int totalPage = 0;
            int pageSize = 6;
            int pageNumber = (page ?? 1);
            listAccount = db.Accounts.ToList().Where(a => a.role > 1).Select(account => new Account {
                userID = account.userID,
                email = account.email,
                password = account.password,
                userName = account.userName,
                role = account.role,
                address = account.address,
                phoneNumber = account.phoneNumber,
            }).OrderByDescending(product => product.role).ToPagedList(pageNumber, pageSize).ToList();
            totalPage = db.Accounts.Where(a => a.role > 1).Count() / pageSize + 1;
            return Json(new {
                totalPage = totalPage,
                pageIndex = page,
                accountList = listAccount
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdminEditProfile([Bind(Include = "userID,email,password,userName,address,phoneNumber")] Account account) {
            Account a = db.Accounts.Find(account.userID);
            a.email = account.email;
            a.userName = account.userName;
            a.address = account.address;
            a.phoneNumber = account.phoneNumber;
            db.Entry(a).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("AccountManager");
        }
    }
}