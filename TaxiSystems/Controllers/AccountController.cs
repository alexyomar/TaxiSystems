using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;


namespace TaxiSystems.Controllers
{
    public class AccountController : Controller
    {

        // Vistas Parciales

        public ActionResult PartialLogOn()
        {
            return PartialView();
        }

        public ActionResult PartialRegister(Guid? Id)
        {
            ORMDataContext __db = new ORMDataContext();
            ViewData.Model = __db.Zonas.ToList();

            if (Id.HasValue)
            {
                MembershipUser __user = Membership.GetUser();
                ViewBag.Usuario = __db.Usuarios.Where(u => u.UserID.Equals(Guid.Parse(__user.ProviderUserKey.ToString()))).FirstOrDefault();
            }
            return PartialView();
        }

        public ActionResult PartialChoice()
        {
            return PartialView();
        }

        //
        // POST: /Account/LogOn

        [HttpPost]
        public ActionResult LogOn(string UserName, string Password, string RememberMe)
        {
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(UserName, Password))
                {
                    bool __recuerdame = false;

                    if (RememberMe.Equals("on"))
                        __recuerdame = true;

                    FormsAuthentication.SetAuthCookie(UserName, __recuerdame);

                    return RedirectToAction("Index", "Cliente");
                }
                else
                {
                    ViewBag.Error = "Nombre de usuario o contraseña incorrecta.";
                }
            }


            return PartialView("PartialLogon");
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Register(string UserName, string Email, string Password, string ConfirmPassword, string Nombre, string Direccion, int Zona, string TelefonoMovil, string TelefonoCasa, Guid? UserId, string OldPassword)
        {

            ORMDataContext db = new ORMDataContext();
            if (UserId.HasValue)
            {
                MembershipUser __user = Membership.GetUser();
                var usuario = db.Usuarios.Where(u => u.UserID.Equals(Guid.Parse(__user.ProviderUserKey.ToString()))).FirstOrDefault();

                usuario.aspnet_Membership.Email = Email;
                usuario.Nombre = Nombre;
                usuario.Direccion = Direccion;
                usuario.IdZona = Zona;
                usuario.TelefonoMovil = TelefonoMovil;
                usuario.TelefonoCasa = TelefonoCasa;

                if (OldPassword.Length > 1 && Password.Equals(ConfirmPassword))
                {

                    var __msp = Membership.GetUser(usuario.aspnet_Membership.aspnet_User.UserName);
                    bool __password = __msp.ChangePassword(OldPassword, Password);
                }

                return RedirectToAction("Index", "Cliente");


            }
            else
            {
                // Intento de registrar al usuario
                MembershipCreateStatus createStatus;
                Membership.CreateUser(UserName, Password, Email, null, null, true, null, out createStatus);
                MembershipUser __user = Membership.GetUser(UserName);

                if (createStatus == MembershipCreateStatus.Success)
                {
                    try
                    {
                        // Insertamos en base de datos

                        db.Usuarios.InsertOnSubmit(new Usuario()
                        {
                            UserID = (Guid)__user.ProviderUserKey,
                            Nombre = Nombre,
                            Direccion = Direccion,
                            TelefonoCasa = TelefonoCasa,
                            TelefonoMovil = TelefonoMovil,
                            IdZona = Zona
                        });

                        db.SubmitChanges();

                        FormsAuthentication.SetAuthCookie(UserName, false);
                        return RedirectToAction("Index", "Cliente");
                    }
                    catch (Exception ex)
                    {
                        Membership.DeleteUser(UserName);
                        ViewBag.Error = ex.Message;
                        ViewData.Model = db.Zonas.ToList();
                        return PartialView("PartialRegister");
                    }

                }
                else
                {
                    ViewBag.Error = ErrorCodeToString(createStatus);
                }

                ORMDataContext db2 = new ORMDataContext();
                ViewData.Model = db2.Zonas.ToList();
                //Si llega aqui fue porque algo salio mal.
                return PartialView("PartialRegister");
            }
        }

        //
        // GET: /Account/ChangePassword

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/ChangePassword

        [Authorize]
        [HttpPost]
        //public ActionResult ChangePassword(ChangePasswordModel model)
        //{
        //    if (ModelState.IsValid)
        //    {

        //        // ChangePassword will throw an exception rather
        //        // than return false in certain failure scenarios.
        //        bool changePasswordSucceeded;
        //        try
        //        {
        //            MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
        //            changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
        //        }
        //        catch (Exception)
        //        {
        //            changePasswordSucceeded = false;
        //        }

        //        if (changePasswordSucceeded)
        //        {
        //            return RedirectToAction("ChangePasswordSuccess");
        //        }
        //        else
        //        {
        //            ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
        //        }
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}

        //
        // GET: /Account/ChangePasswordSuccess

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }



        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
