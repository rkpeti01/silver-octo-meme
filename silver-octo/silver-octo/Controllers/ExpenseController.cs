﻿using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using silver_octo.ViewModels;
using silver_octo.Models;
using silver_octo.Data;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Claims;

namespace silver_octo.Controllers
{
    [Authorize]
    public class ExpensesController : Controller
    {
        private ApplicationDbContext _context;

        public ExpensesController()
        {
            this._context = new ApplicationDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            // TODO: Not sure what disposing boolean is supposed to be here.
            _context.Dispose();
        }


        public ViewResult Index()
        {
            return View();
        }

        [Route("Expenses/{id}")]
        public ActionResult Details(int id)
        {
            var expense = _context.Expenses.SingleOrDefault(b => b.Id == id);

            if (expense == null)
            {
                return StatusCode(404);
            }

            return View(expense);
        }


        [Route("Expenses/Edit/{id}")]
        public ActionResult Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var expense = _context.Expenses.SingleOrDefault(e => e.Id == id);

            if (expense == null)
            {
                return StatusCode(404);
            }

            var categories = _context.BudgetItems.Where(b => b.ApplicationUserId == userId).ToList();
            var viewModel = new ExpenseFormViewModel()
            {
                Expense = expense,
                BudgetItems = categories
            };
            ViewBag.Title = "Edit Expense Entry";
            return View("ExpenseForm", viewModel);
        }


        public ActionResult New()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var viewModel = new ExpenseFormViewModel()
            {
                Expense = new Expense()
                {
                    EntryDate = DateTime.Now,
                    ExpenseDate = DateTime.Now.Date
                },
                BudgetItems = _context.BudgetItems.Where(b => b.ApplicationUserId == userId).ToList()
            };
            ViewBag.Title = "New Expense Entry";
            return View("ExpenseForm", viewModel);
        }

        //[HttpPost]
        //public ActionResult Create(Expense expense)
        //{
        //    _context.Expenses.Add(expense);
        //    _context.SaveChanges();

        //    return RedirectToAction("Index", "Expenses");
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(Expense expense)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(expense.Name) || expense.BudgetItemId.Equals(null) || !ModelState.IsValid)
            {
                var viewModel = new ExpenseFormViewModel()
                {
                    Expense = expense,
                    BudgetItems = _context.BudgetItems.Where(b => b.ApplicationUserId == userId).ToList()
                };

                return View("ExpenseForm", viewModel);
            }

            expense.ApplicationUserId = userId;

            if (expense.Id == 0) // if new expense
            {
                expense.EntryDate = DateTime.Now;
                _context.Expenses.Add(expense);
            }
            else
            {
                var expenseInDb = _context.Expenses.Single(e => e.Id == expense.Id);
                expenseInDb.Name = expense.Name;
                expenseInDb.ExpenseAmount = expense.ExpenseAmount;
                expenseInDb.ExpenseDate = expense.ExpenseDate;
                expenseInDb.BudgetItemId = expense.BudgetItemId;
            }
            _context.SaveChanges();

            return RedirectToAction("Index", "Expenses");
        }
    }
}
