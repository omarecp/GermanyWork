using System.Collections.Generic;
using RestfullService.Models;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ContiTeves.Common;
using ContiTeves.EDM.Common;
using ContiTeves.EDM.Client.EDMClient.FailOver;
using ContiTeves.Common.Util;
using ContiTeves.EDM.Common.Util;
using ContiTeves.Productionline.Misc;
using static RestfullService.Models.XdlRamReadFile;
using ContiTeves.EDM;
using EdmFileType = ContiTeves.EDM.EdmFileType;
using EdmMetaDataType = ContiTeves.EDM.EdmMetaDataType;

namespace RestfullService.Data

{
    public class IResftfullServiceData : IRestfullService
        {
        public IEnumerable<XdlRamReadFile> GetServiceInfo()
        {
            var info = new List<XdlRamReadFile>

            {
             new XdlRamReadFile {}
            };
            return info;
        }

        public XdlRamReadFile[] GetFiles(string swConfig, XdlRamReadFile.RequestParameter[] requestParameters, XdlRamReadFile.PortMapping portMapping)
        {
            var resp = GetInfosFromEdm(swConfig);
            string swName = resp.SwName;
            string xmlContent = resp.FileContent;
            string failsafeInfo = resp.FailsafeInfo;
            return GetFilesWoEdm(swConfig, swName, xmlContent, requestParameters, portMapping, failsafeInfo);
        }

        public XdlRamReadFile GetCommandByName(string name)
        {
            
            
              return new XdlRamReadFile { ProposedTemplate = name };
            

           
        }








        private XdlRamReadFile[] GetFilesWoEdm(string swConfig, string swName,
          string xmlContent, RequestParameter[] requestParameters, PortMapping portMapping, string failsafeInfo)
        {
            List<XdlRamReadFile> resp = new List<XdlRamReadFile>();
            // create valve sorting handler
            ValveSorting valveSorting = new ValveSorting();
            valveSorting.ValveSortingDictionary = new Dictionary<ValveSorting.Ports, ValveSorting.LogicalMeaning>()
            {
            {ValveSorting.Ports.E_INLET_VALVE, (ValveSorting.LogicalMeaning)portMapping.PortE},
            {ValveSorting.Ports.D_INLET_VALVE, (ValveSorting.LogicalMeaning)portMapping.PortD},
            {ValveSorting.Ports.C_INLET_VALVE, (ValveSorting.LogicalMeaning)portMapping.PortC},
            {ValveSorting.Ports.F_INLET_VALVE, (ValveSorting.LogicalMeaning)portMapping.PortF},
            {ValveSorting.Ports.A_CUT_VALVE, (ValveSorting.LogicalMeaning)portMapping.PortA},
            {ValveSorting.Ports.B_CUT_VALVE, (ValveSorting.LogicalMeaning)portMapping.PortB}
            };
            // loop over requested files
            foreach (RequestParameter reqParam in requestParameters)
            {   
                string outputFilename = string.Empty;
                string filter = string.Empty;
                GetFilterAndFilename(reqParam.FileType, out filter, out outputFilename);
                //Changed this to use New Environment 
                // Environment.CurrentDirectory = AppContext.BaseDirectory;
                string dirStr = Path.Combine(Environment.CurrentDirectory,
                    @"App_Data\XPCRamReadTemplateFiles");
                var dirInfo = new DirectoryInfo(dirStr);
                var res = RamFileCreator.FindAndFillXprtFiles(xmlContent, filter, dirInfo,
                    true, reqParam.ProposedTemplate, swConfig, swName);
                #region Resort ECU-CAL values (in a pretty dirty way)
                if (res.FilledTemplateName.Contains("FLUX") && !res.FilledTemplateName.Contains("FLUX_MKC"))
                {
                    if (!string.IsNullOrEmpty(res.XpcFileContent))
                    {
                        // resort the ECU_CAL values
                        StringReader sr = new StringReader(res.XpcFileContent);
                        List<string> lines = new List<string>();
                        string line = string.Empty;
                        while (null != (line = sr.ReadLine()))
                        {
                            lines.Add(line);
                        }
                        int idx = lines.FindLastIndex(FindReadLines);
                        int startIdx = idx - 5;
                        string[] unsortedEcuCalLines = lines.GetRange(startIdx, 6).ToArray();
                        try
                        {
                            string[] sortedEcuCalLines =
                                valveSorting.ResortCalibValues(unsortedEcuCalLines);
                            lines.RemoveRange(startIdx, 6);
                            lines.InsertRange(startIdx, sortedEcuCalLines);
                            StringBuilder sb = new StringBuilder();
                            foreach (string li in lines)
                            {
                                sb.AppendLine(li);
                            }
                            res.XpcFileContent = sb.ToString();
                        }
                        catch (Exception)
                        {
                            res.ErrorMessage = "Cannot sort ECU_CAL data. Please check your port selection!";
                        }
                    }
                }
                #endregion
                resp.Add(new XdlRamReadFile()
                {
                    FileType = reqParam.FileType,
                    FileName = outputFilename,
                    Content = res.XpcFileContent,
                    ErrorMessage = res.ErrorMessage,
                    FilledTemplate = res.FilledTemplateName,
                    CouldHaveFilledTemplate = res.CouldHaveFilledTemplateName,
                    ProposedTemplate = reqParam.ProposedTemplate
                });
            }
            resp.Add(new XdlRamReadFile()
            {
                FileType = XdlRamReadFile.FileTypeEnum.RAM_FS_INFO,
                FileName = "",
                Content = failsafeInfo,
                ErrorMessage = "",
                FilledTemplate = "",
                CouldHaveFilledTemplate = "",
                ProposedTemplate = ""
            });
            resp.Add(new XdlRamReadFile()
            {
                FileType = XdlRamReadFile.FileTypeEnum.PLANT_XML,
                FileName = "",
                Content = xmlContent,
                ErrorMessage = "",
                FilledTemplate = "",
                CouldHaveFilledTemplate = "",
                ProposedTemplate = ""
            });
            return resp.ToArray();
        }

