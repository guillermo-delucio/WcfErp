﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using WcfErp.Modelos;
using WcfErp.Modelos.PVenta;

namespace WcfErp.Servicios.PVenta
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "WcfCierreCaja" en el código, en svc y en el archivo de configuración a la vez.
    // NOTA: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione WcfCierreCaja.svc o WcfCierreCaja.svc.cs en el Explorador de soluciones e inicie la depuración.
    public class WcfCierreCaja : ServiceBase<Movtos_Cajas, EmpresaContext>, IWcfCierreCaja
    {
      
    }
}
