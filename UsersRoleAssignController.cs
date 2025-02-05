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
    public class UsersRoleAssignController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        //user role assign view page load function
        public ActionResult UsersRoleAssign()
        {

            List<MenuModel> MenuList = DashboardMenuReload();
            ViewBag.menuList = MenuList;
            if (Session["dtLoginData"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!MenuList[0].hasPermission)
                return RedirectToAction("Index", "Home");

            DataTable dtUsersRoleInfo = new DataTable();
            
            string ConnString = "USER_MANAGEMENT";
            Database_ora db = new Database_ora(true, ConnString, "ODP");
            
            Oracle.ManagedDataAccess.Client.OracleParameter[] prmUsersRoleInfo = new Oracle.ManagedDataAccess.Client.OracleParameter[1];
            prmUsersRoleInfo[0] = db.MakeOutParameter("cur_data", Oracle.ManagedDataAccess.Client.OracleDbType.RefCursor, ParameterDirection.Output);
            dtUsersRoleInfo = db.GetDataSet("DPG_ADMIN_ROLE_MENU_MAP.DPD_USER_ROLE_LIST", prmUsersRoleInfo).Tables[0];

            ViewBag.usersRoleList = makeUsersRoleData(dtUsersRoleInfo);
            ViewBag.menuList = DashboardMenuReload();
            return View("~/Views/UserManagement/UsersRoleAssign.cshtml");

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
                if (dtMenuInfo.Rows[i]["MENU_URL"].ToString().Contains("UsersRoleAssign/UsersRoleAssign"))
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
                UsersRoleModel.ROLE_DESCR = dtUsersRoleInfo.Rows[i]["ROLE_NAME"].ToString();
                UsersRoleModel.ROLE_ID = dtUsersRoleInfo.Rows[i]["ROLE_ID"].ToString();
                dataBundle.Add(UsersRoleModel);
            }
            return dataBundle;
        }

        //unassigned menu function
        public ActionResult UnAssignedMenu(string roleId)
        {
            DataTable dtUnAssignMenu = new DataTable();

            string ConnString = "USER_MANAGEMENT";
            Database_ora db = new Database_ora(true, ConnString, "ODP");

            Oracle.ManagedDataAccess.Client.OracleParameter[] prmUnAssignMenu = new Oracle.ManagedDataAccess.Client.OracleParameter[2];
            prmUnAssignMenu[0] = db.MakeOutParameter("cur_data", Oracle.ManagedDataAccess.Client.OracleDbType.RefCursor, ParameterDirection.Output);
            prmUnAssignMenu[1] = db.MakeInParameter("p_Role_ID", roleId, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            dtUnAssignMenu = db.GetDataSet("DPG_ADMIN_ROLE_MENU_MAP.DPD_USER_MENU_LIST_UNASSIGN", prmUnAssignMenu).Tables[0];

            var getList = makeDataUnAssign(dtUnAssignMenu);
            return Json(getList, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);
        }

        //make data unassign menu function
        public List<MenuSelectModel> makeDataUnAssign(DataTable dtUnAssignMenu)
        {
            List<MenuSelectModel> dataBundle = new List<MenuSelectModel>();

            for (int i = 0; i < dtUnAssignMenu.Rows.Count; i++)
            {
                MenuSelectModel MenuSelectModel = new MenuSelectModel();
                MenuSelectModel.MENU_ITEM_ID = dtUnAssignMenu.Rows[i]["MENU_ITEM_ID"].ToString();
                MenuSelectModel.MENU_NAME = dtUnAssignMenu.Rows[i]["MENU_NAME"].ToString();
                dataBundle.Add(MenuSelectModel);
            }
            return dataBundle;
        }

        //assigned menu function
        public ActionResult AssignedMenu(string roleId)
        {
            DataTable dtAssignMenu = new DataTable();

            string ConnString = "USER_MANAGEMENT";
            Database_ora db = new Database_ora(true, ConnString, "ODP");

            Oracle.ManagedDataAccess.Client.OracleParameter[] prmAssignMenu = new Oracle.ManagedDataAccess.Client.OracleParameter[2];
            prmAssignMenu[0] = db.MakeOutParameter("cur_data", Oracle.ManagedDataAccess.Client.OracleDbType.RefCursor, ParameterDirection.Output);
            prmAssignMenu[1] = db.MakeInParameter("p_Role_ID", roleId, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            dtAssignMenu = db.GetDataSet("DPG_ADMIN_ROLE_MENU_MAP.DPD_USER_MENU_LIST_ASSIGN", prmAssignMenu).Tables[0];

            var getList = makeDataAssign(dtAssignMenu);
            return Json(getList, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);
        }

        //make data unassign menu function
        public List<MenuSelectModel> makeDataAssign(DataTable dtAssignMenu)
        {
            List<MenuSelectModel> dataBundle = new List<MenuSelectModel>();

            for (int i = 0; i < dtAssignMenu.Rows.Count; i++)
            {
                MenuSelectModel MenuSelectModel = new MenuSelectModel();
                MenuSelectModel.MENU_ITEM_ID = dtAssignMenu.Rows[i]["MENU_ITEM_ID"].ToString();
                MenuSelectModel.MENU_NAME = dtAssignMenu.Rows[i]["MENU_NAME"].ToString();
                dataBundle.Add(MenuSelectModel);
            }
            return dataBundle;
        }

        //save data menu mapping function
        public ActionResult SaveData(string roleId, string userCode, string menuItemId)
        {
            string[] menuItemIds = menuItemId.Split('~');

            int status = 0;
            string[] message = new string[3];

            string ConnString = "USER_MANAGEMENT";
            Database_ora db = new Database_ora(true, ConnString, "ODP");

            Oracle.ManagedDataAccess.Client.OracleParameter[] Params = new Oracle.ManagedDataAccess.Client.OracleParameter[4];
            Params[0] = db.MakeOutParameter("o_Status", Oracle.ManagedDataAccess.Client.OracleDbType.Int32, ParameterDirection.Output);
            Params[1] = db.MakeInParameter("p_ROLE_ID", roleId, Oracle.ManagedDataAccess.Client.OracleDbType.Int32);
            Params[2] = db.MakeCollectionParameter("p_MENU_ITEM_ID", menuItemIds, Oracle.ManagedDataAccess.Client.OracleDbType.Int32, menuItemIds.Length);
            Params[3] = db.MakeInParameter("p_User", userCode, Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2);
            status = db.RunProcedureWithReturnVal("DPG_ADMIN_ROLE_MENU_MAP.DPD_ADMIN_ROLE_MENU_MAP", Params);

            if (status == 1)
            {
                message[0] = "User role assign successfully completed !!";
                message[1] = "#5cb85c";
                message[2] = status.ToString();
            }
            else
            {
                message[0] = "User role assign failed. Please try again.";
                message[1] = "#e35b5a";
                message[2] = status.ToString();
            }

            return Json(message, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);
        }


    }
}