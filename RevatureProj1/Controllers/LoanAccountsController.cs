using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Banking.Models;
using RevatureProj1.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace RevatureProj1.Controllers
{
    [Authorize]
    public class LoanAccountsController : Controller
    {
        private static readonly Random getrandom = new Random();

        private readonly ApplicationDbContext _context;

        public LoanAccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LoanAccounts
        public async Task<IActionResult> Index()
        {
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var loanList = await _context.LoanAccounts.Where(d => d.UserID == user).ToListAsync();

            return View(loanList);
        }

        // GET: LoanAccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loanAccount = await _context.LoanAccounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (loanAccount == null)
            {
                return NotFound();
            }

            return View(loanAccount);
        }

        // GET: LoanAccounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LoanAccounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Balance,IsOpen,InterestRate,AccountNumber,UserID")] LoanAccount loanAccount)
        {
            if (ModelState.IsValid)
            {
                //var userId = _context._httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
                loanAccount.UserID = user;
                loanAccount.IsOpen = true;
                loanAccount.InterestRate = randomInterestRate();
                loanAccount.AccountNumber = randomAccountNumber();
                _context.Add(loanAccount);
                AddTransaction(loanAccount, loanAccount.Balance, "Loan");

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(loanAccount);
        }

        // GET: LoanAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            return View();
        }

        // POST: LoanAccounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Balance,IsOpen,InterestRate,AccountNumber,UserID")] LoanAccount loanAccount)
        {
            return View();
        }

        // GET: LoanAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var loanAccount = await _context.LoanAccounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (loanAccount == null || loanAccount.UserID != user)
            {
                return NotFound();
            }

            return View(loanAccount);
        }

        // POST: LoanAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loanAccount = await _context.LoanAccounts.FindAsync(id);
            loanAccount.IsOpen = false;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DirectDeposit(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var loanAccount = await _context.LoanAccounts.FindAsync(id);
            if (loanAccount == null || loanAccount.UserID != user)
            {
                return NotFound();
            }
            return View(loanAccount);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DirectDeposit(int id, LoanAccount loanAccount)
        {
            if (id != loanAccount.Id)
            {
                return NotFound();
            }

            var currAcct = await _context.LoanAccounts.FirstOrDefaultAsync(m => m.Id == id);
            if (ModelState.IsValid)
            {
                try
                {
                    if(loanAccount.Balance > currAcct.Balance)
                    {
                        ModelState.AddModelError("", "Cannot Pay for more than the loan");
                        return View(currAcct);
                    }

                    var newBalance = currAcct.Balance - loanAccount.Balance;
                    if (CheckDepositAmount(currAcct, loanAccount)) //check if deposit amount positive
                    {
                        return View(currAcct);
                    }

                    AddTransaction(currAcct,loanAccount.Balance, "Direct Payment");

                    currAcct.Balance = newBalance;
                    //_context.Update(checkingAccount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoanAccountExists(loanAccount.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(currAcct);
        }

        public async Task<IActionResult> AccountTransactions(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var loanAccount = await _context.LoanAccounts.FindAsync(id);
            if (loanAccount == null || loanAccount.UserID != user)
            {
                return NotFound();
            }
            var transactionList = await _context.AccountTransactions.Where(d => d.UserID == loanAccount.UserID && d.AccountNum == loanAccount.AccountNumber).ToListAsync();

            if (transactionList == null)
            {
                return NotFound();
            }
            return View(transactionList);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AccountTransactions(int? id, DateTime start, DateTime end, string sort10 = "")
        {
            var loanAccount = await _context.LoanAccounts.FindAsync(id);
            var transactionList = await _context.AccountTransactions.Where(d => d.UserID == loanAccount.UserID && d.AccountNum == loanAccount.AccountNumber).ToListAsync();

            if (sort10.Equals("yes"))
            {
                transactionList.Reverse();
                var filterList10 = transactionList.Take(10);
                return View(filterList10);
            }

            if (start == DateTime.MinValue || end == DateTime.MinValue) //did not enter start or end and pressed submit
            {
                return View(transactionList);
            }

            if ((DateTime.Compare(end, start)) < 0 || (DateTime.Compare(start, end) > 0)) //dates entered not compatible
            {
                return View(transactionList);
            }

            var filterList = new List<Transaction>();
            foreach (var item in transactionList)
            {
                if ((DateTime.Compare(start, item.TimeTransaction) < 0) && (DateTime.Compare(end, item.TimeTransaction) > 0))
                {
                    filterList.Add(item);
                }

            }

            return View(filterList);

        }

        private bool CheckDepositAmount(LoanAccount currAcct, LoanAccount loanAcct)
        {
            if (loanAcct.Balance == 0)
            {

                ModelState.AddModelError("", "Did not specify Deposit Amount");
                return true;
            }
            return false;
        }

        private bool LoanAccountExists(int id)
        {
            return _context.LoanAccounts.Any(e => e.Id == id);
        }

        private string randomAccountNumber()
        {
            string acct = "";
            for (int ctr = 0; ctr <= 9; ctr++)
                acct += getrandom.Next(9).ToString();
            return acct;
        }

        private decimal randomInterestRate()
        {
            double percent = .1 + ((.3 - .1) * getrandom.NextDouble());
            decimal interest = (decimal)(Math.Round(percent, 2)); //to generate random interest rate
            return interest;
        }


        private void AddTransaction(LoanAccount currAcct, decimal balance, string v)
        {
            Transaction newTrans = new Transaction()
            {
                AccountNum = currAcct.AccountNumber,
                Amount = balance,
                Type = v,
                UserID = currAcct.UserID,
                TimeTransaction = DateTime.Now

            };

            _context.AccountTransactions.Add(newTrans);


        }
    }
}
