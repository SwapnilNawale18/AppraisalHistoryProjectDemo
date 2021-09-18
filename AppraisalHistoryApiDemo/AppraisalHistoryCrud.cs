using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
namespace AppraisalHistoryApiDemo
{
    public class AppraisalHistoryCrud
    {
        static SqlConnection con;
        static SqlCommand cmd;
        static SqlDataReader dr;
        
        public static void CreateConnection()
        {
            string constr = "Data Source = DESKTOP-TMI53VD; Initial Catalog = TCompany; Integrated Security = true; User ID = sa; Password=pass@123";
            con = new SqlConnection();
            con.ConnectionString = constr;
        }
        public static void InsertData(string firstName, string lastName, string gender, string email,string contactNo, string address, int departmentID)
        {
            LastInsertedID();
            con.Open();
            /*
            INSERT INTO Employee(FirstName, LastName, Gender, Email ,ContactNo, Address)
            VALUES('Swapnil', 'Nawale', 'Male', 'swapnilnawale18@gmail.com', '7738244575', 'New Bombay');
            */
            
            string query = "INSERT INTO Employee(FirstName, LastName, Gender, Email, ContactNo, Address, DepartmentID) VALUES(@fn, @ln, @gen, @em, @con, @addr, @did); ";
            cmd = new SqlCommand(query, con);
            cmd.Parameters.Add(new SqlParameter("fn", firstName));
            cmd.Parameters.Add(new SqlParameter("ln", lastName));
            cmd.Parameters.Add(new SqlParameter("gen", gender));
            cmd.Parameters.Add(new SqlParameter("em", email));
            cmd.Parameters.Add(new SqlParameter("con", contactNo));
            cmd.Parameters.Add(new SqlParameter("addr", address));
            cmd.Parameters.Add(new SqlParameter("did", departmentID));
            var r = cmd.ExecuteNonQuery();
            con.Close();
        }
        public static int LastInsertedID()
        {
            con.Open();
            string query = "SELECT top 1 EmployeeID FROM Employee ORDER BY EmployeeID DESC;";
            cmd = new SqlCommand(query, con);
            dr = cmd.ExecuteReader();
            dr.Read();
            int lastID = Convert.ToInt32(dr[0]);
            dr.Close();
            con.Close();
            return lastID;
        }
        public static void InsertNewEmployee(string firstName, string lastName, string gender, string email, string contactNo, string address, int departmentID, string jobRoleName)
        {
            int employeeID, jobRoleId;
            InsertData(firstName, lastName, gender, email, contactNo, address, departmentID);
            employeeID = LastInsertedID();
            jobRoleId = GetJobRoleId(jobRoleName);
            con.Open();
            string query = "INSERT INTO AppraisalInformation(EmployeeID, JobStatusChangeDate, CurrentRoleID, NewRoleID) VALUES("+ employeeID + ", GETDATE(), 0, "+jobRoleId+"); ";

            cmd = new SqlCommand(query, con);
            var r =cmd.ExecuteNonQuery();
            //Console.WriteLine("Added {0} new employee", r);
            con.Close();
        }
        public static void DisplayData()
        {
            con.Open();
            string query = "select Employee.EmployeeID, FirstName, LastName, Performance, DepartmentName from Employee, Department where Employee.DepartmentID = Department.DepartmentID;";
            cmd = new SqlCommand(query, con);
            dr = cmd.ExecuteReader();
            Console.WriteLine("ID\tFirst Name\tLast Name\tPerformance");
            while (dr.Read())
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", dr[0], dr[1], dr[2], dr[3], dr[4]);
            }                
            dr.Close();
            string cmdcount = "Select count(*) from Employee";
            cmd = new SqlCommand(cmdcount, con);
            int count = (int)cmd.ExecuteScalar();
            Console.WriteLine("{0} Employees in T-Company", count);
            con.Close();
        }
        public static void DisplayPerformanceDetails()
        {
            con.Open();
            string query = "select emp.EmployeeID, emp.FirstName, emp.LastName,jr.JobRoleName,emp.Performance,ai.JobStatusChangeDate from AppraisalInformation ai join JobRole jr on ai.NewRoleID = jr.JobRoleID, Employee emp where JobStatusChangeDate = (select max(JobStatusChangeDate) from AppraisalInformation where EmployeeID = emp.EmployeeID)";
            cmd = new SqlCommand(query, con);
            dr = cmd.ExecuteReader();
            Console.WriteLine("ID\tFirst Name\tLast Name\tPerformance\tDepartment");
            while (dr.Read())
            {
                Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", dr[0], dr[1], dr[2], dr[3], dr[4], dr[5]);
            }
            dr.Close();
            string cmdcount = "Select count(*) from Employee";
            cmd = new SqlCommand(cmdcount, con);
            int count = (int)cmd.ExecuteScalar();
            Console.WriteLine("{0} Employees in T-Company", count);
            con.Close();
        }
        public static void DisplayJobRoles()
        {
            con.Open();
            string query = "SELECT * FROM JobRole;";
            cmd = new SqlCommand(query, con);
            dr = cmd.ExecuteReader();
            Console.WriteLine("Job ID\tJob Name");
            while (dr.Read())
            {
                Console.WriteLine("{0}\t{1}", dr[0], dr[1]);
            }
            dr.Close();
            string cmdcount = "Select count(*) from Employee";
            cmd = new SqlCommand(cmdcount, con);
            int count = (int)cmd.ExecuteScalar();
            Console.WriteLine("{0} Job Roles in T-Company", count);
            con.Close();
        }

