using CONTROLADORA;
using ENTIDADES;
using PROYECTO_WEB.TESIS.Filters;
using Microsoft.AspNetCore.Mvc;
using PROYECTO_WEB.TESIS.Helpers;

namespace PROYECTO_WEB.TESIS.Controllers
{
    [AuthFilter]
    public class ClienteController2 : Controller
    {
        // Renombrado para no colisionar con ClienteController (admin)
        // En el routing se mapea como "Cliente"
    }
}