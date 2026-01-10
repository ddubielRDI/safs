-- ============================================
-- SASQUATCH Demo - Tumwater Enrollment Data
-- Based on OSPI 2024-25 Final Enrollment Data
-- District Code: 34033 (Tumwater School District)
-- ============================================

USE SASQUATCH_Demo;
GO

-- ============================================
-- Create Enrollment Submission for October 2024
-- ============================================

DECLARE @SubmissionId INT;

INSERT INTO dbo.EnrollmentSubmissions (
    DistrictCode, SchoolYear, Month, SubmissionStatus,
    SubmittedBy, SubmittedDate, CreatedDate
)
VALUES (
    '34033', '2024-25', 2,  -- Month 2 = October
    'Submitted',
    'tumwater.admin', DATEADD(DAY, -5, GETDATE()),
    DATEADD(DAY, -7, GETDATE())
);

SET @SubmissionId = SCOPE_IDENTITY();

-- ============================================
-- Load Enrollment Detail Data
-- Based on 2024-25 OSPI data for Tumwater
-- K-12 FTE breakdown from enrollment summary
-- ============================================

-- Tumwater High School (0001) - Grades 9-12
INSERT INTO dbo.EnrollmentData (SubmissionId, SchoolCode, GradeLevel, ProgramType, Headcount, FTE, PriorMonthHeadcount, PriorMonthFTE) VALUES
(@SubmissionId, '0001', '09', 'BasicEd', 310, 309.50, 305, 304.50),
(@SubmissionId, '0001', '10', 'BasicEd', 298, 297.25, 292, 291.25),
(@SubmissionId, '0001', '11', 'BasicEd', 285, 283.75, 288, 286.75),
(@SubmissionId, '0001', '12', 'BasicEd', 265, 263.50, 260, 258.50),
(@SubmissionId, '0001', '09', 'RunningStart', 12, 6.00, 10, 5.00),
(@SubmissionId, '0001', '10', 'RunningStart', 18, 9.00, 15, 7.50),
(@SubmissionId, '0001', '11', 'RunningStart', 45, 22.50, 42, 21.00),
(@SubmissionId, '0001', '12', 'RunningStart', 68, 34.00, 65, 32.50);

-- Black Hills High School (0002) - Grades 9-12
INSERT INTO dbo.EnrollmentData (SubmissionId, SchoolCode, GradeLevel, ProgramType, Headcount, FTE, PriorMonthHeadcount, PriorMonthFTE) VALUES
(@SubmissionId, '0002', '09', 'BasicEd', 285, 284.25, 280, 279.25),
(@SubmissionId, '0002', '10', 'BasicEd', 272, 271.00, 268, 267.00),
(@SubmissionId, '0002', '11', 'BasicEd', 258, 256.75, 262, 260.75),
(@SubmissionId, '0002', '12', 'BasicEd', 245, 243.25, 240, 238.25),
(@SubmissionId, '0002', '09', 'RunningStart', 8, 4.00, 7, 3.50),
(@SubmissionId, '0002', '10', 'RunningStart', 15, 7.50, 12, 6.00),
(@SubmissionId, '0002', '11', 'RunningStart', 38, 19.00, 35, 17.50),
(@SubmissionId, '0002', '12', 'RunningStart', 55, 27.50, 52, 26.00);

-- Tumwater Middle School (0003) - Grades 6-8
INSERT INTO dbo.EnrollmentData (SubmissionId, SchoolCode, GradeLevel, ProgramType, Headcount, FTE, PriorMonthHeadcount, PriorMonthFTE) VALUES
(@SubmissionId, '0003', '06', 'BasicEd', 245, 245.00, 240, 240.00),
(@SubmissionId, '0003', '07', 'BasicEd', 252, 252.00, 248, 248.00),
(@SubmissionId, '0003', '08', 'BasicEd', 238, 238.00, 235, 235.00);

-- Bush Middle School (0004) - Grades 6-8
INSERT INTO dbo.EnrollmentData (SubmissionId, SchoolCode, GradeLevel, ProgramType, Headcount, FTE, PriorMonthHeadcount, PriorMonthFTE) VALUES
(@SubmissionId, '0004', '06', 'BasicEd', 232, 232.00, 228, 228.00),
(@SubmissionId, '0004', '07', 'BasicEd', 240, 240.00, 236, 236.00),
(@SubmissionId, '0004', '08', 'BasicEd', 225, 225.00, 222, 222.00);