        public static void UpdateData(int employeeID, string email)
        {
            con.Open();
            string query = "UPDATE Employee SET email = '" + email + "' WHERE EmployeeID = " + employeeID + "; ";
            cmd = new SqlCommand(query, con);
            int r = (int)cmd.ExecuteNonQuery();
            Console.WriteLine("{0} E-mail updated", r);
            con.Close();
        }
        public static void DeleteData(int employeeID)
        {
            con.Open();
            string query = "DELETE from Employee where EmployeeID = '" + employeeID + "'";
            cmd = new SqlCommand(query, con);
            int r = cmd.ExecuteNonQuery();
            if (r == 0)
            {
                Console.WriteLine("No such Employee ID", r);
            }
            else
            {
                Console.WriteLine("{0} Record Deleted", r);
            }
            con.Close();
        }
        public static void DeleteExistingEmployee(int employeeID)
        {
            DeleteData(employeeID);
            con.Open();
            string query = "DELETE from AppraisalInformation where EmployeeID = '" + employeeID + "'";
            cmd = new SqlCommand(query, con);
            int r = cmd.ExecuteNonQuery();
            con.Close();
        }
        public static void JoinedAsNowManager()
        {
            bool recordsFound=false;
            con.Open();
            string query = "select * from Employee where Employee.EmployeeID in (select EmployeeID from AppraisalInformation where NewRoleID in (select JobRoleID from JobRole where JobRoleName = 'Assistant Manager' or JobRoleName = 'Senior Manager') INTERSECT select EmployeeID from AppraisalInformation where CurrentRoleID = (select JobRoleID from JobRole where JobRoleName = 'Intern')); ";
            cmd = new SqlCommand(query, con);
            dr = cmd.ExecuteReader();
            Console.WriteLine("EmpId\t\tFirstName\t\t");
            while (dr.Read())
            {
                Console.WriteLine("{0}\t\t\t{1} {2}", dr[0], dr[1], dr["LastName"]);
                recordsFound = true;
            }
            if(recordsFound !=true)
            {
                Console.WriteLine("There are no employees who joined as a intern and now are managers");
            }
            con.Close();
        }
        public static void MaxAppraisal()
        {
            con.Open();
            string query = "select * From Employee where EmployeeID in( select top 1 with ties EmployeeID from AppraisalInformation group by EmployeeID order by COUNT(EmployeeID) desc ); ";
            cmd = new SqlCommand(query, con);
            dr = cmd.ExecuteReader();
            Console.WriteLine("EmpId\t\t\tName");
            while (dr.Read())
            {
                Console.WriteLine("{0}\t\t\t{1} {2}", dr[0], dr[1], dr["LastName"]);
            }
            con.Close();
        }
        public static void NotChangedAftAppraisal()
        {
            bool recordsFound = false;
            con.Open();
            string query = "select * From Employee where EmployeeID in (select EmployeeID from AppraisalInformation group by EmployeeID having count(EmployeeID) = 2); ";
            cmd = new SqlCommand(query, con);
            dr = cmd.ExecuteReader();
            Console.WriteLine("EmpId\t\t\tName");
            while (dr.Read())
            {
                Console.WriteLine("{0}\t\t\t{1} {2}", dr[0], dr[1], dr["LastName"]);
                recordsFound = true;
            }
            if (recordsFound != true)
            {
                Console.WriteLine("There are no employees whose role was not changed after appraisal.");
            }
            con.Close();
        }
        public static void NotHadAppraisal()
        {
            bool recordsFound = false;
            con.Open();
            string query = "select * From Employee where EmployeeID in(select EmployeeID from AppraisalInformation group by EmployeeID having count(EmployeeID) = 1); ";
            cmd = new SqlCommand(query, con);
            dr = cmd.ExecuteReader();
            Console.WriteLine("EmpId\t\t\tName");
            while (dr.Read())
            {
                Console.WriteLine("{0}\t\t\t{1} {2}", dr[0], dr[1], dr["LastName"]);
            }
            if (recordsFound != true)
            {
                Console.WriteLine("There are no employees who did not have appraisal");
            }
            con.Close();
        }

