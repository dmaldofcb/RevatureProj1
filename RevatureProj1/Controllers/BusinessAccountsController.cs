﻿using System;
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
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var businessAccount = await _context.BusinessAccounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (businessAccount == null || businessAccount.UserID != user)
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
                            AddTransaction(currAcct, overdraftTotal, "Overdraft");
                            AddTransaction(currAcct, currAcct.Balance, "Withdraw");
                            currAcct.Balance = newBalance;
                            await _context.SaveChangesAsync();
                        }
                        else if (currAcct.Balance == 0)
                        {
                            var overdraft = businessAcct.Balance;
                            var overdraftTotal = overdraft + CalculateOverdraftInterest(overdraft, currAcct);
                            currAcct.Overdraft += overdraftTotal;
                            newBalance = 0M;
                            AddTransaction(currAcct, overdraftTotal, "Overdraft");
                            currAcct.Balance = newBalance;
                            await _context.SaveChangesAsync();
                        }
                    }
                    else if (businessAcct.Balance == 0)
                    {
                        ModelState.AddModelError("", "Did not specify Withdraw Amount");
                        return View(currAcct);
                    }
                    else
                    {
                        AddTransaction(currAcct, businessAcct.Balance, "Withdraw");
                        currAcct.Balance = newBalance;
                        await _context.SaveChangesAsync();
                    }
                    
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
                    decimal newBalance = 0;
                    if (CheckDepositAmount(currAcct, businessAcct)) //check if deposit amount positive
                    {
                        return View(currAcct);
                    }
                    if (currAcct.Overdraft > 0)
                    {
                        if(businessAcct.Balance > currAcct.Overdraft) // pay off the entire overdraft and deposit whats left
                        {
                            newBalance = businessAcct.Balance - currAcct.Overdraft;
                            AddTransaction(currAcct, currAcct.Overdraft, "Overdraft Pay");
                            currAcct.Overdraft = 0M;
                            AddTransaction(currAcct, newBalance, "Deposit");

                        }
                        else if(businessAcct.Balance < currAcct.Overdraft)
                        {
                            currAcct.Overdraft -= businessAcct.Balance;
                            AddTransaction(currAcct, businessAcct.Balance, "Overdraft Pay");

                        }

                    }
                    else
                    {
                        newBalance = currAcct.Balance + businessAcct.Balance;
                        AddTransaction(currAcct, businessAcct.Balance, "Deposit");
                    }


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

        public async Task<IActionResult> AccountTransactions(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }
             var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var businessAcct = await _context.BusinessAccounts.FindAsync(id);
            if (businessAcct == null || businessAcct.UserID != user)
            {
                return NotFound();
            }
            var transactionList = await _context.AccountTransactions.Where(d => d.UserID == businessAcct.UserID && d.AccountNum == businessAcct.AccountNumber).ToListAsync();

            if (transactionList == null)
            {
                return NotFound();
            }
            return View(transactionList);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AccountTransactions(int? id, DateTime start, DateTime end, string sort10="")
        {
            var bussAcct = await _context.BusinessAccounts.FindAsync(id);
            var transactionList = await _context.AccountTransactions.Where(d => d.UserID == bussAcct.UserID && d.AccountNum == bussAcct.AccountNumber).ToListAsync();

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

            ViewBag.Account = "bus";
            ViewBag.Success = false;

            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currAcct = await _context.BusinessAccounts.FirstOrDefaultAsync(m => m.Id == id);

            var checkList = await _context.CheckingAccounts.Where(d => d.UserID == user && d.IsOpen != false).ToListAsync();
            var bussinesList = await _context.BusinessAccounts.Where(d => d.UserID == user && d.Id != id && d.IsOpen != false).ToListAsync();
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
            var currAcct = await _context.BusinessAccounts.FirstOrDefaultAsync(m => m.Id == id);
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.Success = false;

            ViewBag.AcctInfo = currAcct;
            var checkList = await _context.CheckingAccounts.Where(d => d.UserID == user && d.IsOpen != false).ToListAsync();
            var bussinesList = await _context.BusinessAccounts.Where(d => d.UserID == user && d.Id != id && d.IsOpen != false).ToListAsync();
            var loanList = await _context.LoanAccounts.Where(d => d.UserID == user && d.IsOpen != false && d.Balance > 0).ToListAsync();

            CheckingTransferViewModel transferList = new CheckingTransferViewModel()
            {
                CheckingList = checkList,
                BusinessList = bussinesList,
                LoanList = loanList
            };

            if (amount < 0)
            {
                ModelState.AddModelError("", "Amount Can't be negative");
                return View(transferList);
            }

            if (ModelState.IsValid)
            {

                var checkAccount = await _context.CheckingAccounts.FirstOrDefaultAsync(m => m.UserID == currAcct.UserID && m.AccountNumber == accountTransfer);
                var bussiAccount = await _context.BusinessAccounts.FirstOrDefaultAsync(m => m.UserID == currAcct.UserID && m.AccountNumber == accountTransfer);
                var loanAccount = await _context.LoanAccounts.FirstOrDefaultAsync(m => m.UserID == currAcct.UserID && m.AccountNumber == accountTransfer);


                var newBalance = 0M;
                if(currAcct.Overdraft > 0)
                {
                    var overdraftTotal = amount + CalculateOverdraftInterest(amount, currAcct);
                    currAcct.Overdraft += overdraftTotal;
                }
                else
                {
                    if(amount > currAcct.Balance)
                    {
                        var newAmount = amount - currAcct.Balance;
                        var overdraftTotal = newAmount + CalculateOverdraftInterest(newAmount, currAcct);
                        currAcct.Overdraft += overdraftTotal;
                    }
                    else
                    {
                        newBalance = currAcct.Balance - amount;
                    }
                }

               //  var newBalance = currAcct.Balance - amount;

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


    }
}
