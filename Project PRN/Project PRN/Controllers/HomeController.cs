using Project_PRN.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project_PRN.Controllers {
    public class HomeController : Controller {
        private ProjectPRNEntities3 db = new ProjectPRNEntities3();
        public ViewResult Index() {

            return View();
        }

        public ViewResult Error() {

            return View();
        }

        public ActionResult Contact() {

            if (Session["user"] != null) {
                return View();
            } else {
                return RedirectToRoute(new {
                    controller = "Accounts",
                    action = "SignIn",
                    id = UrlParameter.Optional
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact([Bind(Include = "userid, email, content, date, contactid, subject")] Contact contact) {
            try {
                db.Configuration.ProxyCreationEnabled = false;

                if (ModelState.IsValid) {
                    //check is fill all
                    if (contact.email != null && !contact.email.Equals("")
                        && contact.subject != null && !contact.subject.Equals("")
                        && contact.content != null && !contact.content.Equals("")) {
                        var userId = Int32.Parse(Session["user"].ToString());
                        Account currAccount = db.Accounts.Find(userId);
                        contact.userid = userId;
                        contact.date = DateTime.Now;
                        contact.Account = currAccount;
                        contact.status = false;
                        db.Contacts.Add(contact);
                        db.SaveChanges();
                        return RedirectToRoute(new {
                            controller = "Home",
                            action = "Index",
                            id = UrlParameter.Optional
                        });
                    } else {
                        ViewBag.Message = "Please Fill all input with valid value!";
                        return View();
                    }
                } else {
                    ViewBag.Message = "An error happen when mapping model!";
                    return View();
                }
                
            } catch (Exception e) {
                return RedirectToAction("Error");
            }

        }

    }
}