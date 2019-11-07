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
    public class CheckingAccountsController : Controller
    {
        private static readonly Random getrandom = new Random();

        private readonly ApplicationDbContext _context;
        //private readonly IHttpContextAccessor _httpContextAccessor;

        public CheckingAccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CheckingAccounts
        public async Task<IActionResult> Index()
        {
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var checkList = await _context.CheckingAccounts.Where(d => d.UserID == user).ToListAsync();

            return View(checkList);
        }

        // GET: CheckingAccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkingAccount = await _context.CheckingAccounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (checkingAccount == null)
            {
                return NotFound();
            }

            return View(checkingAccount);
        }

        // GET: CheckingAccounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CheckingAccounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Balance,IsOpen,InterestRate")] CheckingAccount checkingAccount)
        {
            if (ModelState.IsValid)
            {
                //var userId = _context._httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
                checkingAccount.UserID = user;
                checkingAccount.IsOpen = true;
                checkingAccount.Balance = 0M;
                checkingAccount.InterestRate = .5M;
                checkingAccount.AccountNumber = randomAccountNumber();
                _context.Add(checkingAccount);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(checkingAccount);
        }

        // GET: CheckingAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            return View();
        }

        // POST: CheckingAccounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Balance,IsOpen,InterestRate")] CheckingAccount checkingAccount)
        {
            return View();
        }

        // GET: CheckingAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checkingAccount = await _context.CheckingAccounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (checkingAccount == null)
            {
                return NotFound();
            }

            return View(checkingAccount);
        }

        // POST: CheckingAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var checkingAccount = await _context.CheckingAccounts.FindAsync(id);
            checkingAccount.IsOpen = false;
            //_context.CheckingAccounts.Remove(checkingAccount);
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

            var checkingAccount = await _context.CheckingAccounts.FindAsync(id);
            if (checkingAccount == null || checkingAccount.UserID != user)
            {
                return NotFound();
            }
            return View(checkingAccount);
          
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(int id, CheckingAccount checkingAccount)
        {
            if (id != checkingAccount.Id)
            {
                return NotFound();
            }

            var currAcct = await _context.CheckingAccounts.FirstOrDefaultAsync(m => m.Id == id);
            if (ModelState.IsValid)
            {
                try
                {
                   
                    var newBalance = currAcct.Balance - checkingAccount.Balance;
                    
                    if(CheckWithdrawOverdraft(currAcct, checkingAccount)) //check if enough funds to withdraw
                    {
                        return View(currAcct);
                    }
                    AddTransaction(currAcct,checkingAccount.Balance,"Withdraw");
                    currAcct.Balance = newBalance;
                    //_context.Update(checkingAccount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CheckingAccountExists(checkingAccount.Id))
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

            var checkingAccount = await _context.CheckingAccounts.FindAsync(id);
            if (checkingAccount == null || checkingAccount.UserID != user)
            {
                return NotFound();
            }
            return View(checkingAccount);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(int id, CheckingAccount checkingAccount)
        {
            if (id != checkingAccount.Id)
            {
                return NotFound();
            }

            var currAcct = await _context.CheckingAccounts.FirstOrDefaultAsync(m => m.Id == id);
            if (ModelState.IsValid)
            {
                try
                {

                    var newBalance = currAcct.Balance + checkingAccount.Balance;
                    if (CheckDepositAmount(currAcct, checkingAccount)) //check if deposit amount positive
                    {
                        return View(currAcct);
                    }

                    AddTransaction(currAcct, checkingAccount.Balance, "Deposit");

                    currAcct.Balance = newBalance;
                    //_context.Update(checkingAccount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CheckingAccountExists(checkingAccount.Id))
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

        public async Task<IActionResult> Transfer(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var checkList = await _context.CheckingAccounts.Where(d => d.UserID == user && d.Id != id).ToListAsync();
            if (checkList.Count == 0)
            {
                return Content("No Accounts");
            }
            return View(checkList);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(int id, CheckingAccount checkingAccount)
        {
           // ViewBag.Amount = 0;
            //var checkList = await _context.CheckingAccounts.Where(d => d.UserID == user && d.Id != id).ToListAsync();
            return Content($"{checkingAccount.AccountNumber} Balance:{checkingAccount.Balance}");
            //if (id != checkingAccount.Id)
            //{
            //    return NotFound();
            //}

            //var currAcct = await _context.CheckingAccounts.FirstOrDefaultAsync(m => m.Id == id);
            //if (ModelState.IsValid)
            //{
            //    try
            //    {

            //        var newBalance = currAcct.Balance + checkingAccount.Balance;
            //        if (CheckDepositAmount(currAcct, checkingAccount)) //check if deposit amount positive
            //        {
            //            return View(currAcct);
            //        }

            //        currAcct.Balance = newBalance;
            //        //_context.Update(checkingAccount);
            //        await _context.SaveChangesAsync();
            //    }
            //    catch (DbUpdateConcurrencyException)
            //    {
            //        if (!CheckingAccountExists(checkingAccount.Id))
            //        {
            //            return NotFound();
            //        }
            //        else
            //        {
            //            throw;
            //        }
            //    }
            //    return RedirectToAction(nameof(Index));
            //}

           // return View();
        }

      

        public async Task<IActionResult> AccountTransactions(int? id)
        {
           
            if (id == null)
            {
                return NotFound();
            }
            // var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var checkingAccount = await _context.CheckingAccounts.FindAsync(id);
            //var transaction = await _context.CheckingAccounts.FindAsync(id);
            var transactionList = await _context.AccountTransactions.Where(d => d.UserID == checkingAccount.UserID && d.AccountNum == checkingAccount.AccountNumber).ToListAsync();

            if (transactionList == null)
            {
                return NotFound();
            }
            return View(transactionList);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AccountTransactions(int? id,DateTime start, DateTime end)
        {
            //var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var checkingAccount = await _context.CheckingAccounts.FindAsync(id);
            //return Content($"{start.ToString()} {end.ToString()} {checkingAccount.AccountNumber}");
            //var transaction = await _context.CheckingAccounts.FindAsync(id);
            var transactionList = await _context.AccountTransactions.Where(d => d.UserID == checkingAccount.UserID && d.AccountNum == checkingAccount.AccountNumber).ToListAsync();
            if((DateTime.Compare(end, start)) < 0 || (DateTime.Compare(start, end) > 0))
            {
                return View(transactionList);
            }
            
            var filterList = new List<Transaction>();
            foreach (var item in transactionList)
            {
                if((DateTime.Compare(start,item.TimeTransaction) < 0) && (DateTime.Compare(end,item.TimeTransaction) > 0))
                {
                    filterList.Add(item);
                }

            }

            return View(filterList);

        }

        private bool CheckingAccountExists(int id)
        {
            return _context.CheckingAccounts.Any(e => e.Id == id);
        }

        private bool CheckWithdrawOverdraft(CheckingAccount currAcct, CheckingAccount checkingAccount)
        {
            if ((currAcct.Balance - checkingAccount.Balance) < 0)
            {
                
                ModelState.AddModelError("", "Not Enough Funds To Withdraw");
                return true;
            }
            else if (checkingAccount.Balance == 0)
            {
                
                ModelState.AddModelError("", "Did not specify Withdraw Amount");
                return true;
            }
            return false;
        }

        private bool CheckDepositAmount(CheckingAccount currAcct, CheckingAccount checkingAccount)
        {
            if (checkingAccount.Balance == 0)
            {
                
                ModelState.AddModelError("", "Did not specify Deposit Amount");
                return true;
            }
            return false;
        }

        private void AddTransaction(CheckingAccount currAcct, decimal balance, string v)
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

        private string randomAccountNumber()
        {
            string acct = "";
            for (int ctr = 0; ctr <= 9; ctr++)
                acct += getrandom.Next(9).ToString();
            return acct;
        }
    }
}
