using Microsoft.AspNetCore.Mvc;
using NET_TASK.Data;
using NET_TASK.Models;
using System.Net.Mime;
using System.Security.Claims;
using System.Text.Json;

namespace NET_TASK.Controllers
{
    public class HomeController : Controller
    {
        private IWebHostEnvironment webHost;
        private ApplicationDbContext db;
        public HomeController(ApplicationDbContext _db, IWebHostEnvironment _wh)
        {
            db = _db;
            webHost = _wh;
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
            
            List<FolderTempModel> folds = new List<FolderTempModel>();          
            foreach (var file in files)
            {
                var folders = file.Split('/').ToList();
                folds.Add(new FolderTempModel(folders[0] + "/", 0, "null", Guid.NewGuid()));
                for (int i = 1; i < folders.Count; i++)
                {
                    string str = "";
                    folders.GetRange(0, i).ForEach(x => str += x + "/");                  
                    folds.Add(new FolderTempModel(str + folders[i] + "/", i, str, Guid.NewGuid()));
                }
            }  
            folds = folds.Distinct().ToList();          
            
            List<CatalogWithParent> catalogs = new List<CatalogWithParent>();          
            foreach(var lvl in folds.Select(x => x.level))
            {
                var foldsByLvl = folds.Where(x => x.level == lvl).ToList();
                var foldsParentsByLvl = folds.Where(x => x.level == lvl - 1).ToList();
                foreach (var foldByLvl in foldsByLvl)
                {
                    try
                    {
                        var parent = foldsParentsByLvl.FirstOrDefault(x => x.name == foldByLvl.parent);
                        CatalogWithParent curCat = new()
                        {
                            UserID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                            Name = foldByLvl.name,
                            Id = foldByLvl.id,
                            ParentName = parent?.name
                        };
                        curCat.ParentID = parent == null ? new Guid("00000000-0000-0000-0000-000000000000") : parent.id;
                        if (!catalogs.Any(x=>x.ParentName+x.Name==curCat.ParentName+curCat.Name))
                            catalogs.Add(curCat);
                    } catch { }
                }
            }
                        
            foreach (var item in catalogs)
            {
                string nm = item.Name.Contains('/') ?
                                item.Name.Remove(item.Name.Length - 1).Contains('/') ?
                                    item.Name.Remove(item.Name.Length - 1).Substring(item.Name.Remove(item.Name.Length - 1).LastIndexOf('/')+1) :
                                    item.Name.Remove(item.Name.Length - 1)
                            : item.Name;
                db.Catalogs.Add(new Catalog()
                {
                    Id = item.Id,
                    Name = nm,
                    ParentID = item.ParentID,
                    UserID = item.UserID
                });
            }
            db.SaveChanges();
            return RedirectToAction("WelcomePage", "Home");
        }

        [HttpGet]
        public IActionResult DeleteFolder(Guid id)
        {
            bool isUserFolder;
            Guid guid = DeleteFolderFromDB(id, out isUserFolder);
            return guid != new Guid("00000000-0000-0000-0000-000000000000") ? Index(guid) : 
                isUserFolder ? Index(guid) :
                RedirectToAction("WelcomePage");
        }

        [HttpGet]
        public IActionResult GetUsersCatalogs()
        {
            if (string.IsNullOrWhiteSpace(User.FindFirst(ClaimTypes.NameIdentifier)?.Value)) return BadRequest();
            var catalogs = db.Catalogs
                .Where(y => y.ParentID == new Guid("00000000-0000-0000-0000-000000000000"))
                .Where(y => y.UserID == User.FindFirst(ClaimTypes.NameIdentifier).Value);
            
            IndexViewModel ivm = new IndexViewModel()
            {
                Catalogs = catalogs                
            };
            return View("Index", ivm);
        }

