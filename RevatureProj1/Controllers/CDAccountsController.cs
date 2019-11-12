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
using RevatureProj1.ViewModels;


namespace RevatureProj1.Controllers
{
    [Authorize]
    public class CDAccountsController : Controller
    {
        private static readonly Random getrandom = new Random();

        private readonly ApplicationDbContext _context;

        public CDAccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CDAccounts
        public async Task<IActionResult> Index()
        {
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cdAccounts = await _context.TermAccounts.Where(d => d.UserID == user).ToListAsync();

            return View(cdAccounts);
        }

        // GET: CDAccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cDAccounts = await _context.TermAccounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cDAccounts == null)
            {
                return NotFound();
            }

            return View(cDAccounts);
        }

        // GET: CDAccounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CDAccounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Balance,IsOpen,InterestRate,AccountNumber,MaturityDate,CreationDate,UserID")] CDAccounts cDAccounts)
        {
            if (ModelState.IsValid)
            {
                //var userId = _context._httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
                cDAccounts.UserID = user;
                cDAccounts.IsOpen = true;
                cDAccounts.CreationDate = DateTime.Today;
                cDAccounts.InterestRate = randomInterestRate();
                cDAccounts.AccountNumber = randomAccountNumber();
                _context.Add(cDAccounts);
                AddTransaction(cDAccounts, cDAccounts.Balance, "CD Deposit");
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cDAccounts);
        }

