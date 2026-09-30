export interface SupportServiceOption {
  code: string;
  name: string;
}

export const SUPPORT_SERVICES: readonly SupportServiceOption[] = [
  { code: 'PERSONAL_CARE', name: 'Personal Care Support' },
  { code: 'DAILY_LIVING', name: 'Assistance with Daily Living' },
  { code: 'HOUSEHOLD_TASKS', name: 'Household Tasks' },
  { code: 'MEAL_PREPARATION', name: 'Meal Preparation' },
  { code: 'COMMUNITY_ACCESS', name: 'Community Access' },
  { code: 'SOCIAL_PARTICIPATION', name: 'Social & Community Participation' },
  { code: 'GROUP_ACTIVITIES', name: 'Group Activities' },
  { code: 'TRANSPORT_ASSISTANCE', name: 'Transport Assistance' },
  { code: 'LIFE_SKILLS', name: 'Life Skills Development' },
  { code: 'SUPPORT_COORDINATION', name: 'Support Coordination' },
  { code: 'RECOVERY_COACHING', name: 'Psychosocial Recovery Coaching' },
  { code: 'OCCUPATIONAL_THERAPY', name: 'Occupational Therapy' },
  { code: 'PHYSIOTHERAPY', name: 'Physiotherapy' },
  { code: 'SPEECH_PATHOLOGY', name: 'Speech Pathology' },
  { code: 'PSYCHOLOGY', name: 'Psychology Support' },
  { code: 'BEHAVIOUR_SUPPORT', name: 'Behaviour Support' },
  { code: 'COMMUNITY_NURSING', name: 'Community Nursing Care' },
  { code: 'EMPLOYMENT_SUPPORT', name: 'Employment Support' }
] as const;

export function supportServiceName(code: string | null | undefined): string {
  if (!code) {
    return '—';
  }

  return SUPPORT_SERVICES.find((item) => item.code === code)?.name ?? code;
}
