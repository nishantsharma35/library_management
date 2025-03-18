using library_management.Models;
using library_management.repository.internalinterface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace library_management.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly dbConnect _context;
        public DashboardController(dbConnect context, ISidebarRepository sidebar) : base(sidebar)
        {
            _context = context;
        }

        [Route("dashboard")]
       
        public IActionResult Index()
        {
            return View();
        }

    }
}


