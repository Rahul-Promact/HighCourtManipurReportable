using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighCourtManipurReportable
{
    public class caseDetail
    {
        public int Id { get; set; }
        public string? Filename { get; set; }
        public string? Court { get; set; }
        public string? Abbr { get; set; }
        public string? CaseNo { get; set; }
        public string? Dated { get; set; }
        public string? CaseName { get; set; }
        public string? Counsel { get; set; }
        public string? Overrule { get; set; }
        public string? OveruleBy { get; set; }
        public string? Citation { get; set; }
        public string? Coram { get; set; }
        public string? Act { get; set; }
        public string? Bench { get; set; }
        public string? Result { get; set; }
        public string? Headnotes { get; set; }
        public string? CaseReferred { get; set; }
        public string? Ssd { get; set; }
        public bool? Reportable { get; set; }
        public string? PdfLink { get; set; }
        public string? Type { get; set; }
        public int? CoramCount { get; set; }
        public string? Petitioner { get; set; }
        public string? Respondent { get; set; }
        public string? BlaCitation { get; set; }
        public string? QrLink { get; set; }
    }
}
