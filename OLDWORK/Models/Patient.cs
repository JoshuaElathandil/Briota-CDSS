namespace PatientPortalApp.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime DateOfEntry { get; set; }
        public string ReportText { get; set; } = string.Empty;
    }
}