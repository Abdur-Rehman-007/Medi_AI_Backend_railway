namespace Backend_APIs.Models;

public partial class Doctorleaf
{
    public int Id { get; set; }

    public int DoctorId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string? Reason { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;
}
