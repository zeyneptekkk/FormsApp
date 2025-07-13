using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using FormsApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace FormsApp.Controllers
{
    public class HomeController : Controller
    {
       
        public HomeController()
        {

        }
        public IActionResult Index(string searchString, string category)
        {
            
            var products = Repository.Products;

            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.SearchString = searchString;
                products = products.Where(p => p.Name.ToLower().Contains(searchString)).ToList();
            }

           

            if (!string.IsNullOrEmpty(category) && category != "0")
            {
                int selectedCategory = int.Parse(category);
                products = products.Where(p => p.CategoryId == selectedCategory).ToList();
            }

           // ViewBag.Categories = new SelectList(Repository.Category, "CategoryId", "Name", category);

            var model = new ProductViewModel
            {
                Products = products,
                Categories=Repository.Category,
                SelectedCategory=category
            };


            return View(model);
        }

        public IActionResult Create()
        {
            
            ViewBag.Categories = Repository.Category;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product model,IFormFile imageFile)
        {
            var extension = "";
            if (imageFile != null)
            {

                var allowedExtensions = new[] { ".jpeg", ".jpg", ".png" };
                extension = Path.GetExtension(imageFile.FileName);

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("", "Ge�erli bir resim se�iniz!");
                }
            }
            
            if(ModelState .IsValid)
            {
                var randomFileName = string.Format($"{Guid.NewGuid().ToString()}{extension}");
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", randomFileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                model.Image = randomFileName;
                model.ProductId = Repository.Products.Count+1;
                Repository.CreateProduct(model);
                return RedirectToAction("Index");
                 
            }
            ViewBag.Categories = Repository.Category;
            
            return View(model);
        }

        public IActionResult Edit(int? id)
        {
            object imageFile = null;
            if (imageFile != null)
            {

                if (id == null)
                {
                    return NotFound();

                }
            }
            var entity = Repository.Products.FirstOrDefault(p => p.ProductId == id);
            if(entity == null)
            {
                return NotFound();
            }
       //     ViewBag.Categories = new SelectList(Repository.Category, "CategoryId", "Name");
            ViewBag.Categories = Repository.Category;
            return View(entity);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id,Product model,IFormFile? imageFile)
        {
            if(id != model.ProductId)
            {

                return NotFound();
            }
            if(imageFile != null)
            {
                var extension = Path.GetExtension(imageFile.FileName);
                var randomFileName = string.Format($"{Guid.NewGuid().ToString()}{extension}");
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", randomFileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                model.Image = randomFileName;
            }
            Repository.EditProduct(model);
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entity = Repository.Products.FirstOrDefault(p => p.ProductId == id);
            if (entity == null)
            {
                return NotFound();
            }
            return View("DeleteConfirm",entity);

        }
        [HttpPost]
        public IActionResult Delete(int id,int ProductId)
        {
            if (id != ProductId)
            {
                return NotFound();
            }
            var entity = Repository.Products.FirstOrDefault(p => p.ProductId == ProductId);
            if (entity == null)
            {
                return NotFound();
            }

            Repository.DeleteProduct(entity);
            return RedirectToAction("Index");
        }
        public IActionResult EditProducts(List<Product> Products)
        {
            foreach(var product in Products)
            {
                Repository.EditIsActive(product);
            }
            return RedirectToAction("Index");
        }
    }

    

   }