        // Explicit predicate delegate.
        private static bool FindReadLines(string line)
        {
            if (line.StartsWith("?RT"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool GetFilterAndFilename(XdlRamReadFile.FileTypeEnum requestFileType,
          out string filter, out string outputFilename)
        {
            switch (requestFileType)
            {
                case XdlRamReadFile.FileTypeEnum.PS:
                    outputFilename = "ReadPS.xpc";
                    filter = "*PS*";
                    break;
                case XdlRamReadFile.FileTypeEnum.FLUX:
                    outputFilename = "ReadFLUX.xpc";
                    filter = "*FLUX*";
                    break;
                case XdlRamReadFile.FileTypeEnum.YA:
                    outputFilename = "ReadYA.xpc";
                    filter = "*YA*";
                    break;
                default:
                    filter = string.Empty;
                    outputFilename = string.Empty;
                    throw new ArgumentOutOfRangeException(nameof(requestFileType), requestFileType, null);
            }
            return true;
        }

        public class RamReadFileCreationResult
        {
            public string XpcFileContent { get; set; }
            public string FilledTemplateName { get; set; }
            public string ErrorMessage { get; set; }
        }

        private string ResortTemplateFile(StreamReader sr, ValveSorting valveSorting)
        {
            // resort template file according to valve layout
            string line;
            StringBuilder sb = new StringBuilder();
            List<string> ecuCalLines = new List<string>();
            while (null != (line = sr.ReadLine()))
            {
                Match match = Regex.Match(line, "ECUCAL_RESULTS_[0-9]");
                if (match.Value != string.Empty)
                {
                    ecuCalLines.Add(match.Value);
                }
                sb.AppendLine(line);
            }
            // resort lines with 'ECUCAL_RESULTS_'
            // standard sorting 
            // ECUCAL_RESULTS 0   1   2   3   4   5 
            // ECU drawing    E   D   C   F   A   B
            // circuits       FL  FR  RL  RR  DK  SK 
            string[] sortedEcuCalLines = valveSorting.ResortCalibValues(ecuCalLines.ToArray());
            // replace ECUCAL_RESULTS_ in template according to new order
            string newTemplateContent = sb.ToString();
            int currentTdx;
            for (int i = 0; i < ecuCalLines.Count; i++)
            {
                int idx = newTemplateContent.LastIndexOf(ecuCalLines[i]);
                int len = ecuCalLines[i].Length;
                newTemplateContent = newTemplateContent.Remove(idx, len).Insert(idx, sortedEcuCalLines[i]);
            }
            return newTemplateContent;
        }

        private class EdmInfos
        {
            public string SwName { get; set; }
            public string FileContent { get; set; }
            public string FailsafeInfo { get; set; }
        }

        /// <summary>
        /// Get the needed infos from EDM
        /// </summary>
        /// <param name="swConfig">SW-Config for which the infos are requested</param>
        /// <returns>string[] { SW-Name, Content of RAM-Data.XML }</returns>
        private EdmInfos GetInfosFromEdm(string swConfig)
        {
            dynamic reqList = new List<EdmFileType>(new EdmFileType[] {
                     EdmFileType.RamData });
            dynamic metaDataReqList = new List<EdmMetaDataType>(new EdmMetaDataType[] {
                     EdmMetaDataType.ControllerCode_FileName,
                     EdmMetaDataType.FailSafeType,
                     EdmMetaDataType.RamFailSafeAreas
            });
            string fileContent = string.Empty;
            string swName = string.Empty;
            string failsafeInfo = "No information";
            ////service object
            ReadServiceFailOver service = CreateReadServiceInstance(
                "jens.radtke@continental-corporation.com", "conti", Properties.Settings.Default.EdmServiceUrl);
            DateTime readServiceCallTime;
            DateTime readServiceFinishTime;
            string responseServer;
            ContiTeves.EDM.Client.EDMClient.FailOver.EdmResponseContainer resp = service.GetEdmData(swConfig,
               reqList, metaDataReqList, out readServiceCallTime, out readServiceFinishTime, out responseServer);
            if (resp.EdmFiles.Count > 0)
            {
                if (resp.EdmFiles[0].FileBytes != null)
                {
                    fileContent = Encoding.UTF8.GetString(resp.EdmFiles[0].FileBytes);
                }
            }
            if (resp?.EdmMetaData.Count > 0)
            {
                bool dynFs = false;
                foreach (dynamic md in resp?.EdmMetaData)
                {
                    switch (md.EdmMetaDataType)
                    {
                        case EdmMetaDataType.RamFailSafeAreas:
                            if ((md.ErrorCode == 0) && (!dynFs))
                            {
                                failsafeInfo = "";
                                string xmlSerializedAddressDataArray = (string)md.Value;
                                var als = (AddressLength[])CommonUtils.DeserializeFromXmlString(
                                    typeof(AddressLength[]), xmlSerializedAddressDataArray);
                                SortedDictionary<int, AddressLength> sortedAls = new SortedDictionary<int, AddressLength>();
                                foreach (var al in als)
                                {
                                    sortedAls.Add(al.Address, al);
                                }
                                foreach (var al in sortedAls.Values)
                                {
                                    failsafeInfo += al.ToString() + System.Environment.NewLine;
                                }
                            }

                            break;

                        case EdmMetaDataType.FailSafeType:
                            if ((FailSafeType)md.Value == FailSafeType.DynamicFailsafe)
                            {
                                dynFs = true;
                                failsafeInfo = "Dynamic failsafe - you don't need addresses.";
                            }
                            break;

                        case EdmMetaDataType.ControllerCode_FileName:
                            if (!string.IsNullOrEmpty(md.Value.ToString()))
                            {
                                swName = Path.GetFileNameWithoutExtension(md.Value.ToString());
                            }
                            break;
                    }
                }
            }
            return new EdmInfos() { SwName = swName, FileContent = fileContent, FailsafeInfo = failsafeInfo };
        }

        private ReadServiceFailOver CreateReadServiceInstance(string login,
        string password, string serviceUrls)
        {
            ////service object declaration
            ReadServiceFailOver service = null;
            ////instanciate service with login/pwd
            if (login != null || password != null)
            {
                if (serviceUrls != null)
                {
                    service = new ReadServiceFailOver(login, password, serviceUrls);
                }
                else
                {
                    service = new ReadServiceFailOver(login, password);
                }
            }
            else
            {
                // login/pwd in config file
                service = new ReadServiceFailOver();
            }
            return service;
        }


    }




}


    







