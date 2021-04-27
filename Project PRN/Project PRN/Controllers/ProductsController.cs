using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Project_PRN.Models;
using System.Configuration;

namespace Project_PRN.Controllers {
    public class ProductsController : Controller {
        private ProjectPRNEntities3 db = new ProjectPRNEntities3();

        // GET: Products
        public ActionResult Product(int? categoryID, int? page, string searchKey) {
            if (categoryID != null) {
                ViewData["categoryID"] = categoryID;
            }
            if (page != null) {
                ViewData["page"] = page;
            } else {
                ViewData["page"] = 1;
            }

            if (searchKey != null) {
                ViewData["searchKey"] = searchKey;
            }
            return View();
        }


        public JsonResult SearchAutocomplete(string keyword) {
            db.Configuration.ProxyCreationEnabled = false;

            List<Product> data = db.Products.ToList().Select(product => new Product {
                productID = product.productID,
                title = product.title,

                image = product.fullImagePath(),


            }).Where(p => p.title.ToLower().StartsWith(keyword.ToLower())).OrderBy(x => x.title).ToList();
            return Json(new {
                data = data,
                status = true
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult HomeProductJson() {
            db.Configuration.ProxyCreationEnabled = false;
            List<Product> listProduct = db.Products.ToList().Select(product => new Product {
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
                status = product.status,
                rate = product.calculateRate(),
                Account = db.Accounts.Find(product.userID),
                Category = db.Categories.Find(product.categoriesID),
                Evaluates = db.Evaluates.Where(e => e.productID == product.productID).ToList()

            }).Where(p => p.status == true).OrderByDescending(product => product.postTime).Take(8).ToList();
            return Json(listProduct, JsonRequestBehavior.AllowGet);

        }
        public JsonResult HomeRateProductJson() {
            db.Configuration.ProxyCreationEnabled = false;
            List<Product> listProduct = db.Products.ToList().Select(product => new Product {
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
                status = product.status,
                rate = product.calculateRate(),
                Account = db.Accounts.Find(product.userID),
                Category = db.Categories.Find(product.categoriesID),
                Evaluates = db.Evaluates.Where(ev => ev.productID == product.productID).ToList()
            }).Where(p => p.status == true).OrderByDescending(product => product.rate).Take(8).ToList();
            return Json(listProduct, JsonRequestBehavior.AllowGet);

        }

        public JsonResult ProductJson(int? page, int? categoryID, string searchKey) {
            db.Configuration.ProxyCreationEnabled = false;
            List<Product> listProduct;
            if (page == null) {
                page = 1;
            }

            int totalPage = 0;

            if (categoryID == null) {
                int pageSize = 9;
                int pageNumber = (page ?? 1);

                listProduct = db.Products.ToList().Select(product => new Product {
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
                    status = product.status,
                    rate = product.calculateRate(),
                    Account = db.Accounts.Find(product.userID),
                    Category = db.Categories.Find(product.categoriesID),
                    Evaluates = db.Evaluates.Where(e => e.productID == product.productID).ToList()

                }).OrderBy(product => product.postTime).ToPagedList(pageNumber, pageSize).ToList();
                totalPage = db.Products.Count() / pageSize + 1;

                if (!String.IsNullOrEmpty(searchKey)) {
                    pageSize = 9;
                    pageNumber = (page ?? 1);


                    listProduct = db.Products.ToList().Select(product => new Product {
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
                        status = product.status,
                        rate = product.calculateRate(),
                        Account = db.Accounts.Find(product.userID),
                        Category = db.Categories.Find(product.categoriesID),
                        Evaluates = db.Evaluates.Where(e => e.productID == product.productID).ToList()

                    }).Where(s => s.title.ToLower().StartsWith(searchKey.ToLower())).OrderBy(product => product.title).ToPagedList(pageNumber, pageSize).ToList();
                    totalPage = db.Products.Where(s => s.title.StartsWith(searchKey)).Count() / pageSize + 1;
                }

            } else {

                int pageSize = 9;
                int pageNumber = (page ?? 1);


                listProduct = db.Products.ToList().Select(product => new Product {
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
                    status = product.status,
                    rate = product.calculateRate(),
                    Account = db.Accounts.Find(product.userID),
                    Category = db.Categories.Find(product.categoriesID),
                    Evaluates = db.Evaluates.Where(e => e.productID == product.productID).ToList()

                }).Where(p => p.categoriesID == categoryID).OrderBy(product => product.postTime).ToPagedList(pageNumber, pageSize).ToList();
                totalPage = db.Products.Where(p => p.categoriesID == categoryID).Count() / pageSize + 1;
                if (!String.IsNullOrEmpty(searchKey)) {
                    pageSize = 9;
                    pageNumber = (page ?? 1);


                    listProduct = db.Products.ToList().Select(product => new Product {
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
                        status = product.status,
                        rate = product.calculateRate(),
                        Account = db.Accounts.Find(product.userID),
                        Category = db.Categories.Find(product.categoriesID),
                        Evaluates = db.Evaluates.Where(e => e.productID == product.productID).ToList()

                    }).Where(s => s.categoriesID == categoryID && s.title.ToLower().StartsWith(searchKey.ToLower())).OrderBy(product => product.title).ToPagedList(pageNumber, pageSize).ToList();
                    totalPage = db.Products.Where(s => s.categoriesID == categoryID && s.title.ToLower().StartsWith(searchKey.ToLower())).Count() / pageSize + 1;

                }

            }


            return Json(new {
                totalPage = totalPage,
                pageIndex = page,
                categoryID = categoryID,
                productList = listProduct
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UploadProduct() {
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "productID,title,author,description,shortDescription,image,price,quantity,sold,postTime,categoriesID,userID")] Product product, HttpPostedFileBase image) {
            if (ModelState.IsValid) {
                product.userID = Int32.Parse(Session["user"].ToString());
                product.postTime = DateTime.Now;
                product.status = true;
                string imgPath = ConfigurationManager.ConnectionStrings["imagePath"].ToString();
                try {
                    if (image != null) {
                        var allowedExtensions = new[] { ".Jpg", ".png", ".jpg", "jpeg" };
                        var ext = Path.GetExtension(image.FileName);
                        if (allowedExtensions.Contains(ext)) //check what type of extension  
                        {
                            string path = Path.Combine(Server.MapPath($"~{imgPath}"), Path.GetFileName(image.FileName));
                            image.SaveAs(path);
                            product.image = image.FileName;
                        } else { return RedirectToAction("UploadProduct", "Products"); }
                    } else {
                        return RedirectToAction("UploadProduct", "Products");
                    }
                    ViewBag.FileStatus = "File uploaded successfully.";
                } catch (Exception) {

                }
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToRoute(new {
                    controller = "Home",
                    action = "Index",
                    id = UrlParameter.Optional
                });
            }
            return RedirectToAction("UploadProduct");
        }

        public ActionResult ProductDetail(int? productID) {
            ViewData["productID"] = productID;
            return View();
        }

        public JsonResult ProductDetailJson(int? productID) {
            db.Configuration.ProxyCreationEnabled = false;
            Product products = db.Products.ToList().Select(product => new Product {
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
                status = product.status,
                rate = product.calculateRate(),
                Account = db.Accounts.Find(product.userID),
                Category = db.Categories.Find(product.categoriesID),
                Evaluates = db.Evaluates.ToList().Select(evaluate => new Evaluate {
                    evaluateID = evaluate.evaluateID,
                    evaluateContent = evaluate.evaluateContent,
                    rate = evaluate.rate,
                    date = evaluate.date,
                    userID = evaluate.userID,
                    productID = evaluate.productID,
                    Account = db.Accounts.Find(evaluate.userID),
                    Product = db.Products.Find(evaluate.productID),



                }).Where(e => e.productID == productID).ToList()

            }).Where(p => p.productID == productID).First();
            return Json(products, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCategoriesJson() {
            db.Configuration.ProxyCreationEnabled = false;
            List<Category> listCategories = db.Categories.ToList();
            return Json(listCategories, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(int? productId) {
            if (Session["user"] != null)
                db.Configuration.ProxyCreationEnabled = false;
            ViewData["productId"] = productId;

            if (Session["user"] == null) {
                return RedirectToAction("SignIn", "Accounts");
            } else {
                int userID = Int32.Parse(Session["user"].ToString());
                Account account = db.Accounts.Find(userID);
                if (account.role == 3 || account.role == 1) {
                    return View();
                } else {
                    return RedirectToAction("Index", "Home");
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "productID,description,shortDescription,price,quantity,status")] Product product) {
            try {
                db.Configuration.ProxyCreationEnabled = false;
                int id = product.productID;
                Product ProductUpdated = db.Products.Find(id);
                ProductUpdated.description = product.description;
                ProductUpdated.shortDescription = product.shortDescription;
                ProductUpdated.price = product.price;
                ProductUpdated.quantity = product.quantity;
                ProductUpdated.status = product.status;

                db.Entry(ProductUpdated).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Product", "Products");
            } catch {
                ViewData["productId"] = product.productID;
                return RedirectToAction("Edit", "Products");
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
