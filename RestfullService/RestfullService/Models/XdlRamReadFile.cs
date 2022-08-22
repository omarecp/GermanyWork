namespace RestfullService.Models
{
    public class XdlRamReadFile
    {
        public enum FileTypeEnum
        {
            PS,
            FLUX,
            YA,
            RAM_FS_INFO,
            PLANT_XML
        }

        private FileTypeEnum _fileType;
        private string _content;
        private string _filename;

        public FileTypeEnum FileType
        {
            get { return _fileType; }
            set { _fileType = value; }
        }


        public string swConfig { get; set; }

        public string Content
        {
            get { return _content; }
            set { _content = value; }
        }

        public string FileName
        {
            get { return _filename; }
            set { _filename = value; }
        }


        public string FilledTemplate { get; set; }


        public string ProposedTemplate { get; set; }


        public string CouldHaveFilledTemplate { get; set; }


        public string ErrorMessage { get; set; }


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

        public class RequestParameterPortMapping
        {
            RequestParameter RequestParameter { get; set; }

            PortMapping PortMapping { get; set; }


        }

    }
}