        [HttpGet]
        public IActionResult Parent(Guid Id, Guid userId)
        {            
            var parId = userId != new Guid("00000000-0000-0000-0000-000000000000") ? 
                db.Catalogs.FirstOrDefault(y => y.Id == Id && y.UserID == userId.ToString())?.ParentID :
                db.Catalogs.FirstOrDefault(y => y.Id == Id)?.ParentID;
            IndexViewModel vm = new()
            {
                CurrentCatalog = db.Catalogs.FirstOrDefault(y => y.Id == parId),
                Catalogs = userId != new Guid("00000000-0000-0000-0000-000000000000") ?
                    db.Catalogs.Where(x => x.ParentID == parId && x.UserID == userId.ToString()).ToList() :
                    db.Catalogs.Where(x => x.ParentID == parId && x.UserID == null).ToList()
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
            IEnumerable<Catalog> testData = new List<Catalog>()
            {
                new Catalog { Id = new Guid("05b15660-4e1b-4e6d-a3b3-05761151280a"), ParentID = new Guid("00000000-0000-0000-0000-000000000000"), Name = "Creating Digital Image" },
                new Catalog { Id = new Guid("f1db1d5f-c97f-45e5-aa39-165ed0806162"), ParentID = new Guid("05b15660-4e1b-4e6d-a3b3-05761151280a"), Name = "Resources" },
                new Catalog { Id = new Guid("4a64b4f6-e007-4243-ac76-3a9ee7851381"), ParentID = new Guid("05b15660-4e1b-4e6d-a3b3-05761151280a"), Name = "Evidence" },
                new Catalog { Id = new Guid("c13b1f42-266a-49ca-932b-62c70da55590"), ParentID = new Guid("05b15660-4e1b-4e6d-a3b3-05761151280a"), Name = "Graphic Products" },
                new Catalog { Id = new Guid("176278c3-a448-4efe-b73b-ba165b8dd45a"), ParentID = new Guid("f1db1d5f-c97f-45e5-aa39-165ed0806162"), Name = "Primary Source" },
                new Catalog { Id = new Guid("4d4b72ec-1f51-4dfa-9d6c-a2385157883d"), ParentID = new Guid("f1db1d5f-c97f-45e5-aa39-165ed0806162"), Name = "Secondary Source" },
                new Catalog { Id = new Guid("26e48c4c-7bff-4a26-a139-291a89a8648d"), ParentID = new Guid("c13b1f42-266a-49ca-932b-62c70da55590"), Name = "Process" },
                new Catalog { Id = new Guid("6e70a745-12c8-493c-ace5-0001c168b47c"), ParentID = new Guid("c13b1f42-266a-49ca-932b-62c70da55590"), Name = "Final Product" }
            };
            if(!db.Catalogs.Contains(testData.ElementAt(0))) {
                db.Catalogs.AddRange(testData);
                db.SaveChanges();
            }
            
            IndexViewModel ivm = new IndexViewModel()
            {
                Catalogs = db.Catalogs.Where(x => x.ParentID == new Guid("00000000-0000-0000-0000-000000000000") & x.UserID == null).ToList(),
                CurrentCatalog = null
            };
        
            
            return View("Index",ivm);
        }

        [HttpGet]
        public IActionResult WelcomePage() => View();
        
        [HttpGet]
        public IActionResult Importer() => View();

        [HttpGet]
        public PhysicalFileResult ExportYourFolders()
        {
            var users_folders = db.Catalogs.Where(x => x.UserID == User.FindFirst(ClaimTypes.NameIdentifier).Value).ToList();
            string json = JsonSerializer.Serialize(users_folders);

            string path = Path.Combine(webHost.ContentRootPath, @$"ImportFiles\{User.FindFirst(ClaimTypes.NameIdentifier).Value}.json");
            System.IO.File.Create(path).Close();
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write(json);
            }
            return PhysicalFile(path, MediaTypeNames.Application.Json, "ImportYourFolders.json");
        }

        [HttpPost]
        public IActionResult ImportYourFolders(IFormFile json)
        {
            using (StreamReader sr = new StreamReader(json.OpenReadStream()))
            {
                var fileContent = sr.ReadToEnd();
                try
                {
                    var catalogs = JsonSerializer.Deserialize<List<Catalog>>(fileContent);
                    foreach (var catalog in catalogs)
                    {
                        if (!db.Catalogs.Contains(catalog))
                        {
                            db.Catalogs.Add(catalog);
                        }
                    }
                    db.SaveChanges();
                    return RedirectToAction("WelcomePage", "Home");
                }
                catch (Exception)
                {
                    return BadRequest("Invalid JSON");
                }
            }
        }

        [HttpGet]
        public IActionResult GetImportJsonFile() => View("GetFilesFromJson");
        private Guid DeleteFolderFromDB(Guid id, out bool isUserFolder)
        {
            var catalog = db.Catalogs.FirstOrDefault(x => x.Id == id);
            isUserFolder = false;
            bool zaglushka = true;
            if (catalog == null) return new Guid("00000000-0000-0000-0000-000000000000");
            while (db.Catalogs.Any(x => x.ParentID == catalog.Id))
            {
                List<Catalog> catalogs = db.Catalogs.Where(x => x.ParentID == catalog.Id).ToList();
                foreach (var item in catalogs)
                {
                    DeleteFolderFromDB(item.Id, out zaglushka);
                }
            }
            isUserFolder = catalog.UserID == User.FindFirst(ClaimTypes.NameIdentifier).Value; 
            db.Catalogs.Remove(catalog);
            db.SaveChanges();
            return catalog.ParentID;
        }
    }

    public class FolderTempModel {
        public string name;
        public int level;
        public string parent;
        public Guid id;
        public FolderTempModel(string name, int level, string parent, Guid id)
        {
            this.name = name;
            this.level = level;
            this.parent = parent;
            this.id = id;
        }

        public override bool Equals(object? obj)
        {
            return obj is FolderTempModel model &&
                   name == model.name &&
                   parent == model.parent;
        }
        
        public override int GetHashCode()
        {
            return name.GetHashCode() + parent.GetHashCode();
        }
    }
    public class CatalogWithParent
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? ParentName { get; set; }
        public Guid ParentID { get; set; }
        public string? UserID { get; set; }
    }
}