-- East Olympia Elementary (0005) - Grades K-5
INSERT INTO dbo.EnrollmentData (SubmissionId, SchoolCode, GradeLevel, ProgramType, Headcount, FTE, PriorMonthHeadcount, PriorMonthFTE) VALUES
(@SubmissionId, '0005', 'K', 'BasicEd', 85, 42.50, 82, 41.00),
(@SubmissionId, '0005', '01', 'BasicEd', 92, 92.00, 90, 90.00),
(@SubmissionId, '0005', '02', 'BasicEd', 88, 88.00, 86, 86.00),
(@SubmissionId, '0005', '03', 'BasicEd', 95, 95.00, 93, 93.00),
(@SubmissionId, '0005', '04', 'BasicEd', 90, 90.00, 88, 88.00),
(@SubmissionId, '0005', '05', 'BasicEd', 87, 87.00, 85, 85.00);

-- Littlerock Elementary (0006) - Grades K-5
INSERT INTO dbo.EnrollmentData (SubmissionId, SchoolCode, GradeLevel, ProgramType, Headcount, FTE, PriorMonthHeadcount, PriorMonthFTE) VALUES
(@SubmissionId, '0006', 'K', 'BasicEd', 78, 39.00, 75, 37.50),
(@SubmissionId, '0006', '01', 'BasicEd', 82, 82.00, 80, 80.00),
(@SubmissionId, '0006', '02', 'BasicEd', 79, 79.00, 77, 77.00),
(@SubmissionId, '0006', '03', 'BasicEd', 85, 85.00, 83, 83.00),
(@SubmissionId, '0006', '04', 'BasicEd', 80, 80.00, 78, 78.00),
(@SubmissionId, '0006', '05', 'BasicEd', 76, 76.00, 74, 74.00);

-- Michael T. Simmons Elementary (0007) - Grades K-5
INSERT INTO dbo.EnrollmentData (SubmissionId, SchoolCode, GradeLevel, ProgramType, Headcount, FTE, PriorMonthHeadcount, PriorMonthFTE) VALUES
(@SubmissionId, '0007', 'K', 'BasicEd', 90, 45.00, 88, 44.00),
(@SubmissionId, '0007', '01', 'BasicEd', 98, 98.00, 95, 95.00),
(@SubmissionId, '0007', '02', 'BasicEd', 95, 95.00, 93, 93.00),
(@SubmissionId, '0007', '03', 'BasicEd', 102, 102.00, 100, 100.00),
(@SubmissionId, '0007', '04', 'BasicEd', 96, 96.00, 94, 94.00),
(@SubmissionId, '0007', '05', 'BasicEd', 92, 92.00, 90, 90.00);

-- Peter G. Schmidt Elementary (0008) - Grades K-5
INSERT INTO dbo.EnrollmentData (SubmissionId, SchoolCode, GradeLevel, ProgramType, Headcount, FTE, PriorMonthHeadcount, PriorMonthFTE) VALUES
(@SubmissionId, '0008', 'K', 'BasicEd', 72, 36.00, 70, 35.00),
(@SubmissionId, '0008', '01', 'BasicEd', 78, 78.00, 76, 76.00),
(@SubmissionId, '0008', '02', 'BasicEd', 75, 75.00, 73, 73.00),
(@SubmissionId, '0008', '03', 'BasicEd', 82, 82.00, 80, 80.00),
(@SubmissionId, '0008', '04', 'BasicEd', 77, 77.00, 75, 75.00),
(@SubmissionId, '0008', '05', 'BasicEd', 74, 74.00, 72, 72.00);

-- Tumwater Hill Elementary (0009) - Grades K-5
INSERT INTO dbo.EnrollmentData (SubmissionId, SchoolCode, GradeLevel, ProgramType, Headcount, FTE, PriorMonthHeadcount, PriorMonthFTE) VALUES
(@SubmissionId, '0009', 'K', 'BasicEd', 88, 44.00, 85, 42.50),
(@SubmissionId, '0009', '01', 'BasicEd', 94, 94.00, 92, 92.00),
(@SubmissionId, '0009', '02', 'BasicEd', 90, 90.00, 88, 88.00),
(@SubmissionId, '0009', '03', 'BasicEd', 97, 97.00, 95, 95.00),
(@SubmissionId, '0009', '04', 'BasicEd', 92, 92.00, 90, 90.00),
(@SubmissionId, '0009', '05', 'BasicEd', 89, 89.00, 87, 87.00);

