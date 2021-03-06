﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using WcfErp.Modelos.Generales;

namespace WcfErp.Servicios.Inventarios
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IWcfGruposComponentes" in both code and config file together.
    [ServiceContract]
    public interface IWcfGruposComponentes : ServiciosBase<GrupoComponente>
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "?searchBy=getXTipoComponente&busqueda={busqueda}&_id={_id}",
           BodyStyle = WebMessageBodyStyle.Bare,
           ResponseFormat = WebMessageFormat.Json,
           RequestFormat = WebMessageFormat.Json,
           Method = "GET")]
        List<GrupoComponente> searchXTipoComponente(string busqueda, string _id);
    }
}
