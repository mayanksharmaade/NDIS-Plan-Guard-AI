# Angular integration completed

Integrated frontend support for the new .NET endpoints:

- Service provider employees/support workers
  - list, create, edit, activate/deactivate
- Participant budget plans
  - create plan, view totals, add category allocations/fortnight limits
- Participant services
  - assign employees to participants
  - record service deliveries with service window, hourly rate, location and notes
- Routes and navigation
  - Provider Profile -> Manage employees
  - Participants -> Budget / Services
- All new HTTP services use `environment.apiBaseUrl`.

Validation performed in the integration environment:

- TypeScript check succeeded.
- Angular compiler (`ngc`) succeeded, including template compilation.

The full Angular production bundler was not run in the Linux integration environment because the uploaded `node_modules` contained the Windows esbuild binary. On your Windows machine, run:

```powershell
npm install
npm run build
npm start
```

## Intentionally not changed yet

The claim editor has not yet been changed to submit `ServiceDeliveryId` / `ServiceProviderEmployeeId`, because the claim API contract/workflow must first be updated to accept and validate those fields. The new participant-services screen records the service-delivery data that will be linked in that next backend step.
