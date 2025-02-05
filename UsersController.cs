using System;
using System.Web.Mvc;
using Connection;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using USERMANAGEMENT.Models;
using USERMANAGEMENT.Models.UserManagement;
using USERMANAGEMENT.Models.SetUp;

namespace USERMANAGEMENT.Controllers
{
    public class UsersController : Controller
    {

        //user view page load function
        public ActionResult Users()
        {
            List<MenuModel> MenuList = DashboardMenuReload();
            ViewBag.menuList = MenuList;
            if (Session["dtLoginData"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!MenuList[0].hasPermission)
                return RedirectToAction("Index", "Home");

            DataTable dtUsersInfo = new DataTable();
            DataTable dtUserTypeInfo = new DataTable();
            DataTable dtUsersRoleInfo = new DataTable();
            DataTable dtOrganizationInfo = new DataTable();
            DataTable dtEmployeeList = new DataTable();

            string ConnString = "USER_MANAGEMENT";
            Database_ora db = new Database_ora(true, ConnString, "ODP");

            Oracle.ManagedDataAccess.Client.OracleParameter[] prmUsersInfo = new Oracle.ManagedDataAccess.Client.OracleParameter[1];
            prmUsersInfo[0] = db.MakeOutParameter("cur_data", Oracle.ManagedDataAccess.Client.OracleDbType.RefCursor, ParameterDirection.Output);
            dtUsersInfo = db.GetDataSet("USER_MANAGEMENT.DPG_ADMIN_USER_MST.DPD_USER_MASTER_GRID", prmUsersInfo).Tables[0];

            Oracle.ManagedDataAccess.Client.OracleParameter[] prmUserTypeInfo = new Oracle.ManagedDataAccess.Client.OracleParameter[1];
            prmUserTypeInfo[0] = db.MakeOutParameter("cur_data", Oracle.ManagedDataAccess.Client.OracleDbType.RefCursor, ParameterDirection.Output);
            dtUserTypeInfo = db.GetDataSet("USER_MANAGEMENT.DPG_ADMIN_USER_MST.DPD_USER_TYPE_LIST", prmUserTypeInfo).Tables[0];

            Oracle.ManagedDataAccess.Client.OracleParameter[] prmUsersRoleInfo = new Oracle.ManagedDataAccess.Client.OracleParameter[1];
            prmUsersRoleInfo[0] = db.MakeOutParameter("cur_data", Oracle.ManagedDataAccess.Client.OracleDbType.RefCursor, ParameterDirection.Output);
            dtUsersRoleInfo = db.GetDataSet("USER_MANAGEMENT.DPG_ADMIN_USER_MST.DPD_USER_MASTER_ROLE_LIST", prmUsersRoleInfo).Tables[0];


            //Oracle.ManagedDataAccess.Client.OracleParameter[] prmOrganizationInfo = new Oracle.ManagedDataAccess.Client.OracleParameter[1];
            //prmOrganizationInfo[0] = db.MakeOutParameter("cur_data", Oracle.ManagedDataAccess.Client.OracleDbType.RefCursor, ParameterDirection.Output);
            //dtOrganizationInfo = db.GetDataSet("USER_MANAGEMENT.DPG_ADMIN_USER_MST.DPD_ORG_MST_LIST", prmOrganizationInfo).Tables[0];

            Oracle.ManagedDataAccess.Client.OracleParameter[] prmEMPtype = new Oracle.ManagedDataAccess.Client.OracleParameter[1];
            prmEMPtype[0] = db.MakeOutParameter("cur_data", Oracle.ManagedDataAccess.Client.OracleDbType.RefCursor, ParameterDirection.Output);
            //prmEMPtype[1] = db.MakeInParameter("p_User", userCode, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            dtEmployeeList = db.GetDataSet("USER_MANAGEMENT.DPG_ADMIN_USER_MST.DPD_EMP_LIST", prmEMPtype).Tables[0];


            ViewBag.usersList = makeUsersData(dtUsersInfo);
            ViewBag.userTypeList = makeUserTypeData(dtUserTypeInfo);
            ViewBag.usersRoleList = makeUsersRoleData(dtUsersRoleInfo);
            // ViewBag.organizationList = makeOrganizationData(dtOrganizationInfo);
            ViewBag.EmployeeInfoModel = makeEmployeeData(dtEmployeeList);

            ViewBag.menuList = DashboardMenuReload();

            return View("~/Views/UserManagement/Users.cshtml");
        }

        //make Dashboard Menu Reload function
        public List<MenuModel> DashboardMenuReload()
        {

            string userCode = string.Empty;
            string userName = string.Empty;
            string roleID = string.Empty;

            DataTable dtLoginData = new DataTable();
            if (Session["dtLoginData"] != null)
            {
                dtLoginData = (DataTable)Session["dtLoginData"];
            }

            if (dtLoginData.Rows.Count > 0)
            {
                userCode = Convert.ToString(dtLoginData.Rows[0]["USER_CODE"]);
                userName = Convert.ToString(dtLoginData.Rows[0]["USER_NAME"]);
                roleID = Convert.ToString(dtLoginData.Rows[0]["ROLE_ID"]);
            }

            DataTable dtMenuInfo = new DataTable();

            string ConnString = "USER_MANAGEMENT";
            Database_ora db = new Database_ora(true, ConnString, "ODP");

            Oracle.ManagedDataAccess.Client.OracleParameter[] prmMenuInfo = new Oracle.ManagedDataAccess.Client.OracleParameter[3];
            prmMenuInfo[0] = db.MakeOutParameter("cur_data", Oracle.ManagedDataAccess.Client.OracleDbType.RefCursor, ParameterDirection.Output);
            prmMenuInfo[1] = db.MakeInParameter("p_User_Id", userCode, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            prmMenuInfo[2] = db.MakeInParameter("p_Role_Id", roleID, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            dtMenuInfo = db.GetDataSet("USER_MANAGEMENT.DPG_ADMIN_LOGIN.DPD_ADMIN_MENU", prmMenuInfo).Tables[0];

            List<MenuModel> globalMenuList = makeMenuData(dtMenuInfo);
            return globalMenuList;

        }

        //make Menu Data function
        public List<MenuModel> makeMenuData(DataTable dtMenuInfo)
        {
            List<MenuModel> dataBundle = new List<MenuModel>();
            bool isPresent = false;
            for (int i = 0; i < dtMenuInfo.Rows.Count; i++)
            {
                if (dtMenuInfo.Rows[i]["MENU_URL"].ToString().Contains("Users/Users"))
                {
                    isPresent = true;
                }

                MenuModel MenuModel = new MenuModel();
                MenuModel.hasPermission = false;
                MenuModel.MENU_ITEM_ID = dtMenuInfo.Rows[i]["MENU_ITEM_ID"].ToString();
                MenuModel.PARENT_MENU_ITEM_ID = dtMenuInfo.Rows[i]["PARENT_MENU_ITEM_ID"].ToString();
                MenuModel.MENU_DESCRIPTION = dtMenuInfo.Rows[i]["MENU_DESCRIPTION"].ToString();
                MenuModel.MENU_URL = dtMenuInfo.Rows[i]["MENU_URL"].ToString();
                MenuModel.MENU_ICON = dtMenuInfo.Rows[i]["MENU_ICON"].ToString();
                dataBundle.Add(MenuModel);
            }

            if (isPresent)
            {
                dataBundle[0].hasPermission = true;
            }
            return dataBundle;
        }

        //make users role data function
        public List<UsersModel> makeUsersData(DataTable dtUsersInfo)
        {
            List<UsersModel> dataBundle = new List<UsersModel>();

            for (int i = 0; i < dtUsersInfo.Rows.Count; i++)
            {
                UsersModel UsersModel = new UsersModel();
                UsersModel.USER_ID = dtUsersInfo.Rows[i]["USER_ID"].ToString();
                UsersModel.FULL_NAME = dtUsersInfo.Rows[i]["FULL_NAME"].ToString();
                UsersModel.USER_NAME = dtUsersInfo.Rows[i]["USER_NAME"].ToString();
                UsersModel.MOBILE_NO = dtUsersInfo.Rows[i]["MOBILE_NO"].ToString();
                UsersModel.EMAIL = dtUsersInfo.Rows[i]["EMAIL"].ToString();
                UsersModel.ROLE_ID = dtUsersInfo.Rows[i]["ROLE_ID"].ToString();
                UsersModel.ROLE_NAME = dtUsersInfo.Rows[i]["ROLE_NAME"].ToString();
                UsersModel.USER_TYPE_ID = dtUsersInfo.Rows[i]["USER_TYPE_ID"].ToString();
                UsersModel.USER_TYPE_DESCR = dtUsersInfo.Rows[i]["USER_TYPE_DESCR"].ToString();
                // UsersModel.ORG_CODE = dtUsersInfo.Rows[i]["ORG_CODE"].ToString();
                //UsersModel.ORG_NAME = dtUsersInfo.Rows[i]["ORG_NAME"].ToString();
                dataBundle.Add(UsersModel);
            }
            return dataBundle;
        }

        //make users type data function
        public List<UsersTypeModel> makeUserTypeData(DataTable dtUserTypeInfo)
        {
            List<UsersTypeModel> dataBundle = new List<UsersTypeModel>();

            for (int i = 0; i < dtUserTypeInfo.Rows.Count; i++)
            {
                UsersTypeModel UsersTypeModel = new UsersTypeModel();
                UsersTypeModel.USER_TYPE_DESCR = dtUserTypeInfo.Rows[i]["USER_TYPE_DESCR"].ToString();
                UsersTypeModel.USER_TYPE_ID = dtUserTypeInfo.Rows[i]["USER_TYPE_ID"].ToString();

                dataBundle.Add(UsersTypeModel);
            }
            return dataBundle;
        }

        //Employee TYPE DATA FUNCTION
        public List<EmployeeInfoModel> makeEmployeeData(DataTable dtEmployeeList)
        {
            List<EmployeeInfoModel> dataBundle = new List<EmployeeInfoModel>();
            for (int i = 0; i < dtEmployeeList.Rows.Count; i++)
            {
                EmployeeInfoModel EmployeeInfoModel = new EmployeeInfoModel();
                EmployeeInfoModel.EMP_CODE = dtEmployeeList.Rows[i]["EMP_CODE"].ToString();
                EmployeeInfoModel.FULL_NAME = dtEmployeeList.Rows[i]["FULL_NAME"].ToString();
                dataBundle.Add(EmployeeInfoModel);
            }
            return dataBundle;
        }

        //show Emp Grid  List function
        public ActionResult ShowEmpData(string emp_code)
        {
            DataTable dtEmployeeGridList = new DataTable();

            string ConnString = "USER_MANAGEMENT";
            Database_ora db = new Database_ora(true, ConnString, "ODP");

            //string userCode = GetUsercode();

            Oracle.ManagedDataAccess.Client.OracleParameter[] prmDataList = new Oracle.ManagedDataAccess.Client.OracleParameter[2];
            prmDataList[0] = db.MakeOutParameter("cur_data", Oracle.ManagedDataAccess.Client.OracleDbType.RefCursor, ParameterDirection.Output);
            prmDataList[1] = db.MakeInParameter("p_EMP_CODE", emp_code, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            dtEmployeeGridList = db.GetDataSet("DPG_ADMIN_USER_MST.DPD_EMP_grid", prmDataList).Tables[0];

            var getList = makeEmployeeGridData(dtEmployeeGridList);
            return Json(getList, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);
        }

        //Employee Grid DATA FUNCTION
        public List<EmployeeInfoModel> makeEmployeeGridData(DataTable dtEmployeeGridList)
        {
            List<EmployeeInfoModel> dataBundle = new List<EmployeeInfoModel>();
            for (int i = 0; i < dtEmployeeGridList.Rows.Count; i++)
            {
                EmployeeInfoModel EmployeeInfoModel = new EmployeeInfoModel();
                EmployeeInfoModel.EMP_CODE = dtEmployeeGridList.Rows[i]["EMP_CODE"].ToString();
                EmployeeInfoModel.FULL_NAME = dtEmployeeGridList.Rows[i]["FULL_NAME"].ToString();
                EmployeeInfoModel.GMAIL = dtEmployeeGridList.Rows[i]["GMAIL"].ToString();
                EmployeeInfoModel.PHONE_NO = dtEmployeeGridList.Rows[i]["PHONE_NO"].ToString();
                dataBundle.Add(EmployeeInfoModel);

            }
            return dataBundle;
        }

        //make users role data function
        public List<UsersRoleModel> makeUsersRoleData(DataTable dtUsersRoleInfo)
        {
            List<UsersRoleModel> dataBundle = new List<UsersRoleModel>();

            for (int i = 0; i < dtUsersRoleInfo.Rows.Count; i++)
            {
                UsersRoleModel UsersRoleModel = new UsersRoleModel();
                UsersRoleModel.ROLE_DESCR = dtUsersRoleInfo.Rows[i]["ROLE_NAME"].ToString();
                UsersRoleModel.ROLE_ID = dtUsersRoleInfo.Rows[i]["ROLE_ID"].ToString();

                dataBundle.Add(UsersRoleModel);
            }
            return dataBundle;
        }

        //make Organization data function
        public List<OrganizationListModel> makeOrganizationData(DataTable dtOrganizationInfo)
        {
            List<OrganizationListModel> dataBundle = new List<OrganizationListModel>();

            for (int i = 0; i < dtOrganizationInfo.Rows.Count; i++)
            {
                OrganizationListModel OrganizationListModel = new OrganizationListModel();
                OrganizationListModel.ORG_CODE = dtOrganizationInfo.Rows[i]["ORG_CODE"].ToString();
                OrganizationListModel.ORG_NAME = dtOrganizationInfo.Rows[i]["ORG_NAME"].ToString();

                dataBundle.Add(OrganizationListModel);
            }
            return dataBundle;
        }

        public ActionResult SaveUsers(
            string fullName,
            string userName,
            string userEmail,
            string userPassword,
            string roleName,
            string userType,
            string mobileNo,
            string orgName,
            string rowStatus,
            string users

            )
        {

            int status = 0;
            string[] message = new string[3];

            string ConnString = "USER_MANAGEMENT";
            Database_ora db = new Database_ora(true, ConnString, "ODP");

            Oracle.ManagedDataAccess.Client.OracleParameter[] Params = new Oracle.ManagedDataAccess.Client.OracleParameter[12];
            Params[0] = db.MakeOutParameter("o_status", Oracle.ManagedDataAccess.Client.OracleDbType.Int16, ParameterDirection.Output);
            Params[1] = db.MakeInParameter("p_USER_ID", null, Oracle.ManagedDataAccess.Client.OracleDbType.Int32);
            Params[2] = db.MakeInParameter("p_USER_CODE", userName, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            Params[3] = db.MakeInParameter("p_USER_NAME", fullName, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            Params[4] = db.MakeInParameter("p_EMAIL", userEmail, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            Params[5] = db.MakeInParameter("p_ORG_CODE", orgName, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            Params[6] = db.MakeInParameter("p_MOBILE_NO", mobileNo, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            Params[7] = db.MakeInParameter("p_USER_PASSWORD", userPassword, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            Params[8] = db.MakeInParameter("p_ROLE_ID", roleName, Oracle.ManagedDataAccess.Client.OracleDbType.Decimal);
            Params[9] = db.MakeInParameter("p_USER_TYPE_ID", userType, Oracle.ManagedDataAccess.Client.OracleDbType.Decimal);
            Params[10] = db.MakeInParameter("p_Array_RowStatus", rowStatus, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            Params[11] = db.MakeInParameter("p_User", users, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            status = db.RunProcedureWithReturnVal("USER_MANAGEMENT.DPG_ADMIN_USER_MST.DPD_USER_MASTER", Params);

            if (status == 1)
            {
                message[0] = "User saved successfully !!";
                message[1] = "#5cb85c";
                message[2] = status.ToString();
            }
            else
            {
                message[0] = "User saved failed. Please try again.";
                message[1] = "#e35b5a";
                message[2] = status.ToString();
            }

            return Json(message, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);
        }

        //Update users data
        public ActionResult UpdateUsers(
            string userId,
            string fullName,
            string userName,
            string userEmail,
            string roleName,
            string userType,
            string mobileNo,
            string orgName,
            string rowStatus,
            string users

            )
        {

            int status = 0;
            string[] message = new string[3];

            string ConnString = "USER_MANAGEMENT";
            Database_ora db = new Database_ora(true, ConnString, "ODP");

            Oracle.ManagedDataAccess.Client.OracleParameter[] Params = new Oracle.ManagedDataAccess.Client.OracleParameter[12];
            Params[0] = db.MakeOutParameter("o_status", Oracle.ManagedDataAccess.Client.OracleDbType.Int16, ParameterDirection.Output);
            Params[1] = db.MakeInParameter("p_USER_ID", userId, Oracle.ManagedDataAccess.Client.OracleDbType.Int32);
            Params[2] = db.MakeInParameter("p_USER_CODE", userName, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            Params[3] = db.MakeInParameter("p_USER_NAME", fullName, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            Params[4] = db.MakeInParameter("p_EMAIL", userEmail, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            Params[5] = db.MakeInParameter("p_ORG_CODE", orgName, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            Params[6] = db.MakeInParameter("p_MOBILE_NO", mobileNo, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            Params[7] = db.MakeInParameter("p_USER_PASSWORD", null, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            Params[8] = db.MakeInParameter("p_ROLE_ID", roleName, Oracle.ManagedDataAccess.Client.OracleDbType.Decimal);
            Params[9] = db.MakeInParameter("p_USER_TYPE_ID", userType, Oracle.ManagedDataAccess.Client.OracleDbType.Decimal);
            Params[10] = db.MakeInParameter("p_Array_RowStatus", rowStatus, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            Params[11] = db.MakeInParameter("p_User", users, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            status = db.RunProcedureWithReturnVal("USER_MANAGEMENT.DPG_ADMIN_USER_MST.DPD_USER_MASTER", Params);

            if (status == 1)
            {
                message[0] = "User updated successfully !!";
                message[1] = "#5cb85c";
                message[2] = status.ToString();
            }
            else
            {
                message[0] = "User updated failed. Please try again.";
                message[1] = "#e35b5a";
                message[2] = status.ToString();
            }

            return Json(message, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);
        }

        //suspend users data
        public ActionResult SuspendUsers(
            string userId,
            string user
            )
        {

            int status = 0;
            string[] message = new string[3];

            string ConnString = "USER_MANAGEMENT";
            Database_ora db = new Database_ora(true, ConnString, "ODP");

            Oracle.ManagedDataAccess.Client.OracleParameter[] Params = new Oracle.ManagedDataAccess.Client.OracleParameter[3];
            Params[0] = db.MakeOutParameter("o_status", Oracle.ManagedDataAccess.Client.OracleDbType.Int16, ParameterDirection.Output);
            Params[1] = db.MakeInParameter("p_USER_ID", userId, Oracle.ManagedDataAccess.Client.OracleDbType.Int32);
            Params[2] = db.MakeInParameter("p_User", user, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            status = db.RunProcedureWithReturnVal("USER_MANAGEMENT.DPG_ADMIN_USER_MST.DPD_USER_SUSPEND", Params);

            if (status == 1)
            {
                message[0] = "User suspend successfully !!";
                message[1] = "#5cb85c";
                message[2] = status.ToString();
            }
            else
            {
                message[0] = "User suspend failed. Please try again.";
                message[1] = "#e35b5a";
                message[2] = status.ToString();
            }

            return Json(message, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);
        }

        public ActionResult passValueToView(
            string orgName,
            string orgCode,
            string mobileNumber,
            string emailAddress
            )
        {
            Session["orgName"] = orgName;
            Session["orgCode"] = orgCode;
            Session["mobileNumber"] = mobileNumber;
            Session["emailAddress"] = emailAddress;

            return Json("", "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);
        }
    }
}