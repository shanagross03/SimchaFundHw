using homework_3._4._24.data;
using homework_3._4._24.web.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using System.Diagnostics;

namespace homework_3._4._24.web.Controllers
{
    public class HomeController : Controller
    {
        public SimchaDBManager mgr = new();

        public IActionResult Index()
        {
            return View(new SimchaViewModel()
            {
                Simchas = mgr.GetSimchas(),
                Contributors = mgr.GetContributors(),
                TotalContributors = mgr.GetContributorCount()
            });
        }

        public IActionResult Contributions(int simchaId)
        {
            return View(new ContributorViewModel()
            {
                Contributors = mgr.AddContributedAmountForSimcha(simchaId, mgr.GetContributors()),
                Simcha = mgr.GetSimchas(simchaId).First()
            });
        }

        public IActionResult History(int contribid)
        {
            return View(new TransactionViewModel()
            {
                Transactions = mgr.GetTransactionHistory(contribid),
                Contributor = mgr.GetContributors(contribid).FirstOrDefault()
            }); 
        }

        public IActionResult Contributors()
        {
            return View(new ContributorViewModel()
            {
                Contributors = mgr.GetContributors()
            });
        }

        [HttpPost]
        public IActionResult NewContributor(Contributor c)
        {
            if (c.Id != default)
            {
                mgr.UpdateContributor(c);
            }
            else
            {
                mgr.AddContributor(c);
            }
            return Redirect("/home/contributors");
        }


        [HttpPost]
        public IActionResult NewSimcha(Simcha s)
        {
            mgr.AddSimcha(s);
            return Redirect("/");
        }


        [HttpPost]
        public IActionResult UpdateContributions(List<Contribution> contribution, int simchaId)
        {
            mgr.DeleteContributionsForSimcha(simchaId);
            mgr.AddContributionsForSimcha(contribution);
            return Redirect("/home/index");
        }


        [HttpPost]
        public IActionResult MakeDeposit(Deposit deposit)
        {
            mgr.AddDeposit(deposit);
            return Redirect("/home/contributors");
        }

    }
}