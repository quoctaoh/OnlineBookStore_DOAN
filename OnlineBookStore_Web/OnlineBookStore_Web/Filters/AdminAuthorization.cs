using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OnlineBookStore_Web.Filters
{
    // Kế thừa từ ActionFilterAttribute để biến class này thành một Attribute (bộ lọc)
    public class AdminAuthorization : ActionFilterAttribute
    {
        // Ghi đè phương thức này để code chạy TRƯỚC khi Action của Controller chạy
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 1. Kiểm tra Session AdminId 
            // Lấy giá trị Session "AdminId" đã được lưu trong quá trình đăng nhập Admin
            var adminId = context.HttpContext.Session.GetInt32("AdminId");

            // 2. Logic kiểm tra quyền
            if (!adminId.HasValue)
            {
                // Nếu KHÔNG có AdminId (chưa đăng nhập):
                // Thiết lập kết quả của context (Action sẽ không được chạy)
                // Chuyển hướng đến trang Đăng nhập Admin
                context.Result = new RedirectToActionResult("LoginAdmin", "Account", null);
            }

            // Gọi phương thức gốc để cho phép Action chạy nếu AdminId tồn tại
            base.OnActionExecuting(context);
        }
    }
}