﻿using System;
using System.Web.Mvc;
using DotnetMvcBoilerplate.ViewModels.Install;
using DotnetMvcBoilerplate.Core.Service;
using DotnetMvcBoilerplate.Core.Security;

namespace DotnetMvcBoilerplate.Controllers
{
    public class InstallController : Controller
    {
        private IEncryption _encryption;
        private IUserService _userService;

        public InstallController(IEncryption encryption, IUserService userService)
        {
            _encryption = encryption;
            _userService = userService;
        }

        /// <summary>
        /// Displays the Install form that allows the
        /// user to create the initial admin account.
        /// </summary>
        /// <returns>Form for creating initial admin.</returns>
        [HttpGet]
        public ActionResult Index()
        {
            if (_userService.AtLeastOneExists())
                return Redirect("/");

            return View();
        }

        /// <summary>
        /// Attempts to validate the model to install the
        /// web application, if valid then an admin user is
        /// created. If the model isn't valid then the client
        /// is returned to the install form. Once the primary
        /// admin account has been created, the user is redirect
        /// to the website.
        /// </summary>
        /// <param name="model">Data entered by the client.</param>
        /// <returns>Redirect the user to the site on success, or
        /// redirects the user back to the form when invalid.</returns>
        [HttpPost]
        public ActionResult Index(InstallViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _userService.Create(model.ToUser(_encryption));

            return Redirect("/");
        }
    }
}
