﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WcfErp.Modelos;
using WcfErp.Modelos.Generales;

namespace WcfErp.Servicios.Generales
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "WcfVendedor" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select WcfVendedor.svc or WcfVendedor.svc.cs at the Solution Explorer and start debugging.
    public class WcfVendedor : ServiceBase<Vendedor, EmpresaContext>, IWcfVendedor
    {
        public Vendedor add(Vendedor item)
        {
            throw new NotImplementedException();
        }

        public List<Vendedor> all()
        {
            throw new NotImplementedException();
        }

        public Vendedor delete(string id)
        {
            throw new NotImplementedException();
        }

        public Vendedor get(string id)
        {
            throw new NotImplementedException();
        }

        public Vendedor update(Vendedor item, string id)
        {
            throw new NotImplementedException();
        }
    }
}
