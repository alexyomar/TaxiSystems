using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaxiSystems.Controllers
{
    public class Utils
    {
        ORMDataContext __db = new ORMDataContext();

        public int Disponibilidad()
        {
            var __ret = from unidads in __db.Unidads
                        where
                          !
                            (from carreras in __db.Carreras
                             where
                               Convert.ToString(carreras.FechaCompletado) == ""
                             select new
                             {
                                 carreras.IdUnidad
                             }).Contains(new { IdUnidad = (System.Int32?)unidads.Id })
                        select unidads;

            return __ret.Count();
        }

        public Unidad GetTaxiDisponible()
        {

            var __ret = (from unidads in __db.Unidads
                         where
                           !
                             (from carreras in __db.Carreras
                              where
                                Convert.ToString(carreras.FechaCompletado) == ""
                              select new
                              {
                                  carreras.IdUnidad
                              }).Contains(new { IdUnidad = (System.Int32?)unidads.Id })
                         select unidads).FirstOrDefault();

            return __ret;
        }

        public bool PedirUnidad(int idtaxi, string direccionOrigen, string direccionDestino, int idzonaOrigen, int idZonaDestino, Guid UserID)
        {

            try
            {
                Carrera __data = new Carrera() { DireccionDestino = direccionDestino, DireccionOrigen = direccionOrigen, FechaAsignacion = DateTime.Now, FechaPedido = DateTime.Now, IdUnidad = idtaxi, IdZonaDestino = idZonaDestino, IdZonaOrigen = idzonaOrigen, UserId = UserID };

                __db.Carreras.InsertOnSubmit(__data);
                __db.SubmitChanges();

                return true;
            }
            catch (Exception)
            {

                throw;
            }


        }

        public bool FinalizarRuta(int idcarrera)
        {

            var __item = __db.Carreras.Where(u => u.Id.Equals(idcarrera)).FirstOrDefault();

            __item.FechaCompletado = DateTime.Now;

            __db.SubmitChanges();

            return true;

        }

        public IQueryable<Avance> GetAvances()
        {

            return __db.Avances.OrderBy(u => u.Nombre).AsQueryable();

        }

        public IQueryable<Carrera> GetCarreras()
        {

            return __db.Carreras.AsQueryable();

        }


    }
}