using System.Collections.Generic;
using RestfullService.Models;
using static RestfullService.Models.XdlRamReadFile;

namespace RestfullService.Data
{
    public interface IRestfullService
    {
        IEnumerable<XdlRamReadFile> GetServiceInfo();
        XdlRamReadFile[] GetFiles(string swConfig, RequestParameter[] requestParameters, PortMapping portMapping);
        XdlRamReadFile GetCommandByName(string name);
    }
}
