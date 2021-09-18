CREATE DATABASE TCompany;

USE TCompany;

Create Table Employee
(
	EmployeeID INT PRIMARY KEY IDENTITY(1,1),
	FirstName VARCHAR(30) NOT NULL,
	MiddleName VARCHAR(30),
	LastName VARCHAR(30) NOT NULL,
	Gender VARCHAR(10) NOT NULL,
	Email VARCHAR(50) NOT NULL,
	ContactNo VARCHAR(13) NOT NULL UNIQUE,
	Address VARCHAR(100) NOT NULL,
	DepartmentID INT NOT NULL,
	Performance INT DEFAULT 0
);

SELECT * FROM Employee;

Create Table Department
(
	DepartmentID INT PRIMARY KEY IDENTITY(1,1),
	DepartmentName VARCHAR(30)
);

SELECT * FROM Department;

Create Table AppraisalInformation 
(
	EmployeeID int NOT NULL,
	JobStatusChangeDate DATE NOT NULL,
	CurrentRoleID INT NOT NULL,
	NewRoleID INT,
)

SELECT * FROM AppraisalInformation;

Create Table JobRole
(
	JobRoleID INT PRIMARY KEY IDENTITY(1,1),
	JobRoleName VARCHAR(30)
)

SELECT * FROM JobRole;

--Employee with maximum appraisals
select * From Employee
where EmployeeID in(
select top 1 with ties EmployeeID
from AppraisalInformation
group by EmployeeID
order by COUNT(EmployeeID) desc
);


--List of employees who joined as a intern and now are managers

select *
from Employee
where Employee.EmployeeID in
(select EmployeeID
from AppraisalInformation
where NewRoleID in (select JobRoleID
from JobRole
where JobRoleName='Assistant Manager' or JobRoleName= 'Senior Manager')
INTERSECT
select EmployeeID
from AppraisalInformation
where CurrentRoleID = (select JobRoleID
from JobRole
where JobRoleName='Intern'));

--Employees for who role was not changed after appraisal.
select * From Employee
where EmployeeID in
(select EmployeeID
from AppraisalInformation
group by EmployeeID
having count(EmployeeID) = 2);
--Employees who did not have appraisal
select * From Employee
where EmployeeID in
(select EmployeeID
from AppraisalInformation
group by EmployeeID
having count(EmployeeID) = 1);

--Display Employee Performance records
select emp.EmployeeID, emp.FirstName, emp.LastName,jr.JobRoleName,emp.Performance,ai.JobStatusChangeDate
from AppraisalInformation ai join JobRole jr
on ai.NewRoleID=jr.JobRoleID, Employee emp
where JobStatusChangeDate=(
select max(JobStatusChangeDate) from AppraisalInformation where EmployeeID=emp.EmployeeID)