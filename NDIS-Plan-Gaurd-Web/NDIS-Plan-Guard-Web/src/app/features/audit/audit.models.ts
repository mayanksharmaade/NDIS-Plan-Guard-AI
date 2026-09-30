export interface AuditEventResponse {
  id: string;
  serviceProviderId: string | null;
  entityType: string;
  entityId: string;
  action: string;
  description: string;
  actorIdentityUserId: string | null;
  actorEmail: string | null;
  occurredAtUtc: string;
}
