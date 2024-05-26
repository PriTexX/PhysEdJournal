export type Student = {
  studentGuid: string;
  fullName: string;
  groupNumber: string;
  hasDebtFromPreviousSemester: boolean;
  hadDebtInSemester: boolean;
  archivedVisitValue: number;
  additionalPoints: number;
  pointsForStandards: number;
  isActive: boolean;
  visits: number;
  course: number;
  currentSemesterName: string;
  healthGroup: 'None' | 'Basic';
  department: string | null;
};
