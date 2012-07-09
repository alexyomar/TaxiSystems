using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace TaxiSystems.Controllers
{
    public class ClienteController : Controller
    {
        //
        // GET: /Cliente/

        public ActionResult Index()
        {
            ORMDataContext __db = new ORMDataContext();
            Utils __u = new Utils();
            MembershipUser __user = Membership.GetUser();
            ViewData.Model = __db.Usuarios.Where(u => u.UserID.Equals(Guid.Parse(__user.ProviderUserKey.ToString()))).FirstOrDefault();
            ViewBag.Disponibilidad = __u.Disponibilidad();
            return View();
        }

        public ActionResult PedirTaxi()
        {
            ORMDataContext __db = new ORMDataContext();
            Utils __u = new Utils();
            MembershipUser __user = Membership.GetUser();

            ViewData.Model = __u.GetTaxiDisponible();
            ViewBag.Zonas = __db.Zonas.OrderBy(u => u.Zona1).ToList();
            ViewBag.Usuario = __db.Usuarios.Where(u => u.UserID.Equals(Guid.Parse(__user.ProviderUserKey.ToString()))).FirstOrDefault();
            return View();
        }

        [HttpPost]
        public ActionResult PedirTaxiHandler(string DireccionOrigen, string DireccionDestino, int IdZonaOrigen, int IdZonaDestino, int idTaxi)
        {

            ORMDataContext __db = new ORMDataContext();
            Utils __u = new Utils();
            MembershipUser __user = Membership.GetUser();

            bool ret = __u.PedirUnidad(idTaxi, DireccionOrigen, DireccionDestino, IdZonaOrigen, IdZonaDestino, (Guid)__user.ProviderUserKey);

            return RedirectToAction("Historial");
        }

        public ActionResult Historial()
        {
            ORMDataContext __db = new ORMDataContext();
            Utils __u = new Utils();
            MembershipUser __user = Membership.GetUser();

            if (__user.UserName.Contains("operador"))
                return RedirectToAction("Rutas");
            else
                ViewData.Model = __db.Carreras.Where(u => u.UserId.Equals((Guid)__user.ProviderUserKey)).OrderByDescending(s => s.FechaPedido);

            return View();
        }

        public ActionResult Rutas()
        {
            Utils __u = new Utils();

            ViewData.Model = __u.GetCarreras().OrderByDescending(s => s.FechaPedido);

            return View();
        }

        [HttpPost]
        public ActionResult Rutas(int Id)
        {
            ORMDataContext __db = new ORMDataContext();
            Utils __u = new Utils();

            var __ret = __u.FinalizarRuta(Id);


            return RedirectToAction("Rutas");
        }

        public ActionResult Avances()
        {

            Utils __u = new Utils();

            ViewData.Model = __u.GetAvances();

            return View();

        }

        public ActionResult EditarAvance(Guid? Id)
        {

            ORMDataContext __db = new ORMDataContext();

            Utils __u = new Utils();

            ViewBag.Unidad = __db.Unidads.OrderBy(u => u.Id).ToList();


            if (Id.HasValue)
                ViewData.Model = __u.GetAvances().Where(u => u.UserID.Equals(Id.Value)).FirstOrDefault();


            return View();


        }

        [HttpPost]
        public ActionResult EditarAvance(Guid? UserID, string UserName, string Email, string OldPassword, string Password, string ConfirmPassword, string Nombre, string Direccion, string TelefonoMovil, string TelefonoCasa, int Unidad, string Cedula)
        {
            ORMDataContext __db = new ORMDataContext();

            if (UserID.HasValue)
            {

                var __user = __db.Avances.Where(u => u.UserID.Equals(UserID.Value)).FirstOrDefault();

                if (OldPassword.Length > 1 && Password.Equals(ConfirmPassword))
                {

                    var __msp = Membership.GetUser(__user.aspnet_Membership.aspnet_User.UserName);
                    bool __password = __msp.ChangePassword(OldPassword, Password);
                }

                __user.aspnet_Membership.Email = Email;
                __user.aspnet_Membership.LoweredEmail = Email.ToLower();
                __user.Nombre = Nombre;
                __user.Direccion = Direccion;
                __user.TelefonoCasa = TelefonoCasa;
                __user.TelefonoMovil = TelefonoMovil;
                __user.IdUnidad = Unidad;

                __db.SubmitChanges();

                return RedirectToAction("Avances", "Cliente");

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
                        ORMDataContext db = new ORMDataContext();
                        db.Avances.InsertOnSubmit(new Avance()
                        {
                            UserID = (Guid)__user.ProviderUserKey,
                            Nombre = Nombre,
                            Direccion = Direccion,
                            TelefonoCasa = TelefonoCasa,
                            TelefonoMovil = TelefonoMovil,
                            IdUnidad = Unidad,
                            Cedula = Cedula
                        });

                        db.SubmitChanges();

                        return RedirectToAction("Avances", "Cliente");
                    }
                    catch (Exception ex)
                    {
                        Membership.DeleteUser(UserName);
                        ViewBag.Error = ex.Message;
                        ORMDataContext db = new ORMDataContext();
                        ViewData.Model = db.Zonas.ToList();
                        return View();
                    }

                }
                else
                {
                    ViewBag.Error = createStatus.ToString();
                }

            }

            return View();
        }

        public ActionResult ActivarAvance(Guid Id)
        {
            var __user = Membership.GetUser(Id);

            __user.IsApproved = !__user.IsApproved;

            Membership.UpdateUser(__user);

            return RedirectToAction("Avances");

        }

        public ActionResult Unidades()
        {
            ORMDataContext __db = new ORMDataContext();

            ViewData.Model = __db.Unidads.OrderBy(u => u.Id).ToList();

            return View();
        }

        public ActionResult EditUnidades()
        {
            return View();
        }

        [HttpPost]
        public ActionResult EditUnidades(string Marca, string Modelo, string Placa, int Anno)
        {

            ORMDataContext __db = new ORMDataContext();

            __db.Unidads.InsertOnSubmit(new Unidad() { Año = Anno, Modelo = Modelo, Marca = Marca, Placa = Placa });
            __db.SubmitChanges();

            return RedirectToAction("Unidades");
        }

    }
}
