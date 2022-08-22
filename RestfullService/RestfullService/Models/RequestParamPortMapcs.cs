namespace RestfullService.Models
{
    public class RequestParamPortMapcs
    {

        public class RequestParameter
        {

            public XdlRamReadFile.FileTypeEnum FileType { get; set; }

            public string ProposedTemplate { get; set; }



        }


        public class PortMapping
        {
            public enum CircuitsEnum
            {
                FL = 0,
                FR,
                RL,
                RR,
                // skip outlet valves
                MCI1 = 8,
                MCI2
            }

            public CircuitsEnum PortA { get; set; }

            public CircuitsEnum PortB { get; set; }

            public CircuitsEnum PortC { get; set; }

            public CircuitsEnum PortD { get; set; }

            public CircuitsEnum PortE { get; set; }

            public CircuitsEnum PortF { get; set; }
        }

    }


}

