using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestfullService.Models;
using System.Collections.Generic;
using RestfullService.Data;
using static RestfullService.Models.XdlRamReadFile;

namespace RestfullService.Controllers
{
    [Route("api/ReadRamFileInfo")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IResftfullServiceData _resftfullServiceData = new IResftfullServiceData();
        [HttpPost]
        public ActionResult<IEnumerable<XdlRamReadFile[]>> GetFiles(string swConfig,[FromBody] RequestParameter[] requestParameters,  [FromQuery] PortMapping portMapping)
        {
            var GetFileItems = _resftfullServiceData.GetFiles(swConfig, requestParameters, portMapping);
            return Ok(GetFileItems);
        }


       // public ActionResult<IEnumerable<XdlRamReadFile_Model>> GetserviceInfo()
       // {
        //    var infoItems = _resftfullServiceData.GetServiceInfo();

          //  return Ok(infoItems);

        //}

        [HttpGet("{name}")]
        public ActionResult<IEnumerable<XdlRamReadFile>> GetCommandByName(string name)
        {
            var nameItem = _resftfullServiceData.GetCommandByName(name);
            return Ok(nameItem);
        }

    }
}
