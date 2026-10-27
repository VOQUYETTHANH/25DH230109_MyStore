using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using _25DH230109_MyStore.Models;
using _25DH230109_MyStore.Models.ViewModel;

namespace _25DH230109_MyStore.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private readonly MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Products - tìm kiếm, lọc giá, sắp xếp và phân trang.
        public ActionResult Index(string keyword, decimal? minPrice, decimal? maxPrice,
            string sortOrder, int? page)
        {
            var query = db.Product.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string key = keyword.Trim();
                query = query.Where(p => p.ProductName.Contains(key)
                    || p.ProductDescription.Contains(key)
                    || p.Category.CategoryName.Contains(key));
            }
            if (minPrice.HasValue)
                query = query.Where(p => p.ProductPrice >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(p => p.ProductPrice <= maxPrice.Value);

            switch (sortOrder)
            {
                case "name_desc": query = query.OrderByDescending(p => p.ProductName); break;
                case "price_asc": query = query.OrderBy(p => p.ProductPrice); break;
                case "price_desc": query = query.OrderByDescending(p => p.ProductPrice); break;
                default: query = query.OrderBy(p => p.ProductName); break;
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;
            var vm = new ProductSearchVM
            {
                Keyword = keyword,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortOrder = sortOrder,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Products = query.ToPagedList(pageNumber, pageSize)
            };
            return View(vm);
        }

        // GET: Admin/Products/Details/5
        public ActionResult Details(int? id)
        {
            if (!id.HasValue) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Product.Include(p => p.Category).FirstOrDefault(p => p.ProductID == id.Value);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        // GET: Admin/Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Category, "CategoryID", "CategoryName");
            return View(new Product());
        }

        // POST: Admin/Products/Create
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription,ProductPrice,ProductImage,UploadImg")] Product product)
        {
            if (ModelState.IsValid)
            {
                SaveUploadedImage(product);
                if (string.IsNullOrWhiteSpace(product.ProductImage))
                    product.ProductImage = "~/Content/images/no-image.png";
                db.Product.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Category, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Product.Find(id.Value);
            if (product == null) return HttpNotFound();
            ViewBag.CategoryID = new SelectList(db.Category, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription,ProductPrice,ProductImage,UploadImg")] Product product)
        {
            if (ModelState.IsValid)
            {
                var existing = db.Product.Find(product.ProductID);
                if (existing == null) return HttpNotFound();

                existing.CategoryID = product.CategoryID;
                existing.ProductName = product.ProductName;
                existing.ProductDescription = product.ProductDescription;
                existing.ProductPrice = product.ProductPrice;
                if (product.UploadImg != null && product.UploadImg.ContentLength > 0)
                {
                    product.ProductImage = existing.ProductImage;
                    SaveUploadedImage(product);
                    existing.ProductImage = product.ProductImage;
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Category, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Product.Include(p => p.Category).FirstOrDefault(p => p.ProductID == id.Value);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Product.Find(id);
            if (product == null) return HttpNotFound();
            db.Product.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private void SaveUploadedImage(Product product)
        {
            if (product.UploadImg == null || product.UploadImg.ContentLength <= 0) return;
            string extension = Path.GetExtension(product.UploadImg.FileName);
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                throw new HttpException(400, "Chỉ chấp nhận ảnh JPG, JPEG, PNG, GIF hoặc WEBP.");

            string folder = Server.MapPath("~/Content/images/");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string fileName = Guid.NewGuid().ToString("N") + extension.ToLowerInvariant();
            product.UploadImg.SaveAs(Path.Combine(folder, fileName));
            product.ProductImage = "~/Content/images/" + fileName;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
