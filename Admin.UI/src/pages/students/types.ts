import { number } from 'zod';

export type Student = {
  studentGuid: string;
  fullName: string;
  groupNumber: string;
  hasDebt: boolean;
  hadDebtInSemester: boolean;
  archivedVisitValue: number;
  additionalPoints: number;
  pointsForStandards: number;
  isActive: boolean;
  visits: number;
  course: number;
  currentSemesterName: string;
  healthGroup: HealthGroup;
  department: string | null;
};

export const healthGroups = [
  'None',
  'Basic',
  'Preparatory',
  'Special',
  'HealthLimitations',
];

export type HealthGroup = (typeof healthGroups)[number];

export const healthGroupRus: Record<string, string> = {
  None: 'Нет',
  Basic: 'Базовая',
  Preparatory: 'Подготовительная',
  Special: 'Специальная',
  HealthLimitations: 'Ограниченная',
};
