﻿using Microsoft.AspNet.Identity;
using ExpenseManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
/* This class is created as a part of default project.
 */
namespace ExpenseManager.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


      
        public ActionResult Index()
        {
            return View();
        }
    }
}