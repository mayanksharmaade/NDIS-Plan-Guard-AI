NDIS Plan Guard AI is a portfolio-grade claim decision-support platform designed to demonstrate how modern web engineering, machine learning, structured business rules, and human review can work together in a realistic NDIS-style claims workflow.
The system is built as a full-stack application with a .NET 10 Web API backend, Angular 22 frontend, SQL Server database, and a separate Python machine-learning risk-scoring service. The goal is not to automatically approve or reject claims. Instead, the platform helps reviewers understand the context around a claim, identify unusual or potentially problematic patterns, and make a better-informed human decision.
The project has been designed around a practical enterprise workflow: service providers register, participants and services are managed, claims are created and submitted, risk information is generated, claims enter a review queue, reviewers inspect evidence and decision-support findings, and final review actions are captured in an auditable way.
This repository contains the backend API, Angular web application, and Python ML service in one place so the complete solution can be reviewed as a single portfolio project.
Technology Stack
Backend
- .NET 10 Web API
- ASP.NET Core
- Entity Framework Core 10
- SQL Server
- ASP.NET Core Identity
- JWT authentication
- Role-based authorization
- FluentValidation
- Clean Architecture
Frontend
- Angular 22
- Standalone components
- TypeScript
- SCSS
- Angular routing
- HTTP interceptors
- Route guards
- Role-aware navigation
Machine Learning
- Python
- pandas
- scikit-learn
- joblib
- FastAPI-style HTTP inference service
- Feature validation and risk scoring
- Model metadata and evaluation reports
Repository Structure
NDIS-Plan-Guard-AI/
│
├── NDIS-Plan-Gaurd-AI/          # .NET backend
├── NDIS-Plan-Gaurd-Web/         # Angular frontend
├── NDIS-Plan-Guard-ML/          # Python ML risk service
├── .gitignore
└── README.md
The backend follows a layered structure with Domain, Application, Contracts, Infrastructure, API, and test projects. The frontend is organized by business feature, while the ML service contains data preparation, model training, validation, model artifacts, and inference code.
What the Project Does
NDIS Plan Guard AI models the lifecycle of a claim from creation through human review.
A service provider can register in the system and wait for administrative approval. Once approved, the provider can manage relevant operational information such as participants, provider employees, participant services, budget plans, and claims.
A claim is created with service-delivery information such as the participant, provider, service, units, pricing, and claimed amount. When the claim is submitted, the application performs business validation and sends a structured set of features to the Python risk-scoring service.
The ML service returns a probability, score, risk band, model version, and feature version. The result is stored in the application database so the reviewer can later see the exact risk assessment associated with that claim.
The claim then becomes available to the review workflow. Rather than showing a reviewer only a single risk number, the review screen is designed as a decision-support view. It combines participant context, budget information, provider and service-cost context, explicit rule-based findings, and machine-learning risk factors.
The final decision remains with the human reviewer.
End-to-End Claim Flow
Service Provider Registration
        │
        ▼
Admin / Super Admin Approval
        │
        ▼
Participant Management
        │
        ▼
Provider Employees / Services
        │
        ▼
Participant Budget Plan
        │
        ▼
Claim Creation
        │
        ▼
Claim Validation
        │
        ▼
Claim Submission
        │
        ├──────────────► Deterministic Business Rules
        │
        └──────────────► Python ML Risk Service
                              │
                              ▼
                       Risk Assessment
                              │
        ◄─────────────────────┘
        │
        ▼
Risk + Findings Stored
        │
        ▼
Review Queue
        │
        ▼
Reviewer Opens Claim
        │
        ▼
Decision Support Screen
        │
        ├─ Claim Summary
        ├─ Participant Context
        ├─ Fortnight Budget Context
        ├─ Provider / Service Cost Context
        ├─ Rule-Based Findings
        └─ ML Risk Factors
        │
        ▼
Human Review Decision
        │
        ▼
