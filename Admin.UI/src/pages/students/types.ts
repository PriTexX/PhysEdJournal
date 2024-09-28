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
  'SpecialA',
  'SpecialB',
  'HealthLimitations',
  'Disabled',
] as const;

export type HealthGroup = (typeof healthGroups)[number];

export const healthGroupRus: Record<HealthGroup, string> = {
  None: 'Нет',
  Basic: 'Базовая',
  Preparatory: 'Подготовительная',
  SpecialA: 'Специальная А',
  SpecialB: 'Специальная Б',
  HealthLimitations: 'Ограниченная',
  Disabled: 'Инвалид',
};
