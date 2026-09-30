# NDIS Plan Guard — Angular integration notes

## Included

- Admin/SuperAdmin Plan Template screen (`TotalPlan` or `Fortnightly`).
- Provider participant-budget screen uses a Plan Template dropdown; budget amount/basis auto-populate and remain read-only.
- Employee UI distinguishes employee pay rate.
- Participant service assignment uses a service dropdown and requires an agreed participant billing rate.
- Service delivery is recorded from an active participant/employee assignment; employee/service/rate are derived rather than retyped.
- Create Claim is delivery-driven:
  - select participant
  - select period
  - select service
  - load unclaimed recorded deliveries
  - select compatible deliveries
  - auto-calculate support hours and claim amount
  - example: 14 hours × A$35/hour = A$490
- Claim details show linked delivery evidence.
- Reviewer screen shows employee, delivery count, support hours, agreed billing rate, employee pay rate, expected service cost and fortnight context.

## Run on Windows

```powershell
npm install
npm run build
npm start
```

The Angular TypeScript/template compiler was run successfully against this integrated source.

`node_modules`, `dist`, `.angular` and `.git` are intentionally excluded from the delivered ZIP.

## Recommended end-to-end test

1. Create an employee with a pay rate.
2. Assign the employee to a participant using a service from the dropdown and an agreed billing rate.
3. Record multiple deliveries.
4. Assign a plan template to the participant.
5. Create a claim and verify the hours/rate/amount formula.
6. Submit the claim.
7. Open reviewer decision support and verify the service/budget context.
