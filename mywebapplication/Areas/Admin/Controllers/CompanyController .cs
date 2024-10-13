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
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
  
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
          
        }
        public IActionResult Index()
        {
            List<Company> objectCompanyList = _unitOfWork.Company.GetAll().ToList();
            return View(objectCompanyList);
        }
        public IActionResult Upsert(int? id)
        {
 
            if (id==null || id == 0)
            {
                //Create
                return View(new Company());
            }
            else
            {
                //Update
                Company CompanyObj = _unitOfWork.Company.Get(u=>u.Id == id);
                return View(CompanyObj);
            }
            
        }
        [HttpPost]
        public IActionResult Upsert(Company CompanyObj)
        {
            if (ModelState.IsValid)
            {
                
                if(CompanyObj.Id == 0)
                {
                    _unitOfWork.Company.Add(CompanyObj);
                }
                else
                {
                    _unitOfWork.Company.Update(CompanyObj);
                } 
                _unitOfWork.Save();
                TempData["success"] = "Company is created";
                return RedirectToAction("Index");
            }
            else
            {
                return View(CompanyObj);
            }  
        }
 

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objectCompanyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = objectCompanyList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var deletedCompany = _unitOfWork.Company.Get(Company => Company.Id == id);
            if (deletedCompany == null)
            {
                return Json(new { success = false, message = "Error to delete" });
            }
            
            _unitOfWork.Company.Delete(deletedCompany);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete successfuly" });
        }
    }
}
