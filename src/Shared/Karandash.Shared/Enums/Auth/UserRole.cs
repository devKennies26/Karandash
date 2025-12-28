namespace Karandash.Shared.Enums.Auth;

/* Xarici istifadəçi eyni zamanda sistemdəki birdən çox role'a sahib ola bilər, lakin bizə əsas role lazımdır və bunu ui'da bildirmək məcburiyyətindədir! */
public enum UserRole : byte
{
    Admin = 0,
    Moderator = 1, /* Admindən sonra sistem idarəetməsindən cavabdeh olan */
    ContentCreator = 2, /* Blog və onun kontentlərini idarə edən */

    Student = 3, /* Ya məktəb, ya da universitet şagirdləri/tələbələri üçün */
    Alumni = 4, /* Məzunlar üçün */

    Parent = 5, /* Valideynlər üçün */

    Mentor = 6, /* Mentorluq edənlər üçün */

    Teacher = 7, /* Əsasən, məktəb müəllimləri (ibtidai, orta, lisey) üçün */
    TeacherStaff = 8, /* Məktəb administrativ və digər personal (direktor, müavin, təlim koordinatoru) */

    Lecturer = 9, /* Universitet müəllimləri üçün */
    Professor = 10, /* Universitet professorları üçün */

    UniversityStaff =
        11, /* Universitet administrativ və digər personal (rektorluq, dekan, departament rəhbəri) heyəti üçün */

    Researcher = 12, /* Elm və tədqiqat fəaliyyəti aparanlar üçün */

    Other = 13 /* Sadalanan role'lardan heç birinə uyğun olmayanlar üçün */
}