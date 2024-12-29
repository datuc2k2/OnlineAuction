using System.Runtime.CompilerServices;

namespace BE_AuctionAOT.Common.Base.Entity
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
            this.BuilderObject = builderObject;
            this.ResultCd = code;
        }

        public OutputBuilder<TCatalogName> ValidationMessage(List<OutputMessage> validationErrors)
        {
            if (this.ValidationList is null)
            {
                this.ValidationList = new List<OutputMessage>();
            }

            this.ValidationList.AddRange(validationErrors);
            return this;
        }

        public OutputBuilder<TCatalogName> ValidationMessage(string information, string messageCD, params string[] parameter)
        {
            if (this.ValidationList is null)
            {
                this.ValidationList = new List<OutputMessage>();
            }

            this.ValidationList.AddRange(new List<OutputMessage>
        {
            new OutputMessage { MessageCd = messageCD, Information = information, Parameters = parameter },
        });

            return this;
        }

        public OutputBuilder<TCatalogName> CommonMessage(string messageCd, params string[] parameters)
        {
            if (this.CommonList is null)
            {
                this.CommonList = new List<OutputMessage>();
            }

            this.CommonList.Add(new OutputMessage { MessageCd = messageCd, Parameters = parameters });
            return this;
        }

        public OutputBuilder<TCatalogName> CommonMessageWithInfo(string info, string messageCd, params string[] parameters)
        {
            if (this.CommonList is null)
            {
                this.CommonList = new List<OutputMessage>();
            }

            this.CommonList.Add(new OutputMessage { Information = info, MessageCd = messageCd, Parameters = parameters });
            return this;
        }

        public OutputBuilder<TCatalogName> WithException(Exception error)
        {
            this.Error = error;
            return this;
        }

        public TOutputDto Create<TOutputDto>()
            where TOutputDto : BaseOutputDto, new()
        {
            var output = new TOutputDto();

            output.ResultCd = this.ResultCd;

            output.Exception = this.Error;

            if (this.CommonList is not null || this.ValidationList is not null)
            {
                output.Messages = new OutputMessages
                {
                    Common = this.CommonList,
                    Validation = this.ValidationList,
                };

            }

            return output;
        }



    }
}