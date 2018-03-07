using CourseaApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CourseaApplication.Controllers
{
    public class CourseController : Controller
    {
        // GET: Courses

        [ActionName("Index")]
        public async Task<ActionResult> IndexAsync()
        {
            var items = await DocumentDbRepository<Course>.GetItemsAsync(d => d != null);
            return View(items);
        }

        //display create View
        public ActionResult Create()
        {
            return View(new Course());
        }

        //create a course
        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync(Course course)
        {
            if (ModelState.IsValid)
            {
                if (course.id == null)
                {
                    await DocumentDbRepository<Course>.CreateItemAsync(course);
                }
                else
                {
                    await DocumentDbRepository<Course>.UpdateItemAsync(course.id, course);
                }
                return RedirectToAction("Index");
            }
            return View(course);
        }


        //edit asn item
        [ActionName("Edit")]
        public async Task<ActionResult> EditAsync(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = await DocumentDbRepository<Course>.GetItemAsync(id);

            if(course==null)
            {
                return HttpNotFound();
            }
            return View("Create", course);

            //Course courseedit = await DocumentDbRepository<Course>.UpdateItemAsync(id);
        }

        //delete item
        [ActionName("Delete")]
        public async Task<ActionResult> DeleteAsync(string id)
        {
            if(id==null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Course course = await DocumentDbRepository<Course>.GetItemAsync(id);

            if(course==null)
            {
                return HttpNotFound();
            }
            return View("Delete",course);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmedAsync([Bind(Include="id")] string id)
        {
            await DocumentDbRepository<Course>.DeleteItemAsync(id);
            return RedirectToAction("Index");
        }

        //get detail of an item
        [ActionName("Details")]
        public async Task<ActionResult> DetailsAsync(string id)
        {
            Course course = await DocumentDbRepository<Course>.GetItemAsync(id);
            return View("Details",course);
        }
    }
}