// Global aliases mapping OLD Turkish names to NEW English types
// Keep the DB schema untouched while using English types in codebase.

// Entities (old -> new)
global using Ders = Homework_portal.Models.Course;
global using Odev = Homework_portal.Models.Assignment;
global using Teslim = Homework_portal.Models.Submission;
global using DersKayit = Homework_portal.Models.CourseEnrollment;

// ViewModels (old -> new)
global using DersVM = Homework_portal.Models.ViewModels.CourseVM;
global using OdevVM = Homework_portal.Models.ViewModels.AssignmentVM;
global using OdevlerVM = Homework_portal.Models.ViewModels.AssignmentsVM;
global using TeslimVM = Homework_portal.Models.ViewModels.SubmissionVM;
