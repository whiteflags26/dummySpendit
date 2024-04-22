using dummySpendit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;

namespace dummySpendit.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<ActionResult> Index()
        {
            DateTime StartDate = DateTime.Today.AddDays(-6);
            DateTime EndDate = DateTime.Today.AddDays(1);

            //Console.WriteLine(StartDate.ToString());
            //Console.WriteLine(EndDate.ToString());

            List<Transaction> SelectedTransactions = await _context.Transactions
            .Include(x => x.category)
                .Where(y => y.Date >= StartDate && y.Date <= EndDate && y.UserId == _userManager.GetUserId(User))
                .ToListAsync();

            //Console.WriteLine('*');
            //foreach (Transaction transaction in SelectedTransactions)
            //{
            //    Console.WriteLine(transaction.Amount.ToString());
            //}
            //Console.WriteLine('*');

            // Last 7 day income
            int TotalIncome = SelectedTransactions
                .Where(i => i.category.Type == "Income")
                .Sum(j => j.Amount);

            //Console.WriteLine(TotalIncome.ToString());

            ViewBag.TotalIncome = TotalIncome.ToString("c0");

            // Last 7 day expense
            int TotalExpense = SelectedTransactions
                .Where(i => i.category.Type == "Expense")
                .Sum(j => j.Amount);

            ViewBag.Expense = TotalExpense.ToString("c0");

            //Console.WriteLine(TotalExpense.ToString());

            // Last 7 day balance
            int Balance = TotalIncome - TotalExpense;
            int AbsoluteBalance = (Balance < 0) ? Balance * -1 : Balance;

            ViewBag.Balance = (Balance < 0 ? "-" : "") + AbsoluteBalance.ToString("c0");
            ViewBag.TrueBalance = Balance.ToString();

            //Doughnut Chart - Expense By Category
            ViewBag.DoughnutChartData1 = SelectedTransactions
                .Where(i => i.category.Type == "Expense")
                .GroupBy(j => j.category.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().category.Icon + " " + k.First().category.Title,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString("C0"),
                })
                .OrderByDescending(l => l.amount)
                .ToList();
            //Doughnut Chart - Income By Category
            ViewBag.DoughnutChartData2 = SelectedTransactions
                .Where(i => i.category.Type == "Income")
                .GroupBy(j => j.category.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().category.Icon + " " + k.First().category.Title,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString("C0"),
                })
                .OrderByDescending(l => l.amount)
                .ToList();

            //Spline Chart - Income vs Expense

            //Income
            List<SplineChartData> IncomeSummary = SelectedTransactions
                .Where(i => i.category.Type == "Income")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    income = k.Sum(l => l.Amount)
                })
                .ToList();

            //Expense
            List<SplineChartData> ExpenseSummary = SelectedTransactions
                .Where(i => i.category.Type == "Expense")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    expense = k.Sum(l => l.Amount)
                })
                .ToList();

            //Combine Income & Expense
            string[] Last7Days = Enumerable.Range(0, 7)
                .Select(i => StartDate.AddDays(i).ToString("dd-MMM"))
                .ToArray();

            ViewBag.SplineChartData = from day in Last7Days
                                      join income in IncomeSummary on day equals income.day into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty()
                                      join expense in ExpenseSummary on day equals expense.day into expenseJoined
                                      from expense in expenseJoined.DefaultIfEmpty()
                                      select new
                                      {
                                          day = day,
                                          income = income == null ? 0 : income.income,
                                          expense = expense == null ? 0 : expense.expense,
                                      };
            //Recent Transactions
            ViewBag.RecentTransactions = await _context.Transactions
                .Where(h => h.UserId == _userManager.GetUserId(User))
                .Include(i => i.category)
                .OrderByDescending(j => j.Date)
                .Take(10)
                .ToListAsync();


            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
            
    }

}

public class SplineChartData
{
    public string day;
    public int income;
    public int expense;

}
