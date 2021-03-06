﻿using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using silver_octo_app.Models;
using silver_octo_app.ViewModels;

namespace silver_octo_app.Controllers
{
    public class BudgetItemsController : Controller
    {
        private ApplicationDbContext _context;

        public BudgetItemsController()
        {
            this._context = new ApplicationDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            // TODO: Not sure what disposing boolean is supposed to be here.
            this._context.Dispose();
        }

        [Route("BudgetItems/Edit/{id}")]
        public ActionResult Edit(int id)
        {
            var budgetItem = _context.BudgetItems.SingleOrDefault(b => b.Id == id);

            if (budgetItem == null)
            {
                return StatusCode(404);
            }
            ViewBag.Title = "Edit Budget Item";
            return View("BudgetItemForm",budgetItem);
        }


        public ViewResult Index()
        {
            return View();
        }

        [Route("BudgetItems/Details{id}")]
        public ActionResult Details(int id)
        {
            var budgetItem = _context.BudgetItems.SingleOrDefault(b => b.Id == id);

            if (budgetItem == null)
            {
                return StatusCode(404);
            }

            return View(budgetItem);
        }

        [Route("BudgetItems/entered/{month:range(1,12)}/{day}")]
        public ActionResult ByEntryDate(int month, int day)
        {
            return Content(string.Format("{0}/{1}",month,day));
        }

        [Route("BudgetItems/list")]
        public ActionResult ListBudgetItems()
        {
            var budgetItems = this._context.BudgetItems.ToList();

            var viewModel = new ListBudgetItemsViewModel
            {
                BudgetItems = budgetItems
            };

            return View(viewModel);
        }

        public ActionResult New()
        {
            ViewBag.Title = "New Budget Item";
            BudgetItem budgetItem = new BudgetItem();
            return View("BudgetItemForm",budgetItem);
        }

        [HttpPost]
        public ActionResult Create(BudgetItem budgetItem)
        {
            _context.BudgetItems.Add(budgetItem);
            _context.SaveChanges();

            return RedirectToAction("Index", "BudgetItems");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(BudgetItem budgetItem)
        {
            if (string.IsNullOrEmpty(budgetItem.CategoryName) || !ModelState.IsValid)
            {
                return View("BudgetItemForm", budgetItem);
            }

            if (budgetItem.Id == 0) // if new budgetItem
            {
                _context.BudgetItems.Add(budgetItem);
            }
            else
            {
                var budgetItemInDb = _context.BudgetItems.Single(b => b.Id == budgetItem.Id);
                budgetItemInDb.Amount = budgetItem.Amount;
                budgetItemInDb.CategoryName = budgetItem.CategoryName;
                budgetItemInDb.DateUpdated = budgetItem.DateUpdated;
                budgetItemInDb.Description = budgetItem.Description;
            }


            try
            {
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return RedirectToAction("Index", "BudgetItems");
        }

    }
}
