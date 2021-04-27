using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Project_PRN.Models;

namespace Project_PRN.Controllers {
    public class EvaluatesController : Controller {
        private ProjectPRNEntities3 db = new ProjectPRNEntities3();

        // GET: Evaluates
        public ActionResult Evaluate(int? productID, long? BillId) {
            db.Configuration.ProxyCreationEnabled = false;
            if (Session["user"] == null) {
                return RedirectToAction("SignIn", "Accounts");
            } else {
                Bill bill = db.Bills.Where(p => p.BillID == BillId).FirstOrDefault();
                if (bill.status == 2) {
                    ViewData["productID"] = productID;
                    return View();
                } else {
                    return RedirectToAction("Index", "Home");
                }
            }
        }


        // POST: Evaluates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "evaluateID,evaluateContent,rate,date,userID,productID")] Evaluate evaluate) {
            db.Configuration.ProxyCreationEnabled = false;
            if (ModelState.IsValid) {
                evaluate.userID = Int32.Parse(Session["user"].ToString());
                evaluate.date = DateTime.Now;
                db.Evaluates.Add(evaluate);
                db.SaveChanges();
                return RedirectToRoute(new {
                    controller = "Home",
                    action = "Index",
                    id = UrlParameter.Optional
                });
            }

            return RedirectToAction("Home");
        }


        protected override void Dispose(bool disposing) {
            if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
