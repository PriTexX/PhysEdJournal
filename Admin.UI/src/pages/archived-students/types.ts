import type { WorkType } from '../points/types';
import type { StandardType } from '../standards/types';

export type VisitsHistory = {
  date: Date;
  points: number;
  teacherGuid: string;
  studentGuid: string;
};

export type StandardsHistory = VisitsHistory & {
  standardType: StandardType;
  comment: string | null;
};

export type PointsHistory = VisitsHistory & {
  workType: WorkType;
  comment: string | null;
};

export type ArchivedStudent = {
  id: number;
  studentGuid: string;
  semesterName: string;
  fullName: string;
  groupNumber: string;
  visits: number;
  totalPoints: number;
  visitsHistory: VisitsHistory[];
  standardsHistory: StandardsHistory[];
  pointsHistory: PointsHistory[];
};
