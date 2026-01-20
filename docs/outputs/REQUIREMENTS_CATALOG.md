# SASQUATCH Requirements Catalog

**SASQUATCH** - School Apportionment System for Quality, Accountability, Transparency, and Calculations Hub

Generated: 2026-01-19

---

## Executive Summary

This catalog contains **243** normalized requirements extracted from the SASQUATCH RFP documentation.

### Statistics

| Metric | Count |
|--------|-------|
| Total Requirements | 243 |
| Data Collection | 82 |
| All | 55 |
| Data Calculations | 32 |
| Data Reporting | 31 |
| Technical | 27 |
| Sys All | 16 |

### By Priority

| Priority | Count |
|----------|-------|
| High | 8 |
| Medium | 222 |
| Low | 13 |

### By Complexity

| Complexity | Count |
|------------|-------|
| High | 31 |
| Medium | 129 |
| Low | 83 |

---

## Data Collection

*82 requirements*

### 0013ENR

**Priority:** 🔴 High | **Complexity:** 📉 Low | **Type:** Non-Functional

**Background:** Inconsistent data when revisions override confirmed counts

> The new system should maintain version control of submissions. Original monthly counts should be archived and accessible for audit purposes, even if later revisions occur.

**Pain Point:** Earlier, OSPI staff noted that finalized monthly counts could be overwritten by later revisions, making it impossible to retrieve the original version. SME clarified this is less critical now, as final counts are accepted as authoritative, but some staff still see value in audit history.

**Tags:** security, audit, enrollment-reporting

---

### 001APP

**Priority:** 🔴 High | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Manual Handling of revenue forecasting and budget preparation Data

> The apportionment system must be able to directly ingest and process revenue forecasting and budget preparation data without requiring staff to manually download, regroup, and re-upload files. By automating the regrouping of item codes and ensuring alignment with apportionment categories, the system will eliminate a repetitive, time-consuming task that creates risk of human error. This will increase efficiency during monthly processing cycles and reduce the dependency on staff intervention for routine data preparation.

**Pain Point:** Currently, the apportionment process requires staff to download F-203 data, manually regroup item codes to match apportionment categories, and then re-upload the data into the system. This step is both repetitive and error-prone, adding unnecessary complexity to monthly processing. SME confirmed tha...

**Tags:** budget, automation, apportionment

---

### 0010ENR

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Paper-based collections still in use (E672, E525, P213, P223YC, UW enrollment)

> The new system must digitize these collections, providing secure online forms for submission. Data must flow directly into the central system for reporting and apportionment calculations. \n

**Pain Point:** Institutional Education (E672), Home/Hospital (E525), Non-High (P213), Washington Youth Academy (P223YC) and UW enrollment reports are still submitted via paper or email, then manually entered into spreadsheets. This adds workload and increases the chance of transcription errors.

**Tags:** reporting, enrollment-reporting

---

### 0012ENR

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Manual identification of non-submitting districts

> The system must automatically track submission status and generate reminders for outstanding districts. A dashboard should display submission compliance in real time.

**Pain Point:** OSPI staff must manually determine which districts have not submitted their files and then send individual reminder emails. This is time-consuming and risks missing districts.

**Tags:** automation, audit, ui, enrollment-reporting

---

### 002INT

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Technical

**Background:** Dependence on manual processes and staff (SME for enrollment data)

> The system should automate data ingestion and transformation, eliminating the need for manual formatting. Enrollment data should flow directly from the source system into the apportionment database, reducing dependency on staff and ensuring consistent, verified inputs.

**Pain Point:** Data pipelines rely heavily on manual intervention. For enrollment data, SME must request specific pivoted reports from SME, who produces them manually in Excel. This dependency creates bottlenecks, delays, and version control issues where multiple files can circulate, creating doubt in data accurac...

**Tags:** enrollment, automation, data-integration-reporting

---

### 003BUD

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Lack of Automated Notifications (F200 Extensions)

> The system must generate automated notifications (emails or dashboard alerts) to OSPI reviewers when districts or ESDs submit extensions. Notifications should also cascade across stakeholders (district → ESD → OSPI) to ensure smooth workflows.

**Pain Point:** When school districts submit budget extensions (F200), there is no automated alert to SME. He must manually log in and check for pending submissions. This is inefficient and risks delays in review and approval.

**Tags:** automation, ui, budgeting

---

### 003INT

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Missing or fragmented reference data (codes, schools, activities)

> The apportionment system must include fully maintained lookup/reference tables for schools, accounts, and activity codes within the same database environment. This ensures that all reporting processes can directly query human-readable names alongside financial and enrollment data

**Pain Point:** When creating reports, SME must manually join data from multiple servers to resolve codes into meaningful labels (e.g., school names, activity names). This adds complexity, slows down report generation, and introduces opportunities for mismatched joins.

**Tags:** enrollment, budget, reporting, data-integration-reporting

---

### 004PRS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Unfriendly User Interface for Data Submissions

> The new system should provide a secure, intuitive interface where both districts and ESDs can submit, review, and correct data directly. The UI should allow file uploads with built-in validation, but also permit authorized edits without forcing resubmission. Features like guided forms, upload status dashboards, and real-time feedback would reduce submission errors and make the process more efficient for non-technical users.

**Pain Point:** School districts currently submit S-275 data using structured text files formatted to a rigid layout. SME confirmed that while OSPI and ESDs can correct data directly in the system, districts cannot — they must resubmit files. The system lacks a user-friendly web interface, requiring technical exper...

**Tags:** validation, ui, personnel-reporting

---

### 005ENR

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Ability to prohibit revisions during processing

> The system must allow OSPI staff to “lock” submissions for a defined period each month. During this lock window, districts/ESDs cannot submit revisions until OSPI reopens the system.

**Pain Point:** Currently, while OSPI staff are running monthly enrollment and apportionment calculations, districts can still submit changes. This disrupts processing and creates data mismatches. SME specifically requested a “lock button” to prevent this.

**Tags:** enrollment-reporting

---

### 005SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Display Annual Financial Statement changes to public

> The system shall have the ability to display updated post-audit versions of reports such as F-196, Personnel Reporting (S-275), and enrollment data to the public in a timely manner. It shall provide contextual information describing the nature of corrections, the reason for each update, and the effective date of change. The feature shall ensure transparency while maintaining the integrity of the audit trail, distinguishing clearly between pre-audit and post-audit versions.

**Pain Point:** When districts find an error they've made or have a finding by an auditor, they send OSPI a paper correction. OSPI reviews and makes manual updates, but these aren't displayed to the public until the following year's budget, and there's a little note that says that it's been updated. As an improveme...

**Tags:** enrollment, budget, reporting, audit, ui

---

### 006ENR

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Edit process not effective

> The system must display edits in a clear, user-friendly way, highlighting critical errors. Submissions should be blocked until critical edits are resolved, and comments must be meaningful. School-level edit visibility should also be considered.

**Pain Point:** Districts are required to run edits before submission, but often they don’t review them carefully. Some districts provide placeholder comments without addressing the discrepancies. This reduces the quality of enrollment data and pushes the burden to OSPI staff.

**Tags:** validation, ui, enrollment-reporting

---

### 006EXP

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Manual Notifications (System Open, Submission Status)

> The system must include automated notification workflows tied to status changes (system open, submission received, edits required, approval completed). Notifications should be configurable by user role and include both email and in-system alerts. This reduces manual communication overhead, improves timeliness, and ensures districts/ESDs have clear visibility into status changes.

**Pain Point:** Notifications such as system opening, submission readiness, and approvals are currently sent via manual emails. This increases administrative overhead and creates potential for delays or missed deadlines.

**Tags:** validation, automation, security, financial-expenditures-reporting

---

### 006PRS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Manual Notifications to Districts/ESDs

> The system should provide automated notifications via email, dashboard alerts, or SMS integration. For example, when the system opens for the year, all districts should receive a system-generated notice. If a district misses a submission cutoff, automated reminders should trigger. These alerts should be configurable to reduce manual follow-up by OSPI staff.

**Pain Point:** Notifications to school districts and ESDs — such as opening of the reporting year or reminders for late submissions — are still handled manually. SME sends emails or calls districts when deadlines approach. This consumes staff time, creates delays, and risks districts missing deadlines if communica...

**Tags:** integration, automation, ui, personnel-reporting

---

### 006SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Desired future state login process

> The new Apportionment System must adopt a secure, modern login process, such as Entra ID or federated authentication. Users from districts or ESDs should be able to access the system seamlessly with individual credentials.

**Pain Point:** IT Application Services Director wants the new Apportionment System to utilize Entra ID or similar as a customer login process in order to provide a user-friendly, highly secure user management system. Those customers would log into OSPI a little differently than with the current process: either we ...

**Tags:** security, audit, all

---

### 007ENR

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Business

**Background:** Missing school-level validations

> The system should give OSPI the ability to enforce program/school-level validation rules – limiting what type of enrollment can be reported at a given school type (e.g., Open Doors in R-schools, Skill Centers only at designated schools). This could include limiting the grades that can be reported to specific schools (e.g., Kindergarten reported at a high school).

**Pain Point:** Currently, districts can submit logically inconsistent enrollments, such as non-Skill Center FTE at a skill center. These errors go undetected until OSPI staff review reports manually, delaying correction.

**Tags:** enrollment, reporting, validation, enrollment-reporting

---

### 007EXP

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** F-185 Reporting by ESDs (Excel Driven, No Edits)

> The system must support direct online submission of F-185 reports by ESDs with validation and edits built in. It should allow aggregation within the system, generate standard reports, and integrate ESD data into federal reporting calculations. This eliminates manual Excel handling, ensures consistency, and improves data quality before OSPI staff receive files.

**Pain Point:** ESDs currently complete F-185 in Excel templates with no built-in edits. SME aggregates their submissions manually via Tableau and Excel joins. The process is inefficient, error-prone, and lacks quality control.

**Tags:** reporting, validation, integration, financial-expenditures-reporting

---

### 007SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Lock Enrollment data after a certain point

> The system must allow OSPI’s Enrollment and Institutions team to lock Enrollment Reporting (P-223) Enrollment, SAFS ALE, and K–3 Class Size data during specific processing cycles. External users will be restricted from modifying data while OSPI is performing calculations. Lock periods will typically last up to three days each month.

**Pain Point:** Program Supervisor of Enrollment & Institutions wants a way to lock the Enrollment Reporting (P-223) Enrollment, SAFS ALE, and K-3 Class Size to prevent external users from updating data in those systems, during the monthly cycles (of up to three days) while OSPI is processing enrollment, so that ca...

**Tags:** enrollment, reporting, ale\nk-3-class-size\nenrollment-reporting-(p-223)

---

### 008ENR

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Manual reconciliation between P223 and ALE

> The new system must integrate ALE and P223 collections. Submissions should be cross-validated automatically, with discrepancies flagged immediately for districts before final submission.

**Pain Point:** OSPI staff must manually compare P223 and ALE data collections to ensure counts match. When mismatches occur, staff must email districts and request corrections. This is repetitive, slow, and prone to oversight.

**Tags:** validation, integration, automation, enrollment-reporting

---

### 008PRS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Business

**Background:** Data Validation and Edit Limitations

> The system should support configurable edit rules with different enforcement levels. Certain errors should block submission until resolved, while others may remain as warnings. This would improve data quality while still allowing flexibility. A dashboard for tracking unresolved edits should also be provided, so districts know exactly what requires attention.

**Pain Point:** SME described three levels of edits: (1) format-level checks (blocking), (2) warnings, and (3) errors. While format errors stop processing, warnings and errors do not — meaning flawed data can pass through the system. In practice, many edits serve only as advisory, and OSPI must rely on districts to...

**Tags:** validation, audit, ui, personnel-reporting

---

### 008SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Coding inconsistencies between systems must be resolved