        public static int BestPerformer()
        {
            int employeeID=1;
            con.Open();
            string query = "select top 1 EmployeeID, FirstName, LastName, Performance from Employee order by Performance desc;";
            cmd = new SqlCommand(query, con);
            dr = cmd.ExecuteReader();
            Console.WriteLine("EmpId\t\t\tName\t\t\tPerformance");
            while (dr.Read())
            {
                Console.WriteLine("{0}\t\t\t{1} {2}\t\t\t{3}", dr[0], dr[1], dr["LastName"], dr[3]);
                employeeID =Convert.ToInt32(dr[0]);
            }
            con.Close();
            return employeeID;
        }
        public static void UpgradeRoleTwice()
        {
            int employeeID = BestPerformer();
            int currentRoleID = GetNewRoleEmp(employeeID);
            int newRoleID = currentRoleID + 2;
            con.Open();
            string query = "INSERT INTO AppraisalInformation(EmployeeID, JobStatusChangeDate, CurrentRoleID, NewRoleID) VALUES(" + employeeID + ", GETDATE(), " + currentRoleID + ", " + newRoleID + ");";
            cmd = new SqlCommand(query, con);
            int r = cmd.ExecuteNonQuery();
            Console.WriteLine("{0} employee upgraded to upper job role due to best performance", r);
            con.Close();
        }
        public static void EligibleForAppraisal()
        {
            
            con.Open();
            string query = "Select EmployeeID, FirstName, LastName, Performance from Employee where Performance >=(select avg(Performance) from Employee) order by Performance desc;";
            cmd = new SqlCommand(query, con);
            dr = cmd.ExecuteReader();
            Console.WriteLine("EmpId\t\t\tName\t\t\tPerformance");
            while (dr.Read())
            {
                Console.WriteLine("{0}\t\t\t{1} {2}\t\t\t{3}", dr[0], dr[1], dr["LastName"], dr[3]);
            }
            con.Close();
        }
        public static void Appraisal(int employeeID, string jobRoleName)
        {
            EligibleForAppraisal();
            int currentRoleID = GetNewRoleEmp(employeeID);
            int newRoleId = GetJobRoleId(jobRoleName);
            con.Open();
            string query = "INSERT INTO AppraisalInformation(EmployeeID, JobStatusChangeDate, CurrentRoleID, NewRoleID) VALUES(" + employeeID + ", GETDATE(), " + currentRoleID + ", "+ newRoleId + ");";
            cmd = new SqlCommand(query, con);
            int r = cmd.ExecuteNonQuery();
            Console.WriteLine("{0} new role upgraded", r);
            con.Close();
        }
        public static int GetNewRoleEmp(int employeeID)
        {
            int newRoleID=0;
            con.Open();
            string query = "select max(NewRoleID)FROM AppraisalInformation where EmployeeID = "+employeeID+"; ";
            cmd = new SqlCommand(query, con);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                newRoleID = Convert.ToInt32(dr[0]);
            }
            con.Close();
            return newRoleID;
        }
        public static void AddRole(string newRoleName)
        {
            con.Open();
            string query = "INSERT INTO JobRole(JobRoleName)VALUES('"+newRoleName+"'); ";
            cmd = new SqlCommand(query, con);
            int r = cmd.ExecuteNonQuery();
            Console.WriteLine("{0} new role Added", r);
            con.Close();
        }
        public static void ModifyRole(string oldRoleName, string newRoleName)
        {
            con.Open();
            string query = "UPDATE JobRole SET JobRoleName = '"+ newRoleName + "' WHERE JobRoleName = '"+oldRoleName+"';";
            cmd = new SqlCommand(query, con);
            int r = (int)cmd.ExecuteNonQuery();
            Console.WriteLine("Modified {0} Job Role", r);
            con.Close();
        }
        public static void DeleteRole(string jobRoleName)
        {
            UpgradeCurrentRoleID(jobRoleName);
            UpgradeNewRoleID(jobRoleName);
            con.Open();
            string query = "DELETE FROM JobRole WHERE JobRoleName = '" + jobRoleName + "';";
            cmd = new SqlCommand(query, con);
            int r = cmd.ExecuteNonQuery();
            Console.WriteLine("{0} Job Role(s) Deleted", r);
            con.Close();
        }
        public static void UpgradeCurrentRoleID(string jobRoleName)
        {
            int jobRoleId = GetJobRoleId(jobRoleName);
            int upperRoleId = GetUpperRoleId(jobRoleId);
            con.Open();
            string query = "UPDATE AppraisalInformation SET CurrentRoleID = "+ upperRoleId + " WHERE CurrentRoleID = "+ jobRoleId + ";";
            cmd = new SqlCommand(query, con);
            int r = (int)cmd.ExecuteNonQuery();
            Console.WriteLine("Modified {0} Job Role", r);
            con.Close();
        }
        public static void UpgradeNewRoleID(string jobRoleName)
        {
            int jobRoleId = GetJobRoleId(jobRoleName);
            int upperRoleId = GetUpperRoleId(jobRoleId);
            con.Open();
            string query = "UPDATE AppraisalInformation SET NewRoleID = "+ upperRoleId + " WHERE NewRoleID = " + jobRoleId + ";";
            cmd = new SqlCommand(query, con);
            int r = (int)cmd.ExecuteNonQuery();
            Console.WriteLine("Modified {0} Job Role", r);
            con.Close();
        }
        public static int GetUpperRoleId(int employeeID)
        {
            int upperRoleId = 1;
            bool visited = false;
            con.Open();
            string query = "select top 1 JobRoleID from JobRole where JobRoleID > 4 order by JobRoleID asc; ";
            cmd = new SqlCommand(query, con);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                upperRoleId = Convert.ToInt32(dr[0]);
                visited = true;
            }
            con.Close();
            if (visited != true)
            {
                return employeeID;
            }
            return upperRoleId;
        }

        public static int GetLowerRoleId(int employeeID)
        {
            int upperRoleId = 1;
            bool visited = false;
            con.Open();
            string query = "select top 1 JobRoleID from JobRole where JobRoleID < 4 order by JobRoleID desc; ";
            cmd = new SqlCommand(query, con);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                upperRoleId = Convert.ToInt32(dr[0]);
                visited = true;
            }
            con.Close();
            if (visited != true)
            {
                return employeeID;
            }
            return upperRoleId;
        }
        public static int GetJobRoleId(string JobRoleName)
        {
            int jobRoleId=0;
            con.Open();
            string query="select JobRoleID FROM JobRole where JobRoleName = '"+ JobRoleName + "'";
            cmd = new SqlCommand(query, con);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                jobRoleId = Convert.ToInt32(dr[0]);
            }
            con.Close();
            Console.WriteLine("job role id of " + JobRoleName+" is "+ jobRoleId);
            return jobRoleId;
        }
        public static void IncreasePerformance(int employeeID, int performance)
        {
            con.Open();
            string query = "UPDATE Employee SET Performance = "+ performance + " WHERE EmployeeID="+ employeeID + "; ";
            cmd = new SqlCommand(query, con);
            int r = (int)cmd.ExecuteNonQuery();
            Console.WriteLine("Modified {0} Job Role", r);
            con.Close();
        }
    }
}
