namespace Karandash.Shared.Enums.Auth;

/* Xarici istifadəçi eyni zamanda sistemdəki birdən çox role'a sahib ola bilər, lakin bizə əsas role lazımdır və bunu ui'da bildirmək məcburiyyətindədir! */
public enum UserRole : byte
{
    Admin = 0,
    Moderator = 1, /* Şərhləri və forumu idarə edən */
    ContentCreator = 2, /* Blog kontenti idarə edən */

    Guest = 3,

    Student = 4,
    Alumni = 5, /* Məzunlar üçün */

    Parent = 6,

    Mentor = 7, /* Mentorluq edənlər üçün */

    Teacher = 8, /* Əsasən, məktəb müəllimləri (ibtidai, orta, lisey) */
    TeacherStaff = 9, /* Məktəb administrativ və digər personal (direktor, müavin, təlim koordinatoru) */

    Lecturer = 10, /* Universitet müəllimləri üçün */
    Professor = 11, /* Universitet professorları üçün */

    UniversityStaff = 12, /* Universitet administrativ və digər personal (rektorluq, dekan, departament rəhbəri) */

    Researcher = 13, /* Elm və tədqiqat fəaliyyəti aparanlar üçün */

    Other = 14
}