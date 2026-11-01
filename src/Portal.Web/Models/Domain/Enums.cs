namespace Portal.Web.Models.Domain;

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
    InvalidSession
}

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}
