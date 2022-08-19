using Microsoft.AspNetCore.Mvc;
using NET_TASK.Data;
using NET_TASK.Models;
using System.Security.Claims;
using System.Text.Json;

namespace NET_TASK.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db;
        public HomeController(ApplicationDbContext _db)
        {
            db = _db;         
        }
        
        


        
        [HttpPost]
        
        public IActionResult Upload(string json)
        {
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrEmpty(json)) return BadRequest();
            var files = JsonSerializer.Deserialize<List<string>>(json);
            for (int i = 0; i < files.Count; i++)
            {
                files[i] = files[i].Substring(0, files[i].LastIndexOf('/'));
            }
            files = files.Distinct().ToList();

            List<Catalog> folders = new List<Catalog>();

            Catalog curr = new();
            
            foreach (var file in files)
            {
                var folds = file.Split('/');
                for (int i = 0; i < folds.Length-1; i++)
                {
                    if(i==0)
                    {
                        curr = new Catalog() { Name = folds[i], ParentID = new Guid("00000000-0000-0000-0000-000000000000"), Id = Guid.NewGuid(), UserID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value };
                        folders.Add(curr);
                    } else
                    {
                        folders.Add(new Catalog()
                        {
                            Id = Guid.NewGuid(),
                            Name = folds[i],
                            ParentID = curr.Id
                        });
                        curr = folders.Last();
                    }
                }                
            }

            foreach (var folder in folders)
            {
                Console.WriteLine("Name: " + folder.Name + " ParentId: " + folder.ParentID + " Id: " + folder.Id);
                Console.WriteLine("------------------------------");
            }
            return Ok();
        }



        [HttpGet]
        public IActionResult Parent(Guid Id)
        {            
            var parId = db.Catalogs.FirstOrDefault(y => y.Id == Id).ParentID;
            IndexViewModel vm = new()
            {
                CurrentCatalog = db.Catalogs.FirstOrDefault(y => y.Id == parId),
                Catalogs = db.Catalogs.Where(x => x.ParentID == parId).ToList()
            };
            return View("Index", vm);
        }
        
        [HttpPost]
        [HttpGet]
        public IActionResult Index(Guid Id)
        {
            var currentItem = db.Catalogs.FirstOrDefault(y => y.Id == Id);
            var items = db.Catalogs.Where(x => x.ParentID == Id).ToList();
            IndexViewModel vm = new IndexViewModel()
            {
                Catalogs = items,
                CurrentCatalog = currentItem
            };
            return View("Index",vm);
        }

        [HttpGet]
        public IActionResult TestData()
        {            
            return Index(new Guid("00000000-0000-0000-0000-000000000000"));
        }

        [HttpGet]
        public IActionResult WelcomePage() => View();


        [HttpGet]
        public IActionResult Importer() => View();
    }
}
