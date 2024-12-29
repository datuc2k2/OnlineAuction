using System.Runtime.CompilerServices;

namespace BE_AuctionOT_Cronjob.Common.Base.Entity
{
    public static class ComponentExtenstion
    {
        public static OutputBuilder<TCatalogName> Output<TCatalogName>(this TCatalogName builderObject, int resultCd)
        {
            return new OutputBuilder<TCatalogName>(builderObject, resultCd);
        }

    }
    public class OutputBuilder<TCatalogName>
    {

        private TCatalogName BuilderObject { get; set; }

        private int ResultCd { get; set; }

        private Exception Error { get; set; }

        private List<OutputMessage> CommonList { get; set; }

        private List<OutputMessage> ValidationList { get; set; }

        internal OutputBuilder(TCatalogName builderObject, int code)
        {
            BuilderObject = builderObject;
            ResultCd = code;
        }

        public OutputBuilder<TCatalogName> ValidationMessage(List<OutputMessage> validationErrors)
        {
            if (ValidationList is null)
            {
                ValidationList = new List<OutputMessage>();
            }

            ValidationList.AddRange(validationErrors);
            return this;
        }

        public OutputBuilder<TCatalogName> ValidationMessage(string information, string messageCD, params string[] parameter)
        {
            if (ValidationList is null)
            {
                ValidationList = new List<OutputMessage>();
            }

            ValidationList.AddRange(new List<OutputMessage>
        {
            new OutputMessage { MessageCd = messageCD, Information = information, Parameters = parameter },
        });

            return this;
        }

        public OutputBuilder<TCatalogName> CommonMessage(string messageCd, params string[] parameters)
        {
            if (CommonList is null)
            {
                CommonList = new List<OutputMessage>();
            }

            CommonList.Add(new OutputMessage { MessageCd = messageCd, Parameters = parameters });
            return this;
        }

        public OutputBuilder<TCatalogName> CommonMessageWithInfo(string info, string messageCd, params string[] parameters)
        {
            if (CommonList is null)
            {
                CommonList = new List<OutputMessage>();
            }

            CommonList.Add(new OutputMessage { Information = info, MessageCd = messageCd, Parameters = parameters });
            return this;
        }

        public OutputBuilder<TCatalogName> WithException(Exception error)
        {
            Error = error;
            return this;
        }

        public TOutputDto Create<TOutputDto>()
            where TOutputDto : BaseOutputDto, new()
        {
            var output = new TOutputDto();

            output.ResultCd = ResultCd;

            output.Exception = Error;

            if (CommonList is not null || ValidationList is not null)
            {
                output.Messages = new OutputMessages
                {
                    Common = CommonList,
                    Validation = ValidationList,
                };

            }

            return output;
        }



    }
}