using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TequilasRestaurant.Data;
using TequilasRestaurant.Models;

namespace TequilasRestaurant.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        Repository<Product> repository;
        Repository<Ingredient> ingredient_repository;
        Repository<Category> category_repository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            this.repository = new Repository<Product>(context);
            this.ingredient_repository = new Repository<Ingredient>(context);
            this.category_repository = new Repository<Category>(context);
            this._webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            return View(await repository.GetAllAsync());
        }
        public async Task<IActionResult> Details(int id)
        {
            return View(await repository.GetByIdAsync(id, new QueryOptions<Product>() { Includes = "Category" }));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Product,Name")] Product product)
        {
            if (ModelState.IsValid)
            {
                await repository.AddAsync(product);
                return RedirectToAction("Index");
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await repository.DeleteAsync(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Product Not found");
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddEdit(int id)
        {
            ViewBag.categorys = await category_repository.GetAllAsync();
            ViewBag.ingredients = await ingredient_repository.GetAllAsync();
            if (id == 0)
            {
                ViewBag.Operation = "Add";
                return View(new Product());
            }
            else
            {
                ViewBag.Operation = "Edit";
                return View(await repository.GetByIdAsync(id, new QueryOptions<Product>() { Includes = "Category,ProductIngredients.Ingredient" }));
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddEdit(Product product, int[] ingredientIds)
        {
            if (ModelState.IsValid)
            {
                if (product.ImageFile != null)
                {
                    string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + product.ImageFile.FileName;
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await product.ImageFile.CopyToAsync(fileStream);
                    }
                    product.ImageUrl = uniqueFileName;
                }
                if (product.ProductId == 0)
                {
                    foreach (int id in ingredientIds)
                    {
                        product.ProductIngredients?.Add(new ProductIngredient { IngredientId = id, ProductId = product.ProductId });
                    }
                    await repository.AddAsync(product);
                }
                else
                {
                    var exisitingProduct = await repository.GetByIdAsync(product.ProductId, new QueryOptions<Product>() { Includes = "Category,ProductIngredients.Ingredient" });
                    exisitingProduct.Name = product.Name;
                    exisitingProduct.Description = product.Description;
                    exisitingProduct.Price = product.Price;
                    exisitingProduct.Stock = product.Stock;
                    exisitingProduct.CategoryId = product.CategoryId;
                    exisitingProduct.ProductIngredients.Clear();
                    foreach (int id in ingredientIds)
                    {
                        exisitingProduct.ProductIngredients?.Add(new ProductIngredient { IngredientId = id, ProductId = product.ProductId });
                    }
                    if (product.ImageFile != null)
                    {
                        exisitingProduct.ImageUrl = product.ImageUrl;
                    }
                    try
                    {
                        await repository.UpdateAsync(exisitingProduct);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Error:{ex.GetBaseException().Message}");
                        ViewBag.categorys = await category_repository.GetAllAsync();
                        ViewBag.ingredients = await ingredient_repository.GetAllAsync();
                        ViewBag.Operation = "Add";
                        return View(new Product());
                    }
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
        }
    }
}