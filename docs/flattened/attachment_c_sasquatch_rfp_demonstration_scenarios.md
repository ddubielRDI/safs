# SASQUATCH RFP Demonstrations

**Please note: This section describes a body of work that falls outside Consultants’ initial RFP response; no action is required from you at this time.** Instead, this section describes the demonstration that Consultants with the best response will be asked to perform before the Apparently Successful Bidder(s) is selected. We reserve the right to modify this section prior to its final distribution.

After reviewing Consultants’ RFP responses, OSPI will establish which are preferred, and request that this subset of Consultants demonstrate their proposal for one or more modules. Consultants may be asked to demonstrate fewer than all three work sections if their proposed solution does not address each, or if OSPI is interested in fewer work section solutions from the Consultant than they provided.

For each work section you are asked to demonstrate, please provide complete information about the ‘connective tissue’ linking your section or sections to the others. For example, if you are asked only to demonstrate your solution for Data Calculations, please include information about the interfaces, processes, and business rules that must be implemented to collect data from the Data Collection module to Data Calculations, and to distribute results of the Calculations to the Data Reporting module.

Demonstrations may be hosted on-line (via Teams) or in person, although the former is preferred by OSPI. The presentations will not be recorded, so Consultants’ non-copyrighted intellectual property will not be subject to public records requests.

Two hours will be allotted for each demonstration, regardless of the number of sections the Consultant has been asked to demonstrate. This timespan must be sufficient to permit extensive (30 – 45 minutes of) questions from reviewers. We recommend that you spend your allotted time devoted to the solution, and not about your entities’ background and skills.

To the degree possible, the Consultant should demonstrate working software, rather than paper mockups or process flow maps. It is not required that this software be able to permit an end-to-end flow; in other words, demonstrating small, stand-alone functional units will suffice.

For each work section the Consultant plans to bid, the Consultant must use SAFS data files that are publicly available at <https://ospi.k12.wa.us/safs-data-files>, to demonstrate the following functionality using real-world data. The data set (i.e., the district and school year selected by OSPI) will be Tumwater School District data from the 2024-25 school year. (District Code 34033).

Each demonstration scenario may earn up to 300 points.

Demonstrate how your solution for any work section you are submitting will support the following acceptance criteria:

## Data Collection

* 1. Enrollment Example:
* Provide an interface for the district to electronically upload their monthly enrollment, including by resident district and at the school level
* Support manual entry and revisions by school districts of their monthly enrollment, with limited technical support or technical training
* Demonstrate system data validations to meet the enrollment reporting rules, using these scenarios:
  + Compare a month’s enrollment data to the prior month’s numbers
  + Trigger edits where month-to-month differences are statistically significant
  + Districts can review/make corrections or submit comments to explain why the data is correct
  1. Budget Example
* Provide an interface for the district to electronically upload their monthly financial information
* Demonstrate system data validations to meet budget reporting rules, using these scenarios:
  + Demonstrate edits that check completeness of the incoming report
  + Compare a month’s financial data to the prior month’s numbers
  + Trigger edits where month-to-month differences are statistically significant
  + Demonstrate edits that note unreasonable amounts
  + Trigger edits to ensure program/activity/object combinations are valid
  1. A user-friendly graphical user interface for OSPI-based users to enter and review data by districts, and reviewing and approving the submitted data sets at regional (ESD), and state levels
  2. Lock data for all districts, a subset of districts, or a single district for monthly calculation purposes, and annually for audit purposes

## Data Calculation

Data sets for the following are populated and updated as described:

1. **Production**:

A.1. **Production** (a ‘live’ set of tables); data reporting updates from school districts (and occasionally OSPI) are added here after being validated and approved

A.2. Submitted data can be reviewed, adjustments can be made by OSPI, and apportionment calculations can be run for all districts, a subset of districts, or a single district.

A.3. Audit trails exist for any adjustments made to calculations, state constants, and district data.

1. **Sandbox**: Authorized users from any of three user populations—OSPI, school districts, and the legislature—can copy data, statewide constants, and formulae from production that they are entitled to see, and modify them to create scenarios

B.1. Sandbox users can create and manipulate many scenarios simultaneously

B.2. Users can view and compare multiple scenarios simultaneously

B.3. Users can view and compare a scenario to production (on different windows if necessary)

## Data Reporting

1. The system generates monthly enrollment reports
2. The system generates an annual budget report (“projections”)
3. The system generates an annual financial statement (“actuals”)
4. The system generates data into various Excel reports, to support users who want to generate their own ad-hoc reporting, with limited technical support or technical training
5. System reporting provides native functionality (e.g., customized API integration) to share any data with external systems