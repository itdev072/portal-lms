using Portal.Web.Models.Domain;

namespace Portal.Web.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int StudentCount { get; set; }
    public int TeacherCount { get; set; }
    public int LevelCount { get; set; }
    public int ActiveClassSections { get; set; }
}

public class CreateUserViewModel
{
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Student;
}
