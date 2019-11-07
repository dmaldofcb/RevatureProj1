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
    public class BusinessAccountsController : Controller
    {
        private static readonly Random getrandom = new Random();

        private readonly ApplicationDbContext _context;

        public BusinessAccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BusinessAccounts
        public async Task<IActionResult> Index()
        {
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var checkList = await _context.BusinessAccounts.Where(d => d.UserID == user).ToListAsync();

            return View(checkList);
        }

        // GET: BusinessAccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var businessAccount = await _context.BusinessAccounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (businessAccount == null)
            {
                return NotFound();
            }

            return View(businessAccount);
        }

        // GET: BusinessAccounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BusinessAccounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Balance,Overdraft,IsOpen,InterestRate,AccountNumber,UserID")] BusinessAccount businessAccount)
        {
            if (ModelState.IsValid)
            {
                var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
                businessAccount.UserID = user;
                businessAccount.IsOpen = true;
                businessAccount.Balance = 0M;
                businessAccount.Overdraft = 0M;
                businessAccount.InterestRate = randomInterestRate();
                businessAccount.AccountNumber = randomAccountNumber();
                _context.Add(businessAccount);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(businessAccount);
        }

        // GET: BusinessAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            return View();
        }

        // POST: BusinessAccounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Balance,Overdraft,IsOpen,InterestRate,AccountNumber,UserID")] BusinessAccount businessAccount)
        {
            return View();
        }

        // GET: BusinessAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var businessAccount = await _context.BusinessAccounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (businessAccount == null)
            {
                return NotFound();
            }

            return View(businessAccount);
        }

        // POST: BusinessAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bussAcct = await _context.BusinessAccounts.FindAsync(id);
            bussAcct.IsOpen = false;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Withdraw(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var bussinessAcct = await _context.BusinessAccounts.FindAsync(id);
            if (bussinessAcct == null || bussinessAcct.UserID != user)
            {
                return NotFound();
            }
            return View(bussinessAcct);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(int id, BusinessAccount businessAcct)
        {
            if (id != businessAcct.Id)
            {
                return NotFound();
            }

            var currAcct = await _context.BusinessAccounts.FirstOrDefaultAsync(m => m.Id == id);
            if (ModelState.IsValid)
            {
                try
                {

                    var newBalance = currAcct.Balance - businessAcct.Balance;

                    if (CheckWithdrawOverdraft(currAcct, businessAcct)) //check if enough funds to withdraw
                    {
                        if (currAcct.Balance > 0) //overdraft the first time
                        {
                            var overdraft = businessAcct.Balance - currAcct.Balance;
                            var overdraftTotal = overdraft + CalculateOverdraftInterest(overdraft, currAcct);
                            currAcct.Overdraft += overdraftTotal; //Initialize Overdraft amount for first time you overdraft
                            newBalance = 0M;
                        }
                        else if (currAcct.Balance == 0)
                        {
                            var overdraft = businessAcct.Balance;
                            var overdraftTotal = overdraft + CalculateOverdraftInterest(overdraft, currAcct);
                            currAcct.Overdraft += overdraftTotal;
                            newBalance = 0M;
                        }
                    }
                    else if (businessAcct.Balance == 0)
                    {
                        ModelState.AddModelError("", "Did not specify Withdraw Amount");
                        return View(currAcct);
                    }
                    // AddTransaction(currAcct, checkingAccount.Balance, "Withdraw");
                    currAcct.Balance = newBalance;
                    //_context.Update(checkingAccount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BusinessAccountExists(businessAcct.Id))
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

        public async Task<IActionResult> Deposit(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var bussAcct = await _context.BusinessAccounts.FindAsync(id);
            if (bussAcct == null || bussAcct.UserID != user)
            {
                return NotFound();
            }
            return View(bussAcct);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(int id, BusinessAccount businessAcct)
        {
            if (id != businessAcct.Id)
            {
                return NotFound();
            }

            var currAcct = await _context.BusinessAccounts.FirstOrDefaultAsync(m => m.Id == id);
            if (ModelState.IsValid)
            {
                try
                {

                    var newBalance = currAcct.Balance + businessAcct.Balance;
                    if (CheckDepositAmount(currAcct, businessAcct)) //check if deposit amount positive
                    {
                        return View(currAcct);
                    }

                   // AddTransaction(currAcct, checkingAccount.Balance, "Deposit");

                    currAcct.Balance = newBalance;
                    //_context.Update(checkingAccount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BusinessAccountExists(businessAcct.Id))
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

        private decimal CalculateOverdraftInterest(decimal overdraft, BusinessAccount currAcct)
        {
            //throw new NotImplementedException();
            decimal amount = 0;
            amount = (overdraft * currAcct.InterestRate);

            return amount;
        }

        private bool CheckWithdrawOverdraft(BusinessAccount currAcct, BusinessAccount bussAcct)
        {
            if (bussAcct.Balance > currAcct.Balance)
            {
                return true;
            }
            return false;
        }

        private bool CheckDepositAmount(BusinessAccount currAcct, BusinessAccount checkingAccount)
        {
            if (checkingAccount.Balance == 0)
            {

                ModelState.AddModelError("", "Did not specify Deposit Amount");
                return true;
            }
            return false;
        }

        private bool BusinessAccountExists(int id)
        {
            return _context.BusinessAccounts.Any(e => e.Id == id);
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
    }
}
