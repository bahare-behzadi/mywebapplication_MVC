using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using mywebapplication.DataAccess.Data;
using mywebapplication.DataAccess.Repository.IRepository;
using mywebapplication.Models.ViewModels;
using mywebapplication.Models.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using mywebapplication.Utility;





namespace MyWebApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objectProductList = _unitOfWork.Product.GetAll(includeproperties:"Category").ToList();
            return View(objectProductList);
        }
        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString(),
            });
            ProductVM productvm = new()
            {
                categoryList = CategoryList,
                Product = new Product()
            };
            if (id==null || id == 0)
            {
                //Create
                return View(productvm);
            }
            else
            {
                //Update
                productvm.Product = _unitOfWork.Product.Get(u=>u.Id == id);
                return View(productvm);
            }
            
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productvmObj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if(file !=null)
                {
                    string fileName= Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(wwwRootPath, @"Images\Products");
                    if(!productvmObj.Product.ImageUrl.IsNullOrEmpty() ) 
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, productvmObj.Product.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var filestream = new FileStream(Path.Combine(filePath,fileName), FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    }
                    productvmObj.Product.ImageUrl = @"\Images\Products\"+fileName;
                }
                if(productvmObj.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productvmObj.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(productvmObj.Product);
                } 
                _unitOfWork.Save();
                TempData["success"] = "Product is created";
                return RedirectToAction("Index");
            }
            else
            {
                productvmObj.categoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                });
                return View(productvmObj);
            }  
        }
 

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objectProductList = _unitOfWork.Product.GetAll(includeproperties: "Category").ToList();
            return Json(new { data = objectProductList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var deletedProduct = _unitOfWork.Product.Get(product => product.Id == id);
            if (deletedProduct == null)
            {
                return Json(new { success = false, message = "Error to delete" });
            }
            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, deletedProduct.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _unitOfWork.Product.Delete(deletedProduct);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete successfuly" });
        }
    }
}
