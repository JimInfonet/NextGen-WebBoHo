using InfonetCSCR.Reporting.Models;
using InfonetCSCR.Reporting.Repository;

namespace InfonetCSCR.Reporting.Service
{
    public class UserService
    {
        private EncryptionService encryptionService;
        private DBHelper dbHelper;
        public UserService()
        {
            dbHelper = new DBHelper();
            encryptionService = new EncryptionService();
        }

        public bool VerifyUser(User loginUser)
        {
            var user = dbHelper.GetUser(loginUser.Code);
            if (user != null)
            {
                return (loginUser.Password == encryptionService.DecryptText(user.Password));
            }
            return false;
        }

        public bool HasAccessRight(string pName, User user)
        {
            return dbHelper.HasAccessRight(pName, user);
        }

        public string GetCompnayName()
        {
            return dbHelper.GetCompnayName();
        }
    }
}