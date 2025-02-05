using System;
using System.Web.Mvc;
using Connection;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using USERMANAGEMENT.Models;
using USERMANAGEMENT.Models.UserManagement;

namespace USERMANAGEMENT.Controllers.UserManagement
{
    public class UsersRoleController : Controller
    {
        //user role view page load function
        public ActionResult UsersRole()
        {

            List<MenuModel> MenuList = DashboardMenuReload();
            ViewBag.menuList = MenuList;
            if (Session["dtLoginData"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!MenuList[0].hasPermission)
                return RedirectToAction("Index", "Home");

            UsersRoleGrid();

            return View("~/Views/UserManagement/UsersRole.cshtml");

        }

        public void UsersRoleGrid()
        {
            DataTable dtUsersRoleInfo = new DataTable();

            string ConnString = "USER_MANAGEMENT";
            Database_ora db = new Database_ora(true, ConnString, "ODP");

            Oracle.ManagedDataAccess.Client.OracleParameter[] prmUsersRoleInfo = new Oracle.ManagedDataAccess.Client.OracleParameter[1];
            prmUsersRoleInfo[0] = db.MakeOutParameter("cur_data", Oracle.ManagedDataAccess.Client.OracleDbType.RefCursor, ParameterDirection.Output);
            dtUsersRoleInfo = db.GetDataSet("DPG_ADMIN_ROLE_MST.DPD_ADMIN_USER_ROLE_GIRD", prmUsersRoleInfo).Tables[0];

            var userRoleGrid = makeUsersRoleData(dtUsersRoleInfo);
            ViewBag.usersRoleList = userRoleGrid;
            ViewBag.menuList = DashboardMenuReload();
            ViewBag.userUpdateList = Json(userRoleGrid, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);
        }

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
            dtMenuInfo = db.GetDataSet("DPG_ADMIN_LOGIN.DPD_ADMIN_MENU", prmMenuInfo).Tables[0];

            List<MenuModel> globalMenuList = makeMenuData(dtMenuInfo);
            return globalMenuList;

        }

        public List<MenuModel> makeMenuData(DataTable dtMenuInfo)
        {
            List<MenuModel> dataBundle = new List<MenuModel>();
            bool isPresent = false;
            for (int i = 0; i < dtMenuInfo.Rows.Count; i++)
            {
                if (dtMenuInfo.Rows[i]["MENU_URL"].ToString().Contains("UsersRole/UsersRole"))
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

        //make user role data function
        public List<UsersRoleModel> makeUsersRoleData(DataTable dtUsersRoleInfo)
        {
            List<UsersRoleModel> dataBundle = new List<UsersRoleModel>();

            for (int i = 0; i < dtUsersRoleInfo.Rows.Count; i++)
            {
                UsersRoleModel UsersRoleModel = new UsersRoleModel();
                UsersRoleModel.ROLE_DESCR = dtUsersRoleInfo.Rows[i]["ROLE_DESCR"].ToString();
                UsersRoleModel.ROLE_ID = dtUsersRoleInfo.Rows[i]["ROLE_ID"].ToString();
                dataBundle.Add(UsersRoleModel);
            }
            return dataBundle;
        }

        //save user role function
        public ActionResult SaveUsersRole(string roleID, string roleName, string rowStatus, string users)
        {
            string[] usersRoleID = roleID.Split('~');
            string[] usersRoleName = roleName.Split('~');
            string[] usersRowStatus = rowStatus.Split('~');
            
            int status = 0;
            string[] message = new string[3];

            string ConnString = "USER_MANAGEMENT";
            Database_ora db = new Database_ora(true, ConnString, "ODP");

            Oracle.ManagedDataAccess.Client.OracleParameter[] Params = new Oracle.ManagedDataAccess.Client.OracleParameter[5];
            Params[0] = db.MakeOutParameter("io_status", Oracle.ManagedDataAccess.Client.OracleDbType.Int32, ParameterDirection.Output);
            Params[1] = db.MakeCollectionParameter("p_ROLE_ID", usersRoleID, Oracle.ManagedDataAccess.Client.OracleDbType.Decimal, usersRoleID.Length);
            Params[2] = db.MakeCollectionParameter("p_ROLE_DESCR", usersRoleName, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2, usersRoleName.Length);
            Params[3] = db.MakeCollectionParameter("p_Array_RowStatus", usersRowStatus, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2, usersRowStatus.Length);
            Params[4] = db.MakeInParameter("p_User", users, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            status = db.RunProcedureWithReturnVal("DPG_ADMIN_ROLE_MST.DPD_USER_ROLE_INSRT_UPDT_DEL", Params);


            if (status == 1)
            {
                UsersRoleGrid();
                message[0] = "User role saved successfully !!";
                message[1] = "#5cb85c";
                message[2] = status.ToString();
            }
            else
            {
                message[0] = "User role failed. Please try again.";
                message[1] = "#e35b5a";
                message[2] = status.ToString();
            }

            return Json(message, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);
        }
    }
}