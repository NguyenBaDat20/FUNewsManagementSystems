﻿using FUDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyMvcApp.Utilities;
using Services;

namespace MyMvcApp.Controllers
{
    [ValidateJwtToken]
    public class NewsArticleController : Controller
    {
        private readonly INewsArticleService _newsArticleService;
        private readonly ICategoryService _categoryService;
        private readonly ISystemAccountService _systemAccountService;

        public NewsArticleController(INewsArticleService newsArticleService, ICategoryService categoryService, ISystemAccountService systemAccountService)
        {
            _newsArticleService = newsArticleService;
            _categoryService = categoryService;
            _systemAccountService = systemAccountService;
        }

        /*  public async Task<IActionResult> Index()
          {
              var news = await _newsArticleService.GetNewsArticles();
              return View(news);
          }*/
        public async Task<IActionResult> Index(string search)
        {
            var allItems = await _newsArticleService.GetNewsArticles();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                allItems = allItems.Where(a => a.NewsTitle.ToLower().Contains(search)
                    || (a.Tags != null && a.Tags.Any(t => t.TagName.ToLower().Contains(search))))
                    .ToList();
            }

            return View(allItems);
        }
        public async Task<IActionResult> Details(string id)
        {
            var news = await _newsArticleService.GetNewsArticleById(id);
            if (news == null) return NotFound();
            return View(news);
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _categoryService.GetCategories();
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "CategoryName");

            var users = await _systemAccountService.GetAccounts();
            ViewBag.CreatedById = new SelectList(users, "AccountId", "AccountName");
            ViewBag.UpdatedById = new SelectList(users, "AccountId", "AccountName");

            return View();
        }

        /*[HttpPost]
        public async Task<IActionResult> Create(NewsArticleDTO dto)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate dropdowns nếu ModelState không valid
                var categories = await _categoryService.GetCategories();
                ViewBag.CategoryId = new SelectList(categories, "CategoryId", "CategoryName");

                var users = await _systemAccountService.GetAccounts();
                ViewBag.CreatedById = new SelectList(users, "AccountId", "AccountName");
                ViewBag.UpdatedById = new SelectList(users, "AccountId", "AccountName");
            }

            try
            {
                // Tự động tạo ID nếu chưa có
                if (string.IsNullOrEmpty(dto.NewsArticleId))
                {
                    dto.NewsArticleId = Guid.NewGuid().ToString();
                }

                // Thêm logging để kiểm tra
                Console.WriteLine($"Creating news article with ID: {dto.NewsArticleId}");

                dto.ModifiedDate = DateTime.Now;

                await _newsArticleService.Create(dto);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log lỗi
                Console.WriteLine($"Error creating news article: {ex.Message}");

                TempData["ErrorMessage"] = "An error occurred while creating the news article";
                return RedirectToAction(nameof(Create));
            }
        }*/
        [HttpPost]
        public async Task<IActionResult> Create(NewsArticleDTO dto)
        {
            if (!ModelState.IsValid)
            {
                // Populate dropdowns lại nếu có lỗi
                var categories = await _categoryService.GetCategories();
                ViewBag.CategoryId = new SelectList(categories, "CategoryId", "CategoryName");

                var users = await _systemAccountService.GetAccounts();
                ViewBag.CreatedById = new SelectList(users, "AccountId", "AccountName");
                ViewBag.UpdatedById = new SelectList(users, "AccountId", "AccountName");

                return View(dto); // ✅ TRẢ VỀ VIEW nếu model không hợp lệ
            }

            try
            {
                if (string.IsNullOrEmpty(dto.NewsArticleId))
                {
                    /*  dto.NewsArticleId = Guid.NewGuid().ToString();*/
                    var fullGuid = Guid.NewGuid().ToString(); // 36 ký tự
                    var shortGuid = fullGuid.Replace("-", "").Substring(0, 20); // 20 ký tự (không có dấu '-')
                    dto.NewsArticleId = shortGuid;
                }
                dto.ModifiedDate = DateTime.Now;

                await _newsArticleService.Create(dto);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating news article: {ex.Message}");

                TempData["ErrorMessage"] = "An error occurred while creating the news article";

                // Populate dropdowns lại khi exception
                var categories = await _categoryService.GetCategories();
                ViewBag.CategoryId = new SelectList(categories, "CategoryId", "CategoryName");

                var users = await _systemAccountService.GetAccounts();
                ViewBag.CreatedById = new SelectList(users, "AccountId", "AccountName");
                ViewBag.UpdatedById = new SelectList(users, "AccountId", "AccountName");

                return View(dto); // ✅ Trả về lại form để người dùng sửa
            }
        }


        public async Task<IActionResult> Edit(string id)
        {
            var article = await _newsArticleService.GetNewsArticleById(id);
            if (article == null) return NotFound();

            var categories = await _categoryService.GetCategories();
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "CategoryName", article.CategoryId);

            var users = await _systemAccountService.GetAccounts();
            ViewBag.CreatedById = new SelectList(users, "AccountId", "AccountName", article.CreatedById);
            ViewBag.UpdatedById = new SelectList(users, "AccountId", "AccountName", article.UpdatedById);

            return View(article);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, NewsArticleDTO dto)
        {
            if (id != dto.NewsArticleId) return BadRequest();
            if (!ModelState.IsValid) return View(dto);

            dto.ModifiedDate = DateTime.Now;

            await _newsArticleService.Update(dto);
            return RedirectToAction(nameof(MyNews));
        }

        public async Task<IActionResult> Delete(string id)
        {
            var article = await _newsArticleService.GetNewsArticleById(id);
            if (article == null) return NotFound();
            return View(article);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelete(string id)
        {
            try
            {
                await _newsArticleService.Delete(id);

                TempData["SuccessMessage"] = "News article deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting article: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while deleting the news article";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string id)
        {
            try
            {
                var newsArticle = await _newsArticleService.GetNewsArticleById(id);
                if (newsArticle == null)
                {
                    TempData["ErrorMessage"] = "News article not found";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.CategoryName = newsArticle.Category.CategoryName ?? "N/A";
                ViewBag.CreatedByName = newsArticle.CreatedBy.AccountName ?? "N/A";
                ViewBag.UpdatedByName = newsArticle.UpdatedBy.AccountName ?? "N/A";

                return View("Details", newsArticle);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving news article details: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while retrieving news article details";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> MyNews()
        {
            var myArticles = await _newsArticleService.GetNewsArticlesByAuthor();
            return View(myArticles);
        }
    }
}