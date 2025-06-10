using FUDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyMvcApp.Utilities;
using Services;

namespace MyMvcApp.Controllers
{
    [ValidateJwtToken]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /*public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _categoryService.GetCategories();
                return View(categories);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving categories: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading categories";
                return View(new List<CategoryDTO>());
            }
        }*/
        /*    public async Task<IActionResult> Index(string search)
            {
                var allCategories = await _categoryService.GetCategories();

                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    allCategories = allCategories.Where(c => c.CategoryName.ToLower().Contains(search)).ToList();
                }

                return View(allCategories);
            }*/
        public async Task<IActionResult> Index(string search = "")
        {
            var categories = await _categoryService.GetCategories();

            if (!string.IsNullOrEmpty(search))
            {
                categories = categories.Where(c => c.CategoryName.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.ParentCategories = new SelectList(categories, "CategoryId", "CategoryName");

            return View(categories);
        }



        /* [HttpGet]
         public async Task<IActionResult> Create()
         {
             var categories = await _categoryService.GetCategories();

             ViewBag.ParentCategories = new SelectList(categories, "CategoryId", "CategoryName");

             return View(new CategoryDTO { IsActive = true });
         }
 */
        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetCategories();
                ViewBag.ParentCategories = new SelectList(categories, "CategoryId", "CategoryName");
                return View(dto);
            }

            try
            {
                await _categoryService.Create(dto);
                TempData["SuccessMessage"] = "Category created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating category: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to create category";

                var categories = await _categoryService.GetCategories();
                ViewBag.ParentCategories = new SelectList(categories, "CategoryId", "CategoryName");
                return View(dto);
            }
        }*/
        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetCategories();
                ViewBag.ParentCategories = new SelectList(categories, "CategoryId", "CategoryName");
                TempData["ErrorMessage"] = "Invalid data, please check your input.";
                return RedirectToAction("Index");
            }

            try
            {
                await _categoryService.Create(dto);
                TempData["SuccessMessage"] = "Category created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["ErrorMessage"] = "Error creating category.";
                return RedirectToAction("Index");
            }
        }*/
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryService.GetCategories();

            ViewBag.ParentCategories = new SelectList(categories, "CategoryId", "CategoryName");

            // QUAN TRỌNG: Trả về PartialView khi được gọi bởi AJAX để hiển thị trong modal
            // return PartialView("_CreateCategoryPartial", new CategoryDTO { IsActive = true });
            // Để đơn giản và hỗ trợ cả request thông thường và AJAX, bạn có thể kiểm tra:
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CreateCategoryPartial", new CategoryDTO { IsActive = true });
            }
            return View(new CategoryDTO { IsActive = true }); // Dành cho request thông thường (nếu có)
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryDTO dto)
        {
            if (!ModelState.IsValid)
            {
                // Nếu ModelState không hợp lệ, không redirect ngay lập tức.
                // Thay vào đó, trả về PartialView với ModelState errors để hiển thị trong modal.
                var categories = await _categoryService.GetCategories();
                ViewBag.ParentCategories = new SelectList(categories, "CategoryId", "CategoryName");
                // TempData["ErrorMessage"] = "Invalid data, please check your input."; // Bỏ dòng này
                return PartialView("_CreateCategoryPartial", dto); // Trả về lại form với lỗi validation
            }

            try
            {
                await _categoryService.Create(dto);
                // Nếu thành công, trả về JSON hoặc redirect (cho trường hợp AJAX, chúng ta sẽ reload trang)
                TempData["SuccessMessage"] = "Category created successfully!";
                // return RedirectToAction("Index"); // Giữ nguyên cho flow thông thường
                return Json(new { success = true, message = "Category created successfully!" }); // Trả về JSON cho AJAX
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // TempData["ErrorMessage"] = "Error creating category."; // Bỏ dòng này
                ModelState.AddModelError("", "Error creating category: " + ex.Message); // Thêm lỗi vào ModelState

                var categories = await _categoryService.GetCategories();
                ViewBag.ParentCategories = new SelectList(categories, "CategoryId", "CategoryName");
                return PartialView("_CreateCategoryPartial", dto); // Trả về lại form với lỗi từ server
            }
        }



        [HttpGet]
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var category = await _categoryService.GetCategoryById(id);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Category not found";
                    return RedirectToAction(nameof(Index));
                }

                var categories = await _categoryService.GetCategories();
                var parentCandidates = categories.Where(c => c.CategoryId != id);
                ViewBag.ParentCategories = new SelectList(parentCandidates, "CategoryId", "CategoryName", category.ParentCategoryId);

                return View(category);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving category: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while loading the category";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryDTO dto)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetCategories();
                var parentCandidates = categories.Where(c => c.CategoryId != dto.CategoryId);
                ViewBag.ParentCategories = new SelectList(parentCandidates, "CategoryId", "CategoryName", dto.ParentCategoryId);

                return View(dto);
            }

            try
            {
                await _categoryService.Update(dto);
                TempData["SuccessMessage"] = "Category updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating category: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to update category";

                var categories = await _categoryService.GetCategories();
                var parentCandidates = categories.Where(c => c.CategoryId != dto.CategoryId);
                ViewBag.ParentCategories = new SelectList(parentCandidates, "CategoryId", "CategoryName", dto.ParentCategoryId);

                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(short id)
        {
            var category = await _categoryService.GetCategoryById(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        /*[HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                await _categoryService.Delete(id);
                TempData["SuccessMessage"] = "Category deleted successfully";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting category: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to delete category";
            }
            return RedirectToAction(nameof(Index));
        }*/
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                bool isDeleted = await _categoryService.Delete(id);
                if (isDeleted)
                {
                    TempData["SuccessMessage"] = "Category deleted successfully";
                }
                else
                {
                    TempData["ErrorMessage"] = "Cannot delete category because it has related news articles.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting category: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while deleting the category.";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}