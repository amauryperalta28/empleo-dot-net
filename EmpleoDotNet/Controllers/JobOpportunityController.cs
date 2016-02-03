﻿using System.Linq;
using System.Web.Mvc;
using EmpleoDotNet.Core.Dto;
using EmpleoDotNet.Helpers;
using EmpleoDotNet.Services;
using EmpleoDotNet.ViewModel;
using EmpleoDotNet.ViewModel.JobOpportunity;

namespace EmpleoDotNet.Controllers
{
    public class JobOpportunityController : EmpleoDotNetController
    {
        private readonly LocationService _locationService;
        private readonly JobOpportunityService _jobOpportunityService;

        public JobOpportunityController()
        {
            _locationService = new LocationService();
            _jobOpportunityService = new JobOpportunityService();
        }
        
        public ActionResult Index(JobOpportunityPagingParameter model)
        {
            var viewModel = GetSearchViewModel(model);

            var jobOpportunities = _jobOpportunityService.GetAllJobOpportunitiesPagedByFilters(model);

            viewModel.Result = jobOpportunities;

            return View(viewModel);
        }
        
        public ActionResult Detail(int? id)
        {
            if (!id.HasValue)
                return RedirectToAction("Index");

            var vm = _jobOpportunityService.GetJobOpportunityById(id);

            if (vm != null)
            {
                ViewBag.RelatedJobs = 
                    _jobOpportunityService.GetCompanyRelatedJobs(id.Value, vm.CompanyName, vm.CompanyEmail, vm.CompanyUrl);

                var cookieView = $"JobView{vm.Id}";
                if (!CookieHelper.Exists(cookieView))
                {
                    _jobOpportunityService.UpdateViewCount(vm.Id);
                    CookieHelper.Set(cookieView, vm.Id.ToString());
                }

                return View("Detail", vm);
            }
                
            ViewBag.ErrorMessage = 
                "La vacante solicitada no existe. Por favor escoger una vacante válida del listado";
            
            return View("Index");
        }

        public ActionResult New()
        {
            var viewModel = new NewJobOpportunityViewModel();

            LoadLocations(viewModel);

            return View(viewModel);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult New(NewJobOpportunityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadLocations(model);
                ViewBag.ErrorMessage = "Han ocurrido errores de validación que no permiten continuar el proceso";
                return View(model);
            }

            _jobOpportunityService.CreateNewJobOpportunity(model.ToEntity());

            return RedirectToAction("Index");
        }
        public ActionResult Wizard()
        {
            var viewModel = new Wizard
            {
                Locations = _locationService.GetAllLocations().ToSelectList(x => x.Id, x => x.Name)
            };
            return View(viewModel);
        }
        [HttpPost, ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Wizard(Wizard model)
        {
            if (!ModelState.IsValid)
            {
                model.Locations = _locationService.GetAllLocations().ToSelectList(x => x.Id, x => x.Name);
                ViewBag.ErrorMessage = "Han ocurrido errores de validación que no permiten continuar el proceso";
                return View(model);
            }
            _jobOpportunityService.CreateNewJobOpportunity(model.ToEntity());
            return RedirectToAction("Index");
        }
        private void LoadLocations(NewJobOpportunityViewModel viewModel)
        {
            var locations = _locationService.GetAllLocations();

            viewModel.Locations = locations.ToSelectList(x => x.Id, x => x.Name);
        }

        /// <summary>
        /// Transform JobOpportunityPagingParameter into JobOpportunitySearchViewModel with Locations
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private JobOpportunitySearchViewModel GetSearchViewModel(JobOpportunityPagingParameter model)
        {
            var locations = _locationService.GetLocationsWithDefault();

            var viewModel = new JobOpportunitySearchViewModel
            {
                Locations = locations.ToSelectList(l => l.Id, l => l.Name, model.SelectedLocation),
                SelectedLocation = model.SelectedLocation,
                JobCategory = model.JobCategory,
                Keyword = model.Keyword,
                IsRemote = model.IsRemote
            };

            return viewModel;
        }
    }
}