﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WcfErp.Modelos.Generales
{
    public class Marca : ModeloBase<Marca, EmpresaContext>
    {
        public string Clave { get; set; }
    }
}