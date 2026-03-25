namespace HmDianPing.Web.Security
{
    public static class RoleConstants
    {
        public const string User = "User";
        public const string Merchant = "Merchant";
        public const string Admin = "Admin";
        public const string SuperAdmin = "SuperAdmin";

        public const string AdminOrSuperAdmin = Admin + "," + SuperAdmin;
        public const string MerchantOrAdminOrSuperAdmin = Merchant + "," + Admin + "," + SuperAdmin;
    }
}
