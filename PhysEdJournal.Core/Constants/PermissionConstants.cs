﻿using PhysEdJournal.Core.Entities.Types;

namespace PhysEdJournal.Core.Constants;

public static class PermissionConstants
{
    public const TeacherPermissions ADD_POINTS_PERMISSIONS = TeacherPermissions.DefaultAccess;
    public const TeacherPermissions ADD_POINTS_FOR_LMS_PERMISSIONS =
        TeacherPermissions.OnlineCourseAccess | TeacherPermissions.AdminAccess;
    public const TeacherPermissions ADD_POINTS_FOR_COMPETITIONS_PERMISSIONS =
        TeacherPermissions.SecretaryAccess | TeacherPermissions.AdminAccess;
    public const TeacherPermissions INCREASE_VISITS_PERMISSIONS = TeacherPermissions.DefaultAccess;
    public const TeacherPermissions ADD_POINTS_FOR_STANDARDS_PERMISSIONS =
        TeacherPermissions.DefaultAccess;
    public const TeacherPermissions ARCHIVE_PERMISSIONS = FOR_ONLY_ADMIN_USE_PERMISSIONS;
    public const TeacherPermissions ADD_TEACHERS_PERMISSIONS = FOR_ONLY_ADMIN_USE_PERMISSIONS;
    public const TeacherPermissions START_NEW_SEMESTER_PERMISSIONS = FOR_ONLY_ADMIN_USE_PERMISSIONS;
    public const TeacherPermissions UPDATE_STUDENTS_INFO_PERMISSIONS =
        FOR_ONLY_ADMIN_USE_PERMISSIONS;
    public const TeacherPermissions FOR_ONLY_ADMIN_USE_PERMISSIONS =
        TeacherPermissions.AdminAccess | TeacherPermissions.SuperUser;
    public const TeacherPermissions FOR_ONLY_SUPERUSER_USE_PERMISSIONS =
        TeacherPermissions.SuperUser;
}
