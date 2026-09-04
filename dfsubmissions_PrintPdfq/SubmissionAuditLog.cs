using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dfsubmissions_pdfprintq
{
    public class SubmissionAuditLog
    {       
        public string id { get; set; }

        public string SubmissionFormName { get; set; }

        public bool EmailSent { get; set; }

        public bool EmailHadAttachment { get; set; }

        public string EmailAddress { get; set; }

        public string TemplateId { get; set; }

        public DateTime EmailSentOn { get; set; }        
    }
}
