﻿using System;
using System.Linq;
using System.Web;
using DotnetMvcBoilerplate.Models;
using System.Web.Security;
using System.Configuration;
using DotnetMvcBoilerplate.Core.Provider;
using System.Security.Principal;

namespace DotnetMvcBoilerplate.Core.Security
{
    public class SessionAuthentication : ISessionAuthentication
    {
        private IHttpContextProvider _httpContextProvider;

        /// <summary>
        /// Amount of minutes that a cookie should be active for.
        /// </summary>
        private static int InactivateSessionTimeout
        {
            get { return int.Parse(ConfigurationManager.AppSettings["InactivateSessionTimeout"]); }
        }

        /// <summary>
        /// Amount of days that the cookie should be remembered for.
        /// </summary>
        private static int RememberMeTimeout
        {
            get { return int.Parse(ConfigurationManager.AppSettings["RememberMeTimeout"]); }
        }

        public SessionAuthentication(IHttpContextProvider httpContextProvider)
        {
            _httpContextProvider = httpContextProvider;
        }

        /// <summary>
        /// Creates an encrypted FormsAuthenticationTicket tied to the username and setting the roles onto
        /// the UserData property.
        /// </summary>
        /// <param name="username">Name of the ticket.</param>
        /// <param name="roles">Comma seperated list of the roles that the user belongs too.</param>
        /// <returns>Encrypted hash of a FormsAuthenticationTicket</returns>
        private static string CreateEncryptedTicket(string username, string roles, DateTime expireAt, bool isPersistent)
        {
            var ticket = new FormsAuthenticationTicket(1, username, DateTime.Now, expireAt, isPersistent, roles, FormsAuthentication.FormsCookiePath);
            return FormsAuthentication.Encrypt(ticket);
        }

        /// <summary>
        /// Ends the Session.
        /// </summary>
        public void End()
        {
            try { FormsAuthentication.SignOut(); } catch { }

            _httpContextProvider.Session["LoggedInAs"] = null;
        }

        /// <summary>
        /// Gets the date that the ticket should expire based if the user wants to be remembered
        /// when they next visit the site.
        /// </summary>
        /// <param name="remember">Flag indicating whether the user should be rememebered.</param>
        /// <returns>Expiry date for the ticket.</returns>
        private static DateTime GetExpiryDate(bool remember)
        {
            return (remember) ? DateTime.Now.AddDays(RememberMeTimeout) : DateTime.Now.AddMinutes(InactivateSessionTimeout);
        }

        /// <summary>
        /// Extracts the UserData out of the FormsIdentity
        /// on the current HttpContext and sets the Generic
        /// Principal for the Context so that IsInRole can
        /// function as expected.
        /// </summary>
        public void SetRoles()
        {
            FormsIdentity identity = (FormsIdentity)_httpContextProvider.Context.User.Identity;
            FormsAuthenticationTicket ticket = identity.Ticket;
            string[] roles = ticket.UserData.Split(',');
            _httpContextProvider.Context.User = new GenericPrincipal(identity, roles);
        }

        /// <summary>
        /// Sets data about the User onto the Session
        /// that is stored on the current context.
        /// </summary>
        /// <param name="user">User that the data being
        /// set on the Session.</param>
        public void SetSessionData(User user)
        {
            _httpContextProvider.Session["LoggedInAs"] = user.Username;
        }

        /// <summary>
        /// Authenticates the session, which changes the clients
        /// state to logged in.
        /// </summary>
        /// <param name="user">User that has signed in.</param>
        /// <param name="remember">Whether the user should be
        /// remembered next time they visit.</param>
        public void Start(User user, bool remember)
        {
            var roles = string.Join(",", user.Roles.ToArray());
            var expireDate = GetExpiryDate(remember);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, CreateEncryptedTicket(user.Id.ToString(), roles, expireDate, remember));

            if (remember)
                cookie.Expires = expireDate;

            _httpContextProvider.Response.Cookies.Add(cookie);

            SetSessionData(user);
        }
    }

    public interface ISessionAuthentication
    {
        void End();
        void Start(User user, bool remember);
        void SetRoles();
        void SetSessionData(User user);
    }
}