﻿using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WcfErp.Modelos;
using WcfErp.Modelos.PuntoVenta;
using WcfErp.Modelos.PVenta;
using WcfErp.Modelos.Inventarios;
using WcfErp.Modelos.Reportes.Inventarios;

namespace WcfErp.Servicios.PVenta
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "WcfPuntoVenta_Documento" en el código, en svc y en el archivo de configuración a la vez.
    // NOTA: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione WcfPuntoVenta_Documento.svc o WcfPuntoVenta_Documento.svc.cs en el Explorador de soluciones e inicie la depuración.
    public class WcfPuntoVenta_Documento : ServiceBase<PuntoVenta_Documento, EmpresaContext>,  IWcfPuntoVenta_Documento
    {
        public override PuntoVenta_Documento add(PuntoVenta_Documento item)
        {
            try
            {
                EmpresaContext db = new EmpresaContext();

                using (var session = db.client.StartSession())
                {
                    session.StartTransaction();

                    item.Folio = AutoIncrement("Ticket", db.db, session).ToString();


                    /*Colocar el tipo de cambio dependiendo de la moneda*/
                    var builder = Builders<TipodeCambio>.Sort;
                    var filter = builder.Descending("Fecha");

                    List<TipodeCambio> Lst_Tipos = db.TipodeCambio.Filters(filter);
                    String monedaAnterior = "";
                    foreach (PuntoVtaCobros cobro in item.PuntoVtaCobros)
                    {
                        Boolean entro = false;
                        String moneda = "MXN";
                        if (cobro.Tipo.Contains("DLS"))
                            moneda = "DLS";
                        foreach (TipodeCambio cambio in Lst_Tipos)
                        {
                            if (monedaAnterior != moneda)
                                entro = false;

                            if (cambio.Moneda.Simbolo == moneda && entro == false)
                            {
                                cobro.TipodeCambio = cambio;
                                entro = true;
                                monedaAnterior = moneda;
                            }
                        }
                    }

                    /*Salida de inventario*/
                    MovimientosES documentosalida = SalidaInventario(item, db);
                    WcfErp.Servicios.Inventarios.Inventarios inv = new WcfErp.Servicios.Inventarios.Inventarios();

                    inv.add(documentosalida, db, session);
                    db.PuntoVenta_Documento.add(item, db, session);

                    session.CommitTransaction();
                }

                return item;
            }
            catch (Exception ex)
            {
                Error(ex, "");
                return null;
            }
        }

        public PuntoVenta_Documento CrearDevolucion(PuntoVenta_Documento item)
        {
            try
            {
                EmpresaContext db = new EmpresaContext();

                using (var session = db.client.StartSession())
                {
                    session.StartTransaction();

                    PuntoVenta_Documento venta = db.PuntoVenta_Documento.get(item._id, db);

                    if (venta.Estatus == "CANCELADO")
                        throw new Exception("Esta venta ya se encuentra devuelta, no es posible continuar");


                    /*Entrada de inventario*/
                    MovimientosES documentoentrada = EntradaInventario(item, db);
                    WcfErp.Servicios.Inventarios.Inventarios inv = new WcfErp.Servicios.Inventarios.Inventarios();

                    item.Estatus = "CANCELADO";
                    inv.add(documentoentrada, db, session);
                    db.PuntoVenta_Documento.update(item, item._id, db, session);

                    session.CommitTransaction();
                }

                return item;
            }
            catch (Exception ex)
            {
                Error(ex, "");
                return null;
            }
        }
       
        public Movtos_Cajas ValidarApertura()
        {
            try
            {
                string usuario = getKeyToken("user", "token");

                EmpresaContext db = new EmpresaContext();

                var builder = Builders<Cajeros>.Filter;
                var filter = builder.Eq("Usuarios.Nombre", usuario);

                List<Cajeros> LstCajeros = db.Cajeros.find(filter, db).ToList();

                if (LstCajeros.Count == 0)
                    throw new Exception("El usuario no es cajero");

                //una vez que es cajero, se tiene que ver si tiene una apertura con una caja
                //si tiene una apertura, que busque la caja a la que hace referencia para ver si está abierta
                //si no está abierta, que haga una apertura
                //si está abierta, que retorne la apertura

                foreach (Cajeros caj in LstCajeros)
                {
                    var builder_Mtocajas = Builders<Movtos_Cajas>.Filter;
                    var filter_Mtocajas = builder_Mtocajas.Eq("TipoMovto", "Apertura") & builder_Mtocajas.Eq("Cajeros._id", caj._id);

                    List<Movtos_Cajas> LstCajasAbiertas = db.Movtos_Cajas.find_apertura(filter_Mtocajas, 5, "_id,Cajas,Cajeros", db).ToList();

                    if (LstCajasAbiertas.Count == 0)
                        throw new Exception("El cajero no tiene apertura de caja");

                    foreach (Movtos_Cajas abierta in LstCajasAbiertas)
                    {
                        var builder_cajas = Builders<Cajas>.Filter;
                        var filter_cajas = builder_cajas.Eq("_id", abierta.Cajas._id) & builder_cajas.Eq("Estado", "ABIERTA");

                        List<Cajas> LstCajas = db.Cajas.find(filter_cajas, db).ToList();

                        if (LstCajas.Count != 0)
                           return abierta;
                    }
                }
                throw new Exception("El cajero no tiene apertura de caja");
            }
            catch (Exception ex)
            {
                Error(ex, "");
                return null;
            }
        }

        public List<PuntoVenta_Documento> ComprasACancelar(string cadena, string skip = null)
        {
            try
            {
                EmpresaContext db = new EmpresaContext();
                var builder = Builders<PuntoVenta_Documento>.Filter;
                var filter = builder.Ne("Estatus","CANCELADO");

                List<PuntoVenta_Documento> enviar = new List<PuntoVenta_Documento> { };
                List<PuntoVenta_Documento> Lista = db.PuntoVenta_Documento.Filters(filter, cadena, skip);
                foreach(PuntoVenta_Documento venta in Lista)
                {

                    PuntoVenta_Documento vta = db.PuntoVenta_Documento.get(venta._id, "PuntoVtaCobros", db);
                    Boolean bo = true;
                    foreach (PuntoVtaCobros cobro in vta.PuntoVtaCobros)
                    {
                        if (!cobro.Tipo.Contains("EFECTIVO"))
                        {
                            bo = false;
                            break;
                        }
                    }
                    if (bo)
                    {
                        enviar.Add(venta);
                    }
                }

                return enviar;
            }
            catch (Exception ex)
            {
                Error(ex, "");
                return null;
            }
        }

        public MovimientosES SalidaInventario(PuntoVenta_Documento item, EmpresaContext db)
        {
            try
            {
                MovimientosES documentosalida = new MovimientosES();
                documentosalida.Concepto = db.Concepto.get("5d4cbb5d92a3d9c568660d2a", db); ////Concepto de salida por venta de mostrador
                documentosalida.Almacen = item.Almacen;
                documentosalida.Fecha = item.Fecha;
                documentosalida.Descripcion = "Salida de inventario de venta de mostrador con el Folio " + item.Folio;
                documentosalida.Sistema_Origen = "PV";
                documentosalida.Cancelado = "NO";


                //Separo la fecha del doumento en dia mes y año
                documentosalida.Dia = item.Fecha.Day;
                documentosalida.Mes = item.Fecha.Month;
                documentosalida.Ano = item.Fecha.Year;

                foreach (PuntoVtaDet detalle in item.PuntoVtaDet)
                {
                    //Se crea el articulo dentro del documento de salida
                    documentosalida.Detalles_ES.Add(new Detalles_ES
                    {
                        Articulo = detalle.Articulo,
                        Cantidad = Math.Abs((double)detalle.Cantidad),
                        Clave = detalle.Articulo.Clave,
                    });
                }
                return documentosalida;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public MovimientosES EntradaInventario(PuntoVenta_Documento item, EmpresaContext db)
        {
            try
            {
                MovimientosES documentoentrada = new MovimientosES();
                documentoentrada.Concepto = db.Concepto.get("5dfba6592938b55b30c3c257", db); ////Concepto de entrada por devolución
                documentoentrada.Almacen = item.Almacen;
                documentoentrada.Fecha = DateTime.Now;
                documentoentrada.Descripcion = "Entrada de inventario por devolución de mercancía por venta de mostrador con el Folio " + item.Folio;
                documentoentrada.Sistema_Origen = "DEV";
                documentoentrada.Cancelado = "NO";


                //Separo la fecha del doumento en dia mes y año
                documentoentrada.Dia = documentoentrada.Fecha.Day;
                documentoentrada.Mes = documentoentrada.Fecha.Month;
                documentoentrada.Ano = documentoentrada.Fecha.Year;

                foreach (PuntoVtaDet detalle in item.PuntoVtaDet)
                {
                    //Se crea el articulo dentro del documento de salida
                    documentoentrada.Detalles_ES.Add(new Detalles_ES
                    {
                        Articulo = detalle.Articulo,
                        Cantidad = Math.Abs((double)detalle.Cantidad),
                        Clave = detalle.Articulo.Clave,
                    });
                }
                return documentoentrada;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<PuntoVenta_Documento> CrearCancelacion(List<PuntoVenta_Documento> items)
        {
            try
            {
                EmpresaContext db = new EmpresaContext();

                string vtaCan = "";
                using (var session = db.client.StartSession())
                {
                    session.StartTransaction();
                    foreach(PuntoVenta_Documento item in items)
                    {
                        PuntoVenta_Documento venta = db.PuntoVenta_Documento.get(item._id, db);

                        if (venta.Estatus != "CANCELADOXT" && venta.Estatus != "CANCELADO")
                        {
                            /*Entrada de inventario*/
                            MovimientosES documentoentrada = EntradaInventario(item, db);
                            WcfErp.Servicios.Inventarios.Inventarios inv = new WcfErp.Servicios.Inventarios.Inventarios();

                            item.Estatus = "CANCELADOXT";
                            inv.add(documentoentrada, db, session);
                            db.PuntoVenta_Documento.update(item, item._id, db, session);
                        }
                        else
                        {
                            vtaCan += venta.Folio + ",";
                        }
                    }
                    session.CommitTransaction();
                }
                if(vtaCan != "")
                {
                    throw new Exception("Las siguientes ventas ya se encuentran canceladas: " + vtaCan);
                }
                return items;
            }
            catch (Exception ex)
            {
                Error(ex, "");
                return null;
            }
        }
    }
}
