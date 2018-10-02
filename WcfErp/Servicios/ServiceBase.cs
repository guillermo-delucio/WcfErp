﻿using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WcfErp.Modelos;
using WcfErp.Modelos.Generales;

namespace WcfErp.Servicios
{
    public class ServiceBase<Modelo>
    {
        public virtual Modelo add(Modelo item)
        {
            try
            {
                MongoClient client = new MongoClient();
                IMongoDatabase db = client.GetDatabase("PAMC861025DB7");

                IMongoCollection<Modelo> CollectionClientes = db.GetCollection<Modelo>(typeof(Modelo).Name);

                CollectionClientes.InsertOne(item);

                return item;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public virtual List<Modelo> search(string busqueda)
        {
            try
            {
                MongoClient client = new MongoClient();
                IMongoDatabase db = client.GetDatabase("PAMC861025DB7");

                IMongoCollection<Modelo> Collection = db.GetCollection<Modelo>(typeof(Modelo).Name);

                var filter = Builders<Modelo>.Filter.Regex("Nombre", new BsonRegularExpression(busqueda, "i"));

                List<Modelo> Documentos = Collection.Find<Modelo>(filter).ToList();

                return Documentos;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //Metodo para dar respuesta las peticiones OPTION CORS
        public void GetOptions()
        {
        }
    }
}