        // GET: CDAccounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            return View();
        }

        // POST: CDAccounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Balance,IsOpen,InterestRate,AccountNumber,MaturityDate,CreationDate,UserID")] CDAccounts cDAccounts)
        {
            return View();
        }

        // GET: CDAccounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cDAccounts = await _context.TermAccounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cDAccounts == null || cDAccounts.UserID != user)
            {
                return NotFound();
            }

            return View(cDAccounts);
        }

        // POST: CDAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cDAccounts = await _context.TermAccounts.FindAsync(id);
            cDAccounts.IsOpen = false;
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

            var cdAccount = await _context.TermAccounts.FindAsync(id);
            if (cdAccount == null || cdAccount.UserID != user)
            {
                return NotFound();
            }
            return View(cdAccount);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(int id, CDAccounts cDAccount)
        {
            if (id != cDAccount.Id)
            {
                return NotFound();
            }

            var currAcct = await _context.TermAccounts.FirstOrDefaultAsync(m => m.Id == id);
            if (ModelState.IsValid)
            {
                try
                {
                    var newBalance = currAcct.Balance - cDAccount.Balance;

                    int num = DateTime.Compare(DateTime.Today, currAcct.MaturityDate);
                    if (num < 0)
                    {
                        ModelState.AddModelError("", "Cannot Withdraw Maturity Date has not been reached");
                        return View(currAcct);
                    }

                    if (CheckWithdrawOverdraft(currAcct, cDAccount)) //check if enough funds to withdraw
                    {
                        return View(currAcct);
                    }
                    AddTransaction(currAcct, cDAccount.Balance, "Direct Withdraw");
                    currAcct.Balance = newBalance;
                    //_context.Update(checkingAccount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CDAccountsExists(cDAccount.Id))
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
             //return Content(currAcct.AccountNumber);

            return View(currAcct);
        }

        public async Task<IActionResult> AccountTransactions(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cDAccounts = await _context.TermAccounts.FindAsync(id);
            if (cDAccounts == null || cDAccounts.UserID != user)
            {
                return NotFound();
            }
            var transactionList = await _context.AccountTransactions.Where(d => d.UserID == cDAccounts.UserID && d.AccountNum == cDAccounts.AccountNumber).ToListAsync();

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
            var cDAccounts = await _context.TermAccounts.FindAsync(id);
            var transactionList = await _context.AccountTransactions.Where(d => d.UserID == cDAccounts.UserID && d.AccountNum == cDAccounts.AccountNumber).ToListAsync();

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

        public async Task<IActionResult> Transfer(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ViewBag.Account = "chk";
            ViewBag.Success = false;

            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currAcct = await _context.TermAccounts.FirstOrDefaultAsync(m => m.Id == id);

            var checkList = await _context.CheckingAccounts.Where(d => d.UserID == user && d.IsOpen != false).ToListAsync();
            var bussinesList = await _context.BusinessAccounts.Where(d => d.UserID == user && d.IsOpen != false).ToListAsync();
            var loanList = await _context.LoanAccounts.Where(d => d.UserID == user && d.IsOpen != false && d.Balance > 0).ToListAsync();
            ViewBag.AcctInfo = currAcct;
            CheckingTransferViewModel transferList = new CheckingTransferViewModel()
            {
                CheckingList = checkList,
                BusinessList = bussinesList,
                LoanList = loanList
            };

            if (checkList == null || bussinesList == null || loanList == null)
            {
                return NotFound();
            }

            return View(transferList);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(int id, string accountTransfer, decimal amount)
        {
            var currAcct = await _context.TermAccounts.FirstOrDefaultAsync(m => m.Id == id);
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.Success = false;

            ViewBag.AcctInfo = currAcct;
            var checkList = await _context.CheckingAccounts.Where(d => d.UserID == user && d.IsOpen != false).ToListAsync();
            var bussinesList = await _context.BusinessAccounts.Where(d => d.UserID == user && d.IsOpen != false).ToListAsync();
            var loanList = await _context.LoanAccounts.Where(d => d.UserID == user && d.IsOpen != false && d.Balance > 0).ToListAsync();

            CheckingTransferViewModel transferList = new CheckingTransferViewModel()
            {
                CheckingList = checkList,
                BusinessList = bussinesList,
                LoanList = loanList
            };

            if (ModelState.IsValid)
            {

                var checkAccount = await _context.CheckingAccounts.FirstOrDefaultAsync(m => m.UserID == currAcct.UserID && m.AccountNumber == accountTransfer);
                var bussiAccount = await _context.BusinessAccounts.FirstOrDefaultAsync(m => m.UserID == currAcct.UserID && m.AccountNumber == accountTransfer);
                var loanAccount = await _context.LoanAccounts.FirstOrDefaultAsync(m => m.UserID == currAcct.UserID && m.AccountNumber == accountTransfer);

                int num = DateTime.Compare(DateTime.Today, currAcct.MaturityDate);
                if (num < 0)
                {
                    ModelState.AddModelError("", "Cannot Transfer, Maturity Date has not been reached");
                    return View(transferList);
                }

                if (CheckWithdrawOverdraft(currAcct, amount)) //check if enough funds to withdraw
                {
                    return View(transferList);
                }

                var newBalance = currAcct.Balance - amount;
                if (checkAccount != null)
                {

                    var newDestBalance = checkAccount.Balance + amount;
                    currAcct.Balance = newBalance;
                    checkAccount.Balance = newDestBalance;

                    AddTransaction(checkAccount, amount, "Transfer In");
                    AddTransaction(currAcct, amount, "Transfer Out");

                    await _context.SaveChangesAsync();

                    ViewBag.Success = true;
                    ViewBag.AccountNumber = currAcct.AccountNumber;
                    ViewBag.Amount = amount;

                    return View(transferList);
                }

                if (bussiAccount != null)
                {
                    var newDestBalance = 0M;
                    if (bussiAccount.Overdraft > 0)
                    {
                        if (amount > bussiAccount.Overdraft)
                        {
                            var newAmount = amount - bussiAccount.Overdraft;
                            bussiAccount.Overdraft = 0M;
                            newDestBalance = bussiAccount.Balance + newAmount;
                        }
                        else
                        {
                            bussiAccount.Overdraft -= amount;
                        }
                    }
                    else
                    {
                        newDestBalance = bussiAccount.Balance + amount;
                    }

                    currAcct.Balance = newBalance;
                    bussiAccount.Balance = newDestBalance;
                    AddTransaction(bussiAccount, amount, "Transfer In");
                    AddTransaction(currAcct, amount, "Transfer Out");

                    await _context.SaveChangesAsync();

                    ViewBag.Success = true;
                    ViewBag.AccountNumber = currAcct.AccountNumber;
                    ViewBag.Amount = amount;

                    return View(transferList);
                }


                if (loanAccount != null)
                {
                    if (amount > loanAccount.Balance)
                    {
                        ModelState.AddModelError("", "Transfer amount is greater than balance in Loan Account");
                        return View(transferList);
                    }
                    var newDestBalance = loanAccount.Balance - amount;
                    currAcct.Balance = newBalance;
                    loanAccount.Balance = newDestBalance;

                    AddTransaction(loanAccount, amount, "Transfer In");
                    AddTransaction(currAcct, amount, "Transfer Out");

                    await _context.SaveChangesAsync();

                    ViewBag.Success = true;
                    ViewBag.AccountNumber = currAcct.AccountNumber;
                    ViewBag.Amount = amount;

                    return View(transferList);

                }
            }

            return View(transferList);
        }

        private bool CDAccountsExists(int id)
        {
            return _context.TermAccounts.Any(e => e.Id == id);
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

        private bool CheckWithdrawOverdraft(CDAccounts currAcct, CDAccounts cdAcct)
        {
            if ((currAcct.Balance - cdAcct.Balance) < 0)
            {

                ModelState.AddModelError("", "Not Enough Funds To Withdraw");
                return true;
            }
            else if (cdAcct.Balance == 0)
            {

                ModelState.AddModelError("", "Did not specify Withdraw Amount");
                return true;
            }
            return false;
        }

        private void AddTransaction(CDAccounts currAcct, decimal balance, string v)
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

        private void AddTransaction(BusinessAccount currAcct, decimal balance, string v)
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

        private bool CheckWithdrawOverdraft(CDAccounts currAcct, decimal amount)
        {
            if ((currAcct.Balance - amount) < 0)
            {

                ModelState.AddModelError("", "Not Enough Funds To Withdraw");
                return true;
            }
            else if (amount == 0)
            {

                ModelState.AddModelError("", "Did not specify Withdraw Amount");
                return true;
            }
            else if (amount < 0)
            {
                ModelState.AddModelError("", "Amount Can't be negative");
                return true;
            }
            return false;
        }
    }
}
