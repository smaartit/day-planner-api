namespace DayPlannerApi;

public class Task
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Description { get; set; }
    public string Color { get; set; }
    public bool AllDay { get; set; }
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }  
}