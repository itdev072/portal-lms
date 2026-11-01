namespace Portal.Api.Models.Domain;

public enum UserRole
{
    Student,
    Teacher,
    Admin
}

public enum QuizType
{
    MultipleChoice,
    TrueFalse,
    FillInBlank
}

public enum ProgressStatus
{
    NotStarted,
    InProgress,
    Completed
}

public enum AttemptItemType
{
    Quiz,
    Challenge
}

public enum AttendanceStatus
{
    Present,
    OutOfRange,
    Expired,
    InvalidSession,
    Absent,
    Late,
    Excused
}

public enum AttendanceSource
{
    Qr,
    Manual
}

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}

public enum CourseCategory
{
    Programming,
    WebDevelopment,
    DataScience,
    Design,
    Business,
    Language
}