Audit Trail / Dashboard
Authentication and Roles
The system uses JWT-based authentication and ASP.NET Core Identity.
The main roles currently represented are:
- SuperAdmin
- Admin
- Reviewer
- ServiceProvider
Role-based authorization controls access to API endpoints and Angular routes.
A service provider registration goes through an approval workflow rather than immediately receiving unrestricted access. Administrative users can review provider registrations and manage system access.
Participant and Service Management
Participants are central to the claim process. Their records can contain identifying and support-related information required by the application, including support categories and contact details.
The system also supports provider employees and participant-service assignments so claim information can be connected to the people and services involved in delivery.
Budget plans and service allocations provide the financial context required when assessing a claim.
Claim Management
Claims move through a controlled lifecycle.
Typical actions include:
1. Create a claim.
2. Edit claim information.
3. Validate required data.
4. Submit the claim.
5. Generate risk information.
6. Place the claim into the review workflow.
7. Review the evidence.
8. Record a human decision.
9. Capture the result in the audit trail.
The intention is to make the workflow traceable rather than treating a claim as a simple database record.
Decision-Support Design
One of the most important design decisions in this project is that a reviewer should never be expected to approve or reject a claim based only on an ML score.
The reviewer screen therefore combines multiple sources of evidence.
Claim Summary
The reviewer can see information such as:
- Claim number
- Claim status
- Claimed amount
- Risk score
- Risk band
Participant Context
The reviewer can inspect relevant participant information, including:
- Participant identity
- NDIS number
- Primary support category
- Support domains
Fortnight Budget Context
The planned decision-support model includes fortnight-based budget calculations such as:
- Fortnight allocation
- Amount already spent
- Remaining amount before the current claim
- Current claim amount
- Whether the claim exceeds the fortnight budget
- Amount above the available budget
- Fortnight start and end dates
This provides better context than comparing a claim against an entire plan budget.
Provider and Service-Cost Context
The reviewer can also inspect cost-related information such as:
- Provider hourly rate
- Service hours
- Expected service cost
- Claimed amount
- Claimed rate
- Typical or expected rate
- Variance between expected and claimed cost
This helps the reviewer understand why a claim may have been flagged.
Deterministic Findings
In addition to ML scoring, the project is designed to support explicit business-rule findings.
Examples include:
FORTNIGHT_BUDGET_EXCEEDED
DUPLICATE_SERVICE
RATE_MISMATCH
AMBIGUOUS_RATE
AMOUNT_MISMATCH
EXCESSIVE_SERVICE_HOURS
OVERLAPPING_SERVICE
CONCURRENT_PROVIDER_SERVICE
IMPOSSIBLE_LOCATION_OVERLAP
These findings are intended to be human-readable on the reviewer screen.
For example, instead of displaying only:
Risk Score: 78
the application should be able to explain that the claim was flagged because the participant's fortnight budget would be exceeded, the claimed rate differs from the expected rate, or another service overlaps with the same delivery period.
This evidence-first approach is a core part of the project.
Machine-Learning Risk Scoring
The project contains a separate Python ML service responsible for claim risk scoring.
When a claim is submitted, the .NET application builds the required feature set and calls the ML service over HTTP.
The ML response can include:
- Availability status
- Risk probability
- Risk score
- Risk band
- Model version
- Feature version
- Risk factors
- Scoring timestamp
The assessment is stored in SQL Server so the result remains associated with the claim.
The project currently includes versioned model artifacts and metadata, including V1 and V2 development work.
Keeping the ML service separate from the .NET application also demonstrates a realistic service boundary between application logic and model inference.
Human-in-the-Loop Review
The ML component is deliberately treated as decision support.
The application does not assume that a high score automatically means a claim should be rejected, or that a low score automatically means it should be approved.
A reviewer is expected to consider:
- Claim information
- Participant support context
- Budget position
- Provider information
- Service cost
- Deterministic findings
- ML risk factors
- Previous review or audit information
The final decision is therefore explainable, reviewable, and attributable to a human user.
Audit and Dashboard
The platform includes audit and dashboard capabilities to support operational visibility.
Audit records can capture important workflow events and provide traceability across claim processing and review activity.
Dashboard functionality is designed to show summary information for users such as administrators and reviewers, including claim activity and workflow status.
Architecture
The backend follows Clean Architecture principles.
Angular Frontend
       │
       ▼
ASP.NET Core API
       │
       ▼
Application Layer
       │
       ├────────────► Domain
       │
       ▼
Infrastructure
       │
       ├────────────► SQL Server
       │
       └────────────► Python ML Service
The major responsibilities are separated as follows:
Domain contains core entities, enums, and domain concepts.
Application contains business services, abstractions, validation, claim workflows, review logic, and decision-support models.
Contracts contains API-facing request and response contracts.
Infrastructure contains Entity Framework Core persistence, Identity, security services, migrations, and the ML HTTP integration.
API contains controllers, dependency injection, authentication configuration, and HTTP concerns.
Current Development Focus
The project has reached a functional technical checkpoint, but further refinement is planned.
Important next-stage improvements include:
- Fortnight-based budgeting
- Provider hourly-rate validation
- Service-hour calculations
- Duplicate-service detection
- Overlapping-service detection
- Concurrent-provider-service validation
- Impossible-location-overlap checks
- Rate anomaly detection
- Claim amount versus expected service-cost validation
- Improved reviewer explanations
- Redis distributed caching
- Polly retry and timeout policies
- Circuit breakers
- API rate limiting
- CI/CD
- Docker and containerization
- Deployment hardening
Running the Solution
The project is divided into three applications.
Backend
Navigate to the backend solution and restore/build the .NET projects.
dotnet restore
dotnet build
dotnet run --project src/NDIS.Api
The SQL Server connection string should be configured using .NET User Secrets or environment variables.
Angular
Navigate to the Angular application:
npm install
ng serve
Then open the local Angular development URL shown in the terminal.
ML Service
Create a Python virtual environment and install dependencies:
python -m venv .venv
Activate the environment and run:
pip install -r requirements.txt
Then start the ML inference service according to the commands documented in the ML project.
The .NET API currently expects the risk-scoring service at:
http://localhost:8001
Security
Sensitive values are not intended to be stored directly in the repository.
The project uses configuration placeholders for secrets such as database connection strings. Development secrets should be stored using mechanisms such as:
- .NET User Secrets
- Environment variables
- Deployment secret stores
Real passwords, private keys, JWT signing secrets, and cloud credentials should never be committed to Git.
Project Purpose
NDIS Plan Guard AI is primarily a software engineering and AI portfolio project.
It demonstrates:
- Full-stack application development
- Clean Architecture
- ASP.NET Core API design
- Angular enterprise UI development
- SQL Server and Entity Framework Core
- Authentication and authorization
- Workflow-based application design
- Python ML integration
- Explainable decision support
- Human-in-the-loop system design
- Auditability
- Production-readiness planning
The project is continuing to evolve, with the current emphasis on strengthening the business-rule engine, reviewer evidence, resilience, and deployment readiness.
Disclaimer
This is a portfolio and learning project. It is not an official NDIS system and is not affiliated with or endorsed by the National Disability Insurance Agency. The ML output is designed as decision-support information only and should not be interpreted as an automated eligibility, compliance, or payment decision.