-- Black Lake Elementary (0010) - Grades K-5
INSERT INTO dbo.EnrollmentData (SubmissionId, SchoolCode, GradeLevel, ProgramType, Headcount, FTE, PriorMonthHeadcount, PriorMonthFTE) VALUES
(@SubmissionId, '0010', 'K', 'BasicEd', 82, 41.00, 80, 40.00),
(@SubmissionId, '0010', '01', 'BasicEd', 88, 88.00, 86, 86.00),
(@SubmissionId, '0010', '02', 'BasicEd', 85, 85.00, 83, 83.00),
(@SubmissionId, '0010', '03', 'BasicEd', 92, 92.00, 90, 90.00),
(@SubmissionId, '0010', '04', 'BasicEd', 87, 87.00, 85, 85.00),
(@SubmissionId, '0010', '05', 'BasicEd', 84, 84.00, 82, 82.00);

-- ============================================
-- Add Special Education FTE
-- ============================================
INSERT INTO dbo.EnrollmentData (SubmissionId, SchoolCode, GradeLevel, ProgramType, Headcount, FTE, PriorMonthHeadcount, PriorMonthFTE) VALUES
(@SubmissionId, '0001', '09', 'SpecialEd', 28, 28.00, 27, 27.00),
(@SubmissionId, '0001', '10', 'SpecialEd', 25, 25.00, 24, 24.00),
(@SubmissionId, '0001', '11', 'SpecialEd', 22, 22.00, 21, 21.00),
(@SubmissionId, '0001', '12', 'SpecialEd', 18, 18.00, 17, 17.00),
(@SubmissionId, '0003', '06', 'SpecialEd', 24, 24.00, 23, 23.00),
(@SubmissionId, '0003', '07', 'SpecialEd', 26, 26.00, 25, 25.00),
(@SubmissionId, '0003', '08', 'SpecialEd', 22, 22.00, 21, 21.00),
(@SubmissionId, '0005', 'K', 'SpecialEd', 8, 8.00, 7, 7.00),
(@SubmissionId, '0005', '01', 'SpecialEd', 10, 10.00, 9, 9.00),
(@SubmissionId, '0005', '02', 'SpecialEd', 12, 12.00, 11, 11.00);

-- ============================================
-- Add sample edit/warning for demo
-- (This triggers the month-over-month variance warning)
-- ============================================
INSERT INTO dbo.EnrollmentEdits (SubmissionId, EditRuleId, Severity, Message, FieldName, FieldValue)
SELECT
    @SubmissionId,
    'ENR-001',
    'Warning',
    'Headcount changed by ' + CAST(CAST((e.Headcount - e.PriorMonthHeadcount) * 100.0 / NULLIF(e.PriorMonthHeadcount, 0) AS DECIMAL(5,1)) AS VARCHAR) + '% from prior month',
    'Headcount',
    CAST(e.Headcount AS VARCHAR) + ' (was ' + CAST(e.PriorMonthHeadcount AS VARCHAR) + ')'
FROM dbo.EnrollmentData e
WHERE e.SubmissionId = @SubmissionId
  AND e.PriorMonthHeadcount > 0
  AND ABS((e.Headcount - e.PriorMonthHeadcount) * 100.0 / e.PriorMonthHeadcount) > 10;

PRINT 'Tumwater enrollment data loaded successfully.';
PRINT 'Submission ID: ' + CAST(@SubmissionId AS VARCHAR);
GO

-- ============================================
-- Summary Query to verify data
-- ============================================
SELECT
    s.SchoolName,
    s.SchoolType,
    SUM(ed.Headcount) AS TotalHeadcount,
    SUM(ed.FTE) AS TotalFTE,
    SUM(ed.PriorMonthHeadcount) AS PriorHeadcount,
    COUNT(*) AS RecordCount
FROM dbo.EnrollmentData ed
JOIN dbo.Schools s ON ed.SchoolCode = s.SchoolCode
JOIN dbo.EnrollmentSubmissions es ON ed.SubmissionId = es.SubmissionId
WHERE es.DistrictCode = '34033'
GROUP BY s.SchoolName, s.SchoolType
ORDER BY s.SchoolType, s.SchoolName;
GO
