﻿using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WcfErp.Modelos;
using WcfErp.Modelos.Generales;

namespace WcfErp.Servicios.Generales
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "WcfPuestos" en el código, en svc y en el archivo de configuración a la vez.
    // NOTA: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione WcfPuestos.svc o WcfPuestos.svc.cs en el Explorador de soluciones e inicie la depuración.
    public class WcfPuestos : ServiceBase<Puesto, EmpresaContext>, IWcfPuestos
    {
        public Puesto delete(string id)
        {
            throw new NotImplementedException();
        }

        public List<Puesto> searchXDepartamento(string busqueda, string _id)
        {
            try
            {
                MongoClient client = new MongoClient();
                IMongoDatabase db = client.GetDatabase(getKeyToken("empresa","token"));

                IMongoCollection<Puesto> Collection = db.GetCollection<Puesto>(typeof(Puesto).Name);

                //var filter = Builders<SubgrupoComponente>.Filter.Regex("Nombre", new BsonRegularExpression(busqueda, "i")) && ;
                var builder = Builders<Puesto>.Filter;
                var filter = builder.Regex("Nombre", new BsonRegularExpression(busqueda, "i")) & builder.Eq("Departamento._id", _id);

                List<Puesto> Documentos = Collection.Find<Puesto>(filter).ToList();

                return Documentos;
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
