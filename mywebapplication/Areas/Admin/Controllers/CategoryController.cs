using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mywebapplication.DataAccess.Data;
using mywebapplication.DataAccess.Repository.IRepository;
using mywebapplication.Models;
using mywebapplication.Utility;



namespace MyWebApplication.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Category> objectCtegoryList = _unitOfWork.Category.GetAll().ToList();
            return View(objectCtegoryList);
        }
        public IActionResult Creat()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Creat(Category categoryObj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(categoryObj);
                _unitOfWork.Save();
                TempData["success"] = "Category is created";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var category = _unitOfWork.Category.Get(u => u.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost]
        public IActionResult Edit(Category categoryObj)
        {

            _unitOfWork.Category.Update(categoryObj);
            _unitOfWork.Save();
            TempData["success"] = "Category is Updated";
            return RedirectToAction("Index");

        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var category = _unitOfWork.Category.Get(u => u.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            Category? categoryObj = _unitOfWork.Category.Get(u => u.Id == id);
            if (categoryObj == null)
            {
                return NotFound();
            }
            _unitOfWork.Category.Delete(categoryObj);
            _unitOfWork.Save();
            TempData["success"] = "Category is deleted";
            return RedirectToAction("Index");
        }
    }
}