> The system must standardize item codes across budgeting (F195, F196, Estimate for State Revenues (Estimate for State Revenues (F-203)) and payment (Apportionment) systems. Currently, inconsistent coding leads to confusion and manual corrections. A unified coding structure will minimize errors and reduce time spent reconciling data. Standardization will also support smoother cross-system data integration and reporting.

**Pain Point:** The budgeting and payment system presently has numerous item codes that differ between the systems but have the same meaning. Resolving these differences will reduce errors and time spent creating files.

**Tags:** budget, reporting, integration, all

---

### 009ENR

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Lack of electronic uploads for ALE

> The new system must support electronic batch uploads for ALE data, allowing districts to upload files rather than manually entering records.

**Pain Point:** If ALE remains its own collection, districts are forced to manually key data into the system one district at a time. This is tedious and error-prone, especially for large districts with multiple ALE programs.

**Tags:** enrollment-reporting

---

### 012SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Masks for integer fields

> The system must enforce data-entry validation rules for all integer fields. When entering numbers, users may input digits only, and the system should automatically display commas for readability once data entry is complete. Attempts to exceed the permitted digit limit should trigger an error message. This control minimizes data entry errors and ensures consistent numeric formatting throughout the system.

**Pain Point:** When the focus is on an integer field, the system will refuse anything typed by the user except numbers. When the user tabs from the field, the system will display commas in the appropriate places. So if the field type is 9 integer, then the max field length would be 11 (nine digits plus two commas)...

**Tags:** validation, automation, ui, all

---

### 013SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Masks for decimal fields

> The system must validate decimal inputs by allowing only digits and one decimal point within the field. Users may not exceed the configured number of decimal places or total digits. If invalid input is detected, the system must display a clear, human-readable error message. This feature maintains data precision and consistency for all numeric fields requiring decimals.

**Pain Point:** When the focus is in a decimal number field, the system will ignore anything typed by the user except numbers and the decimal. The system will ignore anything typed after the maximum number of decimal places allowed. So, if the field type is 9.2 decimal, then the max field length would be 14 (nine d...

**Tags:** validation, ui, all

---

### 016SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Number values must decimal align

> The system must ensure all numeric data fields and report outputs are aligned by decimal place. Decimal alignment supports readability and consistency across all screens and printed reports. This requirement applies to both input and display formats.

**Pain Point:** Numeric fields on data input screens, and displays in reports, must decimal align.

**Tags:** reporting, ui, all

---

### 017SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Negative numbers are expressed with a minus sign [-] printed to their left when displayed in reports

> All negative numeric values must display with a minus sign placed directly before the number (e.g., “−201,050.00”). Negative numbers must appear in red font for quick identification. This format ensures clarity and consistency across all reports and visual displays. It reduces confusion when interpreting financial and analytical data.

**Pain Point:** Negative numbers in reports and displays are expressed with a minus sign [-] printed to their left, so as to maintain decimal alignment. e.g.$ -201,050.00. Negative numbers must be displayed in red font.

**Tags:** budget, reporting, ui, all

---

### 019SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Actions triggered by clicking ‘Save’ or ‘Save and Return’

> When a user saves data, the system must automatically perform field-level edits, refresh calculated fields, and store the latest values. Clicking “Save” keeps the user on the same screen, while “Save and Return” navigates back to the previous view. This behavior promotes workflow efficiency and consistency. Users receive immediate feedback on data validity while maintaining control of navigation.

**Pain Point:** When the user clicks ‘Save’ or ‘Save and Return’ on a data entry page, the system runs field-level edits, updates calculated fields, and save his or her changes.\nIf the user clicked 'Save', the same screen continues to be displayed.\nIf the user clicked 'Save and Return', the previous screen then d...

**Tags:** calculation, validation, automation, all

---

### 021SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Multiple Users Accessing a Page or Table Simultaneously

> The system must permit multiple users to view or access the same page or table concurrently. If a user makes a change while another user is viewing the same data, the system must alert the second user and provide an option to review the latest updates.

**Pain Point:** The system must permit multiple users to access a page or table at the same time. If a user is viewing a page when another user makes a change to it, the system shall alert and permit the user to review changes made by the other user, and prioritize which changes to save.

**Tags:** security, all

---

### 022SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Most districts contract some SAFS work to third-party vendors

> The system must accommodate access for third-party vendors who perform fiscal management and SAFS reporting on behalf of school districts. These vendors should have permissions aligned with the districts they serve, allowing them to view or edit relevant financial data. Access controls must ensure vendors can only interact with authorized district records.

**Pain Point:** Most of Washington's districts contract with third-party vendors to assist with fiscal decision making, security, and SAFS reporting services. As such, they will require system licenses and access and visibility to at least some districts' financial records, once granted permission by that district.

**Tags:** budget, reporting, validation, security, all

---

### 030SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Data Input Types

> The system must allow districts to submit data through various secure methods, including APIs, fixed-length files, and comma-delimited files via SFTP. Supporting multiple input types accommodates the diverse technical environments of Washington’s school districts.

**Pain Point:** District users must be able to submit data to the Apportionment Systems via an API, or fixed length files or comma-delimited files via SFTP.

**Tags:** integration, all

---

### 033SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Capacity to accept large files

> The system must be capable of handling large file uploads in a single process without requiring multiple submissions. Currently, users must re-upload large datasets several times to complete validations. Improved capacity will streamline workflows and ensure efficiency in high-volume data environments.

**Pain Point:** In contrast to the existing system, large input files must be accepted by the system in a single process. Currently, it is necessary to submit large files multiple times in order for all all validations to run.

**Tags:** validation, automation, all

---

### 034SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Consistent Format/Layout of Data Input

> The system must enforce a common file format for all incoming data submissions across external systems. This will replace the current inconsistent formats that require multiple mappings. Standardized formatting simplifies integration, reduces errors, and accelerates processing.

**Pain Point:** The system currently accepts data from multiple external systems, each of which has its own unique format. It is recommended that, where possible, Apportionment accepts a common file format that source systems adhere to.

**Tags:** integration, all

---

### 036SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Incoming data can include one or many districts

> The system must allow users, such as ESDs or vendors, to upload data files that contain information for one or multiple districts simultaneously.

**Pain Point:** Some external users (e.g., ESDs, contracted district vendors) work with multiple districts' Apportionment Data, and would benefit from having the ability to submit data from those multiple districts in a single data exchange.

**Tags:** all

---

### 037SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** CRUD Capacity for some external user types

> Authorized external users (e.g., ESDs or contracted vendors) must have the ability to create, read, update, and delete specific apportionment data records before submission to OSPI. The system should provide audit trails for all changes to ensure accountability. Users must also receive reminders to align updates with source systems.

**Pain Point:** Some external users (e.g., ESDs, contracted district vendors) provide support by reviewing Apportionment data files before sending them to OSPI. Such users would benefit from a viewing/editing console to help quality control, and for tracking who's submitted. If the system provides such a feature to...

**Tags:** audit, all

---

### 068SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** WYSIWG 'programming' for applying new funds, codes, formulae, etc.

> The system must allow users to apply new item codes, funds, and formulae using a visual interface with checkboxes or dropdowns, rather than manual coding.

**Pain Point:** When an item code, fund, or formula is added to the system, the user should be able to use checkboxes to indicate what processes to which it should apply

**Tags:** calculation, ui, all

---

### 075SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Accommodate school/building-level data where appropriate

> The system must support inclusion of school- or building-level data where applicable to ensure comprehensive reporting and analysis.

**Pain Point:** Update apportionment systems to accommodate school/building-level data, where appropriate.

**Tags:** reporting, all

---

### 076SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Upload ALE data, compare to Enrollment Reporting (P-223)

> The system shall have the ability to electronically upload Alternative Learning Experience (ALE) data and compare it against Enrollment Reporting (P-223) and Enrollment Reporting (P-223)H submissions for the same district and time period. It shall validate that student FTE counts are appropriately categorized across regular, ALE, and summer (Enrollment Reporting (P-223)S) submissions, ensuring funding rates and headcount distributions are accurate and reasonable.

**Pain Point:** Provide a method by which SAFS ALE data can be uploaded electronically, rather than by typing in information from paper forms, as now. Once entered, test whether the numbers match the Enrollment Reporting (P-223) data for the same district and time period.

**Tags:** enrollment, reporting, validation, ale\nenrollment-reporting-(p-223)

---

### 078SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Communication and integration with external systems and processes

> The system must support communication and integration with external systems and processes, through OSPI approved methods such as Managed File Transfers (MFTs), Application Programming Interfaces (APIs), Enterprise Interface Builders (EIBs), or other OSPI approved system integrations. Integration must use industry standard best practices, and provide documented methods for these interactions.

**Pain Point:** The system must support communication and integration with external systems and processes, through OSPI approved methods such as Managed File Transfers (MFTs), Application Programming Interfaces (APIs), Enterprise Interface Builders (EIBs), or other OSPI approved system integrations. Integration mus...

**Tags:** integration, ui, external-integrations

---

### 079SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Links to other OSPI data systems used by districts

> The system must include direct navigation links to other OSPI systems frequently used by districts, such as WINS and Highly Capable data sources.

**Pain Point:** The system must provide links on a single page to permit district users to integrate with WINS and Highly Capable data sources

**Tags:** external-integrations

---

### 080SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** API or MFT to receive Internal OSPI data

> The system must include an API or MFT interface to securely receive data from internal OSPI systems.

**Pain Point:** The system must provide an API or MFT to RECEIVE information from internal OSPI systems.

**Tags:** integration, ui, external-integrations

---

### 081SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Budget (e.g., All funds, General Fund, ASB Fund) and Revenue edits

> The system must support automated validation (edit) checks on district budget and revenue data across all funds, including General Fund and ASB Fund. These edits must run when triggered by a user and identify inconsistencies or errors before submission.

**Pain Point:** The system must run Budget (e.g., All funds, General Fund, ASB Fund) and Revenue edits against budgets, when requested by the user, through a submittal hierarchy culminating in “Approved” by OSPI.

**Tags:** budget, validation, automation, budgeting-and-accounting-system-(f-195)

---

### 082SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Edit failure messages

> If a budget fails one or more edit checks, the system must generate a human-readable message for each error. These messages should clearly identify the issue and its location within the document.

**Pain Point:** If a budget fails one or more edits, the system must provide a human-readable message for each failure instance and an indicator of the location of the error(s).

**Tags:** budget, validation, budgeting-and-accounting-system-(f-195)

---

### 088SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Reminder emails to ESDs for school districts that have not submitted budget files

> The system must automatically generate reminder emails to ESDs for districts that have not yet submitted their budget files by the due date. The notification should include district names and status updates.

**Pain Point:** Provide ability to OSPI SAFS Staff to automatically generate reminder emails to ESDs for school districts that have not submitted budget files, requesting a status update.

**Tags:** budget, automation, budgeting-and-accounting-system-(f-195)

---

### 089SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Update ‘Enter Budget Document’ function

> The system must enhance the “Enter Budget Document” feature to include integrated access to the F195 Budget Edits Report, Estimate for State Revenues (Estimate for State Revenues (F-203) Edits Report, and Estimate for State Revenues (Estimate for State Revenues (F-203) data.

**Pain Point:** Update the print feature ‘Enter Budget Document’ to include F-195 Budget Edits Report, Estimate for State Revenues (F-203) Edits Report, and Estimate for State Revenues (F-203) data.

**Tags:** budget, reporting, validation, integration, security

---

### 090SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Add 4-year projection to Estimate for State Revenues (Estimate for State Revenues (F-203)

> The Estimate for State Revenues (Estimate for State Revenues (F-203) report must include a four-year budget projection feature that prepopulates data into the Four-year Budget Plan (F-195F). This enhancement supports the legislature’s requirement for districts to maintain a balanced four-year budget plan.

**Pain Point:** The Estimate for State Revenues (Estimate for State Revenues (F-203) should include a four-year projection, which could prepopulate the Four-year Budget Plan (F-195F).

**Tags:** budget, reporting, four-year-budget-plan-(f-195f)\nestimate-for-state-revenues-(estimate-for-state-revenues-(f-203)

---

### 091SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Make changes to Annual Financial Statement (F-196) revision, not original, assuming that the Statement has already been revised

> If an Annual Financial Statement (F-196) form has been revised previously, subsequent updates must apply to the revised version rather than the original. The system must maintain both the original and revised documents for transparency.

**Pain Point:** If an Annual Financial Statement (F-196) that has previously been revised must be updated again, then the system should provide a mode in which the changes can be made to the already revised version, not the original Annual Financial Statement (F-196). The system should display to the public both th...

**Tags:** budget, annual-financial-statement-(f-196)

---

### 092SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Annual Financial Statement (F-196) Revisions never result in more than one official revised copy

> The system shall have the ability to maintain and display both the original and the most recent official revised version of each district’s Annual Financial Statement (F-196) report for a given school year. It shall retain all internal versions for audit and historical purposes but publicly display only the most recent approved revision. The system shall include configurable controls to define how many district-level corrections are permitted before OSPI approval is required.

**Pain Point:** The Annual Financial Statement (F-196) should contain and display an original version for a given district and school year, and, if necessary, a revised copy. The revised copy may have had one or many revisions.

**Tags:** budget, reporting, audit, ui, annual-financial-statement-(f-196)

---

### 093SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Prior-Year Annual Financial Statement (F-196) revisions must include corrected indirect rate

> When revising prior-year Annual Financial Statement (F-196) data, the system must automatically incorporate the correct ot corrected indirect rate calculations.

**Pain Point:** Updates to a prior-year Annual Financial Statement (F-196) must incorporate the correct/corrected indirect rate calculation.

**Tags:** budget, automation, annual-financial-statement-(f-196)

---

### 095SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Creating Budget Extension Statement (F-200) by one or more funds

> The system shall have the ability to allow authorized district users to create Budget Extension Statement (F-200) for one or more funds—including General, Capital Project, Debt Service, ASB, and Transportation Vehicle Funds—directly within the Budgeting and Accounting System (F-195) workflow. It shall retain the original budget version alongside all subsequent revisions, allowing users to view, track, and submit extensions without requiring a separate Budget Extension Statement (F-200) form. All changes shall be version-controlled, timestamped, and subject to applicable approval workflows.

**Pain Point:** The system must support district users to optionally create General, Capital Project, Debt Service, ASB, and Transportation Vehicles Fund Budget Extension Statement (F-200)

**Tags:** budget, integration, automation, audit, budget-extension-statement-(f-200)

---

### 096SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Districts can import budget extension fund files

> The system shall have the ability to import budget extension fund files for Budgeting and Accounting System (F-195) versions using current file formats (e.g., .csv, .xls) and future integration methods such as APIs. It shall allow authorized users to upload, review, and replace existing budget fund data with imported file contents while maintaining version history. Each import shall trigger validation, confirmation, and logging to ensure accuracy and traceability of all uploaded budget data.

**Pain Point:** The system must support districts to import one or more budget extension fund file“versions”, or view imported data files from their contracted vendor. Importing a budget fund file version will replace the existing budget fund with the imported file data.

**Tags:** budget, validation, integration, automation, audit

---

### 097SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** CRUD Revenue updates for Budget Extension Statement (F-200)

> Authorized district users must be able to create, read, update, and delete revenue and financing service records within Budget Extension Statement (F-200). This includes the ability to adjust specific fund categories such as General, ASB, or Capital Projects.

**Pain Point:** The system must support district users to be able to break out and update revenues and other financing services for General, Capital Project, Debt Service, ASB, and Transportation Vehicles Fund Budget Extension Statement (F-200).

**Tags:** budget, integration, budget-extension-statement-(f-200)

---

### 099SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Support manual and automated budget reviews

> The system must enable authorized OSPI staff to conduct both manual and automated budget reviews. Automated edits will validate compliance with accounting standards, while manual reviews allow for contextual verification.

**Pain Point:** The system must provide processes for authorized OSPI-based users to manually review budget documents as well as run edits against them.

**Tags:** budget, validation, automation, budget-extension-statement-(f-200)

---

### 100SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Support budget reviews with workflow to advance or return budgets

> The system must include a review workflow allowing OSPI staff to approve or return submitted budgets with comments. Returned budgets should include clear explanations of deficiencies or requested revisions.

**Pain Point:** Authorized OSPI-based staff must have the ability to review budgets submitted by school districts. If rejected, the budget must return to the district for revision, accompanied by a message explaining why it failed.

**Tags:** budget, automation, budget-extension-statement-(f-200)

---

### 101SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Support annual budget update reviews with workflow to advance or return the update

> Authorized OSPI staff must be able to review and approve annual budget updates prepared internally or by other OSPI users. The system must support returning updates with feedback for correction.

**Pain Point:** Authorized OSPI-based staff must have the ability to review annual budget updates prepared by other OSPI users. If rejected, the annual budget updates must return to the OSPI staff that wrote the update, accompanied by a message explaining why it failed.

**Tags:** budget, budget-extension-statement-(f-200)

---

### 102SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Old budget submissions must be archived, retrievable

> The system must retain an archive of all submitted budgets, making them retrievable when needed for audits or analysis. Archived data should include timestamps, version history, and submission details.

**Pain Point:** The system must retain a history/archive of submitted budgets, so that users can access them when required.

**Tags:** budget, audit, budget-extension-statement-(f-200)

---

### 105SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Notification when budget is ready for review

> The system shall have the ability to generate and send automated notifications to designated OSPI staff when a district or ESD submits a budget for review. Notifications shall include key submission details such as district name, submission date, and report type (e.g., Budgeting and Accounting System (F-195), Annual Financial Statement (F-196). The system shall also support configurable notification routing so that ESDs, regional reviewers, or specific OSPI roles are alerted when their assigned review or approval task becomes available.

**Pain Point:** The system must provide a notification to appropriate OSPI users when a district or ESD submits a budget for review.

**Tags:** budget, reporting, automation, security, budget-extension-statement-(f-200)

---

### 110SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Ability for OSPI SAFS Staff to stop and restart revisions

> The system shall provide OSPI SAFS staff with administrative controls to stop, cancel, or restart district-submitted revisions at any stage. When multiple overlapping or unfinished revisions exist, authorized OSPI users must be able to select and terminate specific revision instances without requiring district intervention or external scripting. The system shall include audit tracking of all stop/restart actions, including initiator, timestamp, and affected revision IDs.

**Pain Point:** The system must provide ability for OSPI SAFS Staff to stop and restart revisions on demand.

**Tags:** audit, enrollment-reporting-(p-223)

---

### 111SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Automatically notify school districts who have not submitted their monthly enrollment data by due date

> The system must include a feature that automatically notifies school districts who have not submitted their monthly enrollment data by enrollment due date.

**Pain Point:** The system must include a feature that automatically notifies school districts who have not submitted their monthly enrollment data by enrollment due date.

**Tags:** enrollment, automation, enrollment-reporting-(p-223)

---

### 112SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Identify school districts that have not submitted monthly enrollment

> The system must include a reporting function that lists districts with missing or incomplete monthly enrollment submissions.

**Pain Point:** The system must be able to identify school districts that have not submitted monthly enrollment to date, or by deadline

**Tags:** enrollment, reporting, enrollment-reporting-(p-223)

---

### 114SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Personnel Reporting (S-275) data sharing

> The system must share Personnel Reporting (S-275) personnel data with both the Education Data System (EDS) and the OSPI certification system (eCert). .

**Pain Point:** Personnel Reporting (S-275) data is fed to the Education Data System (EDS), and to the OSPI certification system (eCert).

**Tags:** reporting, personnel-reporting-(s-275)

---

### 115SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Employees can have more than a single assignment in one school and school year

> The system shall support multiple concurrent assignments per employee within the same school year and across schools or districts. Each employee record shall be uniquely identified using a transportable unique ID derived from core identifiers (e.g., Certification Number, partial SSN, or equivalent). The system shall ensure that assignment-level FTEs and headcounts aggregate correctly for reporting and calculations without duplication. Reports and calculations must handle multi-assignment employees consistently across all modules (e.g., Personnel Reporting (S-275), payroll, and HR

**Pain Point:** Because one school employee can potentially have many assignments in a single school year, the system must provide both head-counts and FTEs, and use both the employee's Cert # and SSN as primary keys.

**Tags:** enrollment, reporting, personnel-reporting-(s-275)

---

### 121SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Class 1 and 2 Districts

> The system shall have the ability to support both Class 1 and Class 2 district requirements for budget submission and approval. It shall enforce common submission deadlines (by July 10) and differentiate approval timelines—Class 2 budgets approved by July 31 and Class 1 by August 31. The system shall also support Annual Financial Statement (F-196) reporting variations, enabling Class 2 districts to opt for cash-basis reporting while ensuring Class 1 districts follow modified accrual standards, with automatic inclusion or exclusion of applicable data fields.

**Pain Point:** The system must support different functions for two classes of districts: Districts can be classified as 'Class 1', which means that they need to share their budget information with their respective Educational Service District (ESD), or as 'Class 2', which means that they must have their budget app...

**Tags:** budget, reporting, automation, all

---

### 127SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Enrollment Reporting (P-223)S Purpose

> The system shall have the ability to collect, validate, and report summer school enrollments using the Enrollment Reporting (P-223)S form, ensuring it captures the same data elements as the Enrollment Reporting (P-223) form but maintains separate submission and processing capabilities.

**Pain Point:** Districts offering summer school programs, include ALE and Skill Center programs but excluding Open Doors and Running Start use the Enrollment Reporting (P-223)S form to report student enrollment, based on the total enrolled hours. Districts report the total enrolled hours on the electronic Enrollme...

**Tags:** enrollment, reporting, validation, ale\nenrollment-reporting-(p-223)

---

### 128SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Districts can save more than one budget

> The apportionment system shall allow school districts to save multiple budgets within the Budgeting and Accounting System (F-195) module. Users shall be able to create, store, and manage more than one budget version to support scenario planning, revisions, and historical comparisons while maintaining data integrity and auditability.

**Pain Point:** Districts may save one or more budgets in the Budgeting and Accounting System (F-195) portion of the apportionment system

**Tags:** budget, audit, budgeting-and-accounting-system-(f-195)

---

### 129SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Districts can copy an existing budget

> The apportionment system shall allow school districts to copy an existing budget within the Budgeting and Accounting System (F-195) module to create up to two budgets for a given fiscal year.

**Pain Point:** Districts may copy an existing budgets in the Budgeting and Accounting System (F-195) portion of the apportionment system (to create two at most for a given year) in order to try other calculations to assist in their planning.

**Tags:** budget, budgeting-and-accounting-system-(f-195)

---

### 130SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Annual Financial Statement (F-196) must be open to changes for three school years

> The apportionment system shall allow authorized users to make revisions in the Annual Financial Statement (F-196) module for a period of three school years. This functionality supports the State Auditor’s Office (SAO) process, which audits some small districts only once every three years, ensuring that historical data can be updated and corrected as needed while maintaining proper audit trails.

**Pain Point:** To support SAO making audits of some small districts only once every three years, the Annual Financial Statement (F-196) must be open to revisions by authorized users for three years.

**Tags:** budget, audit, annual-financial-statement-(f-196)

---

### 131SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Estimate for State Revenues (Estimate for State Revenues (F-203) Purpose

> The apportionment system shall support the submission, review, and approval process for the Estimate for State Revenues (F-203): Estimate for State Revenues electronic form. Each year, between June 1 and September 10, all 295 Washington school districts shall submit their completed Estimate for State Revenues (F-203) forms to their Educational Service District (ESD). The system shall allow the ESD to review the submitted data, run system validations, and update the form’s status either to return it to the district for corrections or forward it to OSPI for approval.\n\nIn cases where the ESD provides business management services for one or more districts, the system shall allow the ESD to log in as the district, complete the Estimate for State Revenues (F-203) on their behalf, and update the status from “In Process at District” to “Ready for ESD Review.” After review, the ESD shall be able to change the status to “Ready for OSPI Review.” The system shall track all status changes, ensure data integrity, and support final review and approval by OSPI.

**Pain Point:** Every year, between June 1 and September 10, each of Washington's 295 School Districts must submit a completed Estimate for State Revenues (F-203): Estimate for State Revenues electronic form to their Educational Service District (ESD). The ESD reviews the data entered by their Districts, runs the s...

**Tags:** enrollment, validation, audit, estimate-for-state-revenues-(estimate-for-state-revenues-(f-203)

---

### 132SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Estimate for State Revenues (Estimate for State Revenues (F-203) Data Purpose

> The apportionment system shall use Estimate for State Revenues (F-203) data, in combination with each district’s final adopted budget, to calculate state apportionment payments for the months of September through December. From January to August, payments shall be based on actual year-to-date enrollment, staff ratios, and other relevant factors rather than Estimate for State Revenues (F-203) estimates.\n\nThe system shall ensure that incomplete or inaccurate Estimate for State Revenues (F-203) submissions are flagged, as errors can result in miscalculated or delayed apportionment payments. Additionally, the system shall allow districts to submit one or more revisions to their Estimate for State Revenues (F-203) estimates for the current year after the budget has been adopted, maintaining a complete history of all revisions for audit and compliance purposes.

**Pain Point:** OSPI uses Estimate for State Revenues (F-203) data, along with the district’s final adopted budget, to determine state apportionment payments for the months from September to December. (From January to August, apportionment payments are based on actual year-to-date enrollment, staff ratio, and other...

**Tags:** enrollment, budget, calculation, audit, estimate-for-state-revenues-(estimate-for-state-revenues-(f-203)

---

### 133SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Estimate for State Revenues (Estimate for State Revenues (F-203) 'X Option' Purpose

> The apportionment system shall provide the Estimate for State Revenues (F-203) “’X’ option” functionality beginning in January each year, allowing school districts to model the revenue impact of changes to funding formula elements. The system shall prepopulate fields with “state constants,” representing OSPI’s best estimates for each district. Districts shall be able to modify these values to explore scenarios, plan contingencies, and optimize funding strategies. Submission of the Estimate for State Revenues (F-203) “’X’ option” to OSPI shall be optional, and the system shall clearly indicate that these forms are for internal modeling purposes only.

**Pain Point:** Beginning in January of each year, OSPI makes available to school districts a section of the Estimate for State Revenues (F-203) called the “’X’ option”, which districts can use to model the revenue impact of changes to elements of the funding formula. The “’X’ option” contains prepopulated fields c...

**Tags:** calculation, estimate-for-state-revenues-(estimate-for-state-revenues-(f-203)

---

### 134SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Legislature requires schools to plan for four years

> The system shall have the ability to support four-year financial planning and reporting as required by the Washington State Legislature. It shall enable districts to create, submit, and maintain Four-year Budget Plan (F-195F) budgets that include the current fiscal year plus three projected years. The system shall accommodate varying levels of detail for out-year projections, ensure data consistency across all years, and validate that each district’s four-year budget remains balanced in compliance with legislative mandates.

**Pain Point:** The Washington state legislature requires that districts project their budgets for four years because, while the legislature issues and approves a Biannual budget, by law its budget needs to in balance for four years.

**Tags:** budget, reporting, validation, estimate-for-state-revenues-(estimate-for-state-revenues-(f-203)

---

### 135SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Field masking for counts in Enrollment Reporting (P-223)

> The system shall support configurable numeric field formatting across all relevant forms and reports (e.g., Enrollment Reporting (P-223)). For student headcount fields, only whole numbers shall be accepted and displayed; for Student FTE fields, up to two decimal places shall be supported. The system shall allow administrators to define precision and rounding rules for each numeric field to ensure data consistency and flexibility across modules

**Pain Point:** The Enrollment Reporting (P-223) must only accept, display, and output 1) Whole numbers for student headcounts and 2) two-place decimals for Student FTEs.

**Tags:** enrollment, reporting, ui, enrollment-reporting-(p-223)

---

### 136SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Enrollment Reporting (P-223) July and August forms purpose

> The apportionment system shall support the Enrollment Reporting (P-223) monthly forms for July and August, which districts use to report Open Doors and Running Start summer program enrollments. The system shall capture and process these summer enrollment data accurately to ensure correct reporting and apportionment calculations for these programs.

**Pain Point:** Districts reporting Open Doors and Summer Running Start enrollment will use the July and August Enrollment Reporting (P-223) monthly forms.

**Tags:** enrollment, reporting, enrollment-reporting-(p-223)

---

### 138SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Optional summer enrollment reporting

> The system shall support optional summer enrollment submissions for districts, with built-in business rules to determine apportionment timing. If a summer enrollment report is submitted and accepted by OSPI by August 20, the apportionment shall be scheduled for the November payment cycle. Reports submitted after August 20 shall have apportionment scheduled for January. The system shall automatically apply these business rules, track submission dates, and flag late submissions for funding exclusion from levy and LAP calculations.

**Pain Point:** Some districts may choose to report their summer enrollment once in August or sometime before the November 25 deadline. Districts that wait and report their enrollment after the August 20 deadline will not receive their summer apportionment until the following January, and the summer enrollment will...

**Tags:** enrollment, reporting, automation, audit, enrollment-reporting-(p-223)

---

### 139SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Create logical enrollment type limitations based on school type in Enrollment Reporting (P-223)

> The apportionment system shall implement logical validation edits within the Enrollment Reporting (P-223) module to restrict the types of enrollment that can be reported based on each school’s designated grade range and type.

**Pain Point:** The system must provide edits to limit the kinds of enrollment a school can report. For example, it's currently possible for an elementary school to report 12th grade enrollment.

**Tags:** enrollment, reporting, validation, audit, enrollment-reporting-(p-223)

---

### 140SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Personnel Reporting (S-275) System purpose

> The apportionment system shall support the Personnel Reporting (S-275) process, which provides an annual census of school district staffing based on an October 1 snapshot. The system shall collect and maintain data for all staff providing educational services, including both certificated and classified employees, as well as contracted personnel who serve during the school year. The Personnel Reporting (S-275) module shall distinguish between certificated and non-certificated staff as required by OSPI reporting standards. Unlike enrollment data, which is captured monthly, Personnel Reporting (S-275) staffing data shall be collected annually to provide an accurate count of personnel and support statewide analysis of staffing trends, compliance, and funding considerations.

**Pain Point:** The Personnel Reporting (S-275) provides an annual census of school district staffing, based on an October 1 snapshot. This data tells OSPI who is providing educational services--all staff, not just certificated, although there are differences between certificated and other staff. Tracks both hired ...

**Tags:** enrollment, reporting, ui, personnel-reporting-(s-275)

---

### 141SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Employees added after Personnel Reporting (S-275)'s October 1 snapshot

> The apportionment system shall recognize that staff hired after the October 1 snapshot date are not included in the Personnel Reporting (S-275) staffing report but shall still have their salary and expenditure data captured within the F-196 module. The system shall not allow post–October 1 staffing adjustments to the Personnel Reporting (S-275) dataset, as the snapshot represents a fixed annual record. It shall also support and display expected variances between employee counts in the Personnel Reporting (S-275) and F-196 modules, since districts are funded based on student enrollment rather than staffing levels.

**Pain Point:** If staff are hired after October 1, they are not included on the Personnel Reporting (S-275), but their salaries are captured on the Annual Financial Statement (F-196) regardless. Schools cannot make adjustments to that annual snapshot; they are funded by the students they serve, not the educators t...

**Tags:** enrollment, budget, reporting, ui, personnel-reporting-(s-275)

---

### 143SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Resolving educator data conflicts between Personnel Reporting (S-275) and EMS

> The apportionment system shall resolve educator data conflicts between the Personnel Reporting (S-275) module and OSPI’s Educational Management System (EMS) by designating the Personnel Reporting (S-275) record as the authoritative source. In any instance of conflicting educator account information, the Personnel Reporting (S-275) record shall take precedence to ensure proper linkage to certification data and maintain data traceability, accuracy, and compliance with OSPI reporting and audit standards

**Pain Point:** In any case where there is conflict in the educator account information held in Personnel Reporting (S-275) and OSPI's Educational Management Systems (EMS), the Personnel Reporting (S-275) record must be the active record in order to ensure that the record can be traced to the user's certification d...

**Tags:** reporting, audit, personnel-reporting-(s-275)

---

### 144SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** School district, ESD data, user permissions derived from tables shared with other EDS applications

> The apportionment system shall derive School District and Educational Service District (ESD) data—including CCDDD codes, district and ESD names, and their hierarchical relationships—from enterprise tables shared across all Education Data System (EDS) applications. User permissions and access rights shall also be managed centrally through these shared tables. Updates to this data shall be performed by authorized personnel at the database level, ensuring that any changes made are automatically reflected in the Estimate for State Revenues (F-203) system and all related apportionment modules. The system shall maintain real-time synchronization with the shared enterprise data to ensure consistency, eliminate redundant data entry, and preserve data integrity across all EDS applications.

**Pain Point:** School District and ESD data (e.g., CCDDD, Name, relationship of districts to ESDs) and user permissions are shared across the enterprise, and are entered and maintained not through an Estimate for State Revenues (F-203) user interface but via the intervention of an authorized individual who will li...

**Tags:** automation, security, all

---

### 004ENR

**Priority:** 🟢 Low | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Only accept numbers with correct format

> The system must enforce strict validations at data entry: headcount values must be integers, and FTE values must be decimals with exactly two places.

**Pain Point:** Districts sometimes enter invalid formats (e.g., decimals in headcount, or FTE without proper decimal places). These formatting errors disrupt apportionment calculations and require staff intervention.

**Tags:** enrollment, validation, enrollment-reporting

---

### 005BUD

**Priority:** 🟢 Low | **Complexity:** 📈 Medium | **Type:** Business

**Background:** Dependence on ESD Timeliness (F197 Treasurer’s Report)

> The system should include built-in mechanisms to monitor and enforce timely updates from ESDs. Features should include automated reminders for overdue submissions, dashboards displaying real-time submission status across districts, and escalation alerts when delays exceed thresholds. By providing transparency and accountability, OSPI would have the ability to track compliance and mitigate the risks caused by late updates.

**Pain Point:** F-197 reporting is dependent on Educational Service Districts (ESDs) to input reconciled balances based on Treasurer’s Reports. However, ESDs often lag in completing these updates, sometimes by four to six months. As a result, OSPI lacks access to current financial data for projections and reconcili...

**Tags:** automation, audit, ui, budgeting

---

### 006BUD

**Priority:** 🟢 Low | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Business Rules and Edits

> The system must have a clearly defined, transparent, and consistent framework for edits and business rules. Each edit should be documented, with criteria specifying whether it references current-year or prior-year data, and error messages should be written in plain language that users can understand. Additionally, edits should be configurable so OSPI staff can update rules without IT intervention. This would reduce manual checks, streamline reviews, and improve user confidence in the accuracy of system validations.

**Pain Point:** Edits in the budgeting systems (F-195, F-200) are sometimes unclear, inconsistent, or misaligned with actual business rules. SME explained that edits are often used interchangeably with “business rules,” and in some cases, edits force unnecessary manual checks against prior-year values. This inconsi...

**Tags:** validation, budgeting

---

### 009PRS

**Priority:** 🟢 Low | **Complexity:** 📈 Medium | **Type:** Business

**Background:** eCertification System Integration Issues

> The system should include automated reconciliation workflows for mismatched records. Exception dashboards should flag conflicts (e.g., mismatched SSN, name changes) and route them to the appropriate OSPI team for timely resolution. Data synchronization between S-275 and eCertification should be near real time to minimize errors. \n

**Pain Point:** SME explained that S-275 and the eCertification system share common fields like Social Security numbers. In the past, mismatches caused frequent reconciliation issues, though improvements have reduced the volume to only a handful per year. Still, when mismatches occur, OSPI certification staff must ...

**Tags:** automation, ui, personnel-reporting

---

### 038SAFS

**Priority:** 🟢 Low | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Import receipts to district, regardless of who files district's report

> When any report or data file is uploaded, both the submitting entity and the corresponding district must receive confirmation of receipt. This ensures that districts have visibility into their data submissions, even if completed by vendors.

**Pain Point:** While the entity that sends an Apportionment file to OSPI receives confirmation that the file has been received, districts sometimes report via their district vendors that they can't see their data in the OSPI systems. It would be helpful for the districts (and all involved) if the districts are sen...

**Tags:** reporting, all

---

## Data Calculations

*32 requirements*

### 001INT

**Priority:** 🔴 High | **Complexity:** 📊 High | **Type:** Functional

**Background:** Data stored in Excel instead of a centralized database

> The apportionment system must store data in a centralized relational database that retains both current and historical data in a consistent schema. Lookup tables for schools, account codes, and activity codes must be integrated, ensuring a single source of truth. This structure should support direct query, automated reporting, and the ability to generate dashboards without relying on manual Excel manipulations.

**Pain Point:** Currently, critical apportionment and enrollment data is not housed in a centralized database but rather in disparate Excel files and extracts. SME explained that apportionment outputs only exist as Excel extracts stored on the S-Drive, which are inconsistent year over year. This leads to fragile pr...

**Tags:** reporting, integration, automation, ui, data-integration-reporting

---

### 002APP

**Priority:** 🔴 High | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Long Processing Times for Calculations

> The new solution must be designed to process apportionment calculations significantly faster, ideally within one hour, and without locking staff out of the system during processing. This requires scalable system architecture capable of handling large datasets efficiently while supporting concurrent operations. Reducing calculation time and enabling parallel activity will improve productivity, allow staff to verify results sooner, and ensure timely payments to districts.

**Pain Point:** One of the most critical issues raised is system performance. Running apportionment calculations can take between four to six hours, during which time staff are locked out of making other updates. This bottleneck delays downstream processes such as report verification and compliance adjustments. SME...

**Tags:** apportionment

---

### 004APP

**Priority:** 🔴 High | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Manual Compilation of Reports in Adobe

> The system must automatically map revenue codes to budget codes, eliminating the current reliance on Excel spreadsheets. This functionality should be configurable to adjust for legislative or budgetary changes without requiring manual intervention each month. Automating the crosswalk will reduce errors, standardize reporting, and provide greater confidence in the data submitted to the Budget Office and other stakeholders.

**Pain Point:** After calculations, staff must manually convert revenue codes into budget codes using Excel spreadsheets before reports can be shared with the Budget Office. This manual crosswalk is time-consuming and prone to human error. It represents a critical control point that should be automated within the s...

**Tags:** budget, reporting, automation, apportionment

---

### 004BUD

**Priority:** 🔴 High | **Complexity:** 📊 High | **Type:** Functional

**Background:** Limited Transparency into F-200 History and Difficulty Managing Multiple F-200 (Budget Extension) Submissions

> The system must automatically detect and apply the most recent F-200 submission for each district and fund when updating appropriations. Historical submissions should remain archived for audit purposes, but only the latest submission should feed into current year budget calculations and F-196 actuals. This ensures consistency, accuracy, and transparency in budget reporting, even when multiple extensions are submitted throughout the year.

**Pain Point:** Districts frequently submit multiple F-200 budget extensions within the same fiscal year—for example, one in November due to levy revenues and another in March due to grants. Historically, the system struggled to identify which extension was the “most current” for reporting purposes. This created co...

**Tags:** budget, reporting, automation, audit, budgeting

---

### 005APP

**Priority:** 🔴 High | **Complexity:** 📊 High | **Type:** Business

**Background:** ADA Compliance for Reports

> All compliance-related calculations that are currently performed using standalone Excel tools should be integrated into the apportionment system. This includes Learning Assistance Program (LAP) allocations, High Poverty calculations, Physical/Social/Emotional Support (PSES) compliance, and K-3 class size compliance. Incorporating these tools will provide a single source of truth for funding calculations, reduce risk from undocumented or unsupported spreadsheets, and allow for real-time updates and error checking within the system itself.

**Pain Point:** A significant number of compliance-related calculations are performed outside of the apportionment system using Excel-based tools. SME described how programs like the Learning Assistance Program (LAP), High Poverty, Physical/Social/Emotional Support (PSES), and Kindergarten–3rd Grade (K-3) class siz...

**Tags:** validation, integration, apportionment

---

### 0011ENR

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Manual apportionment file preparation

> The system must automate apportionment file preparation. It should calculate FTEs and generate apportionment-ready outputs in the required formats, eliminating reliance on manual Excel work.

**Pain Point:** Currently, SME and others manually extract data, run pivot tables in Excel, and build text files for apportionment. This process is inefficient, repetitive, and highly error-prone.

**Tags:** enrollment, calculation, automation, enrollment-reporting

---

### 001PRS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Manual Redaction of Confidential Records

> The new system must integrate with the confidentiality program database and automatically identify records flagged for redaction. Built-in logic should prevent unauthorized exposure by applying redactions before reports are published. Additionally, the system should maintain an audit trail of redactions to meet compliance requirements and reduce the reliance on individual staff expertise.

**Pain Point:** The current S-275 process still relies on manual redaction of personally identifiable information for individuals in the Address Confidentiality Program. SME explained that certain records cannot be made public, requiring a manual step to identify and remove these from Access reports before publishi...

**Tags:** reporting, integration, automation, audit, personnel-reporting

---

### 001SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Support planning maintenance-level budget

> The system must provide reporting functionality that enables OSPI’s Apportionment team to analyze and respond to legislative requests for details on maintenance-level budget drivers

**Pain Point:** The system must provide a report to support OSPI's apportionment team to respond to the legislature's requests for information about the drivers of the maintenance-level budget.

**Tags:** budget, reporting, all

---

### 001TRB

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Lack of Historical Data Storage

> The system must be enhanced to include a robust data warehouse that stores historical apportionment and financial data. This repository should support multi-year analysis, allow users to query past records without reloading files, and ensure easy traceability for audits and policy validation.

**Pain Point:** The apportionment process currently operates more like a calculator, performing real-time calculations but without storing historical data. As a result, SME and her team must manually extract and compile years of Excel files to perform any trend analysis, forecasting, or validation. This is time-con...

**Tags:** budget, validation, audit, financial-policy-tribal-research

---

### 002TRB

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Absence of Automated Record Retention / Destruction

> The system must enforce policy-driven retention schedules and automate the secure archiving or deletion of records in accordance with RIM rules. This will reduce database load, improve performance, and ensure compliance with state and federal audit standards.

**Pain Point:** The current system does not have automated record retention or destruction processes in place. Over time, this creates large, unmanageable datasets and raises compliance concerns since OSPI is required to adhere to strict records management (RIM) rules. The lack of automation leaves staff with the b...

**Tags:** automation, audit, financial-policy-tribal-research

---

### 003APP

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Business

**Background:** Annual Carryover Recovery Outside the System

> The apportionment system should allow staff to run calculations for specific subsets of districts (e.g., only charter schools or selected districts) in addition to statewide runs. It should also support running multiple calculations in parallel to accommodate testing scenarios or partial recalculations. This flexibility will save staff considerable time and reduce the burden of processing one district at a time, ensuring that recalculations can be targeted and efficient.

**Pain Point:** The system restricts staff to processing calculations either for all school districts or one at a time. It does not allow selective runs, such as calculating only for charter schools or a subset of districts. This limitation adds considerable manual work, especially when staff need to test or reproc...

**Tags:** apportionment

---

### 003EXP

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Complex crosswalk for federal reporting (F-33, NPEFS, SFFLS)

> The system must automate federal reporting crosswalks by embedding mapping logic between the state chart of accounts and federal formats (F-33, NPEFS, SFFLS). It should allow OSPI staff to configure and maintain mapping rules, generate outputs in federally required formats, and update crosswalks as federal definitions evolve. This reduces manual rework, ensures accuracy, and accelerates compliance reporting.

**Pain Point:** OSPI’s chart of accounts differs from federal formats. SME must manually build crosswalks by pulling SQL data into Tableau, exporting to Excel, then re-exporting into custom Excel workbooks. This is highly inefficient and error-prone.

**Tags:** reporting, automation, audit, financial-expenditures-reporting

---

### 003PRS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Manual Calculations for Apportionment and Ratios

> The system must centralize all calculations within a secure and auditable environment. This includes compliance ratios, apportionment metrics, and K-12 penalties. Automated workflows should pull enrollment data directly, apply standardized formulas, and generate results that can be traced and validated. Eliminating Excel/Access dependencies will ensure continuity if key staff leave and allow broader staff access to calculations.

**Pain Point:** SME explained that while the S-275 system collects data, almost all calculations for staff-per-student ratios, compliance checks (K-3, K-12 penalties), and PSES staffing are performed outside the system in Excel or Access. Additionally, enrollment supervisor provides P223 files externally, which SME...

**Tags:** enrollment, calculation, validation, automation, security

---

### 003TRB

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Reliance on Manual Excel Processing & Error-Prone Workflows

> The new system should provide direct database access, user-defined fields, and no-code calculation capabilities to minimize reliance on manual Excel work. It should also include built-in data normalization features to streamline reporting and reduce the risk of discrepancies.

**Pain Point:** Much of the work today depends on exporting Excel files, cleaning them, and creating temporary databases. This reliance on manual processes increases errors, limits scalability, and consumes staff time that could otherwise be spent on higher-value analysis. It also leads to duplicate efforts when di...

**Tags:** reporting, security, financial-policy-tribal-research

---

### 004TRB

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** No Sandbox Environment for Legislative Updates & Testing

> The system should provide a dedicated sandbox environment that allows for safe testing of new formulas, legislative updates, or code changes. This environment must mirror production data, ensuring accurate modeling without disrupting live operations.

**Pain Point:** When new legislation or policy changes impact apportionment formulas, SME and her team cannot test or simulate these updates within the system. Instead, they are forced to build and maintain large Excel-based models (“Mega” and “Mighty”), which are cumbersome, fragile, and prone to inaccuracies. Thi...

**Tags:** calculation, financial-policy-tribal-research

---

### 005INT

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Data inconsistency across years and lack of historical depth

> The system should enforce schema stability and provide access to 10–20 years of historical data. Historical datasets must be retained in a standardized format, enabling longitudinal studies, forecasting, and legislative reporting without manual archival.

**Pain Point:** Apportionment extracts change column layouts from year to year, breaking queries and requiring SME to re-engineer Power Query steps. Additionally, prior-year retention is limited, with only recent data available (post-2019). This prevents long-term trend analysis and weakens forecasting capability.

**Tags:** reporting, security, data-integration-reporting

---

### 005PRS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Dependence on Access Database for Staff Calculations

> The future system must replace Access with a modern database or warehouse solution. Role-based security should allow multiple OSPI staff to run calculations, extract reports, and access data without bottlenecks. Centralizing the data in a supported environment will also enable integration with downstream systems and improve business continuity by reducing reliance on a single expert.

**Pain Point:** Staffing data is still processed and analyzed in Microsoft Access. SME described a workflow where district submissions are exported from EDS into Access, where queries and macros are run to prepare compliance and apportionment data. This approach restricts access to one SME and limits data availabil...

**Tags:** reporting, integration, security, personnel-reporting

---

### 006APP

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

> The system must include built-in functionality to process carryover recovery annually, using F-196 data directly rather than relying on external spreadsheets. By automating this once-a-year process, the system will ensure consistency, reduce the risk of manual miscalculation, and provide clearer audit trails. This will also allow districts to view and verify recovery calculations in real time, improving transparency and accountability.

**Pain Point:** At year-end, carryover recovery must be processed using external spreadsheets. Data from F-196 reports, provided by finance staff, is compared against district expenditures, and unspent funds are clawed back. SME explained that this once-a-year process is entirely manual, requiring reconciliation ou...

**Tags:** budget, automation, audit, apportionment

---

### 006INT

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Business

**Background:** Limited ability for forecasting and proactive analytics

> The system should support forecasting and trend analytics by retaining deep historical data and providing data models optimized for predictive reporting. Standardized schemas and consistent historical records will enable more robust analysis to support decision-makers.

**Pain Point:** Currently, SME can only report on what happened last year. Without structured, historical, and standardized data, forecasting trends such as levy collection or district financial health is difficult and time-intensive. This limits the ability to provide proactive insights for policy and decision-mak...

**Tags:** reporting, data-integration-reporting

---

### 009SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Audit-driven changes must be made clearer

> The system must clearly distinguish audit adjustments within reports by showing when and how each change occurred. Currently, adjustments overwrite prior values without context, making audit trails unclear. The improved design should log adjustment dates, sources, and rationales.

**Pain Point:** When audit adjustments are made, they are added into the adjustment column displayed on a report. When adjustments are made to adjust from the prior year both values have to be added together manually. There is no way to see when each adjustment is made; the values are just replaced with new ones. I...

**Tags:** reporting, audit, ui, all

---

### 010SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Calculations must be brought into the system

> All report calculations currently performed through external macros must be integrated into the Apportionment System. Doing so eliminates dependency on outdated 16-bit macro program and manual workarounds. The system should execute these calculations natively to produce district-level PDFs and other outputs.

**Pain Point:** Numerous reports are still calculated outside of the Apportionment system, using old macros to generate PDF reports for each district. The macro was programed using 16-bit which requires OSPI to use an old computer to run these reports. Normalizing this process will save time and reduce errors.

**Tags:** reporting, integration, all

---

### 027SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Display elements for formulae

> Screens that allow users to create, read, update, or delete (CRUD) formulae must include clear display elements such as formula name, type, expression, item code, and calculation order. These elements provide transparency into how calculations are configured and executed. Authorized users can easily diagnose issues or verify calculation setups.

**Pain Point:** The screens on which authorized users CRUD formulas must provide a Name, Type (e.g., Mathematical Formula), Expression (the formula itself), an Item Code, and a control to order the formulae for calculations, so that the user can see how the system is setup and can diagnose issues.

**Tags:** calculation, ui, all

---

### 064SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Generate real-time displays of recovery and carryover spreadsheet data

> The system must generate real-time updates of Recovery and Carryover data, replacing manual spreadsheet updates.

**Pain Point:** Provide a feature in the system that automatically updates the Recovery and Carryover Spreadsheet Tool so that districts will have real-time data.

**Tags:** all

---

### 072SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Calculate by school as well as district

> The system must provide an optional function to calculate apportionment funding at the individual school level, not only the district level.

**Pain Point:** The system must optionally calculate apportionment by school in addition to district.

**Tags:** calculation, all

---

### 094SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Forecast year-end balances by end of June

> The system shall have the ability to forecast and display year-end balances for all school districts by the end of June. It shall automatically calculate projections using monthly apportionment data, including the final two months of the prior school year and current-year enrollment trends. The system shall provide a reporting option that enables users to toggle between SAFS fiscal year (July–June) and district fiscal year (September–August) views, ensuring flexibility for both audit and operational planning.

**Pain Point:** To help the Apportionment team visualize fiscal year appropriations, the system should provide the ability to forecast districts' remaining balances by the end of June. This calculation should include the final two months of the prior school year, which (along with every other month) takes into cons...

**Tags:** enrollment, reporting, calculation, automation, audit

---

### 103SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Process calculations for multiple districts simultaneously

> The system must support the ability to run calculations for multiple districts at once, improving efficiency during large-scale financial processing.

**Pain Point:** The system must provide the ability to process calculations for multiple districts simultaneously.

**Tags:** budget, budget-extension-statement-(f-200)

---

### 107SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Support 'short payment' adjustments to monthly allotment #%

> The system shall have the ability to adjust monthly apportionment percentages to accommodate short payments when full disbursement is not possible. It shall enable OSPI apportionment staff to set, record, and track temporary reductions and automatically calculate corresponding make-up payments in subsequent months. The system shall maintain an audit trail showing the adjustment reason, date, and reconciliation details for each affected district.

**Pain Point:** School Apportionments are typically allotted each month by a fixed percentage, ranging from 5 - 12.5%, of the whole. Under certain circumstances, the system must support apportionment staff altering the apportionment % for a given school district and month.

**Tags:** calculation, automation, audit, estimate-for-state-revenues-(estimate-for-state-revenues-(f-203)

---

### 113SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Use confirmed monthly enrollment counts as point-in-time data

> The system must store confirmed monthly enrollment counts as point-in-time data for future reporting and analysis. Reports should be based on confirmed counts to ensure consistency.

**Pain Point:** The system must store confirmed monthly enrollment counts as point-in-time data. Update reports to use point-in-time confirmed monthly enrollment counts.

**Tags:** enrollment, reporting, enrollment-reporting-(p-223)

---

### 001ENR

**Priority:** 🟢 Low | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Reports not showing correct totals

> The new system must calculate totals directly from source enrollment data in real time. Any discrepancies should be automatically flagged. The reporting engine should ensure consistency between displayed totals and exported data, eliminating manual reconciliation.

**Pain Point:** During reporting, totals displayed on the screen sometimes don’t reflect actual student-level data. This occurs because the current system relies on cached calculations that may time out, resulting in zero or incomplete totals. Districts and OSPI staff then waste time reconciling mismatched numbers,...

**Tags:** enrollment, reporting, calculation, automation, ui

---

### 002BUD

**Priority:** 🟢 Low | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Inability to Correct Values Without IT Support (F197 & F200)

> The system must allow authorized OSPI staff to adjust F-197 balances and item codes directly through a controlled user interface, with full audit logging to track changes. This would eliminate reliance on IT staff for routine balance corrections, reduce turnaround times for resolving errors, and ensure that financial data used by districts, ESDs, and OSPI is always accurate and up to date.

**Pain Point:** SME frequently encounters issues where balances (e.g., beginning fund balances in F197) do not roll forward correctly. Currently, he cannot change many item codes himself and must request IT to manually fix errors. SME explained that balances frequently fail to roll forward correctly from the prior ...

**Tags:** budget, audit, ui, budgeting

---

### 002ENR

**Priority:** 🟢 Low | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Revisions erase previously reported numbers

> The system must preserve original data when revisions are made. Revision forms must be pre-populated with prior values, and validations must block submission of “zero” revisions unless explicitly intended.

**Pain Point:** When districts create a revision and hit “Save,” sometimes all numbers in the file are zeroed out due to system slowness. This not only forces resubmission but also risks OSPI unknowingly processing blank files, which could lead to incorrect funding allocations.

**Tags:** validation, enrollment-reporting

---

### 003ENR

**Priority:** 🟢 Low | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Deleting a revised file deletes the original

> The system must maintain clear separation between original files and revisions. Only the selected revision should be deleted, and a confirmation step plus audit log must protect original submissions.

**Pain Point:** Staff noted that deleting a faulty revision sometimes also deletes the original baseline file. This creates data integrity risks and could erase official enrollment counts needed for auditing and apportionment.

**Tags:** audit, enrollment-reporting

---

## Data Reporting

*31 requirements*

### 007APP

**Priority:** 🔴 High | **Complexity:** 📈 Medium | **Type:** Business

> The system should be able to compile individual district reports into a single consolidated package automatically, without requiring Adobe Acrobat scripting or manual manipulation. This includes generating both district-level and statewide rollups in standardized formats, with the ability to schedule or batch reports as needed. By automating report compilation and distribution, staff time will be saved, errors minimized, and reports delivered faster to fiscal teams and the public website.

**Pain Point:** The apportionment system produces multiple individual reports for districts, but it lacks the ability to generate a consolidated package. SME must download the reports, then use Adobe Acrobat and JavaScript scripts to compile them into a single deliverable. This manual step is both technical and cum...

**Tags:** reporting, automation, apportionment

---

### 0010PRS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Lack of Ad Hoc Reporting Capability

> The new system must provide robust ad hoc reporting tools with a user-friendly interface. Users should be able to filter, aggregate, and export staffing data without requiring database skills. Role-based permissions should control access to sensitive data fields, while still enabling flexible reporting for authorized users. This capability would empower staff across OSPI to self-serve and reduce bottlenecks on specialized personnel.

**Pain Point:** SME described frequent ad hoc data requests that cannot be fulfilled directly in EDS. Instead, he manually extracts data from Access or Excel, creating custom datasets on demand. This is time-intensive and error-prone, and it prevents broader OSPI staff from accessing staffing data without SME inter...

**Tags:** reporting, security, ui, personnel-reporting

---

### 0014ENR

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Non-ADA compliant reports

> The system must generate ADA-compliant reports by default. Interactive web reports or accessible PDFs should be provided, ensuring compliance with accessibility standards.

**Pain Point:** Reports posted on the web are static PDFs and not ADA compliant. This limits accessibility for users with disabilities and does not meet state and federal compliance standards.

**Tags:** reporting, security, enrollment-reporting

---

### 001BUD

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Manual and Outdated Systems (Access, Excel, Legacy Design)

> The system must migrate to a modern, database-driven architecture interactive front end. This should allow users (districts, ESDs, auditors, unions) to report data dynamically without reliance on Access or manual file handling. Historical data must be standardized and version-tracked so that cross-year comparisons remain valid even when formulas or item codes change. This self-service capability would streamline audit support, reduce IT workload, and ensure timely delivery of accurate financial information to auditors.

**Pain Point:** Much of the financial reporting (F195/F197) relies on outdated tools such as Access databases, Excel spreadsheets, and early-1990s style processes. For example, Access files must be manually created and uploaded to OSPI’s site. Item dictionaries and formulas change year to year, creating inconsisten...

**Tags:** budget, reporting, calculation, security, audit

---

### 002EXP

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Business

**Background:** ADA Compliance for Reports

> The updated system should continue supporting electronic retention and provide secure storage with search and retrieval capabilities, ensuring records are audit-ready without paper.

**Pain Point:** OSPI staff previously printed expenditure pages for review and record retention. SME confirmed this is no longer required since documents are stored electronically.

**Tags:** audit, financial-expenditures-reporting

---

### 002PRS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Business

**Background:** Manual ADA Compliance Updates

> The future system should generate ADA-compliant outputs by default. Reports should follow Section 508 and WCAG standards without manual intervention, ensuring accessibility for screen readers and other assistive technologies. By automating accessibility formatting, OSPI staff can focus on analysis rather than rework, and the agency can ensure consistent compliance with legal accessibility standards.

**Pain Point:** All public-facing reports must meet ADA accessibility requirements, but currently the formatting is applied manually by OSPI staff after generating Access/Excel outputs. SME confirmed that this process is not tied to a fixed monthly cycle but arises whenever reports are prepared, requiring repetitiv...

**Tags:** reporting, automation, security, audit, personnel-reporting

---

### 003SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Provide connectivity between WorkDay and Apportionment

> The system shall have the ability to integrate directly with the State of Washington’s One Washington (OneWA) platform to exchange payment and financial data. The system shall include an method of transmitting approved invoice and apportionment payment data, incorporating all OneWA-required data fields (e.g., SWV vendor identifiers and payment codes). The integration shall follow OneWA’s published interface and security specifications, ensuring seamless, automated payment initiation and reconciliation between the Apportionment and Workday systems

**Pain Point:** The system shall support direct integration with One Washington (One WA), Washington's cloud-based, enterprise-wide system for finance, procurement, budget, HR and payroll processesto exchange invoice and payment data, e.g., to transmit approved invoice data to One WA for payment processing.

**Tags:** budget, integration, automation, security, ui

---

### 004INT

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Reports not structured as reusable stored procedures

> The system must enable SQL-based stored procedures for core reporting needs, reducing reliance on R scripts and manual processing. This supports automation, improves reliability, and makes reporting auditable and repeatable.

**Pain Point:** Currently, SME uses R scripts to join, clean, and prepare data for Tableau visualizations. While functional, this approach lacks scalability and reusability. Routine reports could be encapsulated in SQL stored procedures for consistency, automation, and reduced maintenance. \n\nCustomer prefers full...

**Tags:** reporting, automation, audit, data-integration-reporting

---

### 005TRB

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Non-ADA Compliant Reports

> The system must have native ADA-compliant reporting capabilities, automatically generating outputs that meet accessibility standards. This will reduce remediation effort, improve turnaround time for publishing reports, and ensure equitable access to information.

**Pain Point:** Reports generated from the current system are not ADA-compliant. SME often spends days manually remediating reports to meet accessibility standards before publishing them. This adds unnecessary delays, limits transparency, and introduces risks of non-compliance with accessibility laws.

**Tags:** reporting, automation, security, financial-policy-tribal-research

---

### 007PRS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Manual Production of Simplified Data Versions

> The system must include automated publishing options that generate simplified datasets and reports with predefined field subsets. Configurable export formats (CSV, Excel, PDF) should be supported, with automated scheduling. This ensures consistency and reduces staff effort while maintaining control over sensitive data. \n

**Pain Point:** SME confirmed that staff still manually create simplified versions of S-275 data in Access, Excel, and PDFs for public or stakeholder consumption. This requires reformatting and extraction of selected fields. The process is repetitive and error-prone, with little standardization across outputs.

**Tags:** reporting, automation, personnel-reporting

---

### 008APP

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Non-Functional

> While reports have not been processed through ADA compliance tools in recent years, the system should still be capable of generating reports in an ADA-compliant format if required by state or federal guidelines. This may include automated tagging, accessible formatting, and compatibility with assistive technologies. Clarifying the requirement with OSPI leadership will determine whether ADA compliance remains mandatory, but the system should be designed with accessibility in mind to ensure long-term compliance flexibility.

**Pain Point:** The 2018 process indicated that all reports had to undergo ADA compliance remediation before posting. SME stated she has never performed ADA remediation and suspects that OSPI has an exception to bypass this requirement. This means the original pain point may no longer apply, but it remains an area ...

**Tags:** reporting, automation, security, audit, ui

---

### 028SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Users can view prior seven years of data

> The system must enable users to view current-year estimates and data from the previous six school years using a query function. Access to seven years of historical data supports analysis and trend reporting. This functionality enhances longitudinal data tracking and aids in audit reviews. It ensures that authorized users can access past data without requiring external archives.

**Pain Point:** Users may only view estimates associated with the current school year, however the Query by Item Code function may be used to view data items from the previous six school years.

**Tags:** reporting, security, audit, all

---

### 029SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** File types of report outputs

> All system-generated reports must be exportable in multiple file formats, including XML, CSV, TIFF, PDF, Web Archive, and Excel. Supporting diverse formats provides flexibility for users with varying reporting needs.

**Pain Point:** Report outputs must be available in XML, CSV, TIFF, PDF, Web Archive, and Excel file forma.

**Tags:** reporting, all

---

### 065SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Generate final Charter School Commission and Spokane Charter Authorizer reports on demand

> The system must allow OSPI SAFS staff to generate final Charter School Commission and Spokane Charter Authorizer reports whenever required. On-demand generation eliminates dependence on scheduled batch processes.

**Pain Point:** Provide ability for OSPI SAFS Staff to generate final Charter School Commission and Spokane Charter Authorizer reports on demand.

**Tags:** reporting, all

---

### 066SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Generate budget drivers extract for OFM and legislature on demand

> The system must enable OSPI staff to generate budget driver extracts for the Office of Financial Management (OFM) and legislative bodies as needed. The extract should include critical financial and enrollment metrics used for analysis.

**Pain Point:** Provide ability for OSPI SAFS Staff to generate Budget Drivers extract for OFM and legislature on demand.

**Tags:** enrollment, budget, all

---

### 067SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Generate a single combined apportionment report, grouped by school district

> The system shall have the ability to generate a single, combined apportionment report grouped by school district. It shall allow OSPI SAFS staff to produce both district-specific and statewide “master reports” displaying all calculated or reported data. The reporting interface shall include dynamic filtering and selection tools to isolate subsets of districts (e.g., based on enrollment size or program type such as Running Start) for validation and discrepancy analysis.

**Pain Point:** Provide OSPI SAFS Staff the ability to generate a single combined apportionment report, grouped by school district.

**Tags:** enrollment, reporting, calculation, validation, ui

---

### 071SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Report to crosswalk revenue code and budget codes

> The system must include a report that clearly crosswalks revenue and budget codes, displaying their corresponding relationships. This report will help verify that automated mappings are functioning correctly. It provides transparency for audits and simplifies data validation. Crosswalk reports enhance accuracy and confidence in financial reconciliation.

**Pain Point:** The system must provide a report that crosswalks revenue and budget code data.

**Tags:** budget, reporting, validation, automation, audit

---

### 073SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Show recovery values in district state treasurer report

> The District State Treasurer Report must include an additional column showing recovery values, reflecting unspent funds that OSPI must recover.

**Pain Point:** The system must add a column to the district state treasurer report showing recovery values in a new last column (the amount districts don't spend, and OSPI has to recover from them).

**Tags:** reporting, all

---

### 074SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Automate posting reports

> The system shall have the ability to automate the posting of official reports to OSPI’s public websites, including the Apportionment Reports page and SAFS Data Files repository. It shall support automatic export, formatting (e.g., PDF conversion), and publication workflows, reducing reliance on manual contractor intervention. The system shall also provide a configurable trigger mechanism and allow authorized users to choose between automated or manual posting modes to ensure flexibility and control over report release timing

**Pain Point:** The process of posting reports to the public website is currently done manually and could be automated.

**Tags:** reporting, automation, all

---

### 083SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Budget reports

> The system must allow OSPI staff to generate budget summary reports and combined roll-up reports across all districts. These reports support high-level financial analysis and state-wide fiscal monitoring.

**Pain Point:** The system must support OSPI Staff to run summary budget reports and rollup-combined reports of all districts.

**Tags:** budget, reporting, budgeting-and-accounting-system-(f-195)

---

### 084SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Budget report output as pdf

> The system must support the generation of budget reports in PDF format for easy sharing, viewing, and archival. PDF outputs ensure consistent formatting and readability across devices.

**Pain Point:** The system must support the generation of .pdf versions of summary budget reports and rollup-combined reports of all districts.

**Tags:** budget, reporting, budgeting-and-accounting-system-(f-195)

---

### 085SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Targeted Item # reports

> The system must allow users to generate targeted reports focused on specific budget item numbers, funds, fiscal years, and districts.

**Pain Point:** The system must support OSPI Staff to run reports of budget activity by school district, fund, fiscal year, and item number(s).

**Tags:** budget, reporting, budgeting-and-accounting-system-(f-195)

---

### 086SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Report of imported budget files status

> The system must generate a report that displays the upload status of all imported budget files. The report should indicate whether files are pending, in process, accepted, or rejected.

**Pain Point:** The system must support OSPI Staff to run a report displaying the status of all imported budget files.

**Tags:** budget, reporting, ui, budgeting-and-accounting-system-(f-195)

---

### 098SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Budget Extension reports

> The system must support both OSPI and district staff in generating various reports related to Budget Extension Statement (F-200). These reports should be viewable within the application and exportable to formats such as PDF or Excel.

**Pain Point:** The system must support OSPI or District Staff to run various reports or combination of budget Extension reports for a District. Reports will be displayed in a viewer which can then be exported/saved to pdf or other data formats.

**Tags:** budget, reporting, budget-extension-statement-(f-200)

---

### 104SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Automatically post budget reports to OSPI's website

> The system shall have the ability to post finalized Budget Extension Statement (F-200) budget reports to OSPI’s public website once they are accepted. While the process may include a manual confirmation step, the system shall automatically format and prepare the budget report for publication in the required template, ensuring consistency and accuracy. It shall also maintain a record of all published budgets, including approval date, publishing user, and associated school district details.

**Pain Point:** Automatically post budget reports to OSPI's website when the district's budget is accepted by OSPI.

**Tags:** budget, reporting, automation, budget-extension-statement-(f-200)

---

### 106SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Support payments to be made across districts and across organizations

> The system must support processing of payments across multiple districts or organizations, enabling greater flexibility in financial transactions.

**Pain Point:** The system must support payments to be made across districts and across organizations to allow future growth.

**Tags:** budget, budget-extension-statement-(f-200)

---

### 109SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Enrollment Reports

> The system must support users to make school district enrollment reports, based on status type, serving district, school year, month, enrollment group type, record type, and (optionally) filter to Basic Ed (Enrollment Reporting (P-223)), Ancillary (P240), Non-Standard (Enrollment Reporting (P-223)S), Special Education (Enrollment Reporting (P-223)H) data.

**Pain Point:** The system must support users to make school district enrollment reports, based on status type, serving district, school year, month, enrollment group type, record type, and (optionally) filter to Basic Ed (Enrollment Reporting (P-223)), Ancillary (P240), Non-Standard (Enrollment Reporting (P-223)S)...

**Tags:** enrollment, reporting, enrollment-reporting-(p-223)

---

### 116SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Personnel Reports

> The system must support users to make school district personnel reports, based on imported Personnel Reporting (S-275) (Personnel) data.

**Pain Point:** The system must support users to make school district personnel reports, based on imported Personnel Reporting (S-275) (Personnel) data.

**Tags:** reporting, personnel-reporting-(s-275)

---

### 119SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Reports must include fund-based reporting

> The system must include fund-based reporting, which is a current gap.

**Pain Point:** The system must include fund-based reporting, which is a current gap.

**Tags:** reporting, all

---

### 145SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Automate CTC, Open Doors, and Running Start Data Reporting

> The apportionment system shall automate the reporting process for Community and Technical Colleges (CTC), Open Doors, and Running Start programs. The system shall enable CTCs to report student hours directly to the CEDARS data system, eliminating the need for districts to manually review Enrollment Reporting (P-223) RS reports and re-enter data. Similarly, Open Doors and Running Start program data, which are currently distributed to districts on paper forms for manual entry, shall be transmitted electronically and integrated automatically into the apportionment system

**Pain Point:** Program Supervisor of Enrollment & Institutions wants an automated process for Community and Technical Colleges (CTC) system to be able to report student hours directly to Cedars, rather than as now, where the districts review the Enrollment Reporting (P-223) RS and, if they are OK with the numbers,...

**Tags:** enrollment, reporting, integration, automation, enrollment-reporting-(p-223)

---

### 120SAFS

**Priority:** 🟢 Low | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Users can view only records associated with their organization

> The system shall have the ability to restrict data visibility so users and delegates can view only records associated with their assigned district or ESD.

**Pain Point:** School District and ESD-based users (and their contracted district vendors) can view only records associated with their organization. For School Districts, that means that they can view only records for their School District. For ESDs or contracted vendors, they can select and view, and sometimes ed...

**Tags:** all

---

## All

*55 requirements*

### 001EXP

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Manual & fragmented certification/sign-off process

> The apportionment system must include a secure, cost-effective digital certification workflow that complies with OSPI and SAO approval standards. It should allow for flexible integration with digital signature platforms (e.g., Adobe, DocuSign) and ensure an auditable approval trail. This requirement reduces costs, eliminates paper, and ensures OSPI staff can validate certifications in real time without depending on costly external platforms.

**Pain Point:** Previously, districts and ESDs mailed signed certification pages, which was replaced by DocuSign. While effective, SME noted DocuSign is expensive and may be excessive for the level of security needed. He suggested Adobe’s certificate-based signing could achieve the same outcome at lower cost.

**Tags:** validation, integration, automation, audit, financial-expenditures-reporting

---

### 002SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Support data aggregation and disaggregation for reporting

> The Apportionment System must allow OSPI to aggregate and disaggregate district-level financial data to facilitate comprehensive reporting. The system should replace manual Excel-based aggregation processes with automated, coded data integration from F-196 and other sources. Users must be able to combine and separate data for reporting at the district, ESD, and school levels.

**Pain Point:** OSPI is in the early stages of a process by which we'll be able to identify all EDS data by codes, taking what was an Excel based-system to aggregate Annual Financial Statement data with less manual processing. As such, OSPI wants an easy Apportionment System function that aggregates data from the d...

**Tags:** budget, reporting, integration, automation, all

---

### 004SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Support preparing and loading sandbox with four years of funds and rules

> The system shall have the ability to prepare, load, and manage a four-year sandbox containing funds, rules, and budget assumptions used to simulate legislative impacts on the apportionment system. OSPI staff must be able to enter and update (e.g., enrollment, charter counts, pension rates) these models efficiently whenever new legislative budgets are passed. The sandbox shall include an approval process with time-stamped authorization, maintain multiple saved versions of each scenario, and synchronize finalized legislative updates with the SAFS production environment.

**Pain Point:** In response to each budget passed by the legislature, OSPI needs to be able to load the new rules and funds into the system for the coming four years. This is called the sandbox.

**Tags:** enrollment, budget, annual-financial-statement-(f-196)

---

### 005EXP

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Lack of Documentation & Training

> The system must include embedded documentation, training guides, and knowledge transfer materials. It should also store business rules (e.g., edits, approval conditions) in a transparent, staff-accessible format. This reduces onboarding pain, preserves institutional knowledge, and ensures continuity when staff transitions occur.

**Pain Point:** SME inherited the system with minimal knowledge transfer and had to reverse-engineer processes, creating inefficiency and risk. He highlighted that institutional knowledge was lost when prior staff left.

**Tags:** validation, security, ui, financial-expenditures-reporting

---

### 006TRB

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Data Silos & Inconsistent Use of S-275 Data

> The system should provide a unified data model and shared access framework to ensure that all teams use the same authoritative source. Standardized reporting and integration across systems will reduce duplication, eliminate inconsistencies, and increase trust in outputs.

**Pain Point:** Different OSPI teams pull S-275 and related data in different ways, often resulting in inconsistent outputs and interpretations. This lack of standardization undermines confidence in the data and forces additional reconciliation work across departments.

**Tags:** reporting, integration, security, financial-policy-tribal-research

---

### 007BUD

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Reliance on Outdated Access Files for F-195 Reporting

> The system must replace Access-based reporting with a modern, secure, and scalable solution. A centralized data mart or SQL-based repository should store F-195 data, with self-service reporting capabilities provided through a user-friendly visualization tool such as Tableau. This would allow districts, unions, and OSPI to filter, query, and analyze data without technical barriers, ensuring accuracy, transparency, and long-term sustainability of reporting.

**Pain Point:** F-195 budget reporting outputs are currently generated as Microsoft Access database files. SME noted that this is a legacy approach, prone to corruption, and requires manual intervention to create and distribute. Access is no longer a supported platform for enterprise reporting, and its limitations ...

**Tags:** budget, reporting, security, budgeting

---

### 011SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Charter School and Compensation Increase calculation exceptions must be automated

> Currently, districts are identified in SAFS using the CCDDD (County/District number), which does not distinguish special cases, requiring manual adjustments in Excel. The system shall provide the capability to identify and reclassify exceptions for specialized institutions such as Charter Schools, Tribal Schools, and Juvenile Detention Schools within the apportionment system. It shall automate the process of converting monthly payment and compensation increase data from revenue codes to appropriate AFRS budget codings, using appropriation codes matched to school identifiers. This automation shall replace the current manual Excel-based process and ensure accurate, fund-specific reporting for all school types.

**Pain Point:** Monthly payment data is converted from revenue codes displayed on reports to AFRS budget coding. And because Charter Schools are paid out of a different fund most of their payments need to be removed from each revenue code and combined into a different budget coding. This is also similar to the comp...

**Tags:** budget, reporting, automation, ui, all

---

### 014SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Display of calculated fields

> The system must display calculated values on data entry or reporting screens but restrict users from editing them. These read-only fields should be visually distinct, such as by using a shaded background. This ensures users can view system-derived values for verification while maintaining data integrity.

**Pain Point:** A field that contains a value derived from a system calculation is displayed on the appropriate data input or report page but cannot be edited by the user. Such fields are indicated by a physical differentiation, such as the presence of a grey background.

**Tags:** reporting, calculation, validation, ui, all

---

### 015SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Displaying calculated fields when component data are not complete

> When users save partially completed data, the system must still display calculated field values based on available information. Even if some components of the calculation are missing, the display should show interim results after clicking “Save” or “Save and Return.”

**Pain Point:** Fields that include a value that is derived from a system calculation should, once the ‘Save’ or ‘Save and Return’ button has been clicked, show the calculated value, even if not all fields that contribute to that sum have been populated.

**Tags:** enrollment, calculation, ui, all

---

### 018SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Entry, display, and retention of dates

> The system must standardize date entry, display, and storage formats for accuracy and usability. Dates must be stored as MMDDYYYY and displayed with slashes (MM/DD/YYYY), regardless of input format variations such as MM.DD.YYYY. For month/year-only fields, the system must retain MMYYYY and display it accordingly. This uniformity improves data compatibility and user experience across all modules.

**Pain Point:** Wherever a date that includes the day of the month is required (as opposed to a Month/Year), the date must be retained as MMDDYYYY. When displayed, slashes ("/") will be inserted between the Month and Day, and Day and Year.\nUsers may enter the month as MMDDYYYY, MM/DD/YYYY, MM,DD,YYYY or MM.DD.YYYY...

**Tags:** ui, all

---

### 020SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Action triggered by navigating away from a page with unsaved data

> If a user attempts to leave a page with unsaved changes, the system must prompt them to confirm their action. A clear warning message should inform users that unsaved data will be lost unless saved first.

**Pain Point:** If the user attempts to navigate away from a page without having saved any changes they've made in the current session, the system must display a message asking whether they want to continue the process and lose their data, or if they want to save their changes.

**Tags:** all

---

### 023SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** ADA Standards

> All user interfaces and reports must comply with the Americans with Disabilities Act (ADA) Standards for Accessible Design. The system should support assistive technologies and follow accessibility best practices to ensure usability for all individuals, including those with disabilities.

**Pain Point:** All UIs and reports must comply with ADA Standards for Accessible Design

**Tags:** reporting, security, audit, ui, all

---

### 024SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Common UX across components

> Each component within the Apportionment System (such as F195, Budget Extension Statement (F-200), and Enrollment Reporting (P-223)) must share a consistent user experience and visual design. The interface should maintain uniform navigation, data entry patterns, and reporting layouts.

**Pain Point:** Each component of the Apportionment system must employ a common UX and cohesive, shared design language where possible to provide data-driven screen, reports, perform calculations and run edits.

**Tags:** enrollment, budget, reporting, ui, all

---

### 025SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Common calculation processes across components

> The system must employ common calculation methods, rounding rules, and business logic across all modules where applicable. This consistency eliminates discrepancies between components like F195, Budget Extension Statement (F-200), and Enrollment Reporting (P-223). Shared calculation frameworks enhance reliability and simplify maintenance. Standardizing logic also reduces testing effort and improves data reconciliation across the Apportionment ecosystem.

**Pain Point:** Each component of the Apportionment system must employ common calculations, rounding, business logic, and edit functionality to the degree practical.

**Tags:** enrollment, budget, reporting, audit, all

---

### 026SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Data must be dynamically generated when generating a screen or report

> The system must dynamically retrieve and generate data when a screen or report is accessed, ensuring that users always see current information. Rather than relying on cached or static data, the system should pull live database values.

**Pain Point:** Screens and reports must be dynamically generated based on database values to the extent practical.

**Tags:** reporting, security, all

---

### 031SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Any processing that takes more than 10 seconds to perform requires a progress bar

> The system must display a progress bar for any automated process expected to take longer than 10 seconds, such as importing data or generating PDFs. The progress bar should indicate the time remaining and status of the task.

**Pain Point:** Any automated process (such as creating a PDF file or importing data) that is expected to take more than 10 seconds must display a progress bar with an estimate of the time remaining.

**Tags:** reporting, automation, ui, all

---

### 032SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Human readable error messaging

> Error messages must be clear, descriptive, and easily understood by users without specialized technical knowledge. This includes file upload errors, validation issues, or calculation problems.

**Pain Point:** In contrast to the existing system, error messaging--particularly for file inputs--must be interpretable by a human with no special system knowledge.

**Tags:** validation, all

---

### 035SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** History of Data Submissions

> The system shall have the ability to retain and retrieve historical data submissions, maintaining at minimum three prior school years and the current year, and up to 25 years for specific data categories as required by reporting standards. It shall support version management, allowing users to access, compare, and select from previously saved versions of reports such as enrollment and budget data. Historical data must be available through an integrated archival and retrieval process, replacing legacy paper or file-based method

**Pain Point:** The current system does not retain history of data submissions: once data has been submitted, you cannot get to the previously submitted data. It is recommended an archive/historical data retrevial process be implemented.

**Tags:** enrollment, budget, reporting, integration, security

---

### 039SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Account Codes, Personnel Duty Codes, and other settings must be updatable by system administrator

> System administrators must have the ability to update key financial and personnel configuration codes directly, without programming intervention. This includes account codes and personnel duty codes that may change due to policy or legislative updates.

**Pain Point:** System administrators must be able to make accounting changes, such as to Account Codes and Personnel Duty Code Assignments, without resorting to coding (other than SQL or Boolean commands).

**Tags:** budget, all

---

### 040SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Number of funds and codes must be

> The system must support scalability in the number of account codes, personnel duty codes, and funds. This allows OSPI to adapt to future legislative mandates without re-engineering the system.

**Pain Point:** The system must provide the flexibility to expand the number of funds account codes and personnel duty codes, in response to legislatively mandated changes.

**Tags:** all

---

### 041SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Reporting must include subfunding reporting

> Reports generated by the system must include subfund-level reporting to meet state-level accounting requirements. Subfund data enhances visibility into detailed financial transactions.

**Pain Point:** Add SubFunds to front page reporting (Note: SubFunds changes forced by OneWashington apply on a state level, but not the district level

**Tags:** budget, reporting, all

---

### 042SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** SAO must have timely access to all data

> The State Auditor’s Office (SAO) must be able to access and download all apportionment data promptly, in a format compatible with their systems. Timely access facilitates efficient audits and financial oversight.

**Pain Point:** All Apportionment Systems data must be viewable/downloadable in a timely way by the State Auditor's Office (SAO) in a format that is acceptable to them.

**Tags:** budget, security, audit, all

---

### 043SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Support shifting school year to accommodate enrollment change-based payment adjustments

> The system must allow authorized users to shift school year designations forward or backward to recalculate payments based on enrollment changes. This functionality ensures accurate reconciliation of funding when districts modify their data post-calculation. It enables the Apportionment team to make timely adjustments and maintain financial accuracy.

**Pain Point:** The system must permit authorized staff to manually shift forward to the current year, or in some occasions back for one or more years to accommodate changes to current year payments to districts when they modify their enrollment. In such cases, OSPI calculates the difference from when the year end ...

**Tags:** enrollment, budget, calculation, all

---

### 044SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Override values for districts

> Authorized OSPI users must be able to override default values and set district level values within the system in a bulk fashion, using a file that they upload to the system.

**Pain Point:** Authorized OSPI users must be able to override default values and set district level values within the system in a bulk fashion, using a file that they upload to the system.

**Tags:** all

---

### 045SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Acknowledge un/successful file uploads.

> The system must display immediate confirmation messages after every file upload attempt, indicating success or failure. In case of failure, the message must clearly describe the error and steps to correct it.

**Pain Point:** Whenever a file is uploaded to the system, the system must display a dialog indicating whether the upload was successful. If unsuccessful, the dialog must provide human-readable explanations.

**Tags:** enrollment, ui, all

---

### 046SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Aggregation level selection for reports

> The system must allow authorized users to generate reports at various aggregation levels, such as district, ESD, state, or authorizer. This functionality enables customized reporting to suit different administrative or analytical needs. Users should be able to select the desired level before executing the report.

**Pain Point:** Authorized staff must be able to run reports at various aggregation levels (e.g., District/ESD/State/Authorizer).

**Tags:** reporting, all

---

### 047SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Export All' option for reports

> The system must provide an “Export All” function that allows authorized users to generate multiple reports in bulk through an offline process.

**Pain Point:** Authorized staff must be able to request bulk reports to be generated in an offline process via an “Export All \*” process, which starts offline processes.

**Tags:** reporting, all

---

### 048SAFS

**Priority:** 🟡 Medium | **Complexity:** 📊 High | **Type:** Functional

**Background:** Group reports by type on 'Published Reports' screen

> The system must group reports by type to allow for multiple bulk requests to be run concurrently if needed.

**Pain Point:** The system must group reports by type to allow for multiple bulk requests to be run concurrently if needed.

**Tags:** reporting, all

---

### 049SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Retain only most recent version of a given report

> The system must retain only the most recent request of each report, for a particular set of filtered data (e.g., a particular school in a particular year)

**Pain Point:** The system must retain only the most recent request of each report, for a particular set of filtered data (e.g., a particular school in a particular year)

**Tags:** reporting, all

---

### 050SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Allow OSPI staff to set certain Payment Calendar system configurations

> Authorized OSPI staff must be able to configure key payment calendar parameters within the system, such as School Year, End Advance Month, and Maximum Payment. These configurations directly influence how apportionment calculations are executed.

**Pain Point:** The system must support authorized users to set parameters (e.g., School Year, End Advance Month, School District End of Year, ESD End of Year, and Maximum Payment) for a given payment calendar record, which will influence how calculations work.

**Tags:** all

---

### 051SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Create output data from the system into monthly snapshots by specifying the Paid/Not Paid status and Paid Date for a given month's payment

> The system must support the generation of monthly payment snapshots that capture Paid/Not Paid status and Paid Date for each district. These snapshots provide historical views of financial transactions across months.

**Pain Point:** The system must support authorized users to create output data from the system into monthly snapshots by specifying the Paid/Not Paid status and Paid Date for a given month's payment.

**Tags:** budget, all

---

### 052SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Support running calculations for different constituent levels.

> The system must support authorized users to execute calculation processes on a district-by-district, ESD, funder, or Statewide basis. Each output will feature calculations for all sub-levels (e.g., ESD level will have totals for each district it contains, plus the ESD as a whole).

**Pain Point:** The system must support authorized users to execute calculation processes on a district-by-district, ESD, funder, or Statewide basis. Each output will feature calculations for all sub-levels (e.g., ESD level will have totals for each district it contains, plus the ESD as a whole).

**Tags:** all

---

### 053SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Roll School Years function

> The system must provide functionality for authorized users to “roll” school years, closing the current year and initializing the next with identical structures and data elements. This includes transferring formulas, screens, and reports for continuity.

**Pain Point:** The system must support authorized users to "roll" school years, which entails closing the current school year and opening the next school year, which is initially populated with the same data elements (Item Codes), formulae (calculations), screens and reports as the prior year.

**Tags:** reporting, calculation, all

---

### 054SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Roll back to prior school year

> The system must allow authorized users to “roll back” to a previous school year when necessary. This function temporarily reopens closed data sets for corrections or recalculations.

**Pain Point:** The system must support authorized users to "roll back" school years, which entails closing the current school year and reopening the prior school year.

**Tags:** all

---

### 055SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Create snapshots of enrollment, personnel, and student/teacher ratio

> The system must enable authorized users to create synchronized snapshots of enrollment, personnel, and student-to-teacher ratio data at specific points in time. This includes capturing maximum funded class size and other relevant metrics. The snapshots support monitoring compliance with class size mandates and funding calculations.

**Pain Point:** The system must support authorized users to input the maximum funded class size for a given district, and create a snapshot of school level enrollment data at the current time, and a snapshot of Personnel Reporting (S-275) personnel data at the current time, and the resulting student/teacher ratio, ...

**Tags:** enrollment, all

---

### 056SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Notification when all data has been received

> The system must automatically notify OSPI SAFS staff when all required apportionment data from SAFS and non-SAFS systems has been transmitted.

**Pain Point:** Provide an automated notification to OSPI SAFS Staff when all apportionment data from SAFS and non-SAFS systems has been transmitted to the Apportionment system

**Tags:** automation, all

---

### 057SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Configure district groups for reporting

> The system must allow users to configure groups of districts—such as by ESD or custom-defined sets—for reporting purposes. Users can generate reports for individual districts, ESDs, or all districts collectively.

**Pain Point:** The system must permit users to run reports for ESDs and other groups, as well as 'All Districts' or a single district

**Tags:** reporting, all

---

### 058SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Accept incoming files of metadata

> The system must support uploading of metadata files containing state constants and district-specific data. Accepted file types should include CSV, text-delimited, HTML, and XML formats.

**Pain Point:** The system must accept uploads of state constants and district-specific data into apportionment. Currently, this information comes via a .csv, text delimited, html and xtml files.

**Tags:** all

---

### 059SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Group Item Code in format that can be read by Apportionment

> The system must enhance the Estimate for State Revenues (F-203) component to provide item code groupings in a structured format directly compatible with the Apportionment system.

**Pain Point:** Enhance the Estimate for State Revenues (F-203) system to provide item code groupings in a format that can be readily consumed by the Apportionment system.

**Tags:** all

---

### 062SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Enhance Over Payments Report

> The Overpayments Report must be updated to include automated calculations that show whether districts are projected to be overpaid by year-end.

**Pain Point:** Update Over Payments report to include calculations to indicate whether school districts will be overpaid at the end of the year.

**Tags:** reporting, automation, all

---

### 063SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Cross-walking revenue codes to budget codes

> The system must provide functionality automate cross-walking revenue codes to budget codes, so that apportionment reporting for the Budget Office displays the correct budget code data.

**Pain Point:** Provide functionality in the system to automate cross-walking revenue codes to budget codes, so that apportionment reporting for the Budget Office displays the correct budget code data.

**Tags:** budget, reporting, automation, ui, all

---

### 070SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Highlight changes to system metadata and rule changes from school year to school year

> The system shall have the ability to identify, highlight, and track all metadata and rule changes between school years directly within the application. It shall display which fields, constants, or calculations have changed, the date and user responsible for each change, and maintain a comparison view between the current and previous years. The system shall also include a built-in approval workflow requiring designated reviewers to verify and approve rule or metadata updates before implementation, ensuring transparency and auditability of legislative and operational changes.

**Pain Point:** System should highlight in the system metadata and rule changes from year to year.

**Tags:** automation, audit, ui, all

---

### 087SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Permit districts to update budget metadata before submission

> The system must allow district users to modify budget metadata, such as budget name or description, before submission. Districts should also be able to delete draft budgets still in progress.

**Pain Point:** Provide ability to school district users to change budget name and delete budgets while in process at school district.

**Tags:** budget, budgeting-and-accounting-system-(f-195)

---

### 117SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Determine individuals in confidentiality program

> The system must support authorized users to determine which individuals who are in the confidentiality program

**Pain Point:** The system must support authorized users to determine which individuals who are in the confidentiality program

**Tags:** personnel-reporting-(s-275)

---

### 118SAFS

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Redact personal identifying individuals in confidentiality program

> The system must redact personal identifying information for individuals who are in the confidentiality program

**Pain Point:** The system must redact personal identifying information for individuals who are in the confidentiality program

**Tags:** personnel-reporting-(s-275)

---

### 122SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Authoritative Source of Business Rules

> The system shall have the ability to align all business-rule logic and financial processes with the authoritative sources defined by Washington State law, including RCW 28A.505.140, RCW 28A.710.040(5), and corresponding WAC 392-123. It shall also accommodate cross-references to additional statutory sources such as RCW 28A.150.260 for appropriations and WAC 392-121 for enrollment reporting. The system must be designed to reference and apply these governing rules dynamically, ensuring all SAFS functions adhere to the latest OSPI Accounting Manual and statutory guidance.

**Pain Point:** FINANCE—SCHOOL DISTRICT BUDGETING is administered in Washington State in accordance with the statutes of Revised Code of Washington (RCW) 28A.505.140, which authorizes the superintendent of public instruction to promulgate rules and regulations regarding budgetary procedures and practices by school ...

**Tags:** enrollment, budget, reporting, audit, ui

---

### 123SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Washington State School District and Charter School Fiscal Year

> The apportionment system shall recognize and align with the fiscal year for Washington State school districts and charter schools, which begins on September 1 and ends on August 31. All budgeting, revenue, and expenditure tracking within the system must correspond to this fiscal period to ensure accurate financial reporting and compliance with state regulations.

**Pain Point:** The fiscal year for school districts and charter schools shall begin on September 1 and end on August 31.

**Tags:** budget, reporting, audit, all

---

### 124SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Washington State Government Fiscal Year

> The apportionment system shall recognize and align with the Washington State government fiscal year, which runs from July 1 through June 30 of the following year. The system shall correctly associate financial data with the appropriate state fiscal year, named for the calendar year in which it ends (e.g., July 1, 2023 through June 30, 2024 is Fiscal Year 2024), to ensure accurate reporting and compliance with state financial regulations.

**Pain Point:** The state fiscal year runs from July 1 through June 30 of the following year, and is named for the calendar year in which it ends (e.g., July 1, 2023 through June 30, 2024 is state Fiscal Year 2024).

**Tags:** budget, reporting, all

---

### 125SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Some programs use other definitions of fiscal years

> The apportionment system shall support custom fiscal periods for projects, grants, or programs that operate on a fiscal year differing from the standard school district/charter school or state government fiscal years. The system shall ensure that budgets, revenue, and expenditure tracking for these programs include only the estimated amounts occurring within the specified period, enabling accurate and compliant financial reporting for all nonstandard fiscal periods.

**Pain Point:** Some projects, grants, or programs have a fiscal period that differs from either the school or Washington state government fiscal year, and it may be necessary to prepare program budgets that cover a different time period. The current official budget shall include only the estimated revenues and exp...

**Tags:** budget, reporting, audit, all

---

### 126SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Student and staff days counted within school year

> The apportionment system shall accurately account for student and staff days within the school year, which begins on September 1. The system shall include:\n1) School days scheduled prior to September 1.\n2\_ Staff days and preparatory activities occurring before September 1, as defined in employee collective bargaining contracts for the school year, in accordance with WAC 392-121-031.

**Pain Point:** Per the legislature, the following activities shall be considered to be within the school year that commences September 1:\n• School days scheduled prior to September 1.\n• Staff days and activities in preparation for the school year included in employee collective bargaining contracts for the schoo...

**Tags:** all

---

### 142SAFS

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Differentiation between actual school year and Personnel Reporting (S-275) school year

> The apportionment system shall differentiate between the operational school year used by most districts (typically September through June, with optional summer programs) and the Personnel Reporting (S-275) reporting school year, which runs from September 1 through August 31. The system shall ensure that Personnel Reporting (S-275) data aligns with this defined reporting period, regardless of local school calendars, to maintain compliance with OSPI staffing data standards and ensure consistency across all districts.

**Pain Point:** Most districts run a school year September through June plus, for many, summer programs. In Personnel Reporting (S-275) terms, the school year runs from September 1 through August 31.

**Tags:** reporting, personnel-reporting-(s-275)

---

### 183TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Upload file format minimum requirement

> The system must allow users to upload CSV and Excel documents.

**Tags:** all

---

### 004EXP

**Priority:** 🟢 Low | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Reliance on Consultants for Rollover/System Changes

> The system must empower OSPI staff with role-based administrative controls to manage rollover tasks, edit configurations, and update report templates without external support. This ensures greater agility, reduces costs, and allows staff to align system updates with workload cycles. Consultants should only be required for major upgrades or structural changes, not routine adjustments.

**Pain Point:** SME expressed frustration at relying on external consultants for even minor changes like adjusting formulas, edits, or report text. This dependence creates delays, raises costs, and limits flexibility. Internal staff lack the permissions or tools to perform basic updates.

**Tags:** reporting, validation, security, financial-expenditures-reporting

---

### 069SAFS

**Priority:** 🟢 Low | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Create function and workflow for changing values midyear

> The system must support administrators in modifying funds, codes, or formula values within an active school year when exceptional circumstances require it. Midyear change workflows must include validation and audit tracking to preserve data integrity.

**Pain Point:** While it is exceptional to do so, and despite any additional work it may cause, system administrators must sometimes be able to change a fund, code, or formula value within a school year.

**Tags:** calculation, validation, automation, audit, all

---

### 108SAFS

**Priority:** 🟢 Low | **Complexity:** 📉 Low | **Type:** Business

**Background:** sandbox most accurate when actuals can be entered and forecasting was based on actual enrollment

> The system’s “sandbox” should allow entry of actual data and enrollment-based forecasting to enhance its accuracy. Incorporating real-world figures improves alignment between projections and final budget outcomes.

**Pain Point:** Elements of the sandbox changes from year to year, although it's fundamentally stable.\nThe sandbox sometimes does not match the final budgeted amount; the system would improve accuracy if it supported entering the actuals and if it did forecasting using real enrollment.

**Tags:** enrollment, budget, estimate-for-state-revenues-(estimate-for-state-revenues-(f-203)

---

## Sys All

*16 requirements*

### 146TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Individual logins and passwords

> The final, comprehensive system, inclusive of all three work sections, must require individual logins and passwords that are unique within the system.

**Tags:** audit, all

---

### 147TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** SSL encryption

> The final, comprehensive system, inclusive of all three work sections, must require the system must provide SSL encryption with a minimum key length of 2048 bits.

**Tags:** all

---

### 148TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** WATECH and OSPI Standards

> System password validation must meet all OSPI/WaTech standards (Policy 141.10, Section 6.2)\nhttps://watech.wa.gov/sites/default/files/2023-12/141.10\_SecuringITAssets\_2023\_12\_Parts\_Rescinded.pdf

**Tags:** reporting, validation, all

---

### 149TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** SAML 2.0 compliance

> The solution must be SAML 2.0 compliant and use the Secure Access Washington (SAW) system for external authentication and Azure Active Directory for agency authentication.

**Tags:** security, all

---

### 150TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Firewall

> The system must include a firewall with intrustion detection and prevention systems to block unauthorized access attempts.

**Tags:** security, all

---

### 151TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Log unauthorized attempts

> The system must log unauthorized login attempts by date and time, user ID, device, and location.

**Tags:** audit, all

---

### 152TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** User session timeouts

> User sessions must timeout, requiring the user to log in again, after a configurable length of inactive time within the system.

**Tags:** enrollment, audit, all

---

### 153TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Security audits

> Regular security audits and vulnerability assessments must be performed at least once per year for so long as the system remains in use.

**Tags:** security, audit, all

---

### 154TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Compliance with security guidance

> The system must comply with all relevant security regulations, such as PCI, DSS, HIPAA, FERPA, etc.

**Tags:** security, all

---

### 155TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Data privacy laws

> The system must comply with all data privacy laws, such as GDPR or CCPA, as applicable.

**Tags:** all

---

### 156TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Data encryption in flight and at rest

> The system must encrypt and secure data in transit and at rest. Backup files must be encrypted.

**Tags:** all

---

### 157TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Access controls and authentication methods

> The system must implement access controls and authentication mechanisms that meet or exceed State of Washington requirements and industry standards.

**Tags:** security, all

---

### 158TEC

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** WA data retention and deletion mandates

> The system must enforce State of Washington data retention and deletion policies, to ensure that data is retained long enough to comply with data retention schedules, but no longer than necessary, and is properly disposed of when it is no longer needed.

**Tags:** all

---

### 159TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** WATech securing information technolgy assets

> The solution must comply with Washington State Office of the Chief Information Officer (WaTech Securing Information Technology Assets (Standard No. SEC-04-03-S)).

**Tags:** audit, all

---

### 160TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Disaster planning

> The Bidder must possess disaster and continuity of operations plans that meet the standards required by WaTech Policy 151.10.

**Tags:** all

---

### 161TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Security updates

> The system must feature regular updates and patches to address security vulnerabilities and improve system performance.

**Tags:** security, all

---

## Technical

*27 requirements*

### 162TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Supporting concurrent users

> The solution must support ### (TBD) internal and external concurrent users accessing the system with minimal delayed response time during peak performance times.

**Tags:** security, all

---

### 163TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Transactions load

> The system must be able to provide with minimal degradation ### (TBD) transactions per second during peak load.

**Tags:** all

---

### 164TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Response time

> The system must have a response time of less than 1.5 seconds for 95% of requests.

**Tags:** all

---

### 165TEC

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** Uptime

> The system must have uptime of 99.99% per month, excluding scheduled maintenance windows. Maintenance must be scheduled in collaboration with OSPI to minimize customer impact and allow for communication to third-party users.

**Tags:** all

---

### 166TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Scalability

> The system must be scalable to serve all Washington school districts, ESDs, and Tribal Compact schools, even as their number grows.

**Tags:** all

---

### 167TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Data processing after system failure

> The vendor must provide a reliable method of protecting and retrieveing data in the event of a system failure.

**Tags:** all

---

### 168TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Recovery time objective: single point of failure

> The system must have a Recovery Point Objective (RPO) of less than 30 minutes for a single point of failure.

**Tags:** all

---

### 169TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Recovery time objective: data loss

> The system must have a Recovery Point Objective (RPO) of less than 5 minutes for data loss.

**Tags:** all

---

### 170TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Adaptive to new guidance

> The system must be constructed using technology that can quickly be adapted to include future statutory changes, administrative changes, and/or evolving technology to improve the features and functionality.

**Tags:** audit, all

---

### 171TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Timely support and maintenance

> The system must feature timely support and maintenance to ensure that the system remains secure, reliable, and functional.

**Tags:** all

---

### 172TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Realtime monitoring

> The system must provide a capacity for real-time monitoring of system resource usage, including CPU, memory, disk, and network.

**Tags:** all

---

### 173TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** System administrator alerts

> The system must provide alerting and notification mechanisms to notify the OSPI support team of performance issues or errors.

**Tags:** all

---

### 174TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Analytical tools

> The system must provide reporting and analysis tools to identify performance bottlenecks and opportunities for optimization.

**Tags:** reporting, all

---

### 175TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Trace transactions and requests

> The system must provide an ability to trace transactions and requests through the system for debugging and troubleshooting purposes, including for security breaches.

**Tags:** security, all

---

### 176TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Backup and restoration

> The vendor must provide a reliable backup and restoration services.

**Tags:** all

---

### 177TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** WCAG 2.2 AA compatibility

> The system must comply with the Web Content Accessibility Guidelines (WCAG) 2.1 level AA standard for accessibility.

**Tags:** security, ui, all

---

### 178TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Adaptive design for screen resolutions and sizes

> The system must feature responsive design that adapts to different screen sizes and devices.

**Tags:** all

---

### 179TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** SaaS

> The solution must be implemented as Software-as-a-Service or hosted in the cloud by an OSPI-approved platform, or on vendor, OSPI, or OSPI-appproved vendor-contracted premises.

**Tags:** all

---

### 180TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Browser compatibility

> The system must be compatible with the latest versions of Chrome, Firefox, Safari, and Edge.

**Tags:** all

---

### 181TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** OS operating systems

> The system must be compatible with the latest versions of iOS and Android operating systems.

**Tags:** all

---

### 182TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Mobile and desktop compatibility

> The system must be compatible with both mobile and desktop applications.

**Tags:** all

---

### 184TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** API

> The system must allow users to upload data via APIs.

**Tags:** integration, all

---

### 185TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** Technical support and system administrator documentation

> The Bidder must provide comprehensive documentation of the system architecture, codebase, and deployment procedures.

**Tags:** all

---

### 186TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Business

**Background:** Microsoft cloud hosting preference

> OSPI would prefer (but does not insist upon) having the system hosted on Microsoft Azure Cloud services.

**Tags:** all

---

### 187TEC

**Priority:** 🟡 Medium | **Complexity:** 📉 Low | **Type:** Functional

**Background:** OSPI retains ownership rights of system code and data

> OSPI must retain ownership rights to both data stored within the SAFS system, and the coding of the system itself.

**Tags:** all

---

### 188TEC

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Functional

**Background:** SFTPs must be updated to accord with new OSPI framework

> As OSPI is investigating and will soon settle upon a new framework by which SFTPs are consumed, the SAFS SFTPs must be aligned with that framework, while keeping the consumers' (e.g., Districts) end the same as the current format to the degree possible.

**Tags:** all

---

### 189TEC

**Priority:** 🟡 Medium | **Complexity:** 📈 Medium | **Type:** Non-Functional

**Background:** Limiting Collection of Confidential Personnel Data

> WaTech classifies certain personnel data—such as Social Security Numbers (SSN), residential addresses, personal phone numbers, and professional license information—as Category 3 Confidential Information. SAFS currently collects some of this data (e.g., SSN) to support the S-275 reporting process and integration with external systems like eCert. There is an enterprise-wide initiative underway to reduce the collection of SSNs and other sensitive data wherever possible. (See: https://watech.wa.gov/categorizing-data-state-agency)

**Tags:** reporting, integration, security, all

---